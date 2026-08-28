import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/config/app_config.dart';
import '../models/auth_user.dart';

const _accessTokenKey = 'access_token';
const _refreshTokenKey = 'refresh_token';
const _userKeyPrefix = 'user_';

class TokenStorage {
  String? accessToken;
  String? refreshToken;
}

class AuthState {
  const AuthState({
    this.user,
    this.isLoading = false,
    this.error,
    this.message,
  });

  final AuthUser? user;
  final bool isLoading;
  final String? error;
  final String? message;

  bool get isAuthenticated => user != null;
}

class CheckEmailResult {
  final bool exists;
  final String? message;
  const CheckEmailResult({required this.exists, this.message});
}

class AuthNotifier extends StateNotifier<AuthState> {
  AuthNotifier(this._config, this._tokenStorage) : super(const AuthState()) {
    _dio = _createDio();
    _restoreSession();
  }

  final AppConfig _config;
  final TokenStorage _tokenStorage;
  late final Dio _dio;

  Dio get client => _dio;
  String? get accessToken => _tokenStorage.accessToken;

  Dio _createDio() {
    final dio = Dio(BaseOptions(
      baseUrl: _config.apiBaseUrl,
      connectTimeout: const Duration(seconds: 20),
      receiveTimeout: const Duration(seconds: 30),
      headers: {
        'Content-Type': 'application/json',
        'ngrok-skip-browser-warning': 'true',
      },
    ));

    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) {
        final token = _tokenStorage.accessToken;
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
      onError: (error, handler) async {
        final path = error.requestOptions.path;
        final isAuthEndpoint = path.contains('/api/auth/');

        if (error.response?.statusCode != 401 || isAuthEndpoint) {
          return handler.next(error);
        }

        final refreshToken = _tokenStorage.refreshToken;
        if (refreshToken == null) {
          await logout();
          return handler.next(error);
        }

        bool refreshSucceeded = false;
        try {
          final response = await Dio(BaseOptions(
            baseUrl: _config.apiBaseUrl,
            headers: {'ngrok-skip-browser-warning': 'true'},
          )).post<Map<String, dynamic>>(
            '/api/auth/refresh',
            data: {'refreshToken': refreshToken},
          );
          await applyAuth(AuthResponse.fromJson(response.data!));
          refreshSucceeded = true;
        } catch (_) {
          await logout();
          return handler.next(error);
        }

        if (refreshSucceeded) {
          try {
            final request = error.requestOptions;
            request.headers['Authorization'] = 'Bearer ${_tokenStorage.accessToken}';
            final retryResponse = await dio.fetch(request);
            return handler.resolve(retryResponse);
          } catch (e) {
            return handler.next(e is DioException ? e : error);
          }
        }
      },
    ));

    return dio;
  }

  Future<void> applyAuth(AuthResponse auth) async {
    _tokenStorage.accessToken = auth.accessToken;
    _tokenStorage.refreshToken = auth.refreshToken;

    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_accessTokenKey, auth.accessToken);
    await prefs.setString(_refreshTokenKey, auth.refreshToken);
    await prefs.setInt('${_userKeyPrefix}userId', auth.user.userId);
    await prefs.setString('${_userKeyPrefix}email', auth.user.email);
    await prefs.setString('${_userKeyPrefix}firstName', auth.user.firstName);
    await prefs.setString('${_userKeyPrefix}lastName', auth.user.lastName);
    await prefs.setString('${_userKeyPrefix}userRole', auth.user.userRole);
    await prefs.setString('${_userKeyPrefix}subscriptionTier', auth.user.subscriptionTier);
    await prefs.setBool('${_userKeyPrefix}isEmailVerified', auth.user.isEmailVerified);
    if (auth.user.clinicId != null) {
      await prefs.setInt('${_userKeyPrefix}clinicId', auth.user.clinicId!);
    }

    state = AuthState(user: auth.user);
  }

  Future<void> _restoreSession() async {
    final prefs = await SharedPreferences.getInstance();
    final accessToken = prefs.getString(_accessTokenKey);
    final refreshToken = prefs.getString(_refreshTokenKey);
    final email = prefs.getString('${_userKeyPrefix}email');

    if (accessToken == null || email == null) return;

    _tokenStorage.accessToken = accessToken;
    _tokenStorage.refreshToken = refreshToken;

    state = AuthState(
      user: AuthUser(
        userId: prefs.getInt('${_userKeyPrefix}userId') ?? 0,
        email: email,
        firstName: prefs.getString('${_userKeyPrefix}firstName') ?? '',
        lastName: prefs.getString('${_userKeyPrefix}lastName') ?? '',
        userRole: prefs.getString('${_userKeyPrefix}userRole') ?? 'Owner',
        subscriptionTier: prefs.getString('${_userKeyPrefix}subscriptionTier') ?? 'Free',
        clinicId: prefs.getInt('${_userKeyPrefix}clinicId'),
        isEmailVerified: prefs.getBool('${_userKeyPrefix}isEmailVerified') ?? false,
      ),
    );
  }

  void clearFeedback() {
    state = AuthState(user: state.user, message: null, error: null);
  }

  Future<bool> login(String email, String password) async {
    state = const AuthState(isLoading: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/auth/login',
        data: {'email': email, 'password': password},
      );
      await applyAuth(AuthResponse.fromJson(response.data!));
      return true;
    } catch (e) {
      final errorMsg = _parseError(e, fallback: 'Invalid email or password.');
      state = AuthState(error: errorMsg);
      return false;
    }
  }

  Future<CheckEmailResult> checkEmail(String email) async {
    final clean = email.trim();
    if (clean.isEmpty) return const CheckEmailResult(exists: false);
    try {
      final response = await _dio.get<Map<String, dynamic>>(
        '/api/auth/check-email',
        queryParameters: {'email': clean},
      );
      final exists = response.data?['exists'] as bool? ?? false;
      final message = response.data?['message'] as String?;
      return CheckEmailResult(exists: exists, message: message);
    } catch (_) {
      return const CheckEmailResult(exists: false);
    }
  }

  Future<bool> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String inviteCode,
    String? phoneNumber,
  }) async {
    state = const AuthState(isLoading: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/auth/register',
        data: {
          'email': email.trim(),
          'password': password,
          'firstName': firstName.trim(),
          'lastName': lastName.trim(),
          'inviteCode': inviteCode.trim().toUpperCase(),
          'role': 'Owner',
          if (phoneNumber != null && phoneNumber.trim().isNotEmpty) 'phoneNumber': phoneNumber.trim(),
        },
      );
      final auth = AuthResponse.fromJson(response.data!);
      await applyAuth(auth);
      return true;
    } catch (e) {
      final message = _parseError(e, fallback: 'Unable to create account. Please try again.');
      state = AuthState(error: message);
      return false;
    }
  }

  String _parseError(dynamic error, {String fallback = 'An error occurred.'}) {
    if (error is DioException) {
      if (error.type == DioExceptionType.connectionTimeout ||
          error.type == DioExceptionType.sendTimeout) {
        return 'Connection timed out. Please check your internet connection.';
      }
      if (error.type == DioExceptionType.receiveTimeout) {
        return 'Server took longer than expected to respond. If your account was created, please try signing in.';
      }
      if (error.type == DioExceptionType.connectionError) {
        return 'Unable to reach Triple A server. Please check your internet connection.';
      }

      final data = error.response?.data;
      if (data is Map) {
        // 1. Direct message
        if (data['message'] is String && (data['message'] as String).trim().isNotEmpty) {
          return (data['message'] as String).trim();
        }
        // 2. ASP.NET Core Validation Problem Details dictionary
        if (data['errors'] is Map) {
          final errMap = data['errors'] as Map;
          final errorList = <String>[];
          for (final entry in errMap.entries) {
            final value = entry.value;
            if (value is List) {
              errorList.addAll(value.map((e) => e.toString()));
            } else if (value != null) {
              errorList.add(value.toString());
            }
          }
          if (errorList.isNotEmpty) {
            return errorList.join('\n');
          }
        }
        // 3. ProblemDetails title
        if (data['title'] is String && (data['title'] as String).trim().isNotEmpty) {
          return (data['title'] as String).trim();
        }
      } else if (data is String && data.trim().isNotEmpty && !data.contains('<html')) {
        return data.trim();
      }
    }
    return fallback;
  }

  Future<String?> resendVerification(String email) async {
    state = AuthState(user: state.user, isLoading: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/auth/resend-verification',
        data: {'email': email},
      );
      final msg = response.data?['message'] as String? ?? 'Verification email sent if account exists.';
      state = AuthState(user: state.user, message: msg);
      return msg;
    } catch (e) {
      String msg = 'Failed to send verification email.';
      if (e is DioException && e.response?.data is Map) {
        msg = (e.response?.data['message'] as String?) ?? msg;
      }
      state = AuthState(user: state.user, error: msg);
      return null;
    }
  }

  Future<bool> verifyEmail(String email, String token) async {
    state = AuthState(user: state.user, isLoading: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/auth/verify-email',
        data: {'email': email, 'token': token},
      );
      final msg = response.data?['message'] as String? ?? 'Email verified successfully!';
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('${_userKeyPrefix}isEmailVerified', true);

      AuthUser? updatedUser;
      if (state.user != null) {
        updatedUser = AuthUser(
          userId: state.user!.userId,
          email: state.user!.email,
          firstName: state.user!.firstName,
          lastName: state.user!.lastName,
          userRole: state.user!.userRole,
          subscriptionTier: state.user!.subscriptionTier,
          clinicId: state.user!.clinicId,
          clinicName: state.user!.clinicName,
          clinicInviteCode: state.user!.clinicInviteCode,
          isEmailVerified: true,
        );
      }
      state = AuthState(user: updatedUser ?? state.user, message: msg);
      return true;
    } catch (e) {
      String msg = 'Invalid or expired verification link.';
      if (e is DioException && e.response?.data is Map) {
        msg = (e.response?.data['message'] as String?) ?? msg;
      }
      state = AuthState(user: state.user, error: msg);
      return false;
    }
  }

  Future<String?> forgotPassword(String email) async {
    state = const AuthState(isLoading: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/auth/forgot-password',
        data: {'email': email},
      );
      final message = response.data?['message'] as String? ??
          'If an account exists for that email, we\'ve sent password reset instructions.';
      state = AuthState(message: message);
      return message;
    } on DioException {
      state = const AuthState(error: 'Unable to send reset instructions. Please try again.');
      return null;
    }
  }

  Future<bool> resetPassword(String token, String newPassword) async {
    state = const AuthState(isLoading: true);
    try {
      await _dio.post<Map<String, dynamic>>(
        '/api/auth/reset-password',
        data: {'token': token, 'newPassword': newPassword},
      );
      state = const AuthState(message: 'Your password has been updated. You can sign in now.');
      return true;
    } on DioException catch (e) {
      final message = e.response?.data is Map
          ? (e.response?.data['message'] as String?) ?? 'Invalid or expired reset link.'
          : 'Invalid or expired reset link.';
      state = AuthState(error: message);
      return false;
    }
  }

  Future<bool> changePassword(String currentPassword, String newPassword) async {
    state = AuthState(user: state.user, isLoading: true);
    try {
      await _dio.put<Map<String, dynamic>>(
        '/api/auth/change-password',
        data: {
          'currentPassword': currentPassword,
          'newPassword': newPassword,
        },
      );
      state = AuthState(user: state.user, message: 'Your password has been updated.');
      return true;
    } on DioException catch (e) {
      final message = e.response?.data is Map
          ? (e.response?.data['message'] as String?) ?? 'Unable to change password.'
          : 'Unable to change password.';
      state = AuthState(user: state.user, error: message);
      return false;
    }
  }

  Future<bool> updateProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
  }) async {
    state = AuthState(user: state.user, isLoading: true);
    try {
      final response = await _dio.put<Map<String, dynamic>>(
        '/api/auth/profile',
        data: {
          'firstName': firstName,
          'lastName': lastName,
          if (phoneNumber != null && phoneNumber.isNotEmpty) 'phoneNumber': phoneNumber,
        },
      );
      final updatedUser = AuthUser.fromJson(response.data!);
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('${_userKeyPrefix}firstName', updatedUser.firstName);
      await prefs.setString('${_userKeyPrefix}lastName', updatedUser.lastName);
      if (updatedUser.phoneNumber != null) {
        await prefs.setString('${_userKeyPrefix}phoneNumber', updatedUser.phoneNumber!);
      }
      state = AuthState(user: updatedUser, message: 'Profile updated successfully!');
      return true;
    } on DioException catch (e) {
      final message = e.response?.data is Map
          ? (e.response?.data['message'] as String?) ?? 'Failed to update profile.'
          : 'Failed to update profile.';
      state = AuthState(user: state.user, error: message);
      return false;
    }
  }

  Future<Map<String, dynamic>?> requestDataDeletion({
    required String email,
    required String requestType,
    String? reason,
    String? additionalNotes,
  }) async {
    try {
      final response = await _dio.post(
        '/auth/request-data-deletion',
        data: {
          'email': email,
          'requestType': requestType,
          'reason': reason,
          'additionalNotes': additionalNotes,
        },
      );
      if (response.data is Map<String, dynamic>) {
        return response.data as Map<String, dynamic>;
      }
      return {'success': true, 'message': 'Request submitted successfully.'};
    } on DioException catch (e) {
      final message = e.response?.data is Map
          ? (e.response?.data['message'] as String?) ?? 'Failed to submit request.'
          : 'Failed to submit request.';
      return {'success': false, 'message': message};
    } catch (_) {
      return {'success': false, 'message': 'An unexpected error occurred.'};
    }
  }

  Future<void> logout() async {
    _tokenStorage.accessToken = null;
    _tokenStorage.refreshToken = null;

    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_accessTokenKey);
    await prefs.remove(_refreshTokenKey);
    await prefs.remove('${_userKeyPrefix}email');
    await prefs.remove('${_userKeyPrefix}userId');
    await prefs.remove('${_userKeyPrefix}firstName');
    await prefs.remove('${_userKeyPrefix}lastName');
    await prefs.remove('${_userKeyPrefix}userRole');
    await prefs.remove('${_userKeyPrefix}subscriptionTier');
    await prefs.remove('${_userKeyPrefix}clinicId');
    state = const AuthState();
  }
}

final appConfigProvider = Provider<AppConfig>((ref) => AppConfig.fromEnvironment());

final tokenStorageProvider = Provider<TokenStorage>((ref) => TokenStorage());

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(
    ref.watch(appConfigProvider),
    ref.watch(tokenStorageProvider),
  );
});

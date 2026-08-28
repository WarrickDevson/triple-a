import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';

class TrackingState {
  const TrackingState({
    this.isLoading = false,
    this.isSubmitting = false,
    this.pain = 5,
    this.energy = 5,
    this.mobility = 5,
    this.appetite = 5,
    this.lameness = 5,
    this.error,
    this.lastSaved,
    this.hasTodayLog = false,
  });

  final bool isLoading;
  final bool isSubmitting;
  final int pain;
  final int energy;
  final int mobility;
  final int appetite;
  final int lameness;
  final String? error;
  final DateTime? lastSaved;
  final bool hasTodayLog;

  TrackingState copyWith({
    bool? isLoading,
    bool? isSubmitting,
    int? pain,
    int? energy,
    int? mobility,
    int? appetite,
    int? lameness,
    String? error,
    bool clearError = false,
    DateTime? lastSaved,
    bool? hasTodayLog,
  }) {
    return TrackingState(
      isLoading: isLoading ?? this.isLoading,
      isSubmitting: isSubmitting ?? this.isSubmitting,
      pain: pain ?? this.pain,
      energy: energy ?? this.energy,
      mobility: mobility ?? this.mobility,
      appetite: appetite ?? this.appetite,
      lameness: lameness ?? this.lameness,
      error: clearError ? null : (error ?? this.error),
      lastSaved: lastSaved ?? this.lastSaved,
      hasTodayLog: hasTodayLog ?? this.hasTodayLog,
    );
  }
}

class TrackingNotifier extends StateNotifier<TrackingState> {
  TrackingNotifier(this._dio, this._petId) : super(const TrackingState()) {
    loadTodayLog();
  }

  final Dio _dio;
  final int _petId;

  Future<void> loadTodayLog() async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final response = await _dio.get<List<dynamic>>(
        '/api/pets/$_petId/tracking',
        queryParameters: {'days': 2},
      );

      if (response.data != null && response.data!.isNotEmpty) {
        final now = DateTime.now().toUtc();
        final todayStr =
            '${now.year}-${now.month.toString().padLeft(2, '0')}-${now.day.toString().padLeft(2, '0')}';

        final firstLog = response.data!.first as Map<String, dynamic>;
        final logDate = firstLog['logDate']?.toString();

        if (logDate == todayStr) {
          state = state.copyWith(
            isLoading: false,
            pain: (firstLog['painScore'] as num?)?.toInt() ?? 5,
            energy: (firstLog['energyScore'] as num?)?.toInt() ?? 5,
            mobility: (firstLog['mobilityScore'] as num?)?.toInt() ?? 5,
            appetite: (firstLog['appetiteScore'] as num?)?.toInt() ?? 5,
            lameness: (firstLog['lamenessScore'] as num?)?.toInt() ?? 5,
            hasTodayLog: true,
            lastSaved: DateTime.now(),
          );
          return;
        }
      }
      state = state.copyWith(isLoading: false);
    } catch (e) {
      debugPrint('[TrackingProvider] Failed to load today log for pet $_petId: $e');
      state = state.copyWith(isLoading: false);
    }
  }

  void updateScores({
    int? pain,
    int? energy,
    int? mobility,
    int? appetite,
    int? lameness,
  }) {
    state = state.copyWith(
      pain: pain,
      energy: energy,
      mobility: mobility,
      appetite: appetite,
      lameness: lameness,
      clearError: true,
    );
  }

  Future<bool> submit({
    int? pain,
    int? energy,
    int? mobility,
    int? appetite,
    int? lameness,
  }) async {
    final p = pain ?? state.pain;
    final e = energy ?? state.energy;
    final m = mobility ?? state.mobility;
    final a = appetite ?? state.appetite;
    final l = lameness ?? state.lameness;

    state = state.copyWith(isSubmitting: true, clearError: true);
    try {
      await _dio.post<void>(
        '/api/pets/$_petId/tracking',
        data: {
          'painScore': p,
          'energyScore': e,
          'mobilityScore': m,
          'appetiteScore': a,
          'lamenessScore': l,
        },
      );
      state = state.copyWith(
        isSubmitting: false,
        pain: p,
        energy: e,
        mobility: m,
        appetite: a,
        lameness: l,
        lastSaved: DateTime.now(),
        hasTodayLog: true,
        clearError: true,
      );
      return true;
    } on DioException catch (dioErr) {
      debugPrint(
          '[TrackingProvider] Submit error on /api/pets/$_petId/tracking: status=${dioErr.response?.statusCode}, data=${dioErr.response?.data}');
      String errorMessage = 'Unable to save tracking log. Please try again.';
      if (dioErr.response?.data is Map) {
        final data = dioErr.response!.data as Map<String, dynamic>;
        errorMessage = data['message']?.toString() ??
            data['title']?.toString() ??
            data['error']?.toString() ??
            errorMessage;
      } else if (dioErr.response?.statusCode == 403) {
        errorMessage = 'You do not have permission to log tracking for this pet.';
      } else if (dioErr.response?.statusCode == 404) {
        errorMessage = 'Pet profile not found.';
      } else if (dioErr.type == DioExceptionType.connectionError ||
          dioErr.type == DioExceptionType.connectionTimeout) {
        errorMessage = 'Network connection failed. Ensure your backend is running.';
      }
      state = state.copyWith(isSubmitting: false, error: errorMessage);
      return false;
    } catch (err) {
      debugPrint('[TrackingProvider] Unexpected submit error: $err');
      state = state.copyWith(
        isSubmitting: false,
        error: 'An unexpected error occurred: $err',
      );
      return false;
    }
  }
}

final trackingProvider =
    StateNotifierProvider.family<TrackingNotifier, TrackingState, int>((ref, petId) {
  return TrackingNotifier(ref.read(authProvider.notifier).client, petId);
});

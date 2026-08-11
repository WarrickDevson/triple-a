import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/providers/auth_provider.dart';
import 'features/auth/screens/login_screen.dart';
import 'features/auth/screens/reset_password_screen.dart';
import 'features/auth/screens/signup_screen.dart';
import 'features/auth/screens/verify_email_screen.dart';
import 'features/shell/main_shell.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const ProviderScope(child: MoveWellApp()));
}

class MoveWellApp extends ConsumerWidget {
  const MoveWellApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final config = ref.watch(appConfigProvider);
    final auth = ref.watch(authProvider);

    final resetToken = _resetTokenFromUri();
    final inviteCode = _inviteCodeFromUri();
    final verifyParams = _verifyEmailParamsFromUri();

    Widget home;
    if (verifyParams != null) {
      home = VerifyEmailScreen(
        email: verifyParams['email']!,
        initialToken: verifyParams['token'],
      );
    } else if (resetToken != null) {
      home = ResetPasswordScreen(initialToken: resetToken);
    } else if (auth.isAuthenticated) {
      if (auth.user != null && !auth.user!.isEmailVerified) {
        home = VerifyEmailScreen(email: auth.user!.email);
      } else {
        home = const MainShell();
      }
    } else if (inviteCode != null) {
      home = SignupScreen(initialInviteCode: inviteCode);
    } else {
      home = const LoginScreen();
    }

    return MaterialApp(
      title: config.appName,
      theme: AppTheme.light(),
      home: home,
    );
  }

  String? _resetTokenFromUri() {
    final uri = Uri.base;
    if (uri.path.contains('reset-password') && uri.queryParameters['token'] != null) {
      return uri.queryParameters['token'];
    }
    return null;
  }

  String? _inviteCodeFromUri() {
    final uri = Uri.base;
    if (uri.queryParameters['inviteCode'] != null && uri.queryParameters['inviteCode']!.isNotEmpty) {
      return uri.queryParameters['inviteCode'];
    }
    if (uri.path.contains('register') && uri.queryParameters['inviteCode'] != null) {
      return uri.queryParameters['inviteCode'];
    }
    return null;
  }

  Map<String, String?>? _verifyEmailParamsFromUri() {
    final uri = Uri.base;
    final hasVerifyPath = uri.path.contains('verify');
    final hasToken = uri.queryParameters['token'] != null && uri.queryParameters['token']!.isNotEmpty;
    if (hasVerifyPath || hasToken) {
      final email = uri.queryParameters['email'];
      final token = uri.queryParameters['token'];
      if (email != null && email.isNotEmpty) {
        return {'email': email, 'token': token};
      }
    }
    return null;
  }
}

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/providers/auth_provider.dart';
import 'features/auth/screens/login_screen.dart';
import 'features/auth/screens/reset_password_screen.dart';
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
    final initialRoute = _initialRoute();

    return MaterialApp(
      title: config.appName,
      theme: AppTheme.light(),
      home: initialRoute != null
          ? ResetPasswordScreen(initialToken: initialRoute)
          : auth.isAuthenticated
              ? const MainShell()
              : const LoginScreen(),
    );
  }

  String? _initialRoute() {
    final uri = Uri.base;
    if (uri.path.contains('reset-password') && uri.queryParameters['token'] != null) {
      return uri.queryParameters['token'];
    }
    return null;
  }
}

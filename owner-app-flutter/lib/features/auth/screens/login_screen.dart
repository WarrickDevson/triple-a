import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/config/brand.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/brand_logo.dart';
import '../providers/auth_provider.dart';
import '../../shell/main_shell.dart';
import 'forgot_password_screen.dart';
import 'signup_screen.dart';
import 'verify_email_screen.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _passwordVisible = false;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final email = _emailController.text.trim();
    final ok = await ref.read(authProvider.notifier).login(
          email,
          _passwordController.text,
        );
    if (!mounted) return;
    if (ok) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const MainShell()),
      );
    } else {
      final err = ref.read(authProvider).error ?? '';
      if (err.contains('EMAIL_NOT_VERIFIED') || err.toLowerCase().contains('verify your email')) {
        Navigator.of(context).push(
          MaterialPageRoute(
            builder: (_) => VerifyEmailScreen(email: email),
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authProvider);

    return Scaffold(
      body: Stack(
        fit: StackFit.expand,
        children: [
          const _LoginAtmosphere(),
          SafeArea(
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 420),
                  child: Column(
                    children: [
                      const BrandLogo(size: 64, showTagline: true, light: true),
                      const SizedBox(height: 24),
                      AppPanel(
                        padding: const EdgeInsets.all(28),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Welcome Back',
                              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                    color: AppColors.navy,
                                    fontWeight: FontWeight.w800,
                                  ),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              'Sign in to continue to your ${Brand.name} workspace.',
                              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                    color: AppColors.neutralMuted,
                                  ),
                            ),
                            const SizedBox(height: 28),
                            TextField(
                              controller: _emailController,
                              keyboardType: TextInputType.emailAddress,
                              textInputAction: TextInputAction.next,
                              decoration: const InputDecoration(labelText: 'Email'),
                            ),
                            const SizedBox(height: 16),
                            TextField(
                              controller: _passwordController,
                              obscureText: !_passwordVisible,
                              textInputAction: TextInputAction.done,
                              onSubmitted: (_) => auth.isLoading ? null : _submit(),
                              decoration: InputDecoration(
                                labelText: 'Password',
                                suffixIcon: IconButton(
                                  tooltip: _passwordVisible ? 'Hide password' : 'Show password',
                                  onPressed: () =>
                                      setState(() => _passwordVisible = !_passwordVisible),
                                  icon: Icon(
                                    _passwordVisible
                                        ? Icons.visibility_off_outlined
                                        : Icons.visibility_outlined,
                                    color: AppColors.sage,
                                  ),
                                ),
                              ),
                            ),
                            if (auth.error != null) ...[
                              const SizedBox(height: 12),
                              Text(
                                auth.error!,
                                style: const TextStyle(color: AppColors.alertRed),
                              ),
                            ],
                            const SizedBox(height: 24),
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                onPressed: auth.isLoading ? null : _submit,
                                child: Text(auth.isLoading ? 'Signing in...' : 'Sign In'),
                              ),
                            ),
                            const SizedBox(height: 8),
                            Align(
                              alignment: Alignment.centerRight,
                              child: TextButton(
                                onPressed: () => Navigator.of(context).push(
                                  MaterialPageRoute(builder: (_) => const ForgotPasswordScreen()),
                                ),
                                child: const Text('Forgot password?'),
                              ),
                            ),
                            TextButton(
                              onPressed: () => Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => const SignupScreen()),
                              ),
                              child: const Text('New owner? Create an account'),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),
                      Text(
                        Brand.motto,
                        style: GoogleFonts.caveat(
                          fontSize: 20,
                          color: Colors.white.withValues(alpha: 0.85),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _LoginAtmosphere extends StatelessWidget {
  const _LoginAtmosphere();

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppColors.navy,
            Color(0xFF122A42),
            AppColors.navy,
          ],
        ),
      ),
      child: Stack(
        children: [
          Positioned(
            top: -40,
            left: -60,
            child: _Glow(
              size: 280,
              color: AppColors.sageLight.withValues(alpha: 0.35),
            ),
          ),
          Positioned(
            bottom: 40,
            right: -50,
            child: _Glow(
              size: 260,
              color: AppColors.sage.withValues(alpha: 0.2),
            ),
          ),
        ],
      ),
    );
  }
}

class _Glow extends StatelessWidget {
  const _Glow({required this.size, required this.color});

  final double size;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      child: Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          gradient: RadialGradient(
            colors: [color, color.withValues(alpha: 0)],
          ),
        ),
      ),
    );
  }
}

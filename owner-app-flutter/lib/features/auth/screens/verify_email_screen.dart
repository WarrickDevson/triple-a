import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../providers/auth_provider.dart';
import 'login_screen.dart';

class VerifyEmailScreen extends ConsumerStatefulWidget {
  final String email;
  final String? initialToken;

  const VerifyEmailScreen({
    super.key,
    required this.email,
    this.initialToken,
  });

  @override
  ConsumerState<VerifyEmailScreen> createState() => _VerifyEmailScreenState();
}

class _VerifyEmailScreenState extends ConsumerState<VerifyEmailScreen> {
  bool _isVerifyingToken = false;
  String? _statusMessage;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (widget.initialToken != null && widget.initialToken!.isNotEmpty) {
        _processAutoVerification();
      } else {
        ref.read(authProvider.notifier).clearFeedback();
      }
    });
  }

  Future<void> _processAutoVerification() async {
    setState(() {
      _isVerifyingToken = true;
      _errorMessage = null;
      _statusMessage = null;
    });

    final success = await ref.read(authProvider.notifier).verifyEmail(
          widget.email,
          widget.initialToken!,
        );

    if (!mounted) return;

    setState(() {
      _isVerifyingToken = false;
      if (success) {
        _statusMessage = 'Your email has been verified successfully! You can now sign in.';
      } else {
        _errorMessage = ref.read(authProvider).error ?? 'Invalid or expired verification link.';
      }
    });
  }

  Future<void> _resendVerificationEmail() async {
    setState(() {
      _statusMessage = null;
      _errorMessage = null;
    });

    final msg = await ref.read(authProvider.notifier).resendVerification(widget.email);
    if (!mounted) return;

    setState(() {
      if (msg != null) {
        _statusMessage = msg;
      } else {
        _errorMessage = ref.read(authProvider).error ?? 'Failed to resend verification email.';
      }
    });
  }

  void _goToLogin() {
    ref.read(authProvider.notifier).logout();
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authProvider);

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: AppPanel(
                padding: const EdgeInsets.all(28),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      height: 64,
                      width: 64,
                      decoration: const BoxDecoration(
                        color: AppColors.sageMuted,
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.mark_email_unread_outlined,
                        size: 32,
                        color: AppColors.sage,
                      ),
                    ),
                    const SizedBox(height: 20),
                    Text(
                      'Verify your email',
                      style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                            color: AppColors.navy,
                            fontWeight: FontWeight.w800,
                          ),
                    ),
                    const SizedBox(height: 12),
                    Text(
                      'We sent a verification link to:\n${widget.email}',
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: AppColors.navy,
                        fontWeight: FontWeight.w600,
                        height: 1.4,
                      ),
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'Please check your inbox and click the verification button to activate your account.',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: AppColors.neutralMuted,
                        fontSize: 14,
                        height: 1.4,
                      ),
                    ),
                    if (_isVerifyingToken || auth.isLoading) ...[
                      const SizedBox(height: 20),
                      const CircularProgressIndicator(color: AppColors.sage),
                      const SizedBox(height: 8),
                      const Text(
                        'Verifying token...',
                        style: TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                      ),
                    ],
                    if (_statusMessage != null || auth.message != null) ...[
                      const SizedBox(height: 16),
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: const Color(0xFFECFDF5),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: const Color(0xFFA7F3D0)),
                        ),
                        child: Text(
                          _statusMessage ?? auth.message!,
                          textAlign: TextAlign.center,
                          style: const TextStyle(color: Color(0xFF065F46), fontSize: 13),
                        ),
                      ),
                    ],
                    if (_errorMessage != null || auth.error != null) ...[
                      const SizedBox(height: 16),
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.red.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.red.shade200),
                        ),
                        child: Text(
                          _errorMessage ?? auth.error!,
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.red.shade800, fontSize: 13),
                        ),
                      ),
                    ],
                    const SizedBox(height: 24),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: auth.isLoading ? null : _resendVerificationEmail,
                        child: const Text('Resend Verification Email'),
                      ),
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: TextButton(
                        onPressed: _goToLogin,
                        child: const Text('Back to Sign in'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

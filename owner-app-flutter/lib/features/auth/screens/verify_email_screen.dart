import 'dart:async';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../shell/main_shell.dart';
import '../providers/auth_provider.dart';
import 'login_screen.dart';

const String _cooldownKey = 'resend_cooldown_owner';
const int _cooldownSeconds = 60;
const String _playStoreUrl = 'https://play.google.com/store/apps/details?id=com.devson.triplea';
const String _appDeepLink = 'triplea://app';

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
  int _cooldownRemaining = 0;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _checkCooldown();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (widget.initialToken != null && widget.initialToken!.isNotEmpty) {
        _processAutoVerification();
      } else {
        ref.read(authProvider.notifier).clearFeedback();
      }
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  Future<void> _checkCooldown() async {
    final prefs = await SharedPreferences.getInstance();
    final expiry = prefs.getInt(_cooldownKey);
    if (expiry != null) {
      final now = DateTime.now().millisecondsSinceEpoch ~/ 1000;
      if (expiry > now) {
        setState(() {
          _cooldownRemaining = expiry - now;
        });
        _startTimer();
      } else {
        await prefs.remove(_cooldownKey);
      }
    }
  }

  void _startTimer() {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) async {
      final prefs = await SharedPreferences.getInstance();
      final expiry = prefs.getInt(_cooldownKey);
      if (expiry == null) {
        timer.cancel();
        if (mounted) setState(() => _cooldownRemaining = 0);
        return;
      }
      final now = DateTime.now().millisecondsSinceEpoch ~/ 1000;
      final remaining = expiry - now;
      if (remaining > 0) {
        if (mounted) setState(() => _cooldownRemaining = remaining);
      } else {
        timer.cancel();
        await prefs.remove(_cooldownKey);
        if (mounted) setState(() => _cooldownRemaining = 0);
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
        final isAuth = ref.read(authProvider).isAuthenticated;
        _statusMessage = isAuth
            ? 'Email verified successfully! Redirecting to app...'
            : 'Your email has been verified successfully! You can now sign in.';

        if (isAuth) {
          Future.delayed(const Duration(seconds: 2), () {
            if (mounted) {
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const MainShell()),
              );
            }
          });
        }

        if (kIsWeb) {
          launchUrl(Uri.parse(_appDeepLink), mode: LaunchMode.externalNonBrowserApplication).catchError((_) => false);
        }
      } else {
        _errorMessage = ref.read(authProvider).error ?? 'Invalid or expired verification link.';
      }
    });
  }

  Future<void> _resendVerificationEmail() async {
    if (_cooldownRemaining > 0) return;

    final expiry = (DateTime.now().millisecondsSinceEpoch ~/ 1000) + _cooldownSeconds;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setInt(_cooldownKey, expiry);

    setState(() {
      _statusMessage = null;
      _errorMessage = null;
      _cooldownRemaining = _cooldownSeconds;
    });
    _startTimer();

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
                    if (kIsWeb) ...[
                      const SizedBox(height: 16),
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                        decoration: BoxDecoration(
                          color: AppColors.navy,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Row(
                          children: [
                            const Icon(Icons.android, color: Colors.white, size: 20),
                            const SizedBox(width: 10),
                            const Expanded(
                              child: Text(
                                'Using Android? Get the full experience in the app.',
                                style: TextStyle(color: Colors.white, fontSize: 12),
                              ),
                            ),
                            TextButton(
                              onPressed: () => launchUrl(
                                Uri.parse(_playStoreUrl),
                                mode: LaunchMode.externalApplication,
                              ),
                              style: TextButton.styleFrom(
                                foregroundColor: Colors.white,
                                backgroundColor: AppColors.sage,
                                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                minimumSize: const Size(0, 30),
                              ),
                              child: const Text('GET APP', style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold)),
                            ),
                          ],
                        ),
                      ),
                    ],
                    if (kIsWeb && _statusMessage != null && _statusMessage!.contains('successfully')) ...[
                      const SizedBox(height: 16),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton.icon(
                          onPressed: () => launchUrl(
                            Uri.parse(_appDeepLink),
                            mode: LaunchMode.externalNonBrowserApplication,
                          ),
                          icon: const Icon(Icons.open_in_new, size: 18),
                          label: const Text('Open in Triple A App'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.sage,
                            foregroundColor: Colors.white,
                          ),
                        ),
                      ),
                    ],
                    const SizedBox(height: 24),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: (auth.isLoading || _cooldownRemaining > 0)
                            ? null
                            : _resendVerificationEmail,
                        child: Text(
                          _cooldownRemaining > 0
                              ? 'Resend in ${_cooldownRemaining}s'
                              : 'Resend Verification Email',
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: TextButton(
                        onPressed: _goToLogin,
                        child: Text(auth.isAuthenticated ? 'Back to App' : 'Back to Sign in'),
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

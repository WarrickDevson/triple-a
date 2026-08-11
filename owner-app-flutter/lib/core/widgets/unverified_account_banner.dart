import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../features/auth/providers/auth_provider.dart';

const String _cooldownKey = 'resend_cooldown_owner';
const int _cooldownSeconds = 60;

class UnverifiedAccountBanner extends ConsumerStatefulWidget {
  const UnverifiedAccountBanner({super.key});

  @override
  ConsumerState<UnverifiedAccountBanner> createState() => _UnverifiedAccountBannerState();
}

class _UnverifiedAccountBannerState extends ConsumerState<UnverifiedAccountBanner> {
  int _cooldownRemaining = 0;
  bool _isSending = false;
  bool _sendSuccess = false;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _checkCooldown();
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
      if (_cooldownRemaining > 1) {
        setState(() {
          _cooldownRemaining--;
        });
      } else {
        timer.cancel();
        final prefs = await SharedPreferences.getInstance();
        await prefs.remove(_cooldownKey);
        setState(() {
          _cooldownRemaining = 0;
        });
      }
    });
  }

  Future<void> _handleResend() async {
    final authState = ref.read(authProvider);
    final user = authState.user;
    if (user == null || _cooldownRemaining > 0 || _isSending) return;

    setState(() {
      _isSending = true;
      _sendSuccess = false;
    });

    final msg = await ref.read(authProvider.notifier).resendVerification(user.email);

    if (!mounted) return;

    setState(() {
      _isSending = false;
      if (msg != null) {
        _sendSuccess = true;
      }
    });

    if (msg != null) {
      final expiry = (DateTime.now().millisecondsSinceEpoch ~/ 1000) + _cooldownSeconds;
      final prefs = await SharedPreferences.getInstance();
      await prefs.setInt(_cooldownKey, expiry);
      setState(() {
        _cooldownRemaining = _cooldownSeconds;
      });
      _startTimer();
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);
    final user = authState.user;

    if (user == null || user.isEmailVerified) {
      return const SizedBox.shrink();
    }

    return Container(
      width: double.infinity,
      color: const Color(0xFFFFFBEB),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      child: SafeArea(
        bottom: false,
        top: false,
        child: Row(
          children: [
            const Icon(
              Icons.warning_amber_rounded,
              color: Color(0xFFD97706),
              size: 20,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                _sendSuccess
                    ? 'Verification link sent to ${user.email}!'
                    : 'Your email is unverified. Please check your inbox.',
                style: const TextStyle(
                  color: Color(0xFF92400E),
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
            const SizedBox(width: 8),
            ElevatedButton(
              onPressed: (_cooldownRemaining > 0 || _isSending) ? null : _handleResend,
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFFD97706),
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                minimumSize: Size.zero,
                tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(6),
                ),
                textStyle: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold),
              ),
              child: _isSending
                  ? const SizedBox(
                      height: 12,
                      width: 12,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : Text(
                      _cooldownRemaining > 0
                          ? '${_cooldownRemaining}s'
                          : 'Resend Email',
                    ),
            ),
          ],
        ),
      ),
    );
  }
}

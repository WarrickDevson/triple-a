import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../providers/auth_provider.dart';

class ChangePasswordScreen extends ConsumerStatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  ConsumerState<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends ConsumerState<ChangePasswordScreen> {
  final _currentController = TextEditingController();
  final _newController = TextEditingController();
  bool _currentVisible = false;
  bool _newVisible = false;

  @override
  void dispose() {
    _currentController.dispose();
    _newController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final ok = await ref.read(authProvider.notifier).changePassword(
          _currentController.text,
          _newController.text,
        );
    if (!mounted) return;
    if (ok) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Password updated successfully.')),
      );
      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Change password')),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: AppPanel(
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                TextField(
                  controller: _currentController,
                  obscureText: !_currentVisible,
                  decoration: InputDecoration(
                    labelText: 'Current password',
                    suffixIcon: IconButton(
                      onPressed: () => setState(() => _currentVisible = !_currentVisible),
                      icon: Icon(_currentVisible ? Icons.visibility_off_outlined : Icons.visibility_outlined),
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _newController,
                  obscureText: !_newVisible,
                  decoration: InputDecoration(
                    labelText: 'New password (min 8 characters)',
                    suffixIcon: IconButton(
                      onPressed: () => setState(() => _newVisible = !_newVisible),
                      icon: Icon(_newVisible ? Icons.visibility_off_outlined : Icons.visibility_outlined),
                    ),
                  ),
                ),
                if (auth.error != null) ...[
                  const SizedBox(height: 12),
                  Text(auth.error!, style: const TextStyle(color: AppColors.alertRed)),
                ],
                const SizedBox(height: 24),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: auth.isLoading ? null : _submit,
                    child: Text(auth.isLoading ? 'Updating...' : 'Update password'),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

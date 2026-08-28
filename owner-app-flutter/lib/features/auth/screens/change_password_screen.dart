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
  final _formKey = GlobalKey<FormState>();
  final _currentController = TextEditingController();
  final _newController = TextEditingController();
  bool _currentVisible = false;
  bool _newVisible = false;
  AutovalidateMode _autoValidateMode = AutovalidateMode.disabled;

  @override
  void dispose() {
    _currentController.dispose();
    _newController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      setState(() => _autoValidateMode = AutovalidateMode.onUserInteraction);
      return;
    }

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
            child: Form(
              key: _formKey,
              autovalidateMode: _autoValidateMode,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  TextFormField(
                    controller: _currentController,
                    obscureText: !_currentVisible,
                    decoration: InputDecoration(
                      labelText: 'Current password',
                      suffixIcon: IconButton(
                        onPressed: () => setState(() => _currentVisible = !_currentVisible),
                        icon: Icon(_currentVisible ? Icons.visibility_off_outlined : Icons.visibility_outlined),
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Current password is required';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _newController,
                    obscureText: !_newVisible,
                    decoration: InputDecoration(
                      labelText: 'New password',
                      helperText: 'Min 8 chars, uppercase, lowercase, number & symbol',
                      helperMaxLines: 2,
                      suffixIcon: IconButton(
                        onPressed: () => setState(() => _newVisible = !_newVisible),
                        icon: Icon(_newVisible ? Icons.visibility_off_outlined : Icons.visibility_outlined),
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'New password is required';
                      }
                      if (value.length < 8) {
                        return 'Must be at least 8 characters';
                      }
                      if (!RegExp(r'[a-z]').hasMatch(value)) {
                        return 'Must contain at least one lowercase letter';
                      }
                      if (!RegExp(r'[A-Z]').hasMatch(value)) {
                        return 'Must contain at least one uppercase letter';
                      }
                      if (!RegExp(r'[0-9]').hasMatch(value)) {
                        return 'Must contain at least one number';
                      }
                      if (!RegExp(r'[^a-zA-Z0-9]').hasMatch(value)) {
                        return 'Must contain at least one symbol (e.g. !@#\$)';
                      }
                      if (value == _currentController.text) {
                        return 'New password must be different from current password';
                      }
                      return null;
                    },
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
      ),
    );
  }
}

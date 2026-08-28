import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../providers/auth_provider.dart';
import 'login_screen.dart';
import 'verify_email_screen.dart';

class SignupScreen extends ConsumerStatefulWidget {
  final String? initialInviteCode;
  const SignupScreen({super.key, this.initialInviteCode});

  @override
  ConsumerState<SignupScreen> createState() => _SignupScreenState();
}

class _SignupScreenState extends ConsumerState<SignupScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _inviteController = TextEditingController();

  bool _passwordVisible = false;
  AutovalidateMode _autoValidateMode = AutovalidateMode.disabled;
  String? _localError;
  bool _isDuplicateEmail = false;

  static final _emailRegex = RegExp(
    r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
  );

  static final _saPhoneRegex = RegExp(
    r'^(\+27|27|0)[6-8][0-9]{8}$',
  );

  @override
  void initState() {
    super.initState();
    if (widget.initialInviteCode != null) {
      _inviteController.text = widget.initialInviteCode!.trim().toUpperCase();
    }
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _firstNameController.dispose();
    _lastNameController.dispose();
    _phoneController.dispose();
    _inviteController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _localError = null;
      _isDuplicateEmail = false;
    });

    if (!_formKey.currentState!.validate()) {
      setState(() {
        _autoValidateMode = AutovalidateMode.onUserInteraction;
      });
      return;
    }

    final email = _emailController.text.trim().toLowerCase();

    // Check if email already exists before submitting
    final checkResult = await ref.read(authProvider.notifier).checkEmail(email);
    if (!mounted) return;

    if (checkResult.exists) {
      setState(() {
        _isDuplicateEmail = true;
        _localError = checkResult.message ??
            'This email address is already registered. Please sign in instead.';
      });
      return;
    }

    final ok = await ref.read(authProvider.notifier).register(
          email: email,
          password: _passwordController.text,
          firstName: _firstNameController.text.trim(),
          lastName: _lastNameController.text.trim(),
          inviteCode: _inviteController.text.trim().toUpperCase(),
          phoneNumber: _phoneController.text.trim(),
        );

    if (!mounted) return;

    if (ok) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(
          builder: (_) => VerifyEmailScreen(email: email),
        ),
      );
    } else {
      final error = ref.read(authProvider).error ?? '';
      final isDuplicate = error.toLowerCase().contains('already registered') ||
          error.toLowerCase().contains('already exists');
      setState(() {
        _isDuplicateEmail = isDuplicate;
        _localError = error;
      });
    }
  }

  void _navigateToLogin() {
    final email = _emailController.text.trim();
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) => LoginScreen(
          initialEmail: email.isNotEmpty ? email : null,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authProvider);
    final displayedError = _localError ?? auth.error;

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: AppPanel(
                padding: const EdgeInsets.all(28),
                child: Form(
                  key: _formKey,
                  autovalidateMode: _autoValidateMode,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Create account',
                        style:
                            Theme.of(context).textTheme.headlineSmall?.copyWith(
                                  color: AppColors.navy,
                                  fontWeight: FontWeight.w800,
                                ),
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Enter your clinic invite code from your physiotherapist.',
                        style: TextStyle(color: AppColors.neutralMuted),
                      ),
                      const SizedBox(height: 24),

                      // First name
                      TextFormField(
                        controller: _firstNameController,
                        textInputAction: TextInputAction.next,
                        textCapitalization: TextCapitalization.words,
                        decoration: const InputDecoration(
                          labelText: 'First name',
                          hintText: 'e.g. Jane',
                        ),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) {
                            return 'First name is required';
                          }
                          if (value.trim().length < 2) {
                            return 'Must be at least 2 characters';
                          }
                          if (value.trim().length > 50) {
                            return 'Must be under 50 characters';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Last name
                      TextFormField(
                        controller: _lastNameController,
                        textInputAction: TextInputAction.next,
                        textCapitalization: TextCapitalization.words,
                        decoration: const InputDecoration(
                          labelText: 'Last name',
                          hintText: 'e.g. Doe',
                        ),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) {
                            return 'Last name is required';
                          }
                          if (value.trim().length < 2) {
                            return 'Must be at least 2 characters';
                          }
                          if (value.trim().length > 50) {
                            return 'Must be under 50 characters';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Email
                      TextFormField(
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        textInputAction: TextInputAction.next,
                        autocorrect: false,
                        decoration: const InputDecoration(
                          labelText: 'Email',
                          hintText: 'name@example.com',
                        ),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) {
                            return 'Email address is required';
                          }
                          final trimmed = value.trim();
                          if (!_emailRegex.hasMatch(trimmed)) {
                            return 'Please enter a valid email address';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Phone (optional)
                      TextFormField(
                        controller: _phoneController,
                        keyboardType: TextInputType.phone,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                          labelText: 'Phone (optional)',
                          hintText: '082 123 4567 or +27 82 123 4567',
                        ),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) {
                            return null; // optional
                          }
                          final clean = value.replaceAll(RegExp(r'[\s\-\(\)]'), '');
                          if (!_saPhoneRegex.hasMatch(clean)) {
                            return 'Enter a valid SA mobile number (e.g. 082 123 4567)';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Clinic invite code
                      TextFormField(
                        controller: _inviteController,
                        textInputAction: TextInputAction.next,
                        textCapitalization: TextCapitalization.characters,
                        decoration: const InputDecoration(
                          labelText: 'Clinic invite code',
                          hintText: 'e.g. MW-7B9C32',
                        ),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) {
                            return 'Clinic invite code is required';
                          }
                          if (value.trim().length < 4) {
                            return 'Invite code is too short';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Password
                      TextFormField(
                        controller: _passwordController,
                        obscureText: !_passwordVisible,
                        textInputAction: TextInputAction.done,
                        onFieldSubmitted: (_) => _submit(),
                        decoration: InputDecoration(
                          labelText: 'Password',
                          helperText: 'Min 8 chars, uppercase, lowercase, number & symbol',
                          helperMaxLines: 2,
                          suffixIcon: IconButton(
                            onPressed: () => setState(
                                () => _passwordVisible = !_passwordVisible),
                            icon: Icon(
                              _passwordVisible
                                  ? Icons.visibility_off_outlined
                                  : Icons.visibility_outlined,
                              color: AppColors.sage,
                            ),
                          ),
                        ),
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Password is required';
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
                          return null;
                        },
                      ),

                      // Error Display Banner
                      if (displayedError != null && displayedError.isNotEmpty) ...[
                        const SizedBox(height: 16),
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: Colors.red.shade50,
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: Colors.red.shade200),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                displayedError,
                                style: TextStyle(
                                  color: Colors.red.shade800,
                                  fontSize: 13,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                              if (_isDuplicateEmail) ...[
                                const SizedBox(height: 8),
                                OutlinedButton.icon(
                                  onPressed: _navigateToLogin,
                                  icon: const Icon(Icons.login, size: 16),
                                  label: const Text('Sign in with this email'),
                                  style: OutlinedButton.styleFrom(
                                    foregroundColor: Colors.red.shade900,
                                    side: BorderSide(color: Colors.red.shade400),
                                    minimumSize: const Size(0, 36),
                                    padding: const EdgeInsets.symmetric(
                                        horizontal: 12),
                                  ),
                                ),
                              ],
                            ],
                          ),
                        ),
                      ],

                      const SizedBox(height: 24),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: auth.isLoading ? null : _submit,
                          child: Text(
                            auth.isLoading ? 'Creating account...' : 'Sign up',
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),
                      TextButton(
                        onPressed: _navigateToLogin,
                        child: const Text('Already have an account? Sign in'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

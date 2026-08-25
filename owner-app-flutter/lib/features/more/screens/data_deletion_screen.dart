import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/section_card.dart';
import '../../auth/providers/auth_provider.dart';

class DataDeletionScreen extends ConsumerStatefulWidget {
  const DataDeletionScreen({super.key});

  @override
  ConsumerState<DataDeletionScreen> createState() => _DataDeletionScreenState();
}

class _DataDeletionScreenState extends ConsumerState<DataDeletionScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _emailController;
  final _reasonController = TextEditingController();
  final _notesController = TextEditingController();

  String _scope = 'FullAccountAndData';
  bool _confirmed = false;
  bool _isLoading = false;
  String? _referenceId;
  String? _successMessage;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    final user = ref.read(authProvider).user;
    _emailController = TextEditingController(text: user?.email ?? '');
  }

  @override
  void dispose() {
    _emailController.dispose();
    _reasonController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _openWebPortal() async {
    final uri = Uri.parse('https://mytriplea.co.za/delete-data');
    try {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not open web deletion portal.')),
        );
      }
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (!_confirmed) {
      setState(() {
        _errorMessage = 'Please check the confirmation box to proceed.';
      });
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final res = await ref.read(authProvider.notifier).requestDataDeletion(
          email: _emailController.text.trim(),
          requestType: 'Owner - $_scope',
          reason: _reasonController.text.trim().isEmpty ? null : _reasonController.text.trim(),
          additionalNotes: _notesController.text.trim().isEmpty ? null : _notesController.text.trim(),
        );

    if (!mounted) return;

    setState(() {
      _isLoading = false;
      if (res != null && res['success'] == true) {
        _referenceId = (res['requestReference'] as String?) ??
            'DEL-${DateTime.now().millisecondsSinceEpoch.toRadixString(36).toUpperCase()}';
        _successMessage = (res['message'] as String?) ??
            'Your deletion request has been registered and is being processed in compliance with POPIA.';
      } else {
        _errorMessage = (res?['message'] as String?) ?? 'Failed to submit request. Please try again.';
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return PageWashBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Account & Data Deletion', style: TextStyle(color: AppColors.navy, fontWeight: FontWeight.bold)),
          backgroundColor: Colors.transparent,
          elevation: 0,
          leading: IconButton(
            icon: const Icon(Icons.arrow_back_ios_new_rounded, color: AppColors.navy, size: 20),
            onPressed: () => Navigator.of(context).pop(),
          ),
          actions: [
            IconButton(
              icon: const Icon(Icons.open_in_browser_rounded, color: AppColors.sage),
              tooltip: 'Open in browser',
              onPressed: _openWebPortal,
            ),
          ],
        ),
        body: _referenceId != null ? _buildSuccessView() : _buildFormView(),
      ),
    );
  }

  Widget _buildSuccessView() {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: const Color(0xFFECFDF5),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: const Color(0xFFA7F3D0)),
          ),
          child: Column(
            children: [
              const Icon(Icons.check_circle_outline_rounded, color: Color(0xFF059669), size: 56),
              const SizedBox(height: 16),
              const Text(
                'Deletion Request Received',
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18, color: Color(0xFF065F46)),
              ),
              const SizedBox(height: 8),
              Text(
                'Reference Ticket: $_referenceId',
                style: const TextStyle(
                  fontFamily: 'monospace',
                  fontWeight: FontWeight.bold,
                  fontSize: 15,
                  color: Color(0xFF047857),
                ),
              ),
              const SizedBox(height: 12),
              Text(
                _successMessage ?? '',
                textAlign: TextAlign.center,
                style: const TextStyle(color: Color(0xFF065F46), fontSize: 13, height: 1.45),
              ),
            ],
          ),
        ),
        const SizedBox(height: 24),
        const SectionCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Next Steps & POPIA Timeline',
                style: TextStyle(fontWeight: FontWeight.w700, fontSize: 14, color: AppColors.navy),
              ),
              SizedBox(height: 8),
              Text(
                '• A confirmation has been sent to your email address.\n'
                '• In compliance with POPIA Section 24, all personal profile data, tracking logs, and uploaded media will be permanently deleted within 30 days.\n'
                '• Formal clinical notes and veterinary prescriptions remain subject to statutory clinical record retention periods with your attending clinic.',
                style: TextStyle(color: Color(0xFF374151), fontSize: 13, height: 1.45),
              ),
            ],
          ),
        ),
        const SizedBox(height: 24),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Back to Settings'),
          ),
        ),
      ],
    );
  }

  Widget _buildFormView() {
    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 10, 20, 32),
      children: [
        const SectionCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'POPIA Section 24 & App Store Deletion',
                style: TextStyle(fontWeight: FontWeight.w700, fontSize: 14, color: AppColors.navy),
              ),
              SizedBox(height: 6),
              Text(
                'You may request deletion of your Triple A account and personal data. Submitting this request will schedule your account for permanent closure and remove personal activity tracking logs and videos.',
                style: TextStyle(color: Color(0xFF374151), fontSize: 13, height: 1.45),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        Form(
          key: _formKey,
          child: SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Submit Deletion Request',
                  style: TextStyle(fontWeight: FontWeight.w700, fontSize: 16, color: AppColors.navy),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(
                    labelText: 'Registered Email',
                    hintText: 'name@example.co.za',
                  ),
                  validator: (val) {
                    if (val == null || val.trim().isEmpty) return 'Email is required';
                    if (!val.contains('@')) return 'Enter a valid email';
                    return null;
                  },
                ),
                const SizedBox(height: 14),
                DropdownButtonFormField<String>(
                  initialValue: _scope,
                  isExpanded: true,
                  decoration: const InputDecoration(labelText: 'Deletion Scope'),
                  items: const [
                    DropdownMenuItem(
                      value: 'FullAccountAndData',
                      child: Text('Full Account & All Personal Data', overflow: TextOverflow.ellipsis),
                    ),
                    DropdownMenuItem(
                      value: 'TrackingAndMediaOnly',
                      child: Text('Tracking Logs & Media Only', overflow: TextOverflow.ellipsis),
                    ),
                  ],
                  onChanged: (val) {
                    if (val != null) setState(() => _scope = val);
                  },
                ),
                const SizedBox(height: 14),
                TextFormField(
                  controller: _reasonController,
                  decoration: const InputDecoration(
                    labelText: 'Reason for request (optional)',
                    hintText: 'e.g., Rehabilitation complete',
                  ),
                ),
                const SizedBox(height: 14),
                TextFormField(
                  controller: _notesController,
                  maxLines: 2,
                  decoration: const InputDecoration(
                    labelText: 'Additional notes (optional)',
                    hintText: 'Any specific instructions...',
                  ),
                ),
                const SizedBox(height: 16),
                InkWell(
                  onTap: () => setState(() => _confirmed = !_confirmed),
                  borderRadius: BorderRadius.circular(8),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 4),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        SizedBox(
                          width: 24,
                          height: 24,
                          child: Checkbox(
                            value: _confirmed,
                            activeColor: AppColors.sage,
                            materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                            onChanged: (val) => setState(() => _confirmed = val ?? false),
                          ),
                        ),
                        const SizedBox(width: 10),
                        const Expanded(
                          child: Text(
                            'I confirm that I wish to request deletion of this account and understand access will be permanently revoked.',
                            style: TextStyle(fontSize: 12, color: AppColors.navy, height: 1.35),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                if (_errorMessage != null) ...[
                  const SizedBox(height: 10),
                  Text(
                    _errorMessage!,
                    style: const TextStyle(color: AppColors.alertRed, fontSize: 13),
                  ),
                ],
                const SizedBox(height: 20),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : _submit,
                    child: Text(_isLoading ? 'Submitting Request...' : 'Submit Deletion Request'),
                  ),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        Center(
          child: TextButton.icon(
            onPressed: _openWebPortal,
            icon: const Icon(Icons.language_rounded, size: 16),
            label: const Text('View Web Deletion Portal (mytriplea.co.za/delete-data)'),
          ),
        ),
      ],
    );
  }
}

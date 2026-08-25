import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/section_card.dart';

class PrivacyPolicyScreen extends StatelessWidget {
  const PrivacyPolicyScreen({super.key});

  Future<void> _openWebPolicy(BuildContext context) async {
    final uri = Uri.parse('https://mytriplea.co.za/privacy');
    try {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    } catch (_) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not open browser.')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return PageWashBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Privacy Policy', style: TextStyle(color: AppColors.navy, fontWeight: FontWeight.bold)),
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
              onPressed: () => _openWebPolicy(context),
            ),
          ],
        ),
        body: ListView(
          padding: const EdgeInsets.fromLTRB(20, 10, 20, 32),
          children: [
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.sageMuted,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: AppColors.sageLight.withValues(alpha: 0.3)),
              ),
              child: Row(
                children: [
                  const Icon(Icons.verified_user_rounded, color: AppColors.sage, size: 28),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: const [
                        Text(
                          'POPIA Act No. 4 of 2013',
                          style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy, fontSize: 14),
                        ),
                        SizedBox(height: 2),
                        Text(
                          'Your data is protected under South African law and encrypted in transit.',
                          style: TextStyle(color: AppColors.neutralMuted, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
            _PolicySection(
              title: '1. Responsible Party',
              content:
                  'Triple A (Animal Activity Assistant) operates this platform. We process personal information solely for veterinary rehabilitation care, exercise tracking, and clinical communication.',
            ),
            _PolicySection(
              title: '2. Information We Collect',
              content:
                  '• Account profile: Name, email address, phone number, and clinic invite code.\n'
                  '• Pet records: Pet name, breed, age, weight, and surgical/clinical history.\n'
                  '• Rehabilitation tracking: Daily exercise logs, mobility scores, pain ratings, and check-in comments.\n'
                  '• Media: Exercise form check videos and photos uploaded for physiotherapist review.',
            ),
            _PolicySection(
              title: '3. How Your Data Is Protected',
              content:
                  '• 100% of data in transit is encrypted using Modern TLS (HTTPS).\n'
                  '• Passwords are protected using one-way adaptive cryptographic hashing.\n'
                  '• Strict role-based access controls ensure only you and your authorized clinic physiotherapy team have access to your pet\'s records.\n'
                  '• We never sell your personal information to third parties.',
            ),
            _PolicySection(
              title: '4. Clinical Records Retention',
              content:
                  'While user profile data and activity logs can be deleted upon request, formal clinical assessment notes (SOAP notes) authored by your veterinary physiotherapist must be preserved by the attending clinic for statutory record-keeping periods (typically up to 5 years) in compliance with South African Veterinary Council regulations.',
            ),
            _PolicySection(
              title: '5. Your Rights & Deletion',
              content:
                  'Under POPIA Section 24, you may request access, correction, or deletion of your personal data at any time through our Account & Data Deletion portal or by contacting privacy@mytriplea.co.za.',
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: () => _openWebPolicy(context),
                icon: const Icon(Icons.language_rounded, size: 18),
                label: const Text('Read Full Policy on Web (mytriplea.co.za)'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _PolicySection extends StatelessWidget {
  final String title;
  final String content;

  const _PolicySection({required this.title, required this.content});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: SectionCard(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14, color: AppColors.navy),
            ),
            const SizedBox(height: 6),
            Text(
              content,
              style: const TextStyle(color: Color(0xFF374151), fontSize: 13, height: 1.45),
            ),
          ],
        ),
      ),
    );
  }
}

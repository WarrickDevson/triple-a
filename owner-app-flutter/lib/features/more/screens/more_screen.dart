import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/section_card.dart';
import '../../ai/screens/ai_chat_screen.dart';
import '../../appointments/screens/appointments_screen.dart';
import '../../auth/providers/auth_provider.dart';
import '../../auth/screens/change_password_screen.dart';
import '../../auth/screens/login_screen.dart';
import '../../reminders/screens/reminders_screen.dart';
import '../../tracking/screens/tracking_screen.dart';
import '../../videos/screens/video_inbox_screen.dart';
import '../../videos/screens/video_upload_screen.dart';
import '../../pets/providers/pets_provider.dart';

class MoreScreen extends ConsumerWidget {
  const MoreScreen({super.key});

  Future<void> _openTracking(BuildContext context, WidgetRef ref) async {
    await ref.read(petsProvider.notifier).loadPets();
    if (!context.mounted) return;
    final pets = ref.read(petsProvider).pets;
    if (pets.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Add a pet first to start daily tracking.')),
      );
      return;
    }
    final pet = pets.first;
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => TrackingScreen(petId: pet.petId, petName: pet.petName),
      ),
    );
  }

  Future<void> _signOut(BuildContext context, WidgetRef ref) async {
    await ref.read(authProvider.notifier).logout();
    if (!context.mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authProvider.select((s) => s.user));

    return PageWashBackground(
      child: SafeArea(
        child: ListView(
          padding: const EdgeInsets.only(bottom: 24),
          children: [
            const ShellHeader(title: 'More', showLogo: false),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: SectionCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      user?.firstName != null ? '${user!.firstName} ${user.lastName}' : 'Owner',
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 16,
                        color: AppColors.navy,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      user?.email ?? '',
                      style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            _MoreTile(
              icon: Icons.event_outlined,
              title: 'Appointments',
              subtitle: 'View and request follow-up visits',
              onTap: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const AppointmentsScreen()),
              ),
            ),
            _MoreTile(
              icon: Icons.monitor_heart_outlined,
              title: 'Daily Tracking',
              subtitle: 'Log pain, energy, and mobility',
              onTap: () => _openTracking(context, ref),
            ),
            _MoreTile(
              icon: Icons.videocam_outlined,
              title: 'Upload Exercise Video',
              subtitle: 'Send form videos for physio review',
              onTap: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const VideoUploadScreen()),
              ),
            ),
            _MoreTile(
              icon: Icons.inbox_outlined,
              title: 'Video Feedback',
              subtitle: 'Read your physiotherapist reviews',
              onTap: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const VideoInboxScreen()),
              ),
            ),
            _MoreTile(
              icon: Icons.notifications_outlined,
              title: 'Reminders',
              subtitle: 'Appointments and exercises due',
              onTap: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const RemindersScreen()),
              ),
            ),
            _MoreTile(
              icon: Icons.chat_bubble_outline_rounded,
              title: 'Wellness Assistant',
              subtitle: 'Ask recovery questions anytime',
              onTap: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const AiChatScreen()),
              ),
            ),
            _MoreTile(
              icon: Icons.lock_outline,
              title: 'Change password',
              subtitle: 'Update your account password',
              onTap: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const ChangePasswordScreen()),
              ),
            ),
            const SizedBox(height: 8),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: OutlinedButton(
                onPressed: () => _signOut(context, ref),
                child: const Text('Sign Out'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _MoreTile extends StatelessWidget {
  const _MoreTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 0, 20, 10),
      child: SectionCard(
        onTap: onTap,
        child: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: AppColors.sageMuted,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: AppColors.sage),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: const TextStyle(
                      fontWeight: FontWeight.w700,
                      color: AppColors.navy,
                      fontSize: 15,
                    ),
                  ),
                  Text(
                    subtitle,
                    style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                  ),
                ],
              ),
            ),
            Icon(Icons.chevron_right_rounded, color: AppColors.navy.withValues(alpha: 0.35)),
          ],
        ),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/section_card.dart';
import '../../appointments/models/appointment.dart';
import '../../appointments/providers/appointments_provider.dart';
import '../../appointments/screens/appointments_screen.dart';
import '../../auth/providers/auth_provider.dart';
import '../../pets/models/pet.dart';
import '../../pets/providers/pets_provider.dart';
import '../../pets/screens/pet_detail_screen.dart';
import '../../reminders/providers/reminders_provider.dart';
import '../../reminders/screens/reminders_screen.dart';

class HomeScreen extends ConsumerStatefulWidget {
  const HomeScreen({super.key, this.embedded = false});

  final bool embedded;

  @override
  ConsumerState<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends ConsumerState<HomeScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(remindersProvider.notifier).loadReminders(force: true);
      ref.read(petsProvider.notifier).loadPets(force: true);
      ref.read(appointmentsProvider.notifier).loadAppointments(force: true);
    });
  }

  void _openPet(Pet pet) {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => PetDetailScreen(pet: pet)),
    );
  }

  @override
  Widget build(BuildContext context) {
    final user = ref.watch(authProvider.select((s) => s.user));
    final petsState = ref.watch(petsProvider);
    final appointmentsState = ref.watch(appointmentsProvider);
    final reminderCount = ref.watch(remindersProvider).reminders.length;

    final nextAppointment = _nextUpcoming(appointmentsState.appointments);
    final recentUpdate = _recentUpdate(petsState.pets);

    final content = RefreshIndicator(
      onRefresh: () async {
        await Future.wait([
          ref.read(petsProvider.notifier).loadPets(force: true),
          ref.read(appointmentsProvider.notifier).loadAppointments(force: true),
          ref.read(remindersProvider.notifier).loadReminders(force: true),
        ]);
      },
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 0, 20, 24),
        children: [
          Text(
            'Welcome back, ${user?.firstName ?? 'Owner'}!',
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  color: AppColors.navy,
                  fontWeight: FontWeight.w800,
                ),
          ),
          const SizedBox(height: 4),
          const BrandMotto(),
          const SizedBox(height: 20),
          if (petsState.isLoading && petsState.pets.isEmpty)
            const Center(child: Padding(
              padding: EdgeInsets.all(32),
              child: CircularProgressIndicator(),
            ))
          else if (petsState.pets.isEmpty)
            const AppEmptyState(
              icon: Icons.pets_rounded,
              title: 'No pets yet',
              message: 'Add a pet from the My Pets tab to get started.',
            )
          else ...[
            ...petsState.pets.map((pet) {
              final progress = placeholderWeeklyProgress(pet.petId);
              return Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: SectionCard(
                  onTap: () => _openPet(pet),
                  child: Row(
                    children: [
                      PetAvatar(name: pet.petName, species: pet.species),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              pet.petName,
                              style: const TextStyle(
                                fontWeight: FontWeight.w800,
                                color: AppColors.navy,
                                fontSize: 16,
                              ),
                            ),
                            const SizedBox(height: 2),
                            Text(
                              _petStatusMessage(progress),
                              style: const TextStyle(
                                color: AppColors.neutralMuted,
                                fontSize: 13,
                              ),
                            ),
                            const SizedBox(height: 8),
                            ClipRRect(
                              borderRadius: BorderRadius.circular(4),
                              child: LinearProgressIndicator(
                                value: progress / 100,
                                minHeight: 5,
                                backgroundColor: AppColors.neutralGrey,
                                color: AppColors.sage,
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(width: 12),
                      Column(
                        children: [
                          Text(
                            '${progress.round()}%',
                            style: const TextStyle(
                              fontWeight: FontWeight.w800,
                              color: AppColors.sage,
                              fontSize: 14,
                            ),
                          ),
                          const Text(
                            'This Week',
                            style: TextStyle(fontSize: 10, color: AppColors.neutralMuted),
                          ),
                        ],
                      ),
                      Icon(Icons.chevron_right_rounded, color: AppColors.navy.withValues(alpha: 0.3)),
                    ],
                  ),
                ),
              );
            }),
          ],
          if (nextAppointment != null) ...[
            const SizedBox(height: 8),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Upcoming Appointment',
                  style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy, fontSize: 15),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: nextAppointment.appointmentStatus == 'Requested'
                        ? const Color(0xFFFEF3C7)
                        : const Color(0xFFD1FAE5),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    nextAppointment.appointmentStatus == 'Requested'
                        ? '⏳ Pending Approval'
                        : '✅ Booked',
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
                      color: nextAppointment.appointmentStatus == 'Requested'
                          ? const Color(0xFF92400E)
                          : const Color(0xFF065F46),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            SectionCard(
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const AppointmentsScreen()),
                );
              },
              child: Row(
                children: [
                  Container(
                    width: 48,
                    height: 48,
                    decoration: BoxDecoration(
                      color: AppColors.sageMuted,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(Icons.event_outlined, color: AppColors.sage),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          formatAppointmentDate(nextAppointment.scheduledDateTime),
                          style: const TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy),
                        ),
                        Text(
                          formatAppointmentTime(nextAppointment.scheduledDateTime),
                          style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          '${nextAppointment.petName} · ${nextAppointment.clientNotes ?? 'Physiotherapy session'}',
                          style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ],
          if (recentUpdate != null) ...[
            const SizedBox(height: 16),
            const Text(
              'Recent Update',
              style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy, fontSize: 15),
            ),
            const SizedBox(height: 8),
            SectionCard(
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  CircleAvatar(
                    radius: 18,
                    backgroundColor: AppColors.sageMuted,
                    child: Text(
                      recentUpdate.initials,
                      style: const TextStyle(
                        color: AppColors.sage,
                        fontWeight: FontWeight.w700,
                        fontSize: 12,
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          recentUpdate.text,
                          style: const TextStyle(color: AppColors.neutralDark, height: 1.4),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          formatRelativeTime(recentUpdate.time),
                          style: const TextStyle(color: AppColors.neutralMuted, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ],
          if (reminderCount > 0) ...[
            const SizedBox(height: 16),
            GestureDetector(
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const RemindersScreen()),
                );
              },
              child: SectionCard(
                child: Row(
                  children: [
                    const Icon(Icons.notifications_outlined, color: AppColors.sage),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        '$reminderCount reminder${reminderCount == 1 ? '' : 's'} due soon',
                        style: const TextStyle(fontWeight: FontWeight.w600, color: AppColors.navy),
                      ),
                    ),
                    const Icon(Icons.chevron_right, color: AppColors.neutralMuted),
                  ],
                ),
              ),
            ),
          ],
        ],
      ),
    );

    final notificationButton = IconButton(
      onPressed: () {
        Navigator.of(context).push(
          MaterialPageRoute(builder: (_) => const RemindersScreen()),
        );
      },
      icon: Badge(
        isLabelVisible: reminderCount > 0,
        label: Text('$reminderCount'),
        child: const Icon(Icons.notifications_outlined, color: AppColors.navy),
      ),
    );

    if (widget.embedded) {
      return PageWashBackground(
        child: SafeArea(
          child: Column(
            children: [
              ShellHeader(
                title: 'Dashboard',
                showLogo: true,
                actions: [
                  notificationButton,
                ],
              ),
              Expanded(child: content),
            ],
          ),
        ),
      );
    }

    return AppPageScaffold(
      title: 'Dashboard',
      showBrand: true,
      actions: [
        notificationButton,
      ],
      body: content,
    );
  }

  Appointment? _nextUpcoming(List<Appointment> appointments) {
    final now = DateTime.now();
    final upcoming = appointments
        .where((a) =>
            a.scheduledDateTime.isAfter(now) &&
            a.appointmentStatus.toLowerCase() != 'cancelled')
        .toList()
      ..sort((a, b) => a.scheduledDateTime.compareTo(b.scheduledDateTime));
    return upcoming.isNotEmpty ? upcoming.first : null;
  }

  _RecentUpdate? _recentUpdate(List<Pet> pets) {
    for (final pet in pets) {
      for (final history in pet.medicalHistories) {
        if (history.clinicianNotes != null && history.clinicianNotes!.isNotEmpty) {
          return _RecentUpdate(
            initials: 'LK',
            text: '${pet.petName}: ${history.clinicianNotes}',
            time: DateTime.now().subtract(const Duration(hours: 2)),
          );
        }
      }
    }
    if (pets.isNotEmpty) {
      return _RecentUpdate(
        initials: 'AA',
        text: '${pets.first.petName} is making steady progress this week.',
        time: DateTime.now().subtract(const Duration(hours: 5)),
      );
    }
    return null;
  }

  String _petStatusMessage(double progress) {
    if (progress >= 80) return 'Great progress!';
    if (progress >= 60) return 'Keep up the good work!';
    return 'Every step counts.';
  }
}

class _RecentUpdate {
  const _RecentUpdate({
    required this.initials,
    required this.text,
    required this.time,
  });

  final String initials;
  final String text;
  final DateTime time;
}

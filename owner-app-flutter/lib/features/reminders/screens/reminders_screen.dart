import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../appointments/screens/appointments_screen.dart';
import '../../pets/models/pet.dart';
import '../../pets/providers/pets_provider.dart';
import '../../pets/screens/pets_list_screen.dart';
import '../models/reminder.dart';
import '../providers/reminders_provider.dart';

class RemindersScreen extends ConsumerStatefulWidget {
  const RemindersScreen({super.key});

  @override
  ConsumerState<RemindersScreen> createState() => _RemindersScreenState();
}

class _RemindersScreenState extends ConsumerState<RemindersScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(remindersProvider.notifier).loadReminders(force: true));
  }

  String _formatDueAt(DateTime? value) {
    if (value == null) return 'Due today';
    final local = value.toLocal();
    return '${local.year}-${local.month.toString().padLeft(2, '0')}-${local.day.toString().padLeft(2, '0')} '
        '${local.hour.toString().padLeft(2, '0')}:${local.minute.toString().padLeft(2, '0')}';
  }

  void _openReminder(Reminder reminder) {
    if (reminder.type == 'Appointment') {
      Navigator.of(context).push(
        MaterialPageRoute(builder: (_) => const AppointmentsScreen()),
      );
      return;
    }

    final pets = ref.read(petsProvider).pets;
    Pet? pet;
    for (final item in pets) {
      if (item.petId == reminder.petId) {
        pet = item;
        break;
      }
    }

    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const PetsListScreen()),
    );
    if (pet != null && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Open ${pet.petName}\'s exercise programme to complete today\'s session.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final remindersState = ref.watch(remindersProvider);

    return AppPageScaffold(
      title: 'Reminders',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
        children: [
          AppPanel(
            child: Text(
              'Stay on track with upcoming appointments and exercises due today.',
              style: TextStyle(
                color: AppColors.neutralDark.withValues(alpha: 0.75),
                height: 1.5,
              ),
            ),
          ),
          const SizedBox(height: 16),
          if (remindersState.isLoading)
            const AppPanel(child: Text('Loading reminders...'))
          else if (remindersState.error != null)
            AppPanel(child: Text(remindersState.error!))
          else if (remindersState.reminders.isEmpty)
            const AppEmptyState(
              icon: Icons.notifications_none_outlined,
              title: 'All caught up',
              message: 'No appointments or exercises are due right now.',
            )
          else
            ...remindersState.reminders.map(
              (reminder) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: AppPanel(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(
                            reminder.type == 'Appointment'
                                ? Icons.event_outlined
                                : Icons.fitness_center_outlined,
                            color: reminder.type == 'Appointment'
                                ? AppColors.primaryLight
                                : AppColors.accentAmber,
                          ),
                          const SizedBox(width: 10),
                          Expanded(
                            child: Text(
                              reminder.title,
                              style: const TextStyle(
                                color: AppColors.primaryDark,
                                fontWeight: FontWeight.w800,
                                fontSize: 16,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Text(reminder.body),
                      const SizedBox(height: 6),
                      Text(
                        '${reminder.petName} · ${_formatDueAt(reminder.dueAt)}',
                        style: TextStyle(
                          color: AppColors.neutralDark.withValues(alpha: 0.7),
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(height: 12),
                      Align(
                        alignment: Alignment.centerRight,
                        child: TextButton(
                          onPressed: () => _openReminder(reminder),
                          child: const Text('Open'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }
}

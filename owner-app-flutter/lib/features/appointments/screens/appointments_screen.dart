import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../pets/models/pet.dart';
import '../../pets/providers/pets_provider.dart';
import '../models/appointment.dart';
import '../providers/appointments_provider.dart';

class AppointmentsScreen extends ConsumerStatefulWidget {
  const AppointmentsScreen({super.key});

  @override
  ConsumerState<AppointmentsScreen> createState() => _AppointmentsScreenState();
}

class _AppointmentsScreenState extends ConsumerState<AppointmentsScreen> {
  Pet? _selectedPet;
  DateTime? _selectedDateTime;
  final _notesController = TextEditingController();
  bool _isSubmitting = false;

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(petsProvider.notifier).loadPets(force: true);
      ref.read(appointmentsProvider.notifier).loadAppointments(force: true);
    });
  }

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _pickDateTime() async {
    final now = DateTime.now();
    final date = await showDatePicker(
      context: context,
      initialDate: now.add(const Duration(days: 1)),
      firstDate: now,
      lastDate: now.add(const Duration(days: 365)),
    );
    if (date == null || !mounted) return;

    final time = await showTimePicker(
      context: context,
      initialTime: const TimeOfDay(hour: 10, minute: 0),
    );
    if (time == null) return;

    setState(() {
      _selectedDateTime = DateTime(date.year, date.month, date.day, time.hour, time.minute);
    });
  }

  Future<void> _requestAppointment() async {
    if (_selectedPet == null || _selectedDateTime == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Select a pet and appointment date/time.')),
      );
      return;
    }

    setState(() => _isSubmitting = true);
    final success = await ref.read(appointmentsProvider.notifier).requestAppointment(
          petId: _selectedPet!.petId,
          scheduledDateTime: _selectedDateTime!,
          clientNotes: _notesController.text.trim(),
        );
    if (!mounted) return;
    setState(() => _isSubmitting = false);

    if (success) {
      _notesController.clear();
      setState(() {
        _selectedPet = null;
        _selectedDateTime = null;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Appointment requested.')),
      );
    } else {
      final error = ref.read(appointmentsProvider).error;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error ?? 'Unable to request appointment.')),
      );
    }
  }

  Future<void> _cancelAppointment(Appointment appointment) async {
    final success =
        await ref.read(appointmentsProvider.notifier).cancelAppointment(appointment.appointmentId);
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(success ? 'Appointment cancelled.' : 'Unable to cancel appointment.'),
      ),
    );
  }

  String _formatDateTime(DateTime value) {
    final local = value.toLocal();
    final date =
        '${local.year}-${local.month.toString().padLeft(2, '0')}-${local.day.toString().padLeft(2, '0')}';
    final time =
        '${local.hour.toString().padLeft(2, '0')}:${local.minute.toString().padLeft(2, '0')}';
    return '$date at $time';
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final appointmentsState = ref.watch(appointmentsProvider);
    final upcoming = appointmentsState.appointments
        .where((a) => a.appointmentStatus == 'Scheduled')
        .toList();
    final recent = appointmentsState.appointments
        .where((a) => a.appointmentStatus != 'Scheduled')
        .toList();

    return AppPageScaffold(
      title: 'Appointments',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
        children: [
          AppPanel(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Request appointment',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        color: AppColors.primaryDark,
                        fontWeight: FontWeight.w800,
                      ),
                ),
                const SizedBox(height: 16),
                DropdownButtonFormField<Pet>(
                  initialValue: _selectedPet,
                  decoration: const InputDecoration(
                    labelText: 'Pet',
                    border: OutlineInputBorder(),
                  ),
                  items: petsState.pets
                      .map(
                        (pet) => DropdownMenuItem(
                          value: pet,
                          child: Text(pet.petName),
                        ),
                      )
                      .toList(),
                  onChanged: (pet) => setState(() => _selectedPet = pet),
                ),
                const SizedBox(height: 12),
                OutlinedButton.icon(
                  onPressed: _pickDateTime,
                  icon: const Icon(Icons.calendar_today_outlined),
                  label: Text(
                    _selectedDateTime == null
                        ? 'Select date & time'
                        : _formatDateTime(_selectedDateTime!),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _notesController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                    labelText: 'Notes for your physiotherapist',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 16),
                SizedBox(
                  width: double.infinity,
                  child: FilledButton(
                    onPressed: _isSubmitting ? null : _requestAppointment,
                    child: Text(_isSubmitting ? 'Requesting...' : 'Request Appointment'),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          Text(
            'Upcoming',
            style: Theme.of(context).textTheme.titleSmall?.copyWith(
                  color: AppColors.primaryLight,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 0.8,
                ),
          ),
          const SizedBox(height: 12),
          if (appointmentsState.isLoading)
            const AppPanel(child: Text('Loading appointments...'))
          else if (upcoming.isEmpty)
            const AppEmptyState(
              icon: Icons.event_busy_outlined,
              title: 'No upcoming appointments',
              message: 'Request a follow-up visit when you need one.',
            )
          else
            ...upcoming.map(
              (appointment) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: AppPanel(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        appointment.petName,
                        style: const TextStyle(
                          color: AppColors.primaryDark,
                          fontWeight: FontWeight.w800,
                          fontSize: 16,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        '${appointment.physioName} · ${_formatDateTime(appointment.scheduledDateTime)}',
                        style: TextStyle(
                          color: AppColors.neutralDark.withValues(alpha: 0.7),
                        ),
                      ),
                      if (appointment.clientNotes != null &&
                          appointment.clientNotes!.isNotEmpty) ...[
                        const SizedBox(height: 8),
                        Text(appointment.clientNotes!),
                      ],
                      const SizedBox(height: 12),
                      Align(
                        alignment: Alignment.centerRight,
                        child: TextButton(
                          onPressed: () => _cancelAppointment(appointment),
                          child: const Text('Cancel'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          if (recent.isNotEmpty) ...[
            const SizedBox(height: 8),
            Text(
              'Recent',
              style: Theme.of(context).textTheme.titleSmall?.copyWith(
                    color: AppColors.primaryLight,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.8,
                  ),
            ),
            const SizedBox(height: 12),
            ...recent.map(
              (appointment) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: AppPanel(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '${appointment.petName} · ${appointment.appointmentStatus}',
                        style: const TextStyle(
                          color: AppColors.primaryDark,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(_formatDateTime(appointment.scheduledDateTime)),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

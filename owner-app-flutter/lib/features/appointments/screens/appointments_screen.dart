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
  bool _showCalendarView = false;
  DateTime _selectedCalendarDay = DateTime.now();

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
        const SnackBar(content: Text('Appointment requested. Awaiting physiotherapist approval.')),
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

  Widget _buildStatusBadge(String status) {
    Color bg;
    Color fg;
    String label;
    IconData icon;

    switch (status) {
      case 'Requested':
        bg = const Color(0xFFFEF3C7);
        fg = const Color(0xFF92400E);
        label = '⏳ Pending Approval';
        icon = Icons.hourglass_top_rounded;
        break;
      case 'Scheduled':
        bg = const Color(0xFFD1FAE5);
        fg = const Color(0xFF065F46);
        label = '✅ Booked';
        icon = Icons.check_circle_outline;
        break;
      case 'Rejected':
        bg = const Color(0xFFFEE2E2);
        fg = const Color(0xFF991B1B);
        label = '❌ Declined';
        icon = Icons.cancel_outlined;
        break;
      case 'Completed':
        bg = const Color(0xFFDBEAFE);
        fg = const Color(0xFF1E40AF);
        label = '✔️ Completed';
        icon = Icons.task_alt;
        break;
      case 'Cancelled':
      default:
        bg = const Color(0xFFF3F4F6);
        fg = const Color(0xFF4B5563);
        label = '🚫 Cancelled';
        icon = Icons.block;
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 13, color: fg),
          const SizedBox(width: 4),
          Text(
            label,
            style: TextStyle(
              color: fg,
              fontWeight: FontWeight.w700,
              fontSize: 12,
            ),
          ),
        ],
      ),
    );
  }

  bool _isSameDay(DateTime a, DateTime b) {
    return a.year == b.year && a.month == b.month && a.day == b.day;
  }

  Widget _buildCalendarView(List<Appointment> allAppointments) {
    final now = DateTime.now();
    final firstDayOfMonth = DateTime(_selectedCalendarDay.year, _selectedCalendarDay.month, 1);
    final daysInMonth = DateUtils.getDaysInMonth(_selectedCalendarDay.year, _selectedCalendarDay.month);
    final startingWeekday = firstDayOfMonth.weekday; // 1 = Monday, 7 = Sunday

    final appointmentsOnSelectedDay = allAppointments.where((a) {
      return _isSameDay(a.scheduledDateTime.toLocal(), _selectedCalendarDay);
    }).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        AppPanel(
          child: Column(
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  IconButton(
                    icon: const Icon(Icons.chevron_left),
                    onPressed: () {
                      setState(() {
                        _selectedCalendarDay = DateTime(
                          _selectedCalendarDay.year,
                          _selectedCalendarDay.month - 1,
                          1,
                        );
                      });
                    },
                  ),
                  Text(
                    '${_monthName(_selectedCalendarDay.month)} ${_selectedCalendarDay.year}',
                    style: const TextStyle(
                      fontWeight: FontWeight.w800,
                      color: AppColors.primaryDark,
                      fontSize: 16,
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.chevron_right),
                    onPressed: () {
                      setState(() {
                        _selectedCalendarDay = DateTime(
                          _selectedCalendarDay.year,
                          _selectedCalendarDay.month + 1,
                          1,
                        );
                      });
                    },
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                children: const [
                  _WeekdayLabel('Mon'),
                  _WeekdayLabel('Tue'),
                  _WeekdayLabel('Wed'),
                  _WeekdayLabel('Thu'),
                  _WeekdayLabel('Fri'),
                  _WeekdayLabel('Sat'),
                  _WeekdayLabel('Sun'),
                ],
              ),
              const Divider(height: 16),
              GridView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: 42, // 6 rows of 7
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 7,
                  mainAxisExtent: 44,
                ),
                itemBuilder: (context, index) {
                  final dayOffset = index - (startingWeekday - 1);
                  if (dayOffset < 0 || dayOffset >= daysInMonth) {
                    return const SizedBox.shrink();
                  }

                  final dayDate = DateTime(
                    _selectedCalendarDay.year,
                    _selectedCalendarDay.month,
                    dayOffset + 1,
                  );

                  final isSelected = _isSameDay(dayDate, _selectedCalendarDay);
                  final isToday = _isSameDay(dayDate, now);

                  final dayAppointments = allAppointments.where((a) {
                    return _isSameDay(a.scheduledDateTime.toLocal(), dayDate);
                  }).toList();

                  final hasBooked = dayAppointments.any((a) => a.appointmentStatus == 'Scheduled');
                  final hasRequested = dayAppointments.any((a) => a.appointmentStatus == 'Requested');

                  return GestureDetector(
                    onTap: () {
                      setState(() => _selectedCalendarDay = dayDate);
                    },
                    child: Container(
                      margin: const EdgeInsets.all(2),
                      decoration: BoxDecoration(
                        color: isSelected
                            ? AppColors.primaryDark
                            : isToday
                                ? AppColors.sageMuted
                                : Colors.transparent,
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            '${dayDate.day}',
                            style: TextStyle(
                              color: isSelected
                                  ? Colors.white
                                  : isToday
                                      ? AppColors.primaryDark
                                      : AppColors.neutralDark,
                              fontWeight: isSelected || isToday ? FontWeight.w800 : FontWeight.w500,
                              fontSize: 13,
                            ),
                          ),
                          if (dayAppointments.isNotEmpty) ...[
                            const SizedBox(height: 2),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                if (hasBooked)
                                  Container(
                                    width: 5,
                                    height: 5,
                                    margin: const EdgeInsets.symmetric(horizontal: 1),
                                    decoration: const BoxDecoration(
                                      color: Colors.green,
                                      shape: BoxShape.circle,
                                    ),
                                  ),
                                if (hasRequested)
                                  Container(
                                    width: 5,
                                    height: 5,
                                    margin: const EdgeInsets.symmetric(horizontal: 1),
                                    decoration: const BoxDecoration(
                                      color: Colors.amber,
                                      shape: BoxShape.circle,
                                    ),
                                  ),
                              ],
                            ),
                          ],
                        ],
                      ),
                    ),
                  );
                },
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        Text(
          'Appointments for ${_selectedCalendarDay.day} ${_monthName(_selectedCalendarDay.month)}',
          style: Theme.of(context).textTheme.titleSmall?.copyWith(
                color: AppColors.primaryLight,
                fontWeight: FontWeight.w800,
              ),
        ),
        const SizedBox(height: 8),
        if (appointmentsOnSelectedDay.isEmpty)
          const AppPanel(
            child: Text(
              'No appointments on this date.',
              style: TextStyle(color: AppColors.neutralMuted),
            ),
          )
        else
          ...appointmentsOnSelectedDay.map((appointment) => _buildAppointmentCard(appointment)),
      ],
    );
  }

  String _monthName(int month) {
    const months = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    return months[month - 1];
  }

  Widget _buildAppointmentCard(Appointment appointment) {
    final canCancel = appointment.appointmentStatus == 'Requested' ||
        appointment.appointmentStatus == 'Scheduled';

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: AppPanel(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    appointment.petName,
                    style: const TextStyle(
                      color: AppColors.primaryDark,
                      fontWeight: FontWeight.w800,
                      fontSize: 16,
                    ),
                  ),
                ),
                _buildStatusBadge(appointment.appointmentStatus),
              ],
            ),
            const SizedBox(height: 6),
            Text(
              '${appointment.physioName} · ${_formatDateTime(appointment.scheduledDateTime)}',
              style: TextStyle(
                color: AppColors.neutralDark.withValues(alpha: 0.75),
                fontSize: 13,
              ),
            ),
            if (appointment.clientNotes != null && appointment.clientNotes!.isNotEmpty) ...[
              const SizedBox(height: 8),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: AppColors.neutralGrey.withValues(alpha: 0.3),
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Text(
                  'Your Notes: ${appointment.clientNotes}',
                  style: const TextStyle(fontSize: 12, color: AppColors.neutralDark),
                ),
              ),
            ],
            if (appointment.clinicianNotes != null && appointment.clinicianNotes!.isNotEmpty) ...[
              const SizedBox(height: 8),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: AppColors.sageMuted,
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Text(
                  'Physio Note: ${appointment.clinicianNotes}',
                  style: const TextStyle(fontSize: 12, color: AppColors.primaryDark, fontWeight: FontWeight.w600),
                ),
              ),
            ],
            if (canCancel) ...[
              const SizedBox(height: 10),
              Align(
                alignment: Alignment.centerRight,
                child: TextButton.icon(
                  onPressed: () => _cancelAppointment(appointment),
                  icon: const Icon(Icons.close_rounded, size: 16),
                  label: const Text('Cancel Request'),
                  style: TextButton.styleFrom(
                    foregroundColor: Colors.red.shade700,
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final appointmentsState = ref.watch(appointmentsProvider);

    final activeAppointments = appointmentsState.appointments
        .where((a) => a.appointmentStatus == 'Requested' || a.appointmentStatus == 'Scheduled')
        .toList();
    final pastAppointments = appointmentsState.appointments
        .where((a) => a.appointmentStatus != 'Requested' && a.appointmentStatus != 'Scheduled')
        .toList();

    return AppPageScaffold(
      title: 'Appointments',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
        children: [
          // View mode toggle
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Bookings & Calendar',
                style: TextStyle(
                  fontWeight: FontWeight.w800,
                  color: AppColors.navy,
                  fontSize: 18,
                ),
              ),
              SegmentedButton<bool>(
                segments: const [
                  ButtonSegment(
                    value: false,
                    icon: Icon(Icons.list_alt_rounded, size: 18),
                    label: Text('List'),
                  ),
                  ButtonSegment(
                    value: true,
                    icon: Icon(Icons.calendar_month_rounded, size: 18),
                    label: Text('Calendar'),
                  ),
                ],
                selected: {_showCalendarView},
                onSelectionChanged: (set) {
                  setState(() => _showCalendarView = set.first);
                },
                style: ButtonStyle(
                  visualDensity: VisualDensity.compact,
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),

          AppPanel(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Request New Appointment',
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
                    child: Text(_isSubmitting ? 'Requesting...' : 'Submit Appointment Request'),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          if (_showCalendarView)
            _buildCalendarView(appointmentsState.appointments)
          else ...[
            Text(
              'Upcoming & Active Requests',
              style: Theme.of(context).textTheme.titleSmall?.copyWith(
                    color: AppColors.primaryLight,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.8,
                  ),
            ),
            const SizedBox(height: 12),
            if (appointmentsState.isLoading)
              const AppPanel(child: Text('Loading appointments...'))
            else if (activeAppointments.isEmpty)
              const AppEmptyState(
                icon: Icons.event_busy_outlined,
                title: 'No active appointments',
                message: 'Request a visit when your pet needs a follow-up.',
              )
            else
              ...activeAppointments.map((appointment) => _buildAppointmentCard(appointment)),

            if (pastAppointments.isNotEmpty) ...[
              const SizedBox(height: 16),
              Text(
                'History & Past Requests',
                style: Theme.of(context).textTheme.titleSmall?.copyWith(
                      color: AppColors.primaryLight,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 0.8,
                    ),
              ),
              const SizedBox(height: 12),
              ...pastAppointments.map((appointment) => _buildAppointmentCard(appointment)),
            ],
          ],
        ],
      ),
    );
  }
}

class _WeekdayLabel extends StatelessWidget {
  const _WeekdayLabel(this.text);
  final String text;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 32,
      child: Text(
        text,
        textAlign: TextAlign.center,
        style: const TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.w700,
          color: AppColors.neutralMuted,
        ),
      ),
    );
  }
}

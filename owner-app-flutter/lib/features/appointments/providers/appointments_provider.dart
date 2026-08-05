import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/appointment.dart';

class AppointmentsState {
  const AppointmentsState({
    this.appointments = const [],
    this.isLoading = false,
    this.error,
  });

  final List<Appointment> appointments;
  final bool isLoading;
  final String? error;
}

class AppointmentsNotifier extends StateNotifier<AppointmentsState> {
  AppointmentsNotifier(this._dio) : super(const AppointmentsState());

  final Dio _dio;

  Future<void> loadAppointments({bool force = false}) async {
    if (state.appointments.isNotEmpty && !force) return;

    state = const AppointmentsState(isLoading: true);
    try {
      final response = await _dio.get<List<dynamic>>('/api/appointments');
      final appointments = response.data!
          .map((item) => Appointment.fromJson(item as Map<String, dynamic>))
          .toList();
      state = AppointmentsState(appointments: appointments);
    } on DioException {
      state = const AppointmentsState(error: 'Unable to load appointments.');
    }
  }

  Future<bool> requestAppointment({
    required int petId,
    required DateTime scheduledDateTime,
    String? clientNotes,
  }) async {
    state = AppointmentsState(appointments: state.appointments, isLoading: true);
    try {
      final y = scheduledDateTime.year.toString().padLeft(4, '0');
      final m = scheduledDateTime.month.toString().padLeft(2, '0');
      final d = scheduledDateTime.day.toString().padLeft(2, '0');
      final h = scheduledDateTime.hour.toString().padLeft(2, '0');
      final min = scheduledDateTime.minute.toString().padLeft(2, '0');
      final isoString = '$y-$m-$d' 'T' '$h:$min:00Z';

      final response = await _dio.post<Map<String, dynamic>>(
        '/api/appointments',
        data: {
          'petId': petId,
          'scheduledDateTime': isoString,
          if (clientNotes != null && clientNotes.isNotEmpty) 'clientNotes': clientNotes,
        },
      );
      final appointment = Appointment.fromJson(response.data!);
      state = AppointmentsState(
        appointments: [...state.appointments, appointment]
          ..sort((a, b) => a.scheduledDateTime.compareTo(b.scheduledDateTime)),
      );
      return true;
    } on DioException catch (e) {
      final message = e.response?.data is Map<String, dynamic>
          ? (e.response!.data as Map<String, dynamic>)['message'] as String?
          : null;
      state = AppointmentsState(
        appointments: state.appointments,
        error: message ?? 'Unable to request appointment.',
      );
      return false;
    }
  }

  Future<bool> cancelAppointment(int appointmentId) async {
    try {
      final response = await _dio.put<Map<String, dynamic>>(
        '/api/appointments/$appointmentId/status',
        data: {'status': 'Cancelled'},
      );
      final updated = Appointment.fromJson(response.data!);
      state = AppointmentsState(
        appointments: state.appointments
            .map((a) => a.appointmentId == appointmentId ? updated : a)
            .toList(),
      );
      return true;
    } on DioException {
      state = AppointmentsState(
        appointments: state.appointments,
        error: 'Unable to cancel appointment.',
      );
      return false;
    }
  }
}

final appointmentsProvider =
    StateNotifierProvider<AppointmentsNotifier, AppointmentsState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return AppointmentsNotifier(authNotifier.client);
});

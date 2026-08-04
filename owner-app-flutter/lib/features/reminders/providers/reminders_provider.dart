import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/reminder.dart';

class RemindersState {
  const RemindersState({
    this.reminders = const [],
    this.isLoading = false,
    this.error,
  });

  final List<Reminder> reminders;
  final bool isLoading;
  final String? error;
}

class RemindersNotifier extends StateNotifier<RemindersState> {
  RemindersNotifier(this._dio) : super(const RemindersState());

  final Dio _dio;

  Future<void> loadReminders({bool force = false}) async {
    if (state.reminders.isNotEmpty && !force) return;
    state = const RemindersState(isLoading: true);
    try {
      final response = await _dio.get<List<dynamic>>('/api/reminders');
      final reminders = response.data!
          .map((item) => Reminder.fromJson(item as Map<String, dynamic>))
          .toList();
      state = RemindersState(reminders: reminders);
    } on DioException {
      state = const RemindersState(error: 'Unable to load reminders.');
    }
  }
}

final remindersProvider = StateNotifierProvider<RemindersNotifier, RemindersState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return RemindersNotifier(authNotifier.client);
});

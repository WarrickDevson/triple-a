import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/message.dart';

class MessagesState {
  const MessagesState({
    this.messages = const [],
    this.isLoading = false,
    this.isSending = false,
    this.error,
  });

  final List<PetMessage> messages;
  final bool isLoading;
  final bool isSending;
  final String? error;
}

class MessagesNotifier extends StateNotifier<MessagesState> {
  MessagesNotifier(this._dio) : super(const MessagesState());

  final Dio _dio;
  int? _petId;

  Future<void> loadForPet(int petId, {bool force = false, bool silent = false}) async {
    if (_petId == petId && state.messages.isNotEmpty && !force) return;
    _petId = petId;
    if (!silent && (state.messages.isEmpty || force)) {
      state = MessagesState(messages: state.messages, isLoading: state.messages.isEmpty);
    }
    try {
      final response = await _dio.get<List<dynamic>>('/api/pets/$petId/messages');
      final messages = response.data!
          .map((item) => PetMessage.fromJson(item as Map<String, dynamic>))
          .toList();
      state = MessagesState(messages: messages);
    } on DioException {
      if (!silent) {
        state = MessagesState(messages: state.messages, error: 'Unable to load messages.');
      }
    }
  }

  Future<bool> sendMessage({
    required int petId,
    required String body,
    int? videoSubmissionId,
  }) async {
    state = MessagesState(messages: state.messages, isSending: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/pets/$petId/messages',
        data: {
          'body': body,
          if (videoSubmissionId != null) 'videoSubmissionId': videoSubmissionId,
        },
      );
      final message = PetMessage.fromJson(response.data!);
      state = MessagesState(messages: [...state.messages, message]);
      return true;
    } on DioException catch (e) {
      final message = e.response?.data is Map<String, dynamic>
          ? (e.response!.data as Map<String, dynamic>)['message'] as String?
          : null;
      state = MessagesState(
        messages: state.messages,
        error: message ?? 'Unable to send message.',
      );
      return false;
    }
  }
}

final messagesProvider = StateNotifierProvider<MessagesNotifier, MessagesState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return MessagesNotifier(authNotifier.client);
});

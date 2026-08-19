import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/chat_message.dart';

class AiChatState {
  const AiChatState({
    this.messages = const [],
    this.isSending = false,
    this.error,
  });

  final List<ChatMessage> messages;
  final bool isSending;
  final String? error;
}

class AiChatNotifier extends StateNotifier<AiChatState> {
  AiChatNotifier(this._dio) : super(const AiChatState(
    messages: [
      ChatMessage(
        text: 'Hi! I can answer rehabilitation questions using Triple A Veterinary Physiotherapy educational materials. How can I help?',
        isUser: false,
      ),
    ],
  ));

  final Dio _dio;

  Future<void> send(String text) async {
    final trimmed = text.trim();
    if (trimmed.isEmpty || state.isSending) return;

    state = AiChatState(
      messages: [...state.messages, ChatMessage(text: trimmed, isUser: true)],
      isSending: true,
    );

    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/ai/chat',
        data: {'message': trimmed},
      );
      final data = response.data!;
      final sources = (data['sources'] as List<dynamic>? ?? [])
          .map((item) => ChatSource.fromJson(item as Map<String, dynamic>))
          .toList();

      state = AiChatState(
        messages: [
          ...state.messages,
          ChatMessage(
            text: data['answer'] as String,
            isUser: false,
            sources: sources,
          ),
        ],
      );
    } on DioException {
      state = AiChatState(
        messages: state.messages,
        error: 'Unable to reach the assistant. Please try again.',
      );
    }
  }
}

final aiChatProvider = StateNotifierProvider<AiChatNotifier, AiChatState>((ref) {
  return AiChatNotifier(ref.read(authProvider.notifier).client);
});

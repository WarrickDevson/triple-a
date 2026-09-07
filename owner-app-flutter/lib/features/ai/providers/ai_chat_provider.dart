import 'dart:convert';
import 'dart:io';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/chat_message.dart';
import '../models/chat_session.dart';

class AiChatState {
  const AiChatState({
    this.sessions = const [],
    this.activeSessionId = '',
    this.isSending = false,
    this.error,
  });

  final List<ChatSession> sessions;
  final String activeSessionId;
  final bool isSending;
  final String? error;

  ChatSession? get currentSession {
    if (sessions.isEmpty) return null;
    return sessions.firstWhere(
      (s) => s.id == activeSessionId,
      orElse: () => sessions.first,
    );
  }

  List<ChatMessage> get messages => currentSession?.messages ?? [];

  AiChatState copyWith({
    List<ChatSession>? sessions,
    String? activeSessionId,
    bool? isSending,
    String? error,
  }) {
    return AiChatState(
      sessions: sessions ?? this.sessions,
      activeSessionId: activeSessionId ?? this.activeSessionId,
      isSending: isSending ?? this.isSending,
      error: error,
    );
  }
}

class AiChatNotifier extends StateNotifier<AiChatState> {
  AiChatNotifier(this._dio) : super(const AiChatState()) {
    _loadSessions();
  }

  final Dio _dio;
  static const _sessionsKey = 'ai_chat_sessions_v2';
  static const _activeSessionKey = 'ai_chat_active_session_id';

  static const _initialGreeting =
      'Hi! I can answer rehabilitation questions using Triple A Veterinary Physiotherapy educational materials. '
      'You can also attach photos or documents for review. How can I help?';

  Future<void> _loadSessions() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final raw = prefs.getString(_sessionsKey);
      List<ChatSession> loaded = [];

      if (raw != null && raw.isNotEmpty) {
        final list = jsonDecode(raw) as List<dynamic>;
        loaded = list.map((item) => ChatSession.fromJson(item as Map<String, dynamic>)).toList();
      }

      if (loaded.isEmpty) {
        final initialSession = _createNewSessionObject(title: 'New Consultation');
        loaded = [initialSession];
      }

      final savedActiveId = prefs.getString(_activeSessionKey);
      final activeId = (savedActiveId != null && loaded.any((s) => s.id == savedActiveId))
          ? savedActiveId
          : loaded.first.id;

      state = state.copyWith(sessions: loaded, activeSessionId: activeId);
    } catch (_) {
      final initialSession = _createNewSessionObject(title: 'New Consultation');
      state = state.copyWith(sessions: [initialSession], activeSessionId: initialSession.id);
    }
  }

  Future<void> _saveSessions() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final encoded = jsonEncode(state.sessions.map((s) => s.toJson()).toList());
      await prefs.setString(_sessionsKey, encoded);
      await prefs.setString(_activeSessionKey, state.activeSessionId);
    } catch (_) {}
  }

  ChatSession _createNewSessionObject({String? title}) {
    final now = DateTime.now();
    return ChatSession(
      id: 'session_${now.millisecondsSinceEpoch}',
      title: title ?? 'New Consultation',
      createdAt: now,
      updatedAt: now,
      messages: [
        const ChatMessage(text: _initialGreeting, isUser: false),
      ],
    );
  }

  Future<void> createSession({String? title}) async {
    final newSession = _createNewSessionObject(title: title);
    final updated = [newSession, ...state.sessions];
    state = state.copyWith(
      sessions: updated,
      activeSessionId: newSession.id,
      error: null,
    );
    await _saveSessions();
  }

  Future<void> switchSession(String sessionId) async {
    if (state.sessions.any((s) => s.id == sessionId)) {
      state = state.copyWith(activeSessionId: sessionId, error: null);
      await _saveSessions();
    }
  }

  Future<void> renameSession(String sessionId, String newTitle) async {
    final trimmed = newTitle.trim();
    if (trimmed.isEmpty) return;

    final updated = state.sessions.map((s) {
      if (s.id == sessionId) {
        return s.copyWith(title: trimmed, updatedAt: DateTime.now());
      }
      return s;
    }).toList();

    state = state.copyWith(sessions: updated);
    await _saveSessions();
  }

  Future<void> deleteSession(String sessionId) async {
    final remaining = state.sessions.where((s) => s.id != sessionId).toList();
    if (remaining.isEmpty) {
      final fresh = _createNewSessionObject(title: 'New Consultation');
      state = state.copyWith(sessions: [fresh], activeSessionId: fresh.id);
    } else {
      final nextActive = (state.activeSessionId == sessionId)
          ? remaining.first.id
          : state.activeSessionId;
      state = state.copyWith(sessions: remaining, activeSessionId: nextActive);
    }
    await _saveSessions();
  }

  String _generateSessionLabel(String text, String? attachmentName) {
    if (text.trim().isNotEmpty) {
      var clean = text.trim().replaceAll(RegExp(r'[\r\n]+'), ' ').replaceAll(RegExp(r'[*#_`]+'), '').trim();
      if (clean.length > 34) {
        final spaceIdx = clean.indexOf(' ', 28);
        if (spaceIdx != -1 && spaceIdx <= 38) {
          clean = clean.substring(0, spaceIdx);
        } else {
          clean = clean.substring(0, 32);
        }
        return '$clean...';
      }
      return clean.isNotEmpty ? clean : 'Consultation Session';
    }
    if (attachmentName != null && attachmentName.isNotEmpty) {
      return 'Photo: $attachmentName';
    }
    return 'Consultation Session';
  }

  Future<Map<String, String>?> uploadAttachment(String filePath, String fileName) async {
    try {
      final formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(filePath, filename: fileName),
      });
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/ai/attachments/upload',
        data: formData,
      );
      if (response.data != null) {
        return {
          'attachmentUrl': response.data!['attachmentUrl'] as String,
          'attachmentName': response.data!['attachmentName'] as String,
          'attachmentType': response.data!['attachmentType'] as String,
        };
      }
      return null;
    } catch (_) {
      try {
        final formData = FormData.fromMap({
          'file': await MultipartFile.fromFile(filePath, filename: fileName),
        });
        final response = await _dio.post<Map<String, dynamic>>(
          '/api/messages/attachments/upload',
          data: formData,
        );
        if (response.data != null) {
          return {
            'attachmentUrl': response.data!['attachmentUrl'] as String,
            'attachmentName': response.data!['attachmentName'] as String,
            'attachmentType': response.data!['attachmentType'] as String,
          };
        }
      } catch (_) {}
      return null;
    }
  }

  Future<void> send(
    String text, {
    bool includePetContext = false,
    int? petId,
    String? attachmentPath,
    String? attachmentUrl,
    String? attachmentName,
    String? attachmentType,
  }) async {
    final trimmed = text.trim();
    if ((trimmed.isEmpty && attachmentPath == null && attachmentUrl == null) || state.isSending) return;

    final session = state.currentSession;
    if (session == null) return;

    final displayText = trimmed.isNotEmpty
        ? trimmed
        : (attachmentName != null ? 'Sent attachment: $attachmentName' : 'Sent photo for analysis');

    final userMessage = ChatMessage(
      text: displayText,
      isUser: true,
      attachmentUrl: attachmentUrl,
      attachmentName: attachmentName,
      attachmentPath: attachmentPath,
      attachmentType: attachmentType,
    );

    // Auto-generate session title on the first user query if still using default
    String currentTitle = session.title;
    if (currentTitle == 'New Consultation' || currentTitle == 'General Recovery Consultation') {
      currentTitle = _generateSessionLabel(trimmed, attachmentName);
    }

    final updatedMessages = [...session.messages, userMessage];
    final updatedSession = session.copyWith(
      title: currentTitle,
      messages: updatedMessages,
      updatedAt: DateTime.now(),
    );

    final updatedSessions = state.sessions.map((s) => s.id == session.id ? updatedSession : s).toList();

    state = state.copyWith(
      sessions: updatedSessions,
      isSending: true,
      error: null,
    );
    await _saveSessions();

    try {
      String? base64Data;
      String? resolvedUrl = attachmentUrl;

      if (attachmentPath != null) {
        final file = File(attachmentPath);
        if (await file.exists()) {
          final bytes = await file.readAsBytes();
          base64Data = base64Encode(bytes);

          if (resolvedUrl == null && attachmentName != null) {
            final uploadRes = await uploadAttachment(attachmentPath, attachmentName);
            if (uploadRes != null) {
              resolvedUrl = uploadRes['attachmentUrl'];
            }
          }
        }
      }

      // Collect the most recent previous message turns from this session for conversational continuity
      final prevMessages = session.messages
          .where((m) => m.text.isNotEmpty && m.text != _initialGreeting)
          .toList();
      final recentMessages = prevMessages.length > 10
          ? prevMessages.sublist(prevMessages.length - 10)
          : prevMessages;
      final historyTurns = recentMessages
          .map((m) => {
                'role': m.isUser ? 'user' : 'model',
                'content': m.text,
              })
          .toList();

      final response = await _dio.post<Map<String, dynamic>>(
        '/api/ai/chat',
        data: {
          'message': trimmed.isNotEmpty ? trimmed : displayText,
          'includePetContext': includePetContext,
          'petId': ?petId,
          'attachmentUrl': ?resolvedUrl,
          'attachmentBase64': ?base64Data,
          'attachmentMimeType': ?attachmentType,
          'attachmentName': ?attachmentName,
          'history': historyTurns,
        },
      );

      final data = response.data!;
      final sources = (data['sources'] as List<dynamic>? ?? [])
          .map((item) => ChatSource.fromJson(item as Map<String, dynamic>))
          .toList();

      final aiMessage = ChatMessage(
        text: data['answer'] as String,
        isUser: false,
        sources: sources,
      );

      final postAiMessages = [...updatedSession.messages, aiMessage];
      final finalizedSession = updatedSession.copyWith(
        messages: postAiMessages,
        updatedAt: DateTime.now(),
      );

      final finalSessions = state.sessions.map((s) => s.id == session.id ? finalizedSession : s).toList();

      state = state.copyWith(
        sessions: finalSessions,
        isSending: false,
      );
      await _saveSessions();
    } on DioException {
      state = state.copyWith(
        isSending: false,
        error: 'Unable to reach the assistant. Please try again.',
      );
    } catch (_) {
      state = state.copyWith(
        isSending: false,
        error: 'An unexpected error occurred while sending your request.',
      );
    }
  }
}

final aiChatProvider = StateNotifierProvider<AiChatNotifier, AiChatState>((ref) {
  return AiChatNotifier(ref.read(authProvider.notifier).client);
});

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:signalr_netcore/signalr_client.dart';
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
  MessagesNotifier(this._dio, this._ref) : super(const MessagesState());

  final Dio _dio;
  final Ref _ref;
  int? _petId;
  HubConnection? _hubConnection;

  Future<void> initSignalR() async {
    if (_hubConnection != null && _hubConnection!.state == HubConnectionState.Connected) return;

    final token = _ref.read(authProvider.notifier).accessToken;
    if (token == null) return;

    final baseUrl = _dio.options.baseUrl.replaceAll(RegExp(r'/+$'), '');
    final hubUrl = '$baseUrl/hubs/chat';

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => token,
          ),
        )
        .withAutomaticReconnect()
        .build();

    _hubConnection!.on('ReceiveMessage', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        final message = PetMessage.fromJson(data);
        final exists = state.messages.any((m) => m.messageId == message.messageId);
        if (!exists) {
          state = MessagesState(messages: [...state.messages, message]);
        }
      }
    });

    _hubConnection!.on('MessageRead', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        final messageId = data['messageId'] as int;
        final readAtStr = data['readAt'] as String?;
        final readAt = readAtStr != null ? DateTime.parse(readAtStr) : DateTime.now();
        state = MessagesState(
          messages: state.messages.map((m) {
            return m.messageId == messageId ? PetMessage(
              messageId: m.messageId,
              messageThreadId: m.messageThreadId,
              senderUserId: m.senderUserId,
              senderName: m.senderName,
              body: m.body,
              videoSubmissionId: m.videoSubmissionId,
              attachmentUrl: m.attachmentUrl,
              attachmentName: m.attachmentName,
              attachmentType: m.attachmentType,
              readAt: readAt,
              createdDate: m.createdDate,
            ) : m;
          }).toList(),
        );
      }
    });

    try {
      await _hubConnection!.start();
      if (_petId != null) {
        await _hubConnection!.invoke('JoinPetThread', args: <Object>[_petId!]);
      }
    } catch (_) {
      // Gracefully handle connection error
    }
  }

  Future<void> loadForPet(int petId, {bool force = false, bool silent = false}) async {
    if (_petId == petId && state.messages.isNotEmpty && !force) return;
    
    if (_petId != null && _petId != petId && _hubConnection?.state == HubConnectionState.Connected) {
      await _hubConnection?.invoke('LeavePetThread', args: <Object>[_petId!]);
    }

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

    await initSignalR();
    if (_hubConnection?.state == HubConnectionState.Connected) {
      await _hubConnection?.invoke('JoinPetThread', args: <Object>[petId]);
    }
  }

  Future<Map<String, String>?> uploadAttachment(String filePath, String fileName) async {
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
      return null;
    } catch (_) {
      return null;
    }
  }

  Future<bool> sendMessage({
    required int petId,
    required String body,
    int? videoSubmissionId,
    String? attachmentUrl,
    String? attachmentName,
    String? attachmentType,
  }) async {
    state = MessagesState(messages: state.messages, isSending: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/pets/$petId/messages',
        data: {
          'body': body,
          if (videoSubmissionId != null) 'videoSubmissionId': videoSubmissionId,
          if (attachmentUrl != null) 'attachmentUrl': attachmentUrl,
          if (attachmentName != null) 'attachmentName': attachmentName,
          if (attachmentType != null) 'attachmentType': attachmentType,
        },
      );
      final message = PetMessage.fromJson(response.data!);
      final exists = state.messages.any((m) => m.messageId == message.messageId);
      if (!exists) {
        state = MessagesState(messages: [...state.messages, message]);
      } else {
        state = MessagesState(messages: state.messages);
      }
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

  @override
  void dispose() {
    _hubConnection?.stop();
    super.dispose();
  }
}

final messagesProvider = StateNotifierProvider<MessagesNotifier, MessagesState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return MessagesNotifier(authNotifier.client, ref);
});

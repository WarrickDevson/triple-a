import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../auth/providers/auth_provider.dart';
import '../../pets/models/pet.dart';
import '../models/message.dart';
import '../providers/messages_provider.dart';

class MessageThreadScreen extends ConsumerStatefulWidget {
  const MessageThreadScreen({
    super.key,
    required this.pet,
    this.initialMessage,
    this.initialVideoSubmissionId,
  });

  final Pet pet;
  final String? initialMessage;
  final int? initialVideoSubmissionId;

  @override
  ConsumerState<MessageThreadScreen> createState() => _MessageThreadScreenState();
}

class _MessageThreadScreenState extends ConsumerState<MessageThreadScreen> {
  final _controller = TextEditingController();
  final _scrollController = ScrollController();
  int? _videoSubmissionId;
  Timer? _pollTimer;

  @override
  void initState() {
    super.initState();
    _videoSubmissionId = widget.initialVideoSubmissionId;
    if (widget.initialMessage != null) {
      _controller.text = widget.initialMessage!;
    }
    Future.microtask(() async {
      await ref.read(messagesProvider.notifier).loadForPet(widget.pet.petId, force: true);
      _scrollToBottom();
    });
    _pollTimer = Timer.periodic(const Duration(seconds: 3), (_) {
      if (mounted) {
        ref.read(messagesProvider.notifier).loadForPet(widget.pet.petId, force: true, silent: true);
      }
    });
  }

  @override
  void dispose() {
    _pollTimer?.cancel();
    _controller.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!_scrollController.hasClients) return;
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
      );
    });
  }

  Future<void> _send() async {
    final text = _controller.text.trim();
    if (text.isEmpty) return;
    final success = await ref.read(messagesProvider.notifier).sendMessage(
          petId: widget.pet.petId,
          body: text,
          videoSubmissionId: _videoSubmissionId,
        );
    if (!mounted) return;
    if (success) {
      _controller.clear();
      setState(() => _videoSubmissionId = null);
      _scrollToBottom();
    } else {
      final error = ref.read(messagesProvider).error;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error ?? 'Unable to send message.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final userId = ref.watch(authProvider).user?.userId;
    final messagesState = ref.watch(messagesProvider);

    ref.listen<MessagesState>(messagesProvider, (previous, next) {
      if ((previous?.messages.length ?? 0) < next.messages.length) {
        _scrollToBottom();
      }
    });

    return AppPageScaffold(
      title: widget.pet.petName,
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 12, 20, 0),
            child: Row(
              children: [
                PetAvatar(name: widget.pet.petName, species: widget.pet.species, size: 40),
                const SizedBox(width: 10),
                const Expanded(
                  child: Text(
                    'Physiotherapist',
                    style: TextStyle(fontWeight: FontWeight.w600, color: AppColors.neutralMuted),
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: messagesState.isLoading
                ? const Center(child: CircularProgressIndicator())
                : messagesState.messages.isEmpty
                    ? const Padding(
                        padding: EdgeInsets.all(24),
                        child: AppEmptyState(
                          icon: Icons.forum_outlined,
                          title: 'Start the conversation',
                          message: 'Ask your physiotherapist about recovery progress.',
                        ),
                      )
                    : ListView.builder(
                        controller: _scrollController,
                        padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                        itemCount: messagesState.messages.length,
                        itemBuilder: (context, index) {
                          final message = messagesState.messages[index];
                          return _MessageBubble(
                            message: message,
                            isMine: message.senderUserId == userId,
                          );
                        },
                      ),
          ),
          if (_videoSubmissionId != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Align(
                alignment: Alignment.centerLeft,
                child: Chip(
                  label: Text('Video #$_videoSubmissionId attached'),
                  onDeleted: () => setState(() => _videoSubmissionId = null),
                ),
              ),
            ),
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
              child: Row(
                children: [
                  IconButton(
                    onPressed: () {},
                    icon: const Icon(Icons.attach_file, color: AppColors.neutralMuted),
                  ),
                  Expanded(
                    child: TextField(
                      controller: _controller,
                      decoration: InputDecoration(
                        hintText: 'Type a message...',
                        filled: true,
                        fillColor: AppColors.card,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(24),
                          borderSide: const BorderSide(color: AppColors.neutralGrey),
                        ),
                        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                      ),
                      textInputAction: TextInputAction.send,
                      onSubmitted: (_) => _send(),
                    ),
                  ),
                  const SizedBox(width: 8),
                  IconButton.filled(
                    style: IconButton.styleFrom(
                      backgroundColor: AppColors.sage,
                      foregroundColor: Colors.white,
                    ),
                    onPressed: messagesState.isSending ? null : _send,
                    icon: messagesState.isSending
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                          )
                        : const Icon(Icons.send_rounded),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _MessageBubble extends StatelessWidget {
  const _MessageBubble({
    required this.message,
    required this.isMine,
  });

  final PetMessage message;
  final bool isMine;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: isMine ? Alignment.centerRight : Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        constraints: const BoxConstraints(maxWidth: 300),
        decoration: BoxDecoration(
          color: isMine ? AppColors.sage : AppColors.neutralGrey,
          borderRadius: BorderRadius.only(
            topLeft: const Radius.circular(18),
            topRight: const Radius.circular(18),
            bottomLeft: Radius.circular(isMine ? 18 : 4),
            bottomRight: Radius.circular(isMine ? 4 : 18),
          ),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (!isMine)
              Text(
                message.senderName,
                style: const TextStyle(
                  color: AppColors.sage,
                  fontSize: 12,
                  fontWeight: FontWeight.w700,
                ),
              ),
            if (!isMine) const SizedBox(height: 4),
            Text(
              message.body,
              style: TextStyle(
                color: isMine ? Colors.white : AppColors.navy,
                height: 1.4,
              ),
            ),
            if (message.videoSubmissionId != null) ...[
              const SizedBox(height: 6),
              Text(
                'Video #${message.videoSubmissionId}',
                style: TextStyle(
                  color: isMine ? Colors.white70 : AppColors.neutralMuted,
                  fontSize: 12,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

import 'dart:async';
import 'package:dio/dio.dart';
import 'package:file_picker/file_picker.dart';
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
  String? _attachedFileUrl;
  String? _attachedFileName;
  String? _attachedFileType;

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
  }

  @override
  void dispose() {
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

  void _showAttachmentMenu() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 12),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                ListTile(
                  leading: const Icon(Icons.attach_file_rounded, color: AppColors.navy, size: 28),
                  title: const Text('Attach File / Image from Device', style: TextStyle(fontWeight: FontWeight.w600)),
                  subtitle: const Text('Photos, PDFs, Documents'),
                  onTap: () {
                    Navigator.pop(context);
                    _pickDeviceFile();
                  },
                ),
                const Divider(),
                ListTile(
                  leading: const Icon(Icons.video_collection_rounded, color: AppColors.sage, size: 28),
                  title: const Text('Attach In-App Video Submission', style: TextStyle(fontWeight: FontWeight.w600)),
                  subtitle: const Text('Select from pet exercise uploads'),
                  onTap: () {
                    Navigator.pop(context);
                    _showVideoAttachmentPicker();
                  },
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> _pickDeviceFile() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: ['pdf', 'png', 'jpg', 'jpeg', 'doc', 'docx', 'mp4', 'mov'],
      );
      if (result != null && result.files.single.path != null) {
        final path = result.files.single.path!;
        final name = result.files.single.name;

        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Uploading file from device...'), duration: Duration(seconds: 2)),
        );

        final uploadRes = await ref.read(messagesProvider.notifier).uploadAttachment(path, name);
        if (uploadRes != null) {
          setState(() {
            _attachedFileUrl = uploadRes['attachmentUrl'];
            _attachedFileName = uploadRes['attachmentName'];
            _attachedFileType = uploadRes['attachmentType'];
          });
        } else {
          if (!mounted) return;
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Failed to upload file.')),
          );
        }
      }
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Unable to pick file.')),
      );
    }
  }

  Future<void> _showVideoAttachmentPicker() async {
    final dio = ref.read(authProvider.notifier).client;
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        return FutureBuilder<Response<List<dynamic>>>(
          future: dio.get<List<dynamic>>('/api/pets/${widget.pet.petId}/videos'),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const SizedBox(
                height: 220,
                child: Center(child: CircularProgressIndicator()),
              );
            }
            if (snapshot.hasError || !snapshot.hasData || snapshot.data?.data == null) {
              return Container(
                padding: const EdgeInsets.all(24),
                height: 220,
                child: const Center(
                  child: Text('Unable to load video submissions.'),
                ),
              );
            }
            final rawList = snapshot.data!.data!;
            if (rawList.isEmpty) {
              return Container(
                padding: const EdgeInsets.all(24),
                height: 220,
                child: const Center(
                  child: Text(
                    'No video submissions available for this pet.',
                    style: TextStyle(color: AppColors.neutralMuted),
                  ),
                ),
              );
            }
            return Container(
              padding: const EdgeInsets.all(20),
              constraints: const BoxConstraints(maxHeight: 380),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Attach Video Submission',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: AppColors.navy,
                        ),
                      ),
                      IconButton(
                        icon: const Icon(Icons.close),
                        onPressed: () => Navigator.pop(context),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Expanded(
                    child: ListView.separated(
                      shrinkWrap: true,
                      itemCount: rawList.length,
                      separatorBuilder: (_, _) => const Divider(height: 1),
                      itemBuilder: (context, index) {
                        final item = rawList[index] as Map<String, dynamic>;
                        final id = item['videoSubmissionId'] as int;
                        final title = (item['exerciseTitle'] as String?) ?? 'Video Submission #$id';
                        final isSelected = _videoSubmissionId == id;
                        return ListTile(
                          contentPadding: const EdgeInsets.symmetric(horizontal: 4, vertical: 4),
                          leading: const Icon(Icons.play_circle_fill, color: AppColors.sage, size: 32),
                          title: Text(
                            title,
                            style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
                          ),
                          subtitle: Text('ID: #$id'),
                          trailing: isSelected
                              ? const Icon(Icons.check_circle, color: AppColors.sage)
                              : const Text(
                                  'Attach',
                                  style: TextStyle(
                                    color: AppColors.sage,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                          onTap: () {
                            setState(() => _videoSubmissionId = id);
                            Navigator.pop(context);
                          },
                        );
                      },
                    ),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  Future<void> _send() async {
    final text = _controller.text.trim();
    if (text.isEmpty && _videoSubmissionId == null && _attachedFileUrl == null) return;
    final defaultBody = text.isNotEmpty
        ? text
        : (_attachedFileName != null ? '[Attached File: $_attachedFileName]' : '[Video Submission Attached]');

    final success = await ref.read(messagesProvider.notifier).sendMessage(
          petId: widget.pet.petId,
          body: defaultBody,
          videoSubmissionId: _videoSubmissionId,
          attachmentUrl: _attachedFileUrl,
          attachmentName: _attachedFileName,
          attachmentType: _attachedFileType,
        );
    if (!mounted) return;
    if (success) {
      _controller.clear();
      setState(() {
        _videoSubmissionId = null;
        _attachedFileUrl = null;
        _attachedFileName = null;
        _attachedFileType = null;
      });
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
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 2),
              child: Align(
                alignment: Alignment.centerLeft,
                child: InputChip(
                  avatar: const Icon(Icons.videocam_rounded, size: 18, color: AppColors.sage),
                  label: Text(
                    'Attached Video #$_videoSubmissionId',
                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                  ),
                  onDeleted: () => setState(() => _videoSubmissionId = null),
                  deleteIconColor: AppColors.alertRed,
                  backgroundColor: AppColors.sage.withValues(alpha: 0.15),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                ),
              ),
            ),
          if (_attachedFileName != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 2),
              child: Align(
                alignment: Alignment.centerLeft,
                child: InputChip(
                  avatar: const Icon(Icons.insert_drive_file_rounded, size: 18, color: AppColors.navy),
                  label: Text(
                    'Attached File: $_attachedFileName',
                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                  ),
                  onDeleted: () => setState(() {
                    _attachedFileUrl = null;
                    _attachedFileName = null;
                    _attachedFileType = null;
                  }),
                  deleteIconColor: AppColors.alertRed,
                  backgroundColor: AppColors.navy.withValues(alpha: 0.15),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
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
                    onPressed: _showAttachmentMenu,
                    icon: Icon(
                      Icons.attach_file,
                      color: (_videoSubmissionId != null || _attachedFileUrl != null)
                          ? AppColors.sage
                          : AppColors.neutralMuted,
                    ),
                    tooltip: 'Attach file or video submission',
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

  bool _isImageAttachment(String? type, String? url) {
    if (type != null && type.startsWith('image/')) return true;
    if (url == null) return false;
    final lower = url.toLowerCase();
    return lower.endsWith('.png') ||
        lower.endsWith('.jpg') ||
        lower.endsWith('.jpeg') ||
        lower.endsWith('.webp') ||
        lower.endsWith('.gif');
  }

  String _resolveAttachmentUrl(String path) {
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    return 'http://localhost:5057${path.startsWith('/') ? path : '/$path'}';
  }

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
            if (message.videoSubmissionId != null) ...[
              Container(
                margin: const EdgeInsets.only(bottom: 6),
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                decoration: BoxDecoration(
                  color: isMine ? Colors.white.withValues(alpha: 0.2) : Colors.white,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(
                    color: isMine ? Colors.white30 : AppColors.neutralGrey,
                  ),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.play_circle_fill,
                      size: 18,
                      color: isMine ? Colors.white : AppColors.sage,
                    ),
                    const SizedBox(width: 6),
                    Text(
                      'Video #${message.videoSubmissionId}',
                      style: TextStyle(
                        color: isMine ? Colors.white : AppColors.navy,
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ],
            if (message.attachmentUrl != null) ...[
              Container(
                margin: const EdgeInsets.only(bottom: 6),
                decoration: BoxDecoration(
                  color: isMine ? Colors.white.withValues(alpha: 0.2) : Colors.white,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(
                    color: isMine ? Colors.white30 : AppColors.neutralGrey,
                  ),
                ),
                child: _isImageAttachment(message.attachmentType, message.attachmentUrl)
                    ? ClipRRect(
                        borderRadius: BorderRadius.circular(10),
                        child: Image.network(
                          _resolveAttachmentUrl(message.attachmentUrl!),
                          fit: BoxFit.cover,
                          height: 160,
                          width: double.infinity,
                          errorBuilder: (context, error, stackTrace) => Padding(
                            padding: const EdgeInsets.all(8.0),
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Icon(Icons.broken_image_rounded, size: 18, color: isMine ? Colors.white : AppColors.navy),
                                const SizedBox(width: 6),
                                Text(
                                  message.attachmentName ?? 'Attached Image',
                                  style: TextStyle(color: isMine ? Colors.white : AppColors.navy, fontSize: 12),
                                ),
                              ],
                            ),
                          ),
                        ),
                      )
                    : Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(
                              Icons.insert_drive_file_rounded,
                              size: 18,
                              color: isMine ? Colors.white : AppColors.navy,
                            ),
                            const SizedBox(width: 6),
                            Flexible(
                              child: Text(
                                message.attachmentName ?? 'Attached File',
                                style: TextStyle(
                                  color: isMine ? Colors.white : AppColors.navy,
                                  fontSize: 12,
                                  fontWeight: FontWeight.w600,
                                ),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                          ],
                        ),
                      ),
              ),
            ],
            Text(
              message.body,
              style: TextStyle(
                color: isMine ? Colors.white : AppColors.navy,
                height: 1.4,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

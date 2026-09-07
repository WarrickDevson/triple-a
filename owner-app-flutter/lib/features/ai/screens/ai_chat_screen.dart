import 'dart:io';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../pets/providers/pets_provider.dart';
import '../models/chat_message.dart';
import '../models/chat_session.dart';
import '../providers/ai_chat_provider.dart';
import '../providers/ai_preferences_provider.dart';
import '../widgets/formatted_markdown_text.dart';

class AiChatScreen extends ConsumerStatefulWidget {
  const AiChatScreen({super.key});

  @override
  ConsumerState<AiChatScreen> createState() => _AiChatScreenState();
}

class _AiChatScreenState extends ConsumerState<AiChatScreen> {
  final _controller = TextEditingController();
  final _scrollController = ScrollController();
  bool _hasShownDataNotice = false;

  String? _selectedAttachmentPath;
  String? _selectedAttachmentName;
  String? _selectedAttachmentType;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _checkAndPromptSharing();
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _checkAndPromptSharing({bool force = false}) async {
    if (!mounted) return;
    final sharePetData = ref.read(aiPreferencesProvider).sharePetData;
    if (!sharePetData && (force || !_hasShownDataNotice)) {
      _hasShownDataNotice = true;
      ScaffoldMessenger.of(context).clearSnackBars();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          backgroundColor: AppColors.navy,
          behavior: SnackBarBehavior.floating,
          duration: const Duration(seconds: 8),
          margin: const EdgeInsets.fromLTRB(16, 0, 16, 16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          content: const Row(
            children: [
              Icon(Icons.info_outline_rounded, color: AppColors.sageMuted, size: 22),
              SizedBox(width: 12),
              Expanded(
                child: Text(
                  'Pet details are not shared with the assistant. Enable pet data sharing to get personalized advice tailored to all your pets.',
                  style: TextStyle(color: Colors.white, fontSize: 13, height: 1.3),
                ),
              ),
            ],
          ),
          action: SnackBarAction(
            label: 'Allow',
            textColor: AppColors.sageMuted,
            onPressed: () {
              ref.read(aiPreferencesProvider.notifier).setSharePetData(true);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Pet profiles sharing enabled for all companions.'),
                  duration: Duration(seconds: 3),
                ),
              );
            },
          ),
        ),
      );
    }
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

  Future<void> _pickAttachment() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: ['jpg', 'jpeg', 'png', 'webp', 'pdf'],
      );
      if (result != null && result.files.single.path != null) {
        final origPath = result.files.single.path!;
        final name = result.files.single.name;
        final ext = name.split('.').last.toLowerCase();
        final mimeType = (ext == 'pdf')
            ? 'application/pdf'
            : (ext == 'png' ? 'image/png' : (ext == 'webp' ? 'image/webp' : 'image/jpeg'));

        String safePath = origPath;
        if (!kIsWeb) {
          final src = File(origPath);
          if (await src.exists()) {
            final tempDir = Directory.systemTemp;
            final dest = File('${tempDir.path}/triplea_ai_${DateTime.now().millisecondsSinceEpoch}_$name');
            await src.copy(dest.path);
            safePath = dest.path;
          }
        }

        setState(() {
          _selectedAttachmentPath = safePath;
          _selectedAttachmentName = name;
          _selectedAttachmentType = mimeType;
        });
      }
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Unable to pick attachment.')),
      );
    }
  }

  Future<void> _send() async {
    final text = _controller.text;
    final attachPath = _selectedAttachmentPath;
    final attachName = _selectedAttachmentName;
    final attachType = _selectedAttachmentType;

    if (text.trim().isEmpty && attachPath == null) return;

    _controller.clear();
    setState(() {
      _selectedAttachmentPath = null;
      _selectedAttachmentName = null;
      _selectedAttachmentType = null;
    });

    final prefs = ref.read(aiPreferencesProvider);

    if (!prefs.sharePetData && !_hasShownDataNotice) {
      _checkAndPromptSharing(force: true);
    }

    // Passing petId: null with includePetContext: true shares ALL owner's registered pets
    await ref.read(aiChatProvider.notifier).send(
          text,
          includePetContext: prefs.sharePetData,
          petId: null,
          attachmentPath: attachPath,
          attachmentName: attachName,
          attachmentType: attachType,
        );
    _scrollToBottom();
  }

  String _formatSessionDate(DateTime dt) {
    final now = DateTime.now();
    final isToday = dt.year == now.year && dt.month == now.month && dt.day == now.day;
    final hour = dt.hour.toString().padLeft(2, '0');
    final min = dt.minute.toString().padLeft(2, '0');
    if (isToday) {
      return 'Today $hour:$min';
    }
    return '${dt.day}/${dt.month}/${dt.year}';
  }

  Future<void> _showRenameSessionDialog(BuildContext context, ChatSession session) async {
    final renameController = TextEditingController(text: session.title);
    final formKey = GlobalKey<FormState>();

    final result = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Rename Session', style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
        content: Form(
          key: formKey,
          child: TextFormField(
            controller: renameController,
            autofocus: true,
            decoration: InputDecoration(
              hintText: 'e.g. Barnaby Knee Recovery',
              filled: true,
              fillColor: AppColors.surface,
              border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
            ),
            validator: (val) => (val == null || val.trim().isEmpty) ? 'Please enter a title' : null,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: AppColors.primaryDark),
            onPressed: () {
              if (formKey.currentState?.validate() == true) {
                Navigator.of(ctx).pop(renameController.text.trim());
              }
            },
            child: const Text('Save'),
          ),
        ],
      ),
    );

    if (result != null && result.isNotEmpty && mounted) {
      await ref.read(aiChatProvider.notifier).renameSession(session.id, result);
    }
  }

  Future<void> _confirmDeleteSession(BuildContext context, ChatSession session) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Delete Session', style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
        content: Text('Are you sure you want to delete "${session.title}"? This conversation history cannot be recovered.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: AppColors.alertRed),
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirmed == true && mounted) {
      await ref.read(aiChatProvider.notifier).deleteSession(session.id);
    }
  }

  void _showSessionsSheet(BuildContext context) {
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) {
        return Consumer(
          builder: (context, ref, _) {
            final chatState = ref.watch(aiChatProvider);
            final sessions = chatState.sessions;
            final activeId = chatState.activeSessionId;

            return Container(
              height: MediaQuery.of(context).size.height * 0.68,
              decoration: const BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
              ),
              child: Column(
                children: [
                  Container(
                    margin: const EdgeInsets.only(top: 10, bottom: 6),
                    width: 38,
                    height: 4,
                    decoration: BoxDecoration(
                      color: AppColors.neutralLight,
                      borderRadius: BorderRadius.circular(2),
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Row(
                          children: [
                            const Icon(Icons.forum_outlined, color: AppColors.navy, size: 22),
                            const SizedBox(width: 8),
                            const Text(
                              'Consultation Sessions',
                              style: TextStyle(
                                fontSize: 17,
                                fontWeight: FontWeight.bold,
                                color: AppColors.navy,
                              ),
                            ),
                            const SizedBox(width: 8),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
                              decoration: BoxDecoration(
                                color: AppColors.sageMuted,
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Text(
                                '${sessions.length}',
                                style: const TextStyle(
                                  fontSize: 11,
                                  fontWeight: FontWeight.w700,
                                  color: AppColors.sage,
                                ),
                              ),
                            ),
                          ],
                        ),
                        FilledButton.tonalIcon(
                          style: FilledButton.styleFrom(
                            backgroundColor: AppColors.sageMuted,
                            foregroundColor: AppColors.navy,
                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                          ),
                          onPressed: () async {
                            Navigator.of(ctx).pop();
                            await ref.read(aiChatProvider.notifier).createSession();
                            _scrollToBottom();
                          },
                          icon: const Icon(Icons.add, size: 16),
                          label: const Text('New', style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                        ),
                      ],
                    ),
                  ),
                  const Divider(height: 1, color: AppColors.panelBorder),
                  Expanded(
                    child: ListView.separated(
                      padding: const EdgeInsets.symmetric(vertical: 8),
                      itemCount: sessions.length,
                      separatorBuilder: (_, _) => const Divider(height: 1, indent: 64, color: AppColors.panelBorder),
                      itemBuilder: (context, index) {
                        final session = sessions[index];
                        final isActive = session.id == activeId;
                        final msgCount = session.messages.length;

                        return ListTile(
                          selected: isActive,
                          selectedTileColor: AppColors.sageMuted.withValues(alpha: 0.35),
                          contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
                          leading: CircleAvatar(
                            backgroundColor: isActive ? AppColors.sage : AppColors.surface,
                            foregroundColor: isActive ? Colors.white : AppColors.navy,
                            child: Icon(
                              isActive ? Icons.chat_bubble_rounded : Icons.chat_bubble_outline_rounded,
                              size: 18,
                            ),
                          ),
                          title: Text(
                            session.title,
                            style: TextStyle(
                              fontSize: 14,
                              fontWeight: isActive ? FontWeight.bold : FontWeight.w600,
                              color: isActive ? AppColors.navy : AppColors.neutralDark,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                          subtitle: Text(
                            '$msgCount message${msgCount == 1 ? '' : 's'} • ${_formatSessionDate(session.updatedAt)}',
                            style: const TextStyle(fontSize: 12, color: AppColors.neutralMuted),
                          ),
                          trailing: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              IconButton(
                                icon: const Icon(Icons.edit_outlined, size: 18, color: AppColors.neutralMuted),
                                tooltip: 'Rename session',
                                onPressed: () => _showRenameSessionDialog(context, session),
                              ),
                              if (sessions.length > 1)
                                IconButton(
                                  icon: const Icon(Icons.delete_outline_rounded, size: 18, color: AppColors.neutralMuted),
                                  tooltip: 'Delete session',
                                  onPressed: () => _confirmDeleteSession(context, session),
                                ),
                            ],
                          ),
                          onTap: () {
                            ref.read(aiChatProvider.notifier).switchSession(session.id);
                            Navigator.of(ctx).pop();
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

  void _showPrivacyInfoDialog() {
    showDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.shield_outlined, color: AppColors.sage, size: 24),
            SizedBox(width: 10),
            Text('Pet Data Sharing', style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
          ],
        ),
        content: const Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'When enabled:',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppColors.navy),
            ),
            SizedBox(height: 4),
            Text(
              '• The assistant securely reads all your registered pets\' profiles, medical diagnoses, surgical history, and active rehab programs to provide coordinated, personalized answers.',
              style: TextStyle(fontSize: 13, height: 1.35),
            ),
            SizedBox(height: 10),
            Text(
              'When disabled:',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppColors.navy),
            ),
            SizedBox(height: 4),
            Text(
              '• The assistant provides general veterinary physiotherapy education without accessing any pet records.',
              style: TextStyle(fontSize: 13, height: 1.35),
            ),
            SizedBox(height: 12),
            Text(
              'You can toggle this permission anytime here or in your Settings page.',
              style: TextStyle(fontSize: 12, color: AppColors.neutralMuted),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Close'),
          ),
        ],
      ),
    );
  }

  Widget _buildSessionBar(AiChatState chatState) {
    final current = chatState.currentSession;
    final title = current?.title ?? 'New Consultation';

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 8, 16, 4),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.panelBorder),
      ),
      child: Row(
        children: [
          const Icon(Icons.forum_outlined, size: 16, color: AppColors.sage),
          const SizedBox(width: 8),
          Expanded(
            child: InkWell(
              borderRadius: BorderRadius.circular(6),
              onTap: () => _showSessionsSheet(context),
              child: Row(
                children: [
                  Flexible(
                    child: Text(
                      title,
                      style: const TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                        color: AppColors.navy,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: 4),
                  const Icon(Icons.arrow_drop_down, size: 18, color: AppColors.neutralMuted),
                ],
              ),
            ),
          ),
          if (current != null)
            GestureDetector(
              onTap: () => _showRenameSessionDialog(context, current),
              child: const Padding(
                padding: EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                child: Icon(Icons.drive_file_rename_outline, size: 16, color: AppColors.neutralMuted),
              ),
            ),
          const SizedBox(width: 6),
          InkWell(
            borderRadius: BorderRadius.circular(8),
            onTap: () async {
              await ref.read(aiChatProvider.notifier).createSession();
              _scrollToBottom();
            },
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: AppColors.sageMuted,
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.add, size: 13, color: AppColors.navy),
                  SizedBox(width: 3),
                  Text(
                    'New',
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
                      color: AppColors.navy,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSharingBanner(AiPreferencesState aiPrefs, List<dynamic> pets) {
    final petCount = pets.length;
    String petSummaryTitle;
    String petSummarySubtitle;

    if (!aiPrefs.sharePetData) {
      petSummaryTitle = 'General advice mode';
      petSummarySubtitle = 'Pet details are kept private from AI';
    } else if (petCount == 0) {
      petSummaryTitle = 'Pet profile sharing enabled';
      petSummarySubtitle = 'No pets registered yet under this account';
    } else if (petCount == 1) {
      final pName = pets.first.petName;
      petSummaryTitle = 'Sharing $pName\'s details';
      petSummarySubtitle = 'AI tailors advice to medical & rehab plan';
    } else {
      final names = pets.map((p) => p.petName).join(', ');
      petSummaryTitle = 'Sharing all $petCount pets\' profiles';
      petSummarySubtitle = 'Tailored across: $names';
    }

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 4, 16, 6),
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
      decoration: BoxDecoration(
        color: aiPrefs.sharePetData
            ? AppColors.sageMuted.withValues(alpha: 0.5)
            : AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
          color: aiPrefs.sharePetData
              ? AppColors.sage.withValues(alpha: 0.5)
              : AppColors.panelBorder,
        ),
      ),
      child: Row(
        children: [
          Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              color: aiPrefs.sharePetData
                  ? AppColors.sage.withValues(alpha: 0.2)
                  : Colors.grey.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(
              aiPrefs.sharePetData ? Icons.pets : Icons.shield_outlined,
              size: 18,
              color: aiPrefs.sharePetData ? AppColors.sage : AppColors.neutralMuted,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Flexible(
                      child: Text(
                        petSummaryTitle,
                        style: TextStyle(
                          fontSize: 13,
                          fontWeight: FontWeight.w700,
                          color: aiPrefs.sharePetData
                              ? AppColors.navy
                              : AppColors.neutralDark,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    const SizedBox(width: 4),
                    GestureDetector(
                      onTap: _showPrivacyInfoDialog,
                      child: const Icon(
                        Icons.help_outline_rounded,
                        size: 15,
                        color: AppColors.neutralMuted,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 2),
                Text(
                  petSummarySubtitle,
                  style: const TextStyle(
                    fontSize: 11,
                    color: AppColors.neutralMuted,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
          Switch.adaptive(
            value: aiPrefs.sharePetData,
            activeTrackColor: AppColors.sage,
            onChanged: (val) {
              ref.read(aiPreferencesProvider.notifier).setSharePetData(val);
              if (!val) {
                _checkAndPromptSharing(force: true);
              }
            },
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final chatState = ref.watch(aiChatProvider);
    final aiPrefs = ref.watch(aiPreferencesProvider);
    final pets = ref.watch(petsProvider).pets;

    return AppPageScaffold(
      title: 'Wellness Assistant',
      actions: [
        IconButton(
          icon: Badge(
            isLabelVisible: chatState.sessions.length > 1,
            label: Text('${chatState.sessions.length}', style: const TextStyle(fontSize: 10)),
            backgroundColor: AppColors.sage,
            child: const Icon(Icons.forum_outlined, color: AppColors.navy),
          ),
          tooltip: 'All sessions',
          onPressed: () => _showSessionsSheet(context),
        ),
        IconButton(
          icon: const Icon(Icons.add_comment_outlined, color: AppColors.navy),
          tooltip: 'New consultation session',
          onPressed: () async {
            await ref.read(aiChatProvider.notifier).createSession();
            _scrollToBottom();
          },
        ),
      ],
      body: Column(
        children: [
          _buildSessionBar(chatState),
          _buildSharingBanner(aiPrefs, pets),
          Expanded(
            child: chatState.messages.isEmpty
                ? const Padding(
                    padding: EdgeInsets.all(24),
                    child: AppEmptyState(
                      icon: Icons.chat_bubble_outline_rounded,
                      title: 'Ask anything about recovery',
                      message:
                          'Get guidance on pain, exercises, and daily care. Always follow your physiotherapist\'s advice for medical decisions.',
                    ),
                  )
                : ListView.builder(
                    controller: _scrollController,
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
                    itemCount: chatState.messages.length,
                    itemBuilder: (context, index) {
                      final message = chatState.messages[index];
                      return _ChatBubble(message: message);
                    },
                  ),
          ),
          if (chatState.error != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Text(chatState.error!, style: const TextStyle(color: AppColors.alertRed)),
            ),
          if (_selectedAttachmentPath != null)
            Container(
              margin: const EdgeInsets.fromLTRB(16, 4, 16, 4),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: AppColors.panelBorder),
              ),
              child: Row(
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: _selectedAttachmentType?.startsWith('image/') == true &&
                            File(_selectedAttachmentPath!).existsSync()
                        ? Image.file(
                            File(_selectedAttachmentPath!),
                            width: 42,
                            height: 42,
                            fit: BoxFit.cover,
                          )
                        : Container(
                            width: 42,
                            height: 42,
                            color: AppColors.sageMuted,
                            child: const Icon(Icons.picture_as_pdf_rounded, color: AppColors.sage, size: 24),
                          ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          _selectedAttachmentName ?? 'Attachment',
                          style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: AppColors.navy),
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          _selectedAttachmentType?.startsWith('image/') == true ? 'Photo ready for review' : 'Document attached',
                          style: const TextStyle(fontSize: 11, color: AppColors.neutralMuted),
                        ),
                      ],
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close_rounded, size: 18, color: AppColors.neutralMuted),
                    onPressed: () {
                      setState(() {
                        _selectedAttachmentPath = null;
                        _selectedAttachmentName = null;
                        _selectedAttachmentType = null;
                      });
                    },
                  ),
                ],
              ),
            ),
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
              child: AppPanel(
                padding: const EdgeInsets.fromLTRB(4, 8, 8, 8),
                child: Row(
                  children: [
                    IconButton(
                      icon: const Icon(Icons.attach_file_rounded, color: AppColors.neutralMuted),
                      tooltip: 'Attach photo or document',
                      onPressed: chatState.isSending ? null : _pickAttachment,
                    ),
                    Expanded(
                      child: TextField(
                        controller: _controller,
                        decoration: InputDecoration(
                          hintText: _selectedAttachmentPath != null
                              ? 'Add question for attachment (optional)...'
                              : 'Ask about pain, exercises, recovery...',
                          border: InputBorder.none,
                          enabledBorder: InputBorder.none,
                          focusedBorder: InputBorder.none,
                          filled: false,
                          contentPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 10),
                        ),
                        textInputAction: TextInputAction.send,
                        onSubmitted: (_) => _send(),
                      ),
                    ),
                    const SizedBox(width: 4),
                    IconButton.filled(
                      style: IconButton.styleFrom(
                        backgroundColor: AppColors.primaryDark,
                        foregroundColor: Colors.white,
                      ),
                      onPressed: chatState.isSending ? null : _send,
                      icon: chatState.isSending
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : const Icon(Icons.send),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ChatBubble extends StatelessWidget {
  const _ChatBubble({required this.message});

  final ChatMessage message;

  @override
  Widget build(BuildContext context) {
    final isUser = message.isUser;
    final alignment = isUser ? Alignment.centerRight : Alignment.centerLeft;
    final color = isUser ? AppColors.primaryDark : Colors.white;
    final textColor = isUser ? Colors.white : AppColors.neutralDark;
    final hasAttachment = message.attachmentPath != null || message.attachmentUrl != null;
    final isImage = message.attachmentType?.startsWith('image/') == true ||
        (message.attachmentName != null &&
            (message.attachmentName!.endsWith('.jpg') ||
                message.attachmentName!.endsWith('.jpeg') ||
                message.attachmentName!.endsWith('.png') ||
                message.attachmentName!.endsWith('.webp')));

    return Align(
      alignment: alignment,
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width * 0.82),
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.only(
            topLeft: const Radius.circular(16),
            topRight: const Radius.circular(16),
            bottomLeft: Radius.circular(isUser ? 16 : 4),
            bottomRight: Radius.circular(isUser ? 4 : 16),
          ),
          border: isUser ? null : Border.all(color: AppColors.panelBorder),
          boxShadow: isUser
              ? null
              : const [
                  BoxShadow(
                    color: AppColors.panelShadow,
                    blurRadius: 12,
                    offset: Offset(0, 4),
                  ),
                ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (hasAttachment) ...[
              if (isImage)
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(10),
                    child: message.attachmentPath != null && File(message.attachmentPath!).existsSync()
                        ? Image.file(
                            File(message.attachmentPath!),
                            height: 160,
                            width: double.infinity,
                            fit: BoxFit.cover,
                          )
                        : (message.attachmentUrl != null
                            ? Image.network(
                                message.attachmentUrl!,
                                height: 160,
                                width: double.infinity,
                                fit: BoxFit.cover,
                                errorBuilder: (_, _, _) => Container(
                                  height: 100,
                                  color: Colors.grey.shade200,
                                  child: const Center(child: Icon(Icons.image_outlined)),
                                ),
                              )
                            : const SizedBox()),
                  ),
                )
              else
                Container(
                  margin: const EdgeInsets.only(bottom: 8),
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: isUser ? Colors.white.withValues(alpha: 0.15) : AppColors.surface,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Row(
                    children: [
                      Icon(Icons.picture_as_pdf_rounded, color: isUser ? Colors.white : AppColors.sage, size: 24),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          message.attachmentName ?? 'Document attachment',
                          style: TextStyle(
                            color: textColor,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                ),
            ],
            FormattedMarkdownText(
              text: message.text,
              style: TextStyle(color: textColor, fontSize: 13.5, height: 1.42),
              headingColor: isUser ? Colors.white : AppColors.navy,
              boldColor: isUser ? Colors.white : AppColors.navy,
              bulletColor: isUser ? AppColors.sageMuted : AppColors.sage,
              codeBgColor: isUser ? Colors.white24 : AppColors.sageMuted.withValues(alpha: 0.3),
              dividerColor: isUser ? Colors.white24 : AppColors.panelBorder,
            ),
            if (!isUser && message.sources.isNotEmpty) ...[
              const SizedBox(height: 8),
              Text(
                'Sources',
                style: TextStyle(
                  color: textColor.withValues(alpha: 0.7),
                  fontSize: 11,
                  fontWeight: FontWeight.bold,
                ),
              ),
              for (final source in message.sources)
                Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Text(
                    '• ${source.title}',
                    style: TextStyle(color: textColor.withValues(alpha: 0.85), fontSize: 12),
                  ),
                ),
            ],
          ],
        ),
      ),
    );
  }
}



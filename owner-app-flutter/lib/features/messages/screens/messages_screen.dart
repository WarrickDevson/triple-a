import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/section_card.dart';
import '../../auth/providers/auth_provider.dart';
import '../../pets/models/pet.dart';
import '../../pets/providers/pets_provider.dart';
import '../models/message.dart';
import 'message_thread_screen.dart';

enum MessageFilter { all, unread, archived }

class MessagesScreen extends ConsumerStatefulWidget {
  const MessagesScreen({
    super.key,
    this.embedded = false,
    this.initialPetId,
    this.initialMessage,
    this.initialVideoSubmissionId,
    this.openThreadOnLoad = false,
  });

  final bool embedded;
  final int? initialPetId;
  final String? initialMessage;
  final int? initialVideoSubmissionId;
  final bool openThreadOnLoad;

  @override
  ConsumerState<MessagesScreen> createState() => _MessagesScreenState();
}

class _MessagesScreenState extends ConsumerState<MessagesScreen> {
  final _searchController = TextEditingController();
  MessageFilter _filter = MessageFilter.all;
  final Map<int, List<PetMessage>> _messagesByPet = {};
  bool _loadingThreads = false;

  @override
  void initState() {
    super.initState();
    Future.microtask(_loadThreads);
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadThreads({bool silent = false}) async {
    if (!silent) {
      setState(() => _loadingThreads = true);
      await ref.read(petsProvider.notifier).loadPets(force: true);
    } else {
      await ref.read(petsProvider.notifier).loadPets();
    }
    final pets = ref.read(petsProvider).pets;
    final dio = ref.read(authProvider.notifier).client;
    final userId = ref.read(authProvider).user?.userId;

    final map = <int, List<PetMessage>>{};
    for (final pet in pets) {
      try {
        final response = await dio.get<List<dynamic>>('/api/pets/${pet.petId}/messages');
        final messages = response.data!
            .map((item) => PetMessage.fromJson(item as Map<String, dynamic>))
            .toList();

        if (silent && _messagesByPet.containsKey(pet.petId)) {
          final oldMessages = _messagesByPet[pet.petId] ?? [];
          if (messages.length > oldMessages.length) {
            final newest = messages.last;
            if (userId != null && newest.senderUserId != userId) {
              _showNotificationSnackBar(pet, newest);
            }
          }
        }

        map[pet.petId] = messages;
      } catch (_) {
        map[pet.petId] = [];
      }
    }

    if (!mounted) return;
    setState(() {
      _messagesByPet
        ..clear()
        ..addAll(map);
      if (!silent) _loadingThreads = false;
    });

    if (!silent && widget.openThreadOnLoad && widget.initialPetId != null && pets.isNotEmpty) {
      Pet pet;
      try {
        pet = pets.firstWhere((p) => p.petId == widget.initialPetId);
      } catch (_) {
        pet = pets.first;
      }
      if (mounted) _openThread(pet);
    }
  }

  Future<void> _openThread(Pet pet) async {
    await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => MessageThreadScreen(
          pet: pet,
          initialMessage: widget.initialPetId == pet.petId ? widget.initialMessage : null,
          initialVideoSubmissionId:
              widget.initialPetId == pet.petId ? widget.initialVideoSubmissionId : null,
        ),
      ),
    );
    if (mounted) {
      _loadThreads(silent: true);
    }
  }

  void _showNotificationSnackBar(Pet pet, PetMessage message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).hideCurrentSnackBar();
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.chat_bubble_rounded, color: Colors.white, size: 20),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'New Message for ${pet.petName}',
                    style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white, fontSize: 13),
                  ),
                  Text(
                    message.body,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 12, color: Colors.white70),
                  ),
                ],
              ),
            ),
          ],
        ),
        backgroundColor: AppColors.navy,
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(12),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        duration: const Duration(seconds: 4),
        action: SnackBarAction(
          label: 'View',
          textColor: AppColors.sageLight,
          onPressed: () {
            _openThread(pet);
          },
        ),
      ),
    );
  }

  int _unreadCount(List<PetMessage> messages, int? userId) {
    if (userId == null) return 0;
    return messages.where((m) => m.readAt == null && m.senderUserId != userId).length;
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final userId = ref.watch(authProvider).user?.userId;
    final query = _searchController.text.trim().toLowerCase();

    final threads = petsState.pets.where((pet) {
      final messages = _messagesByPet[pet.petId] ?? [];
      final last = messages.isNotEmpty ? messages.last : null;
      final unread = _unreadCount(messages, userId);

      if (_filter == MessageFilter.unread && unread == 0) return false;
      if (_filter == MessageFilter.archived) return false;

      if (query.isEmpty) return true;
      if (pet.petName.toLowerCase().contains(query)) return true;
      if (last != null && last.body.toLowerCase().contains(query)) return true;
      return false;
    }).toList();

    final body = Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(20, 0, 20, 12),
          child: TextField(
            controller: _searchController,
            onChanged: (_) => setState(() {}),
            decoration: InputDecoration(
              hintText: 'Search messages...',
              prefixIcon: const Icon(Icons.search, color: AppColors.neutralMuted),
              filled: true,
              fillColor: AppColors.card,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide.none,
              ),
            ),
          ),
        ),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Row(
            children: MessageFilter.values.map((filter) {
              final selected = _filter == filter;
              return Padding(
                padding: const EdgeInsets.only(right: 8),
                child: FilterChip(
                  label: Text(_filterLabel(filter)),
                  selected: selected,
                  onSelected: (_) => setState(() => _filter = filter),
                  selectedColor: AppColors.sageMuted,
                  checkmarkColor: AppColors.sage,
                ),
              );
            }).toList(),
          ),
        ),
        const SizedBox(height: 8),
        Expanded(
          child: _loadingThreads
              ? const Center(child: CircularProgressIndicator())
              : petsState.pets.isEmpty
                  ? const Padding(
                      padding: EdgeInsets.all(24),
                      child: AppEmptyState(
                        icon: Icons.pets_rounded,
                        title: 'No pets yet',
                        message: 'Add a pet to message your physiotherapist.',
                      ),
                    )
                  : threads.isEmpty
                      ? const Padding(
                          padding: EdgeInsets.all(24),
                          child: AppEmptyState(
                            icon: Icons.forum_outlined,
                            title: 'No conversations',
                            message: 'Start a conversation with your physiotherapist.',
                          ),
                        )
                      : RefreshIndicator(
                          onRefresh: _loadThreads,
                          child: ListView.separated(
                            padding: const EdgeInsets.fromLTRB(20, 8, 20, 24),
                            itemCount: threads.length,
                            separatorBuilder: (_, __) => const SizedBox(height: 8),
                            itemBuilder: (context, index) {
                              final pet = threads[index];
                              final messages = _messagesByPet[pet.petId] ?? [];
                              final last = messages.isNotEmpty ? messages.last : null;
                              final unread = _unreadCount(messages, userId);

                              return SectionCard(
                                onTap: () => _openThread(pet),
                                child: Row(
                                  children: [
                                    PetAvatar(name: pet.petName, species: pet.species),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            pet.petName,
                                            style: const TextStyle(
                                              fontWeight: FontWeight.w700,
                                              color: AppColors.navy,
                                            ),
                                          ),
                                          const SizedBox(height: 2),
                                          Text(
                                            last?.body ?? 'No messages yet',
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                            style: TextStyle(
                                              color: unread > 0
                                                  ? AppColors.navy
                                                  : AppColors.neutralMuted,
                                              fontWeight:
                                                  unread > 0 ? FontWeight.w600 : FontWeight.w400,
                                              fontSize: 13,
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                    Column(
                                      crossAxisAlignment: CrossAxisAlignment.end,
                                      children: [
                                        if (last != null)
                                          Text(
                                            formatRelativeTime(last.createdDate),
                                            style: const TextStyle(
                                              fontSize: 11,
                                              color: AppColors.neutralMuted,
                                            ),
                                          ),
                                        if (unread > 0) ...[
                                          const SizedBox(height: 6),
                                          Container(
                                            width: 8,
                                            height: 8,
                                            decoration: const BoxDecoration(
                                              color: AppColors.sage,
                                              shape: BoxShape.circle,
                                            ),
                                          ),
                                        ],
                                      ],
                                    ),
                                  ],
                                ),
                              );
                            },
                          ),
                        ),
        ),
      ],
    );

    if (widget.embedded) {
      return PageWashBackground(
        child: SafeArea(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const ShellHeader(title: 'Messages', showLogo: false),
              Expanded(child: body),
            ],
          ),
        ),
      );
    }

    return AppPageScaffold(title: 'Messages', body: body);
  }

  String _filterLabel(MessageFilter filter) => switch (filter) {
        MessageFilter.all => 'All',
        MessageFilter.unread => 'Unread',
        MessageFilter.archived => 'Archived',
      };
}

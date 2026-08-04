import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../pets/models/pet.dart';
import '../../pets/providers/pets_provider.dart';
import '../../shell/main_shell.dart';
import '../models/video_submission.dart';
import '../providers/videos_provider.dart';

class VideoInboxScreen extends ConsumerStatefulWidget {
  const VideoInboxScreen({super.key});

  @override
  ConsumerState<VideoInboxScreen> createState() => _VideoInboxScreenState();
}

class _VideoInboxScreenState extends ConsumerState<VideoInboxScreen> {
  Pet? _selectedPet;

  @override
  void initState() {
    super.initState();
    Future.microtask(() async {
      await ref.read(petsProvider.notifier).loadPets(force: true);
      if (!mounted) return;
      final pets = ref.read(petsProvider).pets;
      if (pets.isNotEmpty) {
        setState(() => _selectedPet = pets.first);
        await ref.read(videosProvider.notifier).loadForPet(pets.first.petId);
      }
    });
  }

  Future<void> _selectPet(Pet? pet) async {
    if (pet == null) return;
    setState(() => _selectedPet = pet);
    await ref.read(videosProvider.notifier).loadForPet(pet.petId);
  }

  String _formatDate(DateTime value) {
    final local = value.toLocal();
    return '${local.year}-${local.month.toString().padLeft(2, '0')}-${local.day.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final videosState = ref.watch(videosProvider);

    return AppPageScaffold(
      title: 'Video Feedback',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
        children: [
          if (petsState.pets.isNotEmpty)
            AppPanel(
              child: DropdownButtonFormField<Pet>(
                initialValue: _selectedPet,
                decoration: const InputDecoration(
                  labelText: 'Pet',
                  border: OutlineInputBorder(),
                ),
                items: petsState.pets
                    .map(
                      (pet) => DropdownMenuItem(
                        value: pet,
                        child: Text(pet.petName),
                      ),
                    )
                    .toList(),
                onChanged: _selectPet,
              ),
            ),
          const SizedBox(height: 16),
          if (petsState.pets.isEmpty)
            const AppEmptyState(
              icon: Icons.pets_rounded,
              title: 'No pets yet',
              message: 'Add a pet to view video feedback.',
            )
          else if (videosState.isLoading)
            const AppPanel(child: Text('Loading submissions...'))
          else if (videosState.error != null)
            AppPanel(child: Text(videosState.error!))
          else if (videosState.submissions.isEmpty)
            const AppEmptyState(
              icon: Icons.inbox_outlined,
              title: 'No submissions yet',
              message: 'Upload an exercise video to receive physiotherapist feedback.',
            )
          else
            ...videosState.submissions.map(_buildSubmissionCard),
        ],
      ),
    );
  }

  Widget _buildSubmissionCard(VideoSubmission submission) {
    final statusColor = submission.isReviewed ? AppColors.successGreen : AppColors.accentAmber;
    final statusLabel = submission.isReviewed ? 'Reviewed' : submission.processingStatus;

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: AppPanel(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    submission.exerciseTitle,
                    style: const TextStyle(
                      color: AppColors.primaryDark,
                      fontWeight: FontWeight.w800,
                      fontSize: 16,
                    ),
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: statusColor.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    statusLabel,
                    style: TextStyle(
                      color: statusColor,
                      fontWeight: FontWeight.w700,
                      fontSize: 12,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 6),
            Text(
              '${submission.petName} · ${_formatDate(submission.createdDate)}',
              style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.7)),
            ),
            if (submission.isReviewed && submission.physioFeedbackNotes != null) ...[
              const SizedBox(height: 12),
              Text(
                'Physio feedback',
                style: TextStyle(
                  color: AppColors.primaryLight,
                  fontWeight: FontWeight.w700,
                  fontSize: 13,
                ),
              ),
              const SizedBox(height: 4),
              Text(submission.physioFeedbackNotes!),
            ] else if (!submission.isReviewed) ...[
              const SizedBox(height: 12),
              Text(
                'Your physiotherapist will review this video soon.',
                style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.7)),
              ),
            ],
            if (submission.isReviewed) ...[
              const SizedBox(height: 12),
              Align(
                alignment: Alignment.centerRight,
                child: TextButton(
                  onPressed: () => Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => MainShell(
                        initialTab: 2,
                        messagesPetId: submission.petId,
                        messagesInitialText:
                            'Hi, I have a question about my ${submission.exerciseTitle} video feedback.',
                        messagesVideoSubmissionId: submission.videoSubmissionId,
                      ),
                    ),
                  ),
                  child: const Text('Ask about this'),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../exercises/widgets/exercise_video_player.dart';
import '../../pets/providers/pets_provider.dart';
import '../../shell/main_shell.dart';
import '../models/video_submission.dart';
import '../providers/videos_provider.dart';
import 'video_upload_screen.dart';

class VideoInboxScreen extends ConsumerStatefulWidget {
  const VideoInboxScreen({super.key, this.initialPetId});

  final int? initialPetId;

  @override
  ConsumerState<VideoInboxScreen> createState() => _VideoInboxScreenState();
}

class _VideoInboxScreenState extends ConsumerState<VideoInboxScreen> {
  int? _activePetId;

  @override
  void initState() {
    super.initState();
    _activePetId = widget.initialPetId;

    Future.microtask(() async {
      await ref.read(petsProvider.notifier).loadPets();
      if (!mounted) return;
      final pets = ref.read(petsProvider).pets;
      if (pets.isNotEmpty) {
        final targetPetId = (_activePetId != null && pets.any((p) => p.petId == _activePetId))
            ? _activePetId!
            : pets.first.petId;
        setState(() {
          _activePetId = targetPetId;
        });
        ref.read(videosProvider.notifier).loadForPet(targetPetId);
      }
    });
  }

  void _selectPet(int petId) {
    if (_activePetId == petId) return;
    setState(() {
      _activePetId = petId;
    });
    ref.read(videosProvider.notifier).loadForPet(petId);
  }

  Future<void> _onRefresh() async {
    if (_activePetId != null) {
      await ref.read(videosProvider.notifier).loadForPet(_activePetId!);
    }
  }

  String _formatDate(DateTime value) {
    final local = value.toLocal();
    return '${local.year}-${local.month.toString().padLeft(2, '0')}-${local.day.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final videosState = ref.watch(videosProvider);
    final pets = petsState.pets;

    final effectiveActivePetId = (_activePetId != null && pets.any((p) => p.petId == _activePetId))
        ? _activePetId!
        : (pets.isNotEmpty ? pets.first.petId : null);

    final currentPetDisplayName = pets.isNotEmpty
        ? (pets.firstWhere((p) => p.petId == effectiveActivePetId, orElse: () => pets.first).petName)
        : 'Pet';

    return AppPageScaffold(
      title: 'Video Feedback',
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (pets.length > 1) ...[
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
              child: Text(
                'SELECT COMPANION',
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 0.8,
                  color: AppColors.neutralDark.withValues(alpha: 0.6),
                ),
              ),
            ),
            SizedBox(
              height: 42,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 20),
                itemCount: pets.length,
                separatorBuilder: (_, _) => const SizedBox(width: 8),
                itemBuilder: (context, index) {
                  final pet = pets[index];
                  final isSelected = pet.petId == effectiveActivePetId;
                  return Material(
                    color: Colors.transparent,
                    child: InkWell(
                      onTap: () => _selectPet(pet.petId),
                      borderRadius: BorderRadius.circular(20),
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                        decoration: BoxDecoration(
                          color: isSelected ? AppColors.primaryDark : Colors.white,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(
                            color: isSelected ? AppColors.primaryDark : AppColors.neutralGrey,
                            width: 1.5,
                          ),
                        ),
                        child: Text(
                          pet.petName,
                          style: TextStyle(
                            color: isSelected ? Colors.white : AppColors.neutralDark,
                            fontWeight: isSelected ? FontWeight.w800 : FontWeight.w600,
                            fontSize: 13,
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
            const SizedBox(height: 8),
          ] else if (pets.length == 1) ...[
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
              child: AppPanel(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                child: Row(
                  children: [
                    const Icon(Icons.pets, size: 20, color: AppColors.primaryLight),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        'Viewing submissions for ${pets.first.petName}',
                        style: const TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w700,
                          color: AppColors.primaryDark,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
          Expanded(
            child: RefreshIndicator(
              onRefresh: _onRefresh,
              child: ListView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.fromLTRB(20, 12, 20, 32),
                children: [
                  if (pets.isEmpty && !petsState.isLoading)
                    const AppEmptyState(
                      icon: Icons.pets_rounded,
                      title: 'No pets yet',
                      message: 'Add a pet companion to view video feedback.',
                    )
                  else if (videosState.isLoading)
                    const Padding(
                      padding: EdgeInsets.symmetric(vertical: 48),
                      child: Center(
                        child: CircularProgressIndicator(),
                      ),
                    )
                  else if (videosState.error != null)
                    AppPanel(
                      child: Column(
                        children: [
                          Text(
                            videosState.error!,
                            style: const TextStyle(color: AppColors.alertRed),
                          ),
                          const SizedBox(height: 12),
                          ElevatedButton(
                            onPressed: _onRefresh,
                            child: const Text('Retry'),
                          ),
                        ],
                      ),
                    )
                  else if (videosState.submissions.isEmpty)
                    AppEmptyState(
                      icon: Icons.video_library_outlined,
                      title: 'No video submissions yet',
                      message:
                          'Upload an exercise form video or general progress update for $currentPetDisplayName to receive physiotherapist review.',
                      action: ElevatedButton.icon(
                        onPressed: () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (_) => const VideoUploadScreen(),
                            ),
                          );
                        },
                        icon: const Icon(Icons.upload_file),
                        label: const Text('UPLOAD VIDEO'),
                      ),
                    )
                  else ...[
                    ...videosState.submissions.map(_buildSubmissionCard),
                    const SizedBox(height: 12),
                    Center(
                      child: OutlinedButton.icon(
                        onPressed: () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (_) => const VideoUploadScreen(),
                            ),
                          );
                        },
                        icon: const Icon(Icons.add_circle_outline, size: 18),
                        label: const Text('Upload Another Video'),
                        style: OutlinedButton.styleFrom(
                          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 10),
                          textStyle: const TextStyle(fontWeight: FontWeight.w700),
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  String _resolveVideoUrl(String path) {
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    final baseUrl = AppConfig.fromEnvironment().apiBaseUrl;
    return '$baseUrl${path.startsWith('/') ? path : '/$path'}';
  }

  void _watchVideo(VideoSubmission submission) {
    final rawUrl = submission.processedVideoStreamingUrl ?? submission.rawVideoStorageUrl;
    if (rawUrl.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Video is not available yet.')),
      );
      return;
    }

    final resolvedUrl = _resolveVideoUrl(rawUrl);

    showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return Dialog(
          backgroundColor: Colors.transparent,
          insetPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 24),
          child: Container(
            constraints: const BoxConstraints(maxWidth: 500),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 16, 8, 8),
                  child: Row(
                    children: [
                      Expanded(
                        child: Text(
                          submission.displayTitle,
                          style: const TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 16,
                            color: AppColors.primaryDark,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      IconButton(
                        icon: const Icon(Icons.close),
                        onPressed: () => Navigator.of(dialogContext).pop(),
                      ),
                    ],
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(12),
                    child: ExerciseVideoPlayer(videoUrl: resolvedUrl),
                  ),
                ),
                if (submission.notes != null && submission.notes!.trim().isNotEmpty) ...[
                  Padding(
                    padding: const EdgeInsets.all(16),
                    child: Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: AppColors.neutralGrey.withValues(alpha: 0.5),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'Owner notes',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.bold,
                              color: AppColors.neutralDark,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            submission.notes!,
                            style: const TextStyle(fontSize: 13),
                          ),
                        ],
                      ),
                    ),
                  ),
                ] else
                  const SizedBox(height: 16),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildSubmissionCard(VideoSubmission submission) {
    final statusColor = submission.isReviewed ? AppColors.successGreen : AppColors.accentAmber;
    final statusLabel = submission.isReviewed ? 'Reviewed' : submission.processingStatus;

    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: AppPanel(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    submission.displayTitle,
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
              '${submission.petName.isNotEmpty ? submission.petName : 'Patient'} · ${_formatDate(submission.createdDate)}',
              style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.7)),
            ),
            if (submission.notes != null && submission.notes!.trim().isNotEmpty) ...[
              const SizedBox(height: 10),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                decoration: BoxDecoration(
                  color: AppColors.neutralGrey.withValues(alpha: 0.4),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Icon(Icons.notes, size: 16, color: AppColors.primaryLight),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        submission.notes!,
                        style: const TextStyle(fontSize: 13, fontStyle: FontStyle.italic),
                      ),
                    ),
                  ],
                ),
              ),
            ],
            if (submission.isReviewed &&
                submission.physioFeedbackNotes != null &&
                submission.physioFeedbackNotes!.trim().isNotEmpty) ...[
              const SizedBox(height: 12),
              const Text(
                'Physio feedback',
                style: TextStyle(
                  color: AppColors.primaryLight,
                  fontWeight: FontWeight.w700,
                  fontSize: 13,
                ),
              ),
              const SizedBox(height: 4),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: AppColors.sage.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: AppColors.sage.withValues(alpha: 0.2)),
                ),
                child: Text(
                  submission.physioFeedbackNotes!,
                  style: const TextStyle(fontSize: 13, color: AppColors.neutralDark),
                ),
              ),
            ] else if (!submission.isReviewed) ...[
              const SizedBox(height: 12),
              Text(
                'Your physiotherapist will review this video soon.',
                style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.7)),
              ),
            ],
            const SizedBox(height: 12),
            const SizedBox(height: 12),
            Row(
              children: [
                OutlinedButton.icon(
                  onPressed: () => _watchVideo(submission),
                  icon: const Icon(Icons.play_circle_outline, size: 18),
                  label: const Text('WATCH VIDEO'),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                    textStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w700),
                  ),
                ),
                const Spacer(),
                IconButton(
                  icon: const Icon(Icons.edit_outlined, size: 20, color: AppColors.primaryLight),
                  tooltip: 'Edit Title & Notes',
                  onPressed: () => _showEditVideoDialog(submission),
                ),
                IconButton(
                  icon: const Icon(Icons.delete_outline_rounded, size: 20, color: AppColors.alertRed),
                  tooltip: 'Delete Video',
                  onPressed: () => _confirmDeleteVideo(submission),
                ),
                if (submission.isReviewed) ...[
                  const SizedBox(width: 4),
                  TextButton(
                    onPressed: () => Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => MainShell(
                          initialTab: 2,
                          messagesPetId: submission.petId,
                          messagesInitialText:
                              'Hi, I have a question about my ${submission.displayTitle} video feedback.',
                          messagesVideoSubmissionId: submission.videoSubmissionId,
                        ),
                      ),
                    ),
                    child: const Text('Ask about this'),
                  ),
                ],
              ],
            ),
          ],
        ),
      ),
    );
  }

  void _showEditVideoDialog(VideoSubmission submission) {
    final titleController = TextEditingController(text: submission.title ?? '');
    final notesController = TextEditingController(text: submission.notes ?? '');
    bool isSaving = false;

    showDialog<void>(
      context: context,
      builder: (dialogCtx) => StatefulBuilder(
        builder: (ctx, setModalState) => AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
          title: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: AppColors.sage.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.edit_outlined, color: AppColors.sage, size: 20),
              ),
              const SizedBox(width: 10),
              const Expanded(
                child: Text(
                  'Edit Video Details',
                  style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: AppColors.navy),
                ),
              ),
            ],
          ),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Video Title:',
                  style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: AppColors.navy),
                ),
                const SizedBox(height: 6),
                TextField(
                  controller: titleController,
                  decoration: InputDecoration(
                    hintText: 'e.g. Walking in the yard, Sit to stand...',
                    filled: true,
                    fillColor: Colors.grey.shade100,
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
                  ),
                ),
                const SizedBox(height: 14),
                const Text(
                  'Owner Notes for Physio:',
                  style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: AppColors.navy),
                ),
                const SizedBox(height: 6),
                TextField(
                  controller: notesController,
                  maxLines: 3,
                  decoration: InputDecoration(
                    hintText: 'Describe how the pet performed or any observations...',
                    filled: true,
                    fillColor: Colors.grey.shade100,
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
                  ),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: isSaving ? null : () => Navigator.pop(dialogCtx),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.sage,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: isSaving
                  ? null
                  : () async {
                      setModalState(() => isSaving = true);
                      final ok = await ref.read(videosProvider.notifier).updateVideo(
                            petId: submission.petId,
                            videoId: submission.videoSubmissionId,
                            title: titleController.text.trim(),
                            notes: notesController.text.trim(),
                          );
                      if (ok && mounted) {
                        if (dialogCtx.mounted) {
                          Navigator.pop(dialogCtx);
                        }
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Video details updated successfully!')),
                        );
                      } else if (mounted) {
                        setModalState(() => isSaving = false);
                      }
                    },
              child: isSaving
                  ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                  : const Text('Save'),
            ),
          ],
        ),
      ),
    );
  }

  void _confirmDeleteVideo(VideoSubmission submission) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Delete Video Submission?'),
        content: Text(
          'Are you sure you want to delete "${submission.displayTitle}"? This video and its review history will be removed.',
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Cancel')),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.alertRed, foregroundColor: Colors.white),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirm == true) {
      final ok = await ref.read(videosProvider.notifier).deleteVideo(
            petId: submission.petId,
            videoId: submission.videoSubmissionId,
          );
      if (ok && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Video submission deleted.')),
        );
      }
    }
  }
}

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../models/rehab_program.dart';
import '../providers/exercise_providers.dart';
import '../widgets/exercise_video_player.dart';

class ExerciseRoutineScreen extends ConsumerWidget {
  const ExerciseRoutineScreen({super.key, required this.program});

  final RehabProgram program;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(exerciseSessionProvider(program));
    final notifier = ref.read(exerciseSessionProvider(program).notifier);

    if (session == null) {
      return const Scaffold(
        body: PageWashBackground(
          child: Center(child: CircularProgressIndicator()),
        ),
      );
    }

    return AppPageScaffold(
      title: program.programTitle,
      actions: [
        TextButton(
          style: TextButton.styleFrom(foregroundColor: Colors.white),
          onPressed: notifier.resetRoutine,
          child: const Text('RESET'),
        ),
      ],
      body: _buildBody(context, session, notifier),
    );
  }

  Widget _buildBody(
    BuildContext context,
    ExerciseSessionState session,
    ExerciseSessionNotifier notifier,
  ) {
    switch (session.phase) {
      case ExerciseEnginePhase.overview:
        return _OverviewPane(session: session, onStart: notifier.startExercise);
      case ExerciseEnginePhase.stepActive:
        return _StepPane(session: session, onNext: notifier.nextStep);
      case ExerciseEnginePhase.exerciseComplete:
        return _OverviewPane(session: session, onStart: notifier.startExercise);
      case ExerciseEnginePhase.programComplete:
        return _CompletePane(session: session);
    }
  }
}

String _resolveMediaUrl(String url) {
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url;
  }
  final base = AppConfig.fromEnvironment().apiBaseUrl;
  if (url.startsWith('/')) {
    return '$base$url';
  }
  return '$base/$url';
}

void _showImageLightbox(BuildContext context, String imageUrl, String title) {
  showDialog(
    context: context,
    builder: (ctx) => Dialog(
      backgroundColor: Colors.black.withValues(alpha: 0.92),
      insetPadding: const EdgeInsets.all(12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(16),
        child: Stack(
          alignment: Alignment.center,
          children: [
            InteractiveViewer(
              panEnabled: true,
              minScale: 0.8,
              maxScale: 4.0,
              child: Image.network(
                imageUrl,
                fit: BoxFit.contain,
                loadingBuilder: (_, child, progress) {
                  if (progress == null) return child;
                  return const Center(child: CircularProgressIndicator(color: Colors.white));
                },
                errorBuilder: (_, _, _) => const Center(
                  child: Text('Unable to load image', style: TextStyle(color: Colors.white70)),
                ),
              ),
            ),
            Positioned(
              top: 12,
              right: 12,
              child: Material(
                color: Colors.black45,
                shape: const CircleBorder(),
                child: IconButton(
                  icon: const Icon(Icons.close, color: Colors.white),
                  onPressed: () => Navigator.of(ctx).pop(),
                ),
              ),
            ),
            Positioned(
              bottom: 12,
              left: 16,
              right: 16,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: Colors.black54,
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  title,
                  textAlign: TextAlign.center,
                  style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w500),
                ),
              ),
            ),
          ],
        ),
      ),
    ),
  );
}

class _OverviewPane extends StatelessWidget {
  const _OverviewPane({required this.session, required this.onStart});

  final ExerciseSessionState session;
  final VoidCallback onStart;

  @override
  Widget build(BuildContext context) {
    final exercise = session.currentExercise;
    final coverUrl = exercise.coverImageUrl ??
        (exercise.steps.isNotEmpty ? exercise.steps.first.imageUrl : null);

    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: ListView(
              children: [
                if (coverUrl != null) ...[
                  ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Stack(
                      children: [
                        AspectRatio(
                          aspectRatio: 16 / 9,
                          child: Image.network(
                            _resolveMediaUrl(coverUrl),
                            fit: BoxFit.cover,
                            errorBuilder: (_, _, _) => Container(
                              color: AppColors.primaryDark.withValues(alpha: 0.08),
                              child: const Icon(Icons.fitness_center, size: 48, color: AppColors.primaryDark),
                            ),
                          ),
                        ),
                        if (exercise.videoUrl != null)
                          Positioned(
                            top: 12,
                            right: 12,
                            child: Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(
                                color: Colors.black.withValues(alpha: 0.65),
                                borderRadius: BorderRadius.circular(20),
                              ),
                              child: const Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(Icons.videocam, color: Colors.white, size: 16),
                                  SizedBox(width: 4),
                                  Text(
                                    'Video Guide',
                                    style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.w600),
                                  ),
                                ],
                              ),
                            ),
                          ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                ],
                AppPanel(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        exercise.title,
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                              color: AppColors.primaryDark,
                              fontWeight: FontWeight.w800,
                            ),
                      ),
                      const SizedBox(height: 8),
                      if (exercise.shortDescription != null)
                        Text(
                          exercise.shortDescription!,
                          style: TextStyle(
                            color: AppColors.neutralDark.withValues(alpha: 0.75),
                            height: 1.45,
                          ),
                        ),
                      const SizedBox(height: 16),
                      Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: [
                          _Chip(label: '${exercise.repetitions} reps'),
                          _Chip(label: '${exercise.sets} sets'),
                          _Chip(
                            label: 'Set ${session.completedSets + 1} of ${exercise.sets}',
                            accent: true,
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                if (exercise.safetyNotes != null) ...[
                  const SizedBox(height: 12),
                  AppPanel(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            const Icon(Icons.warning_amber_rounded, color: Colors.amber, size: 20),
                            const SizedBox(width: 6),
                            Text(
                              'Safety & Precautions',
                              style: Theme.of(context).textTheme.titleSmall?.copyWith(
                                    fontWeight: FontWeight.w800,
                                    color: AppColors.primaryDark,
                                  ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Text(
                          exercise.safetyNotes!,
                          style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.8), height: 1.4),
                        ),
                      ],
                    ),
                  ),
                ],
                if (exercise.commonMistakes != null) ...[
                  const SizedBox(height: 12),
                  AppPanel(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            const Icon(Icons.info_outline, color: Colors.blueAccent, size: 20),
                            const SizedBox(width: 6),
                            Text(
                              'Common mistakes',
                              style: Theme.of(context).textTheme.titleSmall?.copyWith(
                                    fontWeight: FontWeight.w800,
                                    color: AppColors.primaryDark,
                                  ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Text(
                          exercise.commonMistakes!,
                          style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.8), height: 1.4),
                        ),
                      ],
                    ),
                  ),
                ],
              ],
            ),
          ),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: onStart,
              child: const Text('START EXERCISE'),
            ),
          ),
        ],
      ),
    );
  }
}

class _StepPane extends StatelessWidget {
  const _StepPane({required this.session, required this.onNext});

  final ExerciseSessionState session;
  final VoidCallback onNext;

  @override
  Widget build(BuildContext context) {
    final exercise = session.currentExercise;
    final step = session.currentStep;

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
      children: [
        AppPanel(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                exercise.title,
                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                      color: AppColors.primaryDark,
                      fontWeight: FontWeight.w800,
                    ),
              ),
              const SizedBox(height: 8),
              Text(
                'Step ${step.stepNumber} of ${exercise.steps.length} · Set ${session.completedSets + 1} of ${exercise.sets}',
                style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.65), fontWeight: FontWeight.w500),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        if (exercise.videoUrl != null) ...[
          AppPanel(
            padding: EdgeInsets.zero,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: ExerciseVideoPlayer(videoUrl: _resolveMediaUrl(exercise.videoUrl!)),
            ),
          ),
          const SizedBox(height: 16),
        ],
        if (step.imageUrl != null) ...[
          GestureDetector(
            onTap: () => _showImageLightbox(
              context,
              _resolveMediaUrl(step.imageUrl!),
              'Step ${step.stepNumber}: ${exercise.title}',
            ),
            child: AppPanel(
              padding: EdgeInsets.zero,
              child: Stack(
                alignment: Alignment.bottomRight,
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(12),
                    child: AspectRatio(
                      aspectRatio: 16 / 9,
                      child: Image.network(
                        _resolveMediaUrl(step.imageUrl!),
                        fit: BoxFit.cover,
                        errorBuilder: (_, _, _) => Container(
                          color: AppColors.primaryDark.withValues(alpha: 0.05),
                          child: const Center(child: Icon(Icons.broken_image, color: Colors.grey)),
                        ),
                      ),
                    ),
                  ),
                  Container(
                    margin: const EdgeInsets.all(8),
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.black.withValues(alpha: 0.7),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: const Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.zoom_in, color: Colors.white, size: 16),
                        SizedBox(width: 4),
                        Text(
                          'Pinch to Zoom',
                          style: TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w500),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
        ],
        AppPanel(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: AppColors.primaryDark.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Text(
                  'INSTRUCTION',
                  style: TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 0.8,
                    color: AppColors.primaryDark.withValues(alpha: 0.8),
                  ),
                ),
              ),
              const SizedBox(height: 10),
              Text(
                step.stepInstruction,
                style: Theme.of(context).textTheme.bodyLarge?.copyWith(height: 1.55, fontWeight: FontWeight.w500),
              ),
            ],
          ),
        ),
        const SizedBox(height: 24),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: onNext,
            child: Text(session.isLastStep ? 'MARK SET COMPLETE' : 'NEXT STEP'),
          ),
        ),
      ],
    );
  }
}

class _CompletePane extends StatelessWidget {
  const _CompletePane({required this.session});

  final ExerciseSessionState session;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
      child: Column(
        children: [
          Expanded(
            child: Center(
              child: AppPanel(
                padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 36),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.check_circle, color: AppColors.successGreen, size: 64),
                    const SizedBox(height: 16),
                    Text(
                      'Routine complete',
                      style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                            color: AppColors.primaryDark,
                            fontWeight: FontWeight.w800,
                          ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Great work. Today\'s exercise session has been logged.',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: AppColors.neutralDark.withValues(alpha: 0.7),
                        height: 1.45,
                      ),
                    ),
                    if (session.isLoading) ...[
                      const SizedBox(height: 24),
                      const CircularProgressIndicator(),
                    ],
                    if (session.error != null) ...[
                      const SizedBox(height: 16),
                      Text(session.error!, style: const TextStyle(color: AppColors.alertRed)),
                    ],
                  ],
                ),
              ),
            ),
          ),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: session.isLoading ? null : () => Navigator.of(context).pop(),
              child: const Text('BACK TO PROGRAM'),
            ),
          ),
        ],
      ),
    );
  }
}

class _Chip extends StatelessWidget {
  const _Chip({required this.label, this.accent = false});

  final String label;
  final bool accent;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: accent
            ? AppColors.accentAmber.withValues(alpha: 0.15)
            : AppColors.primaryDark.withValues(alpha: 0.07),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        label,
        style: TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.w700,
          color: accent ? AppColors.accentAmber : AppColors.primaryDark,
        ),
      ),
    );
  }
}

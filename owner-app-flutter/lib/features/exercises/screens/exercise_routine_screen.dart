import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
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

class _OverviewPane extends StatelessWidget {
  const _OverviewPane({required this.session, required this.onStart});

  final ExerciseSessionState session;
  final VoidCallback onStart;

  @override
  Widget build(BuildContext context) {
    final exercise = session.currentExercise;
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: ListView(
              children: [
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
                        Text(
                          'Safety',
                          style: Theme.of(context).textTheme.titleSmall?.copyWith(
                                fontWeight: FontWeight.w800,
                                color: AppColors.primaryDark,
                              ),
                        ),
                        const SizedBox(height: 6),
                        Text(exercise.safetyNotes!),
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
                        Text(
                          'Common mistakes',
                          style: Theme.of(context).textTheme.titleSmall?.copyWith(
                                fontWeight: FontWeight.w800,
                                color: AppColors.primaryDark,
                              ),
                        ),
                        const SizedBox(height: 6),
                        Text(exercise.commonMistakes!),
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
                style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.65)),
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
              child: ExerciseVideoPlayer(videoUrl: exercise.videoUrl!),
            ),
          ),
          const SizedBox(height: 16),
        ],
        AppPanel(
          child: Text(
            step.stepInstruction,
            style: Theme.of(context).textTheme.bodyLarge?.copyWith(height: 1.5),
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

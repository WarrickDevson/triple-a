import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../pets/models/pet.dart';
import '../models/rehab_program.dart';
import '../providers/exercise_providers.dart';
import 'exercise_routine_screen.dart';

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

class ExerciseProgramScreen extends ConsumerStatefulWidget {
  const ExerciseProgramScreen({super.key, required this.pet});

  final Pet pet;

  @override
  ConsumerState<ExerciseProgramScreen> createState() => _ExerciseProgramScreenState();
}

class _ExerciseProgramScreenState extends ConsumerState<ExerciseProgramScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref.read(rehabProgramsProvider(widget.pet.petId).notifier).loadPrograms(widget.pet.petId),
    );
  }

  void _showExerciseDetails(BuildContext context, RehabProgramExercise exercise) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => DraggableScrollableSheet(
        initialChildSize: 0.82,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        builder: (_, scrollController) => Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: Column(
            children: [
              Container(
                margin: const EdgeInsets.only(top: 10, bottom: 6),
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.grey.shade300,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Expanded(
                child: ListView(
                  controller: scrollController,
                  padding: const EdgeInsets.fromLTRB(20, 8, 20, 24),
                  children: [
                    Text(
                      exercise.title,
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            color: AppColors.primaryDark,
                            fontWeight: FontWeight.w800,
                          ),
                    ),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      runSpacing: 6,
                      children: [
                        _MetaChip(label: '${exercise.repetitions} reps'),
                        _MetaChip(label: '${exercise.sets} sets'),
                        _MetaChip(label: '${exercise.frequencyPerDay}x daily'),
                      ],
                    ),
                    if (exercise.coverImageUrl != null) ...[
                      const SizedBox(height: 16),
                      ClipRRect(
                        borderRadius: BorderRadius.circular(12),
                        child: AspectRatio(
                          aspectRatio: 16 / 9,
                          child: Image.network(
                            _resolveMediaUrl(exercise.coverImageUrl!),
                            fit: BoxFit.cover,
                            errorBuilder: (_, _, _) => const SizedBox.shrink(),
                          ),
                        ),
                      ),
                    ],
                    if (exercise.shortDescription != null) ...[
                      const SizedBox(height: 14),
                      Text(
                        exercise.shortDescription!,
                        style: TextStyle(
                          color: AppColors.neutralDark.withValues(alpha: 0.75),
                          height: 1.45,
                        ),
                      ),
                    ],
                    if (exercise.safetyNotes != null) ...[
                      const SizedBox(height: 14),
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.amber.shade50,
                          borderRadius: BorderRadius.circular(10),
                          border: Border.all(color: Colors.amber.shade200),
                        ),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Icon(Icons.warning_amber_rounded, color: Colors.amber, size: 20),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                exercise.safetyNotes!,
                                style: TextStyle(color: Colors.amber.shade900, fontSize: 13, height: 1.35),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                    if (exercise.steps.isNotEmpty) ...[
                      const SizedBox(height: 20),
                      Text(
                        'Step-by-Step Instructions',
                        style: Theme.of(context).textTheme.titleSmall?.copyWith(
                              fontWeight: FontWeight.w800,
                              color: AppColors.primaryDark,
                            ),
                      ),
                      const SizedBox(height: 10),
                      ...exercise.steps.map(
                        (st) => Container(
                          margin: const EdgeInsets.only(bottom: 12),
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: Colors.grey.shade50,
                            borderRadius: BorderRadius.circular(10),
                            border: Border.all(color: Colors.grey.shade200),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  CircleAvatar(
                                    radius: 12,
                                    backgroundColor: AppColors.primaryDark,
                                    child: Text(
                                      '${st.stepNumber}',
                                      style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  Text(
                                    'Step ${st.stepNumber}',
                                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 8),
                              Text(st.stepInstruction, style: const TextStyle(height: 1.4)),
                              if (st.imageUrl != null) ...[
                                const SizedBox(height: 8),
                                ClipRRect(
                                  borderRadius: BorderRadius.circular(8),
                                  child: Image.network(
                                    _resolveMediaUrl(st.imageUrl!),
                                    height: 130,
                                    width: double.infinity,
                                    fit: BoxFit.cover,
                                    errorBuilder: (_, _, _) => const SizedBox.shrink(),
                                  ),
                                ),
                              ],
                            ],
                          ),
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final programsState = ref.watch(rehabProgramsProvider(widget.pet.petId));
    final program = programsState.activeProgram;

    return AppPageScaffold(
      title: '${widget.pet.petName} Routine',
      body: _buildBody(programsState, program),
    );
  }

  Widget _buildBody(RehabProgramsState programsState, RehabProgram? program) {
    if (programsState.isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (programsState.error != null) {
      return Padding(
        padding: const EdgeInsets.all(24),
        child: AppEmptyState(
          icon: Icons.error_outline,
          title: 'Unable to load program',
          message: programsState.error,
        ),
      );
    }

    if (program == null) {
      return const Padding(
        padding: EdgeInsets.all(24),
        child: AppEmptyState(
          icon: Icons.fitness_center_outlined,
          title: 'No program assigned',
          message: 'No active rehabilitation program has been assigned yet. Check back after your next physio visit.',
        ),
      );
    }

    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          AppPanel(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  program.programTitle,
                  style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                        color: AppColors.primaryDark,
                        fontWeight: FontWeight.w800,
                      ),
                ),
                if (program.notes != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    program.notes!,
                    style: TextStyle(
                      color: AppColors.neutralDark.withValues(alpha: 0.7),
                      height: 1.45,
                    ),
                  ),
                ],
              ],
            ),
          ),
          const SizedBox(height: 20),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Exercises (${program.exercises.length})',
                style: Theme.of(context).textTheme.titleSmall?.copyWith(
                      color: AppColors.primaryLight,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 0.8,
                    ),
              ),
              const Text(
                'Tap to preview details',
                style: TextStyle(fontSize: 12, color: Colors.grey),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: ListView.separated(
              itemCount: program.exercises.length,
              separatorBuilder: (_, _) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final exercise = program.exercises[index];
                final thumbUrl = exercise.coverImageUrl ??
                    (exercise.steps.isNotEmpty ? exercise.steps.first.imageUrl : null);

                return Material(
                  color: Colors.transparent,
                  child: InkWell(
                    borderRadius: BorderRadius.circular(16),
                    onTap: () => _showExerciseDetails(context, exercise),
                    child: AppPanel(
                      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                      child: Row(
                        children: [
                          if (thumbUrl != null)
                            ClipRRect(
                              borderRadius: BorderRadius.circular(10),
                              child: Stack(
                                children: [
                                  Image.network(
                                    _resolveMediaUrl(thumbUrl),
                                    width: 54,
                                    height: 54,
                                    fit: BoxFit.cover,
                                    errorBuilder: (_, _, _) => _NumberBadge(index: index),
                                  ),
                                  if (exercise.videoUrl != null)
                                    Positioned.fill(
                                      child: Container(
                                        color: Colors.black26,
                                        child: const Icon(Icons.play_circle_fill, color: Colors.white, size: 22),
                                      ),
                                    ),
                                ],
                              ),
                            )
                          else
                            _NumberBadge(index: index),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  exercise.title,
                                  style: const TextStyle(
                                    fontWeight: FontWeight.w700,
                                    color: AppColors.primaryDark,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  '${exercise.repetitions} reps · ${exercise.sets} sets · ${exercise.frequencyPerDay}x daily',
                                  style: TextStyle(
                                    color: AppColors.neutralDark.withValues(alpha: 0.6),
                                    fontSize: 13,
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const Icon(Icons.chevron_right, color: Colors.grey, size: 20),
                        ],
                      ),
                    ),
                  ),
                );
              },
            ),
          ),
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () => Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (_) => ExerciseRoutineScreen(program: program),
                ),
              ),
              child: const Text('START ROUTINE'),
            ),
          ),
        ],
      ),
    );
  }
}

class _NumberBadge extends StatelessWidget {
  const _NumberBadge({required this.index});

  final int index;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 50,
      height: 50,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        color: AppColors.primaryDark.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Text(
        '${index + 1}',
        style: const TextStyle(
          fontWeight: FontWeight.w800,
          color: AppColors.primaryDark,
          fontSize: 16,
        ),
      ),
    );
  }
}

class _MetaChip extends StatelessWidget {
  const _MetaChip({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.primaryDark.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Text(
        label,
        style: const TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: AppColors.primaryDark,
        ),
      ),
    );
  }
}


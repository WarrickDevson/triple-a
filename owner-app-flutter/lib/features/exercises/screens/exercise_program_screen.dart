import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../pets/models/pet.dart';
import '../models/rehab_program.dart';
import '../providers/exercise_providers.dart';
import 'exercise_routine_screen.dart';

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
          Text(
            'Exercises',
            style: Theme.of(context).textTheme.titleSmall?.copyWith(
                  color: AppColors.primaryLight,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 0.8,
                ),
          ),
          const SizedBox(height: 12),
          Expanded(
            child: ListView.separated(
              itemCount: program.exercises.length,
              separatorBuilder: (_, _) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final exercise = program.exercises[index];
                return AppPanel(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  child: Row(
                    children: [
                      Container(
                        width: 36,
                        height: 36,
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
                          ),
                        ),
                      ),
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
                    ],
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

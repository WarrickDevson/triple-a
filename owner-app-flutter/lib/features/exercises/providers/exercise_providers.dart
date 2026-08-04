import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/rehab_program.dart';

class RehabProgramsState {
  const RehabProgramsState({
    this.programs = const [],
    this.isLoading = false,
    this.error,
  });

  final List<RehabProgram> programs;
  final bool isLoading;
  final String? error;

  RehabProgram? get activeProgram =>
      programs.isNotEmpty ? programs.first : null;
}

class RehabProgramsNotifier extends StateNotifier<RehabProgramsState> {
  RehabProgramsNotifier(this._dio) : super(const RehabProgramsState());

  final Dio _dio;

  Future<RehabProgram?> loadPrograms(int petId, {bool force = false}) async {
    if (state.programs.isNotEmpty && !force) {
      return state.activeProgram;
    }

    state = const RehabProgramsState(isLoading: true);
    try {
      final response = await _dio.get<List<dynamic>>('/api/rehab-programs/pet/$petId');
      final programs = response.data!
          .map((item) => RehabProgram.fromJson(item as Map<String, dynamic>))
          .toList();
      state = RehabProgramsState(programs: programs);
      return programs.isNotEmpty ? programs.first : null;
    } on DioException {
      state = const RehabProgramsState(error: 'Unable to load rehabilitation program.');
      return null;
    }
  }
}

final rehabProgramsProvider =
    StateNotifierProvider.family<RehabProgramsNotifier, RehabProgramsState, int>(
  (ref, petId) {
    final authNotifier = ref.read(authProvider.notifier);
    return RehabProgramsNotifier(authNotifier.client);
  },
);

class ExerciseSessionNotifier extends StateNotifier<ExerciseSessionState?> {
  ExerciseSessionNotifier(this._dio, this._petId, this._program)
      : super(null) {
    _restoreProgress();
  }

  final Dio _dio;
  final int _petId;
  final RehabProgram _program;

  String get _cacheKey => 'exercise_session_${_petId}_${_program.rehabProgramId}';

  Future<void> _restoreProgress() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw == null) {
      state = ExerciseSessionState(
        petId: _petId,
        program: _program,
        phase: ExerciseEnginePhase.overview,
      );
      return;
    }

    final data = jsonDecode(raw) as Map<String, dynamic>;
    state = ExerciseSessionState(
      petId: _petId,
      program: _program,
      exerciseIndex: data['exerciseIndex'] as int? ?? 0,
      stepIndex: data['stepIndex'] as int? ?? 0,
      completedSets: data['completedSets'] as int? ?? 0,
      phase: ExerciseEnginePhase.values[data['phase'] as int? ?? 0],
    );
  }

  Future<void> _persist() async {
    if (state == null) return;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(
      _cacheKey,
      jsonEncode({
        'exerciseIndex': state!.exerciseIndex,
        'stepIndex': state!.stepIndex,
        'completedSets': state!.completedSets,
        'phase': state!.phase.index,
      }),
    );
  }

  Future<void> _clearProgress() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_cacheKey);
  }

  void startExercise() {
    if (state == null) return;
    state = state!.copyWith(
      phase: ExerciseEnginePhase.stepActive,
      stepIndex: 0,
      clearError: true,
    );
    _persist();
  }

  void nextStep() {
    if (state == null) return;
    if (state!.isLastStep) {
      completeSet();
      return;
    }

    state = state!.copyWith(stepIndex: state!.stepIndex + 1, clearError: true);
    _persist();
  }

  void completeSet() {
    if (state == null) return;
    final nextCompletedSets = state!.completedSets + 1;
    if (nextCompletedSets < state!.currentExercise.sets) {
      state = state!.copyWith(
        completedSets: nextCompletedSets,
        stepIndex: 0,
        phase: ExerciseEnginePhase.stepActive,
        clearError: true,
      );
      _persist();
      return;
    }

    if (state!.isLastExercise) {
      state = state!.copyWith(
        completedSets: nextCompletedSets,
        phase: ExerciseEnginePhase.programComplete,
        clearError: true,
      );
      _persist();
      _submitCompletion();
      return;
    }

    state = state!.copyWith(
      exerciseIndex: state!.exerciseIndex + 1,
      stepIndex: 0,
      completedSets: 0,
      phase: ExerciseEnginePhase.overview,
      clearError: true,
    );
    _persist();
  }

  Future<void> _submitCompletion() async {
    if (state == null) return;
    state = state!.copyWith(isLoading: true, clearError: true);
    try {
      await _dio.post<void>(
        '/api/pets/$_petId/exercise-sessions',
        data: {
          'rehabProgramId': _program.rehabProgramId,
          'exerciseId': state!.currentExercise.exerciseId,
        },
      );
      await _clearProgress();
      state = state!.copyWith(isLoading: false, clearError: true);
    } on DioException {
      state = state!.copyWith(
        isLoading: false,
        error: 'Routine finished locally, but sync failed. Try again later.',
      );
    }
  }

  void resetRoutine() {
    state = ExerciseSessionState(
      petId: _petId,
      program: _program,
      phase: ExerciseEnginePhase.overview,
    );
    _persist();
  }
}

final exerciseSessionProvider = StateNotifierProvider.family<
    ExerciseSessionNotifier, ExerciseSessionState?, RehabProgram>((ref, program) {
  final authNotifier = ref.read(authProvider.notifier);
  return ExerciseSessionNotifier(authNotifier.client, program.petId, program);
});

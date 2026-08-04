class ExerciseStep {
  const ExerciseStep({
    required this.exerciseStepId,
    required this.stepNumber,
    required this.stepInstruction,
    this.imageUrl,
  });

  final int exerciseStepId;
  final int stepNumber;
  final String stepInstruction;
  final String? imageUrl;

  factory ExerciseStep.fromJson(Map<String, dynamic> json) {
    return ExerciseStep(
      exerciseStepId: json['exerciseStepId'] as int,
      stepNumber: json['stepNumber'] as int,
      stepInstruction: json['stepInstruction'] as String,
      imageUrl: json['imageUrl'] as String?,
    );
  }
}

class RehabProgramExercise {
  const RehabProgramExercise({
    required this.rehabProgramExerciseId,
    required this.exerciseId,
    required this.title,
    required this.repetitions,
    required this.sets,
    required this.frequencyPerDay,
    this.shortDescription,
    this.safetyNotes,
    this.commonMistakes,
    this.videoUrl,
    required this.steps,
  });

  final int rehabProgramExerciseId;
  final int exerciseId;
  final String title;
  final int repetitions;
  final int sets;
  final int frequencyPerDay;
  final String? shortDescription;
  final String? safetyNotes;
  final String? commonMistakes;
  final String? videoUrl;
  final List<ExerciseStep> steps;

  factory RehabProgramExercise.fromJson(Map<String, dynamic> json) {
    return RehabProgramExercise(
      rehabProgramExerciseId: json['rehabProgramExerciseId'] as int,
      exerciseId: json['exerciseId'] as int,
      title: json['title'] as String,
      repetitions: json['repetitions'] as int,
      sets: json['sets'] as int,
      frequencyPerDay: json['frequencyPerDay'] as int,
      shortDescription: json['shortDescription'] as String?,
      safetyNotes: json['safetyNotes'] as String?,
      commonMistakes: json['commonMistakes'] as String?,
      videoUrl: json['videoUrl'] as String?,
      steps: (json['steps'] as List<dynamic>? ?? [])
          .map((item) => ExerciseStep.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }
}

class RehabProgram {
  const RehabProgram({
    required this.rehabProgramId,
    required this.physioId,
    required this.petId,
    required this.programTitle,
    required this.startDate,
    this.endDate,
    this.notes,
    required this.exercises,
  });

  final int rehabProgramId;
  final int physioId;
  final int petId;
  final String programTitle;
  final String startDate;
  final String? endDate;
  final String? notes;
  final List<RehabProgramExercise> exercises;

  factory RehabProgram.fromJson(Map<String, dynamic> json) {
    return RehabProgram(
      rehabProgramId: json['rehabProgramId'] as int,
      physioId: json['physioId'] as int,
      petId: json['petId'] as int,
      programTitle: json['programTitle'] as String,
      startDate: json['startDate'] as String,
      endDate: json['endDate'] as String?,
      notes: json['notes'] as String?,
      exercises: (json['exercises'] as List<dynamic>? ?? [])
          .map((item) => RehabProgramExercise.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }
}

enum ExerciseEnginePhase {
  overview,
  stepActive,
  exerciseComplete,
  programComplete,
}

class ExerciseSessionState {
  const ExerciseSessionState({
    required this.petId,
    required this.program,
    this.exerciseIndex = 0,
    this.stepIndex = 0,
    this.completedSets = 0,
    this.phase = ExerciseEnginePhase.overview,
    this.isLoading = false,
    this.error,
  });

  final int petId;
  final RehabProgram program;
  final int exerciseIndex;
  final int stepIndex;
  final int completedSets;
  final ExerciseEnginePhase phase;
  final bool isLoading;
  final String? error;

  RehabProgramExercise get currentExercise => program.exercises[exerciseIndex];
  ExerciseStep get currentStep => currentExercise.steps[stepIndex];
  bool get isLastStep => stepIndex >= currentExercise.steps.length - 1;
  bool get isLastExercise => exerciseIndex >= program.exercises.length - 1;
  bool get isSetComplete => completedSets >= currentExercise.sets;

  ExerciseSessionState copyWith({
    int? exerciseIndex,
    int? stepIndex,
    int? completedSets,
    ExerciseEnginePhase? phase,
    bool? isLoading,
    String? error,
    bool clearError = false,
  }) {
    return ExerciseSessionState(
      petId: petId,
      program: program,
      exerciseIndex: exerciseIndex ?? this.exerciseIndex,
      stepIndex: stepIndex ?? this.stepIndex,
      completedSets: completedSets ?? this.completedSets,
      phase: phase ?? this.phase,
      isLoading: isLoading ?? this.isLoading,
      error: clearError ? null : (error ?? this.error),
    );
  }
}

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

class ExerciseVideoVariation {
  const ExerciseVideoVariation({
    required this.species,
    this.breedCategory,
    required this.videoUrl,
    this.title,
    this.notes,
  });

  final String species;
  final String? breedCategory;
  final String videoUrl;
  final String? title;
  final String? notes;

  factory ExerciseVideoVariation.fromJson(Map<String, dynamic> json) {
    return ExerciseVideoVariation(
      species: json['species'] as String? ?? 'Canine',
      breedCategory: json['breedCategory'] as String?,
      videoUrl: json['videoUrl'] as String? ?? '',
      title: json['title'] as String?,
      notes: json['notes'] as String?,
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
    this.coverImageUrl,
    this.videoVariations = const [],
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
  final String? coverImageUrl;
  final List<ExerciseVideoVariation> videoVariations;
  final List<ExerciseStep> steps;

  String? resolveVideoUrl({String? species, String? breed}) {
    if (videoVariations.isNotEmpty && (species != null || breed != null)) {
      final s = species?.trim().toLowerCase();
      final b = breed?.trim().toLowerCase();

      // 1. Exact breed or conformation match
      if (b != null && b.isNotEmpty) {
        final breedMatch = videoVariations.firstWhere(
          (v) {
            final vb = v.breedCategory?.trim().toLowerCase();
            final vs = v.species.trim().toLowerCase();
            if (vb == null || vb.isEmpty) return false;
            final speciesMatches = s == null || s.isEmpty || vs == s;
            return speciesMatches && (b.contains(vb) || vb.contains(b));
          },
          orElse: () => const ExerciseVideoVariation(species: '', videoUrl: ''),
        );
        if (breedMatch.videoUrl.isNotEmpty) {
          return breedMatch.videoUrl;
        }
      }

      // 2. Species-only match
      if (s != null && s.isNotEmpty) {
        final speciesMatch = videoVariations.firstWhere(
          (v) {
            final vs = v.species.trim().toLowerCase();
            final vb = v.breedCategory?.trim().toLowerCase();
            return vs == s && (vb == null || vb.isEmpty);
          },
          orElse: () => const ExerciseVideoVariation(species: '', videoUrl: ''),
        );
        if (speciesMatch.videoUrl.isNotEmpty) {
          return speciesMatch.videoUrl;
        }
      }
    }

    return videoUrl;
  }

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
      coverImageUrl: json['coverImageUrl'] as String?,
      videoVariations: (json['videoVariations'] as List<dynamic>? ?? [])
          .map((item) => ExerciseVideoVariation.fromJson(item as Map<String, dynamic>))
          .toList(),
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

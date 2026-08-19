export interface ExerciseStep {
  exerciseStepId: number
  stepNumber: number
  stepInstruction: string
  imageUrl: string | null
}

export interface Exercise {
  exerciseId: number
  title: string
  shortDescription: string | null
  targetedMuscles: string | null
  clinicalPurpose: string | null
  safetyNotes: string | null
  commonMistakes: string | null
  videoUrl: string | null
  targetSpecies: string | null
  conditionCategory: string | null
  difficultyLevel: number
  steps: ExerciseStep[]
}

export interface CreateExerciseStepRequest {
  stepNumber: number
  stepInstruction: string
  imageUrl?: string
}

export interface CreateExerciseRequest {
  title: string
  shortDescription?: string
  targetedMuscles?: string
  clinicalPurpose?: string
  safetyNotes?: string
  commonMistakes?: string
  videoUrl?: string
  targetSpecies?: string
  conditionCategory?: string
  difficultyLevel: number
  steps?: CreateExerciseStepRequest[]
}

export interface ProgramBuilderExercise {
  exerciseId: number
  title: string
  shortDescription: string | null
  repetitions: number
  sets: number
  frequencyPerDay: number
}

export interface CreateRehabProgramExercise {
  exerciseId: number
  repetitions: number
  sets: number
  frequencyPerDay: number
  phaseId?: number
}

export interface CreateRehabProgramRequest {
  petId: number
  programTitle: string
  startDate: string
  endDate?: string
  notes?: string
  exercises: CreateRehabProgramExercise[]
}

export interface RehabProgramExercise {
  rehabProgramExerciseId: number
  exerciseId: number
  title: string
  repetitions: number
  sets: number
  frequencyPerDay: number
  phaseId?: number
  shortDescription: string | null
  safetyNotes: string | null
  commonMistakes: string | null
  videoUrl: string | null
  steps: ExerciseStep[]
}

export interface RehabProgram {
  rehabProgramId: number
  physioId: number
  petId: number
  programTitle: string
  startDate: string
  endDate: string | null
  notes: string | null
  exercises: RehabProgramExercise[]
}

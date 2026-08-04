import type { Exercise } from '../types/exercise'

const EQUIPMENT_BY_CATEGORY: Record<string, string> = {
  Strength: 'None / Mat',
  Mobility: 'None / Mat',
  Balance: 'Airex Pad',
  Flexibility: 'None / Mat',
  'Post-op Recovery': 'Theraband',
  'Pain Management': 'Ice pack',
  'Core Stability': 'Balance disc',
}

export function getExerciseEquipment(exercise: Exercise): string {
  const category = exercise.conditionCategory ?? 'Other'
  if (EQUIPMENT_BY_CATEGORY[category]) return EQUIPMENT_BY_CATEGORY[category]
  if (exercise.title.toLowerCase().includes('airex')) return 'Airex Pad'
  if (exercise.title.toLowerCase().includes('pole')) return 'Ground poles'
  return 'None / Mat'
}

export function getExerciseImage(exercise: Exercise): string | null {
  const stepImage = exercise.steps.find((s) => s.imageUrl)?.imageUrl
  if (stepImage) return stepImage
  return null
}

export function getCategoryLabel(exercise: Exercise): string {
  return exercise.conditionCategory?.trim() || 'Other'
}

export const DIFFICULTY_LABELS: Record<number, string> = {
  1: 'Beginner',
  2: 'Easy',
  3: 'Moderate',
  4: 'Advanced',
  5: 'Expert',
}

export function difficultyLabel(level: number) {
  return DIFFICULTY_LABELS[level] ?? `Level ${level}`
}

export const BODY_REGIONS = [
  'All Regions',
  'Hindlimb',
  'Forelimb',
  'Core',
  'Spine',
  'Full Body',
] as const

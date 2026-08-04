export type ExercisePlanStatus = 'Completed' | 'Pending' | 'Today' | 'Tomorrow'

export interface PlanPhase {
  id: number
  label: string
  title: string
  goals: string[]
}

export const DEFAULT_PHASES: PlanPhase[] = [
  {
    id: 1,
    label: 'Phase 1',
    title: 'Reduce Pain & Inflammation',
    goals: [
      'Reduce pain and inflammation',
      'Improve joint mobility',
      'Activate core muscles',
    ],
  },
  {
    id: 2,
    label: 'Phase 2',
    title: 'Restore Mobility',
    goals: ['Restore normal gait pattern', 'Increase range of motion', 'Build confidence'],
  },
  {
    id: 3,
    label: 'Phase 3',
    title: 'Build Strength & Endurance',
    goals: ['Increase muscle strength', 'Improve cardiovascular fitness', 'Return to activity'],
  },
  {
    id: 4,
    label: 'Phase 4',
    title: 'Maintain & Prevent',
    goals: ['Maintain gains', 'Prevent re-injury', 'Owner education'],
  },
]

const statusCycle: ExercisePlanStatus[] = ['Completed', 'Pending', 'Today', 'Tomorrow']

export function getExerciseStatus(
  rehabProgramExerciseId: number,
  index: number,
): ExercisePlanStatus {
  return statusCycle[(rehabProgramExerciseId + index) % statusCycle.length]!
}

export function statusBadgeClass(status: ExercisePlanStatus) {
  if (status === 'Completed') return 'status-badge status-badge--improving'
  if (status === 'Today') return 'status-badge status-badge--stable'
  if (status === 'Tomorrow') return 'status-badge status-badge--at-risk'
  return 'status-badge status-badge--stable'
}

export function getPlanProgressPercent(exerciseCount: number, completedCount?: number) {
  if (exerciseCount === 0) return 0
  const completed = completedCount ?? Math.ceil(exerciseCount * 0.45)
  return Math.min(100, Math.round((completed / exerciseCount) * 100))
}

export function getNextReviewDate(startDate: string) {
  const d = new Date(startDate)
  d.setDate(d.getDate() + 28)
  return d.toLocaleDateString([], { day: 'numeric', month: 'long', year: 'numeric' })
}

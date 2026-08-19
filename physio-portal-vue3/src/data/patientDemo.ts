export type PatientStatus = 'improving' | 'stable' | 'at-risk'

export interface PatientDemoMeta {
  status: PatientStatus
  phaseLabel: string
  progressPercent: number
  discipline?: string
  height?: string
  vet?: string
  farrier?: string
  saddleFitter?: string
}

const equineDefaults: PatientDemoMeta = {
  status: 'stable',
  phaseLabel: 'Active rehabilitation plan',
  progressPercent: 0,
}

const canineDefaults: PatientDemoMeta = {
  status: 'stable',
  phaseLabel: 'Active rehabilitation plan',
  progressPercent: 0,
}

const felineDefaults: PatientDemoMeta = {
  status: 'stable',
  phaseLabel: 'Active rehabilitation plan',
  progressPercent: 0,
}

const defaultMeta: PatientDemoMeta = {
  status: 'stable',
  phaseLabel: 'Active rehabilitation plan',
  progressPercent: 0,
}

export function getPatientDemoMeta(_petId: number, species?: string): PatientDemoMeta {
  const speciesKey = species?.toLowerCase() ?? ''
  const base =
    speciesKey.includes('equine') || speciesKey.includes('horse')
      ? equineDefaults
      : speciesKey.includes('canine') || speciesKey.includes('dog')
        ? canineDefaults
        : speciesKey.includes('feline') || speciesKey.includes('cat')
          ? felineDefaults
          : defaultMeta

  return { ...base }
}

export function statusDotClass(status: PatientStatus) {
  if (status === 'improving') return 'bg-success-green'
  if (status === 'at-risk') return 'bg-accent-amber'
  return 'bg-neutral-muted'
}

export function statusBadgeClass(status: PatientStatus) {
  if (status === 'improving') return 'status-badge status-badge--improving'
  if (status === 'at-risk') return 'status-badge status-badge--at-risk'
  return 'status-badge status-badge--stable'
}

export function statusLabel(status: PatientStatus) {
  if (status === 'improving') return 'Improving'
  if (status === 'at-risk') return 'At Risk'
  return 'Stable'
}

export const OUTCOME_MEASURES = [
  { key: 'lameness', label: 'Lameness', field: 'lamenessScore' as const },
  { key: 'mobility', label: 'Mobility', field: 'mobilityScore' as const },
  { key: 'energy', label: 'Energy', field: 'energyScore' as const },
  { key: 'pain', label: 'Pain', field: 'painScore' as const },
] as const

export interface PatientUpdate {
  id: number
  name: string
  species: string
  age: string
  note: string
  status: 'improving' | 'stable' | 'at-risk'
  timeAgo: string
  initials: string
}

export interface TaskItem {
  id: number
  label: string
  date: string
  done: boolean
}

export const demoSpeciesBreakdown = [
  { label: 'Dogs', value: 68, color: '#6b7a4d' },
  { label: 'Horses', value: 21, color: '#1a2b3c' },
  { label: 'Cats', value: 7, color: '#9aab7e' },
  { label: 'Other', value: 4, color: '#c5cdb8' },
]

export const demoPatientUpdates: PatientUpdate[] = [
  {
    id: 1,
    name: 'Bella',
    species: 'Labrador',
    age: '6 yrs',
    note: 'Improvement in mobility after laser session',
    status: 'improving',
    timeAgo: '2h ago',
    initials: 'BL',
  },
  {
    id: 2,
    name: 'Maverick',
    species: 'Thoroughbred',
    age: '12 yrs',
    note: 'Stable gait pattern during walk assessment',
    status: 'stable',
    timeAgo: '5h ago',
    initials: 'MV',
  },
  {
    id: 3,
    name: 'Whiskers',
    species: 'Domestic Shorthair',
    age: '4 yrs',
    note: 'Reduced stiffness in hind limbs',
    status: 'improving',
    timeAgo: '1d ago',
    initials: 'WH',
  },
  {
    id: 4,
    name: 'Rocky',
    species: 'German Shepherd',
    age: '8 yrs',
    note: 'Pain score elevated — review plan',
    status: 'at-risk',
    timeAgo: '1d ago',
    initials: 'RK',
  },
]

export const demoTasks: TaskItem[] = [
  { id: 1, label: "Update Bella's treatment plan", date: 'Today', done: false },
  { id: 2, label: 'Recheck: Maverick mobility assessment', date: 'Tomorrow', done: false },
  { id: 3, label: 'Send progress report to owner — Rocky', date: 'Thu', done: false },
  { id: 4, label: 'Review exercise video submission', date: 'Fri', done: true },
]

export const demoProgressStats = {
  averageImprovement: 72,
  trend: '+18% vs last month',
  improving: 20,
  maintaining: 6,
  atRisk: 2,
}

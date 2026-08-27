export type ReportType = 'Progress Report' | 'Discharge Summary' | 'Owner Home Program' | 'SOAP Session Report' | 'Clinical Document'
export type ReportStatus = 'Sent' | 'Generated' | 'Draft'

export interface ReportHistoryItem {
  id: number
  petId: number
  petName: string
  ownerName: string
  species?: string
  breed?: string
  reportType: ReportType
  title: string
  summary?: string
  generatedAt: string
  status: ReportStatus
  soapNoteId?: number | null
  authorName?: string
}

export interface ReportTypeDefinition {
  id: 'progress' | 'discharge' | 'home-program' | 'soap'
  label: ReportType
  badge: string
  description: string
  available: boolean
  suggestedAction: string
}

export const REPORT_TYPES: ReportTypeDefinition[] = [
  {
    id: 'progress',
    label: 'Progress Report',
    badge: 'Clinical Progress',
    description: 'Full rehabilitation progress summary with pain/mobility trends, exercise adherence, and session history.',
    available: true,
    suggestedAction: 'Generate Progress Report',
  },
  {
    id: 'discharge',
    label: 'Discharge Summary',
    badge: 'End of Care',
    description: 'End-of-care summary comparing initial vs final outcomes, discharge criteria met, and continuing home plan.',
    available: true,
    suggestedAction: 'Generate Discharge Summary',
  },
  {
    id: 'home-program',
    label: 'Owner Home Program',
    badge: 'Pet Owner Guide',
    description: 'Owner-friendly printable guide with prescribed home exercises, technique cues, dosage, and safety dos/don\'ts.',
    available: true,
    suggestedAction: 'Generate Home Program',
  },
  {
    id: 'soap',
    label: 'SOAP Session Report',
    badge: 'Clinical Assessment',
    description: 'Detailed clinical assessment of a specific therapy appointment with Subjective, Objective, Action, and Plan.',
    available: true,
    suggestedAction: 'Generate SOAP Summary',
  },
]

export const demoReportHistory: ReportHistoryItem[] = [
  {
    id: 1,
    petId: 1,
    petName: 'Champ',
    ownerName: 'Test 1.0',
    species: 'Canine',
    breed: 'Labrador Retriever',
    reportType: 'Progress Report',
    title: 'Mid-Treatment Rehabilitation Progress Report',
    summary: 'Champ shows excellent response to hydrotherapy and myofascial release. Left stifle ROM restored to 135°. Pain reduced from 6/10 to 2/10.',
    generatedAt: '2026-08-20T10:30:00Z',
    status: 'Sent',
    authorName: 'Dr. S. Devson',
  },
  {
    id: 2,
    petId: 1,
    petName: 'Champ',
    ownerName: 'Test 1.0',
    species: 'Canine',
    breed: 'Labrador Retriever',
    reportType: 'Owner Home Program',
    title: 'Phase 2 Home Exercise Protocol',
    summary: 'Prescribed routine: Cavaletti rails (2x daily, 10 reps), Sit-to-stand squats (3 sets of 8), and 20-min controlled leash walking.',
    generatedAt: '2026-08-15T14:15:00Z',
    status: 'Sent',
    authorName: 'Dr. S. Devson',
  },
  {
    id: 3,
    petId: 2,
    petName: 'Chuck',
    ownerName: 'Test 1.0',
    species: 'Canine',
    breed: 'German Shepherd',
    reportType: 'Discharge Summary',
    title: 'Post-Op TPLO Final Discharge Summary',
    summary: 'Full functional recovery achieved. Symmetrical weight-bearing and resolved lameness (Grade 0/5). Discharged to long-term home maintenance protocol.',
    generatedAt: '2026-08-10T09:00:00Z',
    status: 'Generated',
    authorName: 'Dr. S. Devson',
  },
  {
    id: 4,
    petId: 1,
    petName: 'Champ',
    ownerName: 'Test 1.0',
    species: 'Canine',
    breed: 'Labrador Retriever',
    reportType: 'SOAP Session Report',
    title: 'Session Assessment - Joint Mobilisation & Laser',
    summary: 'Right hip extension measured at 142°. Applied Class IV laser therapy at 4 J/cm². Patient tolerated full underwater treadmill session.',
    generatedAt: '2026-08-05T16:45:00Z',
    status: 'Sent',
    authorName: 'Dr. S. Devson',
  },
]

export function reportStatusClass(status: ReportStatus | string) {
  if (status === 'Sent' || status === 'Shared') return 'status-badge status-badge--improving'
  if (status === 'Draft') return 'status-badge status-badge--at-risk'
  return 'status-badge status-badge--stable'
}

export function formatReportType(rawType: string): ReportType {
  const t = rawType.toUpperCase()
  if (t.includes('DISCHARGE')) return 'Discharge Summary'
  if (t.includes('HOME')) return 'Owner Home Program'
  if (t.includes('SOAP')) return 'SOAP Session Report'
  if (t.includes('DOCUMENT')) return 'Clinical Document'
  return 'Progress Report'
}

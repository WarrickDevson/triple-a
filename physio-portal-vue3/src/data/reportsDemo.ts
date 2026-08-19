export type ReportType = 'Progress Report' | 'Discharge Summary' | 'Owner Home Program'
export type ReportStatus = 'Generated' | 'Sent' | 'Draft'

export interface ReportHistoryItem {
  id: number
  petId: number
  petName: string
  ownerName: string
  reportType: ReportType
  generatedAt: string
  status: ReportStatus
}

export const REPORT_TYPES: { id: string; label: ReportType; description: string; available: boolean }[] = [
  {
    id: 'progress',
    label: 'Progress Report',
    description: 'Full rehabilitation progress summary with outcome measures and session history.',
    available: true,
  },
  {
    id: 'discharge',
    label: 'Discharge Summary',
    description: 'End-of-care summary for owner and referring veterinarian.',
    available: true,
  },
  {
    id: 'home-program',
    label: 'Owner Home Program',
    description: 'Printable home exercise guide for the pet owner.',
    available: true,
  },
]

export const demoReportHistory: ReportHistoryItem[] = [
  {
    id: 1,
    petId: 1,
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    reportType: 'Progress Report',
    generatedAt: '2026-07-25T10:30:00',
    status: 'Sent',
  },
  {
    id: 2,
    petId: 3,
    petName: 'Rocky',
    ownerName: 'James Cooper',
    reportType: 'Progress Report',
    generatedAt: '2026-07-22T14:15:00',
    status: 'Generated',
  },
  {
    id: 3,
    petId: 2,
    petName: 'Maverick',
    ownerName: 'Emma van der Berg',
    reportType: 'Discharge Summary',
    generatedAt: '2026-07-18T09:00:00',
    status: 'Draft',
  },
  {
    id: 4,
    petId: 1,
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    reportType: 'Owner Home Program',
    generatedAt: '2026-07-10T16:45:00',
    status: 'Sent',
  },
]

export function reportStatusClass(status: ReportStatus) {
  if (status === 'Sent') return 'status-badge status-badge--improving'
  if (status === 'Draft') return 'status-badge status-badge--at-risk'
  return 'status-badge status-badge--stable'
}

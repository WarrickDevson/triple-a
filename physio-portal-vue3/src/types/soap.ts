export interface CustomMetricItem {
  name: string
  value: number
  minScale: number
  maxScale: number
  unitOrDescriptor?: string
}

export interface SoapNote {
  soapNoteId: number
  petId: number
  physioId: number
  physioName: string
  appointmentId?: number | null
  sessionDate: string
  subjective: string
  objective: string
  action: string
  plan: string
  stiffnessScore?: number | null
  painScore?: number | null
  lamenessScore?: number | null
  customMetrics: CustomMetricItem[]
  isSharedWithOwner: boolean
  sharedAtUtc?: string | null
  createdAtUtc?: string
}

export interface CreateSoapNoteRequest {
  appointmentId?: number | null
  sessionDate?: string | null
  subjective: string
  objective: string
  action: string
  plan: string
  stiffnessScore?: number | null
  painScore?: number | null
  lamenessScore?: number | null
  customMetrics?: CustomMetricItem[]
  shareWithOwner?: boolean
  diagnosisUpdate?: string
}

export interface UpdateSoapNoteRequest {
  sessionDate?: string | null
  subjective?: string
  objective?: string
  action?: string
  plan?: string
  stiffnessScore?: number | null
  painScore?: number | null
  lamenessScore?: number | null
  customMetrics?: CustomMetricItem[]
  shareWithOwner?: boolean
}

export interface SharedReport {
  sharedReportId: number
  petId: number
  soapNoteId?: number | null
  sharedByPhysioId: number
  sharedByPhysioName: string
  title: string
  reportType: string
  summary?: string | null
  sharedAtUtc: string
}

export interface OwnerSubjectiveNote {
  ownerSubjectiveNoteId: number
  petId: number
  ownerId: number
  ownerName: string
  noteDate: string
  notes: string
  painObserved?: number | null
  energyObserved?: number | null
  isReviewed: boolean
}


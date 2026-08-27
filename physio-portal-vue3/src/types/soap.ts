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
  audioUrl?: string | null
  rawTranscript?: string | null
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
  audioUrl?: string | null
  rawTranscript?: string | null
}

export interface UpdateSoapNoteRequest {
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
  audioUrl?: string | null
  rawTranscript?: string | null
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
  petName?: string
  ownerName?: string
  species?: string
  breed?: string
  isActive?: boolean
}

export interface CreateReportPayload {
  petId: number
  reportType: string
  title: string
  summary?: string
  diagnosis?: string
  dischargeStatus?: string
  maintenancePlan?: string
  veterinarianNotes?: string
  ownerInstructions?: string
  soapNoteId?: number
  shareWithOwner?: boolean
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

// Voice Dictation & Audio Transcription Types
export interface ParseSoapNarrativeRequest {
  transcript: string
  petId?: number | null
  petName?: string | null
  species?: string | null
  targetSection?: 'S' | 'O' | 'A' | 'P' | null
}

export interface StructuredSoapNote {
  subjective: string
  objective: string
  action: string
  plan: string
  stiffnessScore?: number | null
  painScore?: number | null
  lamenessScore?: number | null
  customMetrics: CustomMetricItem[]
  suggestedDiagnosis?: string | null
  rawTranscript: string
  confidenceScore: number
  extractedTerms: string[]
}

export interface SoapTranscriptionResult {
  transcript: string
  structuredNote?: StructuredSoapNote | null
  durationMs: number
  usedLocalFallback: boolean
}

export interface ProcessSessionAudioResponse {
  audioUrl: string
  rawTranscript: string
  structuredNote: StructuredSoapNote
  durationMs: number
  usedLocalFallback: boolean
}

export interface VocabularyCategory {
  category: string
  terms: string[]
}

export interface SoapVocabulary {
  terms: string[]
  categories: VocabularyCategory[]
  autoCorrections: Record<string, string>
}

export type RecordingState = 'idle' | 'recording' | 'paused' | 'processing' | 'transcribing' | 'completed' | 'error'

export interface OfflineSoapRecording {
  id: string
  timestamp: string
  petId?: number | null
  petName?: string | null
  targetSection?: 'S' | 'O' | 'A' | 'P' | 'FULL'
  transcript: string
  audioBlobUrl?: string
  audioMimeType?: string
  isSynced: boolean
}

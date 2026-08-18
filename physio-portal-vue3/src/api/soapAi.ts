import { apiClient } from './client'

export interface PolishSoapSectionRequest {
  sectionName: string
  rawText: string
  petName?: string
  species?: string
  condition?: string
}

export interface PolishSoapSectionResponse {
  sectionName: string
  polishedText: string
  correctionsMade: string[]
  clinicalSummary?: string
  usedCloudAi: boolean
}

export interface AiConfigStatus {
  isCloudAiEnabled: boolean
  provider: string
  modelName: string
  hasApiKey: boolean
}

/**
 * Sends rough draft or dictated clinical text to AI for contextual medical correction & structuring.
 */
export async function polishSoapSection(payload: PolishSoapSectionRequest): Promise<PolishSoapSectionResponse> {
  const { data } = await apiClient.post<PolishSoapSectionResponse>('/soap-notes/ai/polish-section', payload)
  return data
}

/**
 * Uploads an audio blob directly to the backend for Cloud AI transcription.
 */
export async function transcribeSoapAudioBlob(
  audioBlob: Blob,
  petName?: string,
  species?: string
): Promise<{ transcript: string; usedLocalFallback: boolean; durationMs: number }> {
  const formData = new FormData()
  formData.append('file', audioBlob, 'dictation.webm')
  if (petName) formData.append('petName', petName)
  if (species) formData.append('species', species)

  const { data } = await apiClient.post('/soap-notes/dictation/transcribe-audio', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return data
}

/**
 * Checks the active backend AI configuration status (e.g. Gemini key presence).
 */
export async function getAiConfigStatus(): Promise<AiConfigStatus> {
  try {
    const { data } = await apiClient.get<AiConfigStatus>('/soap-notes/ai/config-status')
    return data
  } catch {
    return {
      isCloudAiEnabled: false,
      provider: 'Local',
      modelName: 'gemini-2.0-flash',
      hasApiKey: false
    }
  }
}

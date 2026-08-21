import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { StructuredSoapNote } from '../types/soap'
import { processSessionAudioBlob } from '../api/soapAi'
import { createSoapNote } from '../api/soapNotes'

export interface VoiceSessionJob {
  id: string
  petId: number
  petName: string
  species?: string
  status: 'processing' | 'completed' | 'error'
  recordedAt: string
  audioUrl?: string
  rawTranscript?: string
  structuredNote?: StructuredSoapNote
  errorMessage?: string
}

export interface VoiceSessionNotification {
  id: string
  petId: number
  petName: string
  audioUrl: string
  rawTranscript: string
  structuredNote: StructuredSoapNote
  timestamp: string
}

export const useVoiceSessionStore = defineStore('voiceSession', () => {
  const activeJob = ref<VoiceSessionJob | null>(null)
  const activeNotification = ref<VoiceSessionNotification | null>(null)
  const pendingReviewNote = ref<{
    petId: number
    petName: string
    audioUrl: string
    rawTranscript: string
    structuredNote: StructuredSoapNote
  } | null>(null)

  let notifTimeoutId: ReturnType<typeof setTimeout> | null = null

  async function processVoiceSession(
    audioBlob: Blob,
    petId: number,
    petName: string,
    species: string = 'Canine'
  ): Promise<VoiceSessionJob> {
    const jobId = `job_${Date.now()}`
    const job: VoiceSessionJob = {
      id: jobId,
      petId,
      petName,
      species,
      status: 'processing',
      recordedAt: new Date().toISOString()
    }
    activeJob.value = { ...job }

    try {
      const res = await processSessionAudioBlob(audioBlob, petName, species, petId)

      job.status = 'completed'
      job.audioUrl = res.audioUrl
      job.rawTranscript = res.rawTranscript
      job.structuredNote = res.structuredNote

      // Auto-save directly into database table SoapNotes so the note is permanently recorded
      try {
        await createSoapNote(petId, {
          sessionDate: new Date().toISOString(),
          subjective: res.structuredNote.subjective || res.rawTranscript || 'Voice consultation recorded.',
          objective: res.structuredNote.objective || '',
          action: res.structuredNote.action || '',
          plan: res.structuredNote.plan || '',
          stiffnessScore: res.structuredNote.stiffnessScore,
          painScore: res.structuredNote.painScore,
          lamenessScore: res.structuredNote.lamenessScore,
          customMetrics: res.structuredNote.customMetrics || [],
          shareWithOwner: false,
          audioUrl: res.audioUrl,
          rawTranscript: res.rawTranscript
        })

        // Notify active patient tabs to reload notes list immediately
        window.dispatchEvent(new CustomEvent('soap-note-created', { detail: { petId } }))
      } catch (saveErr) {
        console.warn('Auto-save of background note failed, will keep draft available:', saveErr)
      }

      // Immediately clear processing loading indicator
      activeJob.value = null

      // Trigger user-friendly floating notification banner
      activeNotification.value = {
        id: jobId,
        petId,
        petName,
        audioUrl: res.audioUrl,
        rawTranscript: res.rawTranscript,
        structuredNote: res.structuredNote,
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      }

      // Auto-dismiss notification toast after 10 seconds
      if (notifTimeoutId) clearTimeout(notifTimeoutId)
      notifTimeoutId = setTimeout(() => {
        if (activeNotification.value?.id === jobId) {
          activeNotification.value = null
        }
      }, 10000)

      return job
    } catch (err: any) {
      console.error('Background voice session processing failed:', err)
      job.status = 'error'
      job.errorMessage = err.response?.data?.message || err.message || 'Failed to process voice session with AI.'
      activeJob.value = { ...job }

      // Auto-clear error toast after 8 seconds
      setTimeout(() => {
        if (activeJob.value?.id === jobId && activeJob.value?.status === 'error') {
          activeJob.value = null
        }
      }, 8000)

      return job
    }
  }

  function dismissNotification() {
    if (notifTimeoutId) clearTimeout(notifTimeoutId)
    activeNotification.value = null
  }

  function clearActiveJob() {
    activeJob.value = null
  }

  function triggerReviewFromNotification(notif: VoiceSessionNotification) {
    if (notifTimeoutId) clearTimeout(notifTimeoutId)
    pendingReviewNote.value = {
      petId: notif.petId,
      petName: notif.petName,
      audioUrl: notif.audioUrl,
      rawTranscript: notif.rawTranscript,
      structuredNote: notif.structuredNote
    }
    activeNotification.value = null
  }

  function clearPendingReview() {
    pendingReviewNote.value = null
  }

  return {
    activeJob,
    activeNotification,
    pendingReviewNote,
    processVoiceSession,
    clearActiveJob,
    dismissNotification,
    triggerReviewFromNotification,
    clearPendingReview
  }
})

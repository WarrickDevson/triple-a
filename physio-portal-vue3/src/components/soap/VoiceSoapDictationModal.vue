<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue'
import {
  Mic,
  Square,
  Sparkles,
  X,
  RotateCcw,
  Volume2,
  Save,
  Check,
  AlertCircle,
  Loader2,
  FileText,
  ArrowRight
} from '@lucide/vue'
import type { StructuredSoapNote, CustomMetricItem } from '../../types/soap'
import { useAudioRecorder } from '../../composables/useAudioRecorder'
import AudioWaveformVisualizer from './AudioWaveformVisualizer.vue'
import { parseSoapNarrative } from '../../api/soapNotes'
import { transcribeSoapAudioBlob } from '../../api/soapAi'
import { useVoiceSessionStore } from '../../store/voiceSession'
import { CLINICAL_SAMPLE_CONSULTATIONS, type ClinicalAudioSample } from '../../utils/veterinaryLexicon'

const props = defineProps<{
  isOpen: boolean
  petId: number
  petName: string
  species?: string
}>()

const emit = defineEmits<{
  close: []
  applyStructuredNote: [note: StructuredSoapNote, mode: 'replace' | 'append']
  insertRawTranscript: [rawText: string, targetSection: 'Subjective' | 'Objective' | 'Action' | 'Plan' | 'All']
}>()

const {
  recordingState,
  formattedTime,
  audioLevel,
  waveformFrequencies,
  liveTranscript,
  fullTranscript,
  audioBlob,
  audioUrl,
  errorMessage: recorderError,
  startRecording,
  stopRecording,
  pauseRecording,
  resumeRecording,
  resetRecording,
  saveToOfflineQueue
} = useAudioRecorder()

const transcriptionEngine = ref<'browser' | 'cloud'>('browser')
const voiceSessionStore = useVoiceSessionStore()
const isParsingAi = ref(false)
const isBackgroundProcessing = ref(false)
const structuringError = ref('')
const structuredResult = ref<StructuredSoapNote | null>(null)
const rawInsertSection = ref<'Subjective' | 'Objective' | 'Action' | 'Plan' | 'All'>('Subjective')

// Editable fields for the AI preview card
const editableSubjective = ref('')
const editableObjective = ref('')
const editableAction = ref('')
const editablePlan = ref('')
const editablePain = ref<number | null>(null)
const editableStiffness = ref<number | null>(null)
const editableLameness = ref<number | null>(null)
const editableDiagnosis = ref('')
const editableCustomMetrics = ref<CustomMetricItem[]>([])

onMounted(() => {
  const saved = localStorage.getItem('movewell_dictation_engine')
  if (saved === 'cloud' || saved === 'browser') {
    transcriptionEngine.value = saved
  }
})

function setEngine(engine: 'browser' | 'cloud') {
  transcriptionEngine.value = engine
  localStorage.setItem('movewell_dictation_engine', engine)
}

watch(
  () => props.isOpen,
  (open) => {
    if (!open) {
      resetRecording()
      structuredResult.value = null
      structuringError.value = ''
    }
  }
)

// When speech recognition produces text, ensure liveTranscript has it (browser mode only)
watch(
  () => fullTranscript.value,
  (val) => {
    if (val && recordingState.value === 'recording' && transcriptionEngine.value === 'browser') {
      liveTranscript.value = val
    }
  }
)

function handleStartRecording() {
  startRecording(transcriptionEngine.value === 'browser')
}

async function handleStopRecordingOnly() {
  if (recordingState.value === 'recording' || recordingState.value === 'paused') {
    const recResult = await stopRecording()
    if (transcriptionEngine.value === 'browser' && recResult.transcript.trim()) {
      liveTranscript.value = recResult.transcript.trim()
    }

    // If in Cloud AI Audio mode, run cloud transcription on recorded audio blob
    if (transcriptionEngine.value === 'cloud' && recResult.blob && recResult.blob.size > 0) {
      isParsingAi.value = true
      try {
        const cloudStt = await transcribeSoapAudioBlob(recResult.blob, props.petName, props.species || 'Canine')
        if (cloudStt && cloudStt.transcript) {
          liveTranscript.value = cloudStt.transcript.trim()
        }
      } catch (sttErr) {
        console.warn('Cloud audio STT error:', sttErr)
      } finally {
        isParsingAi.value = false
      }
    }
  }
}

async function handleDoneAndProcessInBackground() {
  isBackgroundProcessing.value = true
  let blobToProcess: Blob | null = null

  if (recordingState.value === 'recording' || recordingState.value === 'paused') {
    const recResult = await stopRecording()
    blobToProcess = recResult.blob && recResult.blob.size > 0 ? recResult.blob : null
  } else if (audioBlob.value && audioBlob.value.size > 0) {
    blobToProcess = audioBlob.value
  } else if (audioUrl.value) {
    try {
      blobToProcess = await fetch(audioUrl.value).then(r => r.blob())
    } catch {
      blobToProcess = null
    }
  }

  if (blobToProcess) {
    voiceSessionStore.processVoiceSession(
      blobToProcess,
      props.petId,
      props.petName,
      props.species || 'Canine'
    )
  } else if (liveTranscript.value.trim()) {
    // If text transcript exists without audio blob, structure and trigger notification
    parseSoapNarrative({
      transcript: liveTranscript.value.trim(),
      petId: props.petId,
      petName: props.petName,
      species: props.species || 'Canine'
    }).then(structured => {
      voiceSessionStore.activeNotification = {
        id: `job_${Date.now()}`,
        petId: props.petId,
        petName: props.petName,
        audioUrl: '',
        rawTranscript: liveTranscript.value.trim(),
        structuredNote: structured,
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      }
    }).catch(err => console.warn('Background narrative parse failed:', err))
  }

  isBackgroundProcessing.value = false
  emit('close')
}

async function handleGenerateAiSummary() {
  const transcriptToParse = liveTranscript.value.trim()

  if (!transcriptToParse) {
    structuringError.value = 'No speech or narrative text to summarize. Please record dictation or type your consultation notes.'
    return
  }

  isParsingAi.value = true
  structuringError.value = ''

  try {
    const structured = await parseSoapNarrative({
      transcript: transcriptToParse,
      petId: props.petId,
      petName: props.petName,
      species: props.species || 'Canine'
    })

    structuredResult.value = structured
    editableSubjective.value = structured.subjective || ''
    editableObjective.value = structured.objective || ''
    editableAction.value = structured.action || ''
    editablePlan.value = structured.plan || ''
    editablePain.value = structured.painScore ?? null
    editableStiffness.value = structured.stiffnessScore ?? null
    editableLameness.value = structured.lamenessScore ?? null
    editableDiagnosis.value = structured.suggestedDiagnosis || ''
    editableCustomMetrics.value = (structured.customMetrics || []).map((m: any) => ({
      name: m.name,
      value: m.value,
      minScale: m.minScale ?? 0,
      maxScale: m.maxScale ?? 100,
      unitOrDescriptor: m.unitOrDescriptor
    }))
  } catch (err: any) {
    console.error('Failed to parse narrative:', err)
    structuringError.value = 'Failed to summarize notes with AI. Please check your internet connection or Gemini API key.'
  } finally {
    isParsingAi.value = false
  }
}

function handleApplyRawToNote() {
  const text = liveTranscript.value.trim()
  if (!text) return
  emit('insertRawTranscript', text, rawInsertSection.value)
  emit('close')
}

function handleApplyStructuredNote(mode: 'replace' | 'append') {
  if (!structuredResult.value) return

  const finalPayload: StructuredSoapNote = {
    ...structuredResult.value,
    subjective: editableSubjective.value,
    objective: editableObjective.value,
    action: editableAction.value,
    plan: editablePlan.value,
    painScore: editablePain.value,
    stiffnessScore: editableStiffness.value,
    lamenessScore: editableLameness.value,
    suggestedDiagnosis: editableDiagnosis.value.trim() ? editableDiagnosis.value.trim() : null,
    customMetrics: editableCustomMetrics.value
  }

  emit('applyStructuredNote', finalPayload, mode)
  emit('close')
}

function handleSelectSample(sample: ClinicalAudioSample) {
  loadClinicalSample(sample)
  liveTranscript.value = sample.transcript
  structuredResult.value = null
  structuringError.value = ''
}

function handleSaveOffline() {
  const text = liveTranscript.value.trim() || fullTranscript.value.trim()
  if (!text) return
  saveToOfflineQueue({
    petId: props.petId,
    petName: props.petName,
    targetSection: 'FULL',
    transcript: text,
    audioBlobUrl: audioUrl.value ?? undefined
  })
  alert('Consultation dictation saved to local offline drafts.')
}

const hasTranscript = computed(() => !!liveTranscript.value.trim())
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-60 flex items-center justify-center overflow-y-auto bg-navy/70 p-4 backdrop-blur-md"
  >
    <div class="relative w-full max-w-4xl rounded-3xl bg-surface p-6 shadow-2xl border border-white/20 transition-all max-h-[92vh] flex flex-col">
      <!-- Modal Header -->
      <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-4 shrink-0">
        <div class="flex items-center gap-3">
          <div class="flex h-11 w-11 items-center justify-center rounded-2xl bg-purple-600 text-white shadow-md shadow-purple-600/30">
            <Mic class="h-6 w-6" />
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h2 class="text-lg font-bold text-navy">Full SOAP Voice Dictation</h2>
              <span class="inline-flex items-center gap-1 rounded-full bg-purple-100 px-2.5 py-0.5 text-[11px] font-bold text-purple-700">
                AI Structured
              </span>
            </div>
            <p class="text-xs text-neutral-muted">
              Patient: <strong class="text-navy">{{ petName }}</strong> · Speak your consultation observations to auto-fill Subjective, Objective, Action, and Plan.
            </p>
          </div>
        </div>

        <div class="flex items-center gap-3">
          <!-- Engine Selector -->
          <div class="flex items-center gap-1 rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-0.5 text-[11px]">
            <button
              type="button"
              class="rounded-lg px-2.5 py-1 font-bold transition-all"
              :class="transcriptionEngine === 'browser' ? 'bg-surface text-navy shadow-xs' : 'text-neutral-muted hover:text-navy'"
              title="Instant streaming in-browser Speech-to-Text ($0 Cost)"
              @click="setEngine('browser')"
            >
              ⚡ Browser STT
            </button>
            <button
              type="button"
              class="rounded-lg px-2.5 py-1 font-bold transition-all"
              :class="transcriptionEngine === 'cloud' ? 'bg-surface text-navy shadow-xs' : 'text-neutral-muted hover:text-navy'"
              title="High-precision Cloud AI Audio transcription with Gemini"
              @click="setEngine('cloud')"
            >
              ☁️ Cloud AI Audio
            </button>
          </div>

          <button
            type="button"
            class="rounded-xl p-2 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
            @click="emit('close')"
          >
            <X class="h-5 w-5" />
          </button>
        </div>
      </div>

      <!-- Modal Body (Scrollable) -->
      <div class="mt-4 space-y-5 overflow-y-auto pr-1 flex-1">
        <!-- Error Alerts -->
        <div
          v-if="structuringError || recorderError"
          class="flex items-center gap-2 rounded-xl bg-amber-50 border border-amber-200 p-3 text-xs text-amber-800"
        >
          <AlertCircle class="h-4 w-4 shrink-0 text-amber-600" />
          <span>{{ structuringError || recorderError }}</span>
        </div>

        <!-- 1. Waveform Visualizer -->
        <AudioWaveformVisualizer
          :recording-state="recordingState"
          :formatted-time="formattedTime"
          :audio-level="audioLevel"
          :frequencies="waveformFrequencies"
          :error-message="recorderError"
          @start="handleStartRecording"
          @pause="pauseRecording"
          @resume="resumeRecording"
          @stop="handleStopRecordingOnly"
        />

        <!-- Recording Controls -->
        <div class="flex flex-wrap items-center justify-between gap-3 rounded-2xl bg-neutral-grey/25 p-4">
          <div class="flex items-center gap-3">
            <button
              v-if="recordingState === 'idle' || recordingState === 'completed'"
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl bg-sage px-6 py-3 text-sm font-bold text-white shadow-lg shadow-sage/30 hover:bg-sage/90 transition-all hover:scale-105 active:scale-95"
              @click="handleStartRecording"
            >
              <Mic class="h-5 w-5 animate-pulse" />
              {{ recordingState === 'completed' ? 'Re-Record Dictation' : 'Start Recording Dictation' }}
            </button>

            <button
              v-else-if="recordingState === 'recording' || recordingState === 'paused'"
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl bg-rose-600 px-6 py-3 text-sm font-bold text-white shadow-lg shadow-rose-600/30 hover:bg-rose-700 transition-all animate-pulse"
              @click="handleStopRecordingOnly"
            >
              <Square class="h-5 w-5 fill-white" />
              Stop Recording
            </button>

            <!-- 1-Tap On-the-Go Background Processing Button (Available during recording OR after stopping!) -->
            <button
              v-if="recordingState === 'recording' || recordingState === 'paused' || audioUrl || hasTranscript"
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl bg-purple-600 px-5 py-3 text-sm font-bold text-white shadow-lg shadow-purple-600/30 hover:bg-purple-700 transition-all hover:scale-105"
              title="Save audio memo to server and process SOAP in background while you move to your next patient"
              @click="handleDoneAndProcessInBackground"
            >
              <Sparkles class="h-5 w-5 animate-pulse" />
              🚗 Save & Process in Background
            </button>
          </div>

          <div v-if="recordingState === 'recording'" class="flex items-center gap-2 text-xs font-semibold text-rose-600 animate-pulse">
            <span class="h-2 w-2 rounded-full bg-rose-600"></span>
            Listening & Transcribing Live...
          </div>
        </div>

        <!-- 2. Audio Playback (if recorded) -->
        <div v-if="audioUrl" class="flex items-center gap-3 rounded-xl border border-neutral-grey/80 bg-surface p-3 text-xs">
          <Volume2 class="h-4 w-4 text-sage shrink-0" />
          <span class="font-semibold text-navy">Audio Playback:</span>
          <audio :src="audioUrl" controls class="h-8 flex-1" />
        </div>

        <!-- Clinical Demo Samples Row for Quick Testing -->
        <div class="flex flex-wrap items-center justify-between gap-2 rounded-2xl bg-neutral-grey/15 p-3 border border-neutral-grey/60">
          <div class="flex items-center gap-1.5 shrink-0">
            <Sparkles class="h-3.5 w-3.5 text-sage" />
            <span class="text-xs font-bold text-navy">Clinical Test Samples:</span>
          </div>
          <div class="flex flex-wrap items-center gap-1.5">
            <button
              v-for="sample in CLINICAL_SAMPLE_CONSULTATIONS"
              :key="sample.id"
              type="button"
              class="inline-flex items-center gap-1 rounded-xl border border-neutral-grey/80 bg-surface px-2.5 py-1 text-[11px] font-semibold text-navy hover:border-sage hover:bg-sage-muted hover:text-sage transition-all shadow-xs active:scale-95"
              :title="sample.transcript"
              @click="handleSelectSample(sample)"
            >
              <span>{{ sample.title }}</span>
              <span class="text-[10px] text-neutral-muted">({{ sample.duration }})</span>
            </button>
          </div>
        </div>

        <!-- 3. RAW SPOKEN TRANSCRIPT (Primary View) -->
        <div class="rounded-2xl border border-neutral-grey/80 bg-surface p-4 space-y-3">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <FileText class="h-4 w-4 text-sage" />
              <label class="block text-xs font-bold uppercase tracking-wider text-navy">
                Spoken Dictation Transcript (Exact Words)
              </label>
            </div>
            <span class="text-[11px] text-neutral-muted">Editable below before inserting</span>
          </div>

          <textarea
            v-model="liveTranscript"
            rows="4"
            class="w-full rounded-xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 text-sm text-navy focus:border-sage focus:outline-none leading-relaxed"
            placeholder="Your spoken words will appear here live as you speak. You can also paste or edit your notes here directly..."
          />

          <!-- Action Bar for Transcript: Choice A vs Choice B -->
          <div v-if="hasTranscript" class="flex flex-wrap items-center justify-between gap-3 pt-2 border-t border-neutral-grey/60">
            <!-- Choice A: Insert Raw Transcript Directly -->
            <div class="flex items-center gap-2">
              <span class="text-xs font-semibold text-navy">Insert directly into:</span>
              <select
                v-model="rawInsertSection"
                class="rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 text-xs text-navy font-semibold focus:border-sage focus:outline-none"
              >
                <option value="Subjective">Subjective (S)</option>
                <option value="Objective">Objective (O)</option>
                <option value="Action">Action (A)</option>
                <option value="Plan">Plan (P)</option>
                <option value="All">Full Note (All Tabs)</option>
              </select>
              <button
                type="button"
                class="inline-flex items-center gap-1.5 rounded-xl border border-sage/60 bg-sage-muted px-3.5 py-1.5 text-xs font-bold text-sage hover:bg-sage hover:text-white transition-all shadow-xs"
                @click="handleApplyRawToNote"
              >
                <span>📋 Insert Raw Text</span>
                <ArrowRight class="h-3.5 w-3.5" />
              </button>
            </div>

            <!-- Choice B: Summarise with Gemini AI -->
            <button
              type="button"
              :disabled="isParsingAi"
              class="inline-flex items-center gap-1.5 rounded-xl border border-purple-300 bg-purple-50 px-4 py-1.5 text-xs font-bold text-purple-700 hover:bg-purple-100 shadow-xs transition-all disabled:opacity-50"
              @click="handleGenerateAiSummary"
            >
              <Loader2 v-if="isParsingAi" class="h-3.5 w-3.5 animate-spin text-purple-600" />
              <Sparkles v-else class="h-3.5 w-3.5 text-purple-600" />
              <span>{{ isParsingAi ? 'Summarising with AI...' : '✨ Summarise into SOAP with AI' }}</span>
            </button>
          </div>
        </div>

        <!-- 4. AI Structured Summary Card (Only shown when explicitly generated) -->
        <div v-if="isParsingAi" class="rounded-2xl border border-purple-200 bg-purple-50/50 p-8 text-center">
          <Loader2 class="mx-auto h-8 w-8 text-purple-600 animate-spin" />
          <p class="mt-2 text-sm font-bold text-navy">Gemini AI is categorizing your transcript into SOAP...</p>
          <p class="text-xs text-neutral-muted">Extracting Subjective, Objective, Action, Plan, scores, and diagnosis.</p>
        </div>

        <div v-else-if="structuredResult" class="rounded-3xl border border-purple-300 bg-surface p-5 shadow-lg space-y-4">
          <!-- Structured Card Header -->
          <div class="flex flex-wrap items-center justify-between gap-2 border-b border-neutral-grey/60 pb-3">
            <div class="flex items-center gap-2">
              <Sparkles class="h-5 w-5 text-purple-600" />
              <h3 class="text-sm font-bold text-navy">✨ AI Structured Summary (Categorized from your words)</h3>
            </div>

            <!-- Diagnosis Tag -->
            <div class="flex items-center gap-2">
              <span class="text-xs font-semibold text-neutral-muted">Diagnosis:</span>
              <input
                type="text"
                v-model="editableDiagnosis"
                placeholder="Suggested diagnosis..."
                class="rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1 text-xs text-navy font-semibold focus:border-purple-500 focus:outline-none"
              />
            </div>
          </div>

          <!-- Extracted Scores Badges -->
          <div class="grid grid-cols-3 gap-3 rounded-2xl bg-neutral-grey/25 p-3 text-center text-xs">
            <div class="rounded-xl bg-surface p-2 shadow-xs">
              <span class="text-neutral-muted text-[11px]">Pain Score</span>
              <div class="mt-1 flex items-center justify-center gap-1">
                <input
                  type="number"
                  v-model.number="editablePain"
                  min="0"
                  max="10"
                  class="w-12 text-center font-bold text-purple-700 border border-neutral-grey/80 rounded"
                />
                <span class="font-bold text-navy">/ 10</span>
              </div>
            </div>

            <div class="rounded-xl bg-surface p-2 shadow-xs">
              <span class="text-neutral-muted text-[11px]">Stiffness Score</span>
              <div class="mt-1 flex items-center justify-center gap-1">
                <input
                  type="number"
                  v-model.number="editableStiffness"
                  min="0"
                  max="10"
                  class="w-12 text-center font-bold text-purple-700 border border-neutral-grey/80 rounded"
                />
                <span class="font-bold text-navy">/ 10</span>
              </div>
            </div>

            <div class="rounded-xl bg-surface p-2 shadow-xs">
              <span class="text-neutral-muted text-[11px]">Lameness Grade</span>
              <div class="mt-1 flex items-center justify-center gap-1">
                <input
                  type="number"
                  v-model.number="editableLameness"
                  min="0"
                  max="5"
                  class="w-12 text-center font-bold text-purple-700 border border-neutral-grey/80 rounded"
                />
                <span class="font-bold text-navy">/ 5</span>
              </div>
            </div>
          </div>

          <!-- Section Review Fields (Editable) -->
          <div class="grid gap-3 sm:grid-cols-2">
            <!-- S - Subjective -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">S · SUBJECTIVE</span>
                <span class="text-[10px] text-purple-700 font-semibold">✨ AI Extracted</span>
              </div>
              <textarea
                v-model="editableSubjective"
                rows="4"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-purple-500 focus:outline-none"
              />
            </div>

            <!-- O - Objective -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">O · OBJECTIVE</span>
                <span class="text-[10px] text-purple-700 font-semibold">✨ AI Extracted</span>
              </div>
              <textarea
                v-model="editableObjective"
                rows="4"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-purple-500 focus:outline-none"
              />
            </div>

            <!-- A - Action -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">A · ACTION</span>
                <span class="text-[10px] text-purple-700 font-semibold">✨ AI Extracted</span>
              </div>
              <textarea
                v-model="editableAction"
                rows="4"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-purple-500 focus:outline-none"
              />
            </div>

            <!-- P - Plan -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">P · PLAN</span>
                <span class="text-[10px] text-purple-700 font-semibold">✨ AI Extracted</span>
              </div>
              <textarea
                v-model="editablePlan"
                rows="4"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-purple-500 focus:outline-none"
              />
            </div>
          </div>

          <!-- Populate Buttons -->
          <div class="flex items-center justify-end gap-2 pt-2 border-t border-neutral-grey/60">
            <button
              type="button"
              class="inline-flex items-center gap-2 rounded-xl border border-purple-300 bg-purple-50 px-4 py-2 text-xs font-bold text-purple-700 hover:bg-purple-100"
              @click="handleApplyStructuredNote('append')"
            >
              Append AI Summary to Note
            </button>

            <button
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl bg-purple-600 px-5 py-2.5 text-xs font-bold text-white shadow-lg shadow-purple-600/30 hover:bg-purple-700 transition-all hover:scale-105"
              @click="handleApplyStructuredNote('replace')"
            >
              <Check class="h-4 w-4" />
              Populate Note with AI Summary
            </button>
          </div>
        </div>
      </div>

      <!-- Modal Footer Actions -->
      <div class="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-neutral-grey/80 pt-4 shrink-0">
        <div class="flex items-center gap-2">
          <button
            type="button"
            class="inline-flex items-center gap-1.5 rounded-xl border border-neutral-grey/80 px-3.5 py-2 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
            @click="handleSaveOffline"
          >
            <Save class="h-3.5 w-3.5" />
            Save Offline Draft
          </button>

          <button
            type="button"
            class="inline-flex items-center gap-1.5 rounded-xl border border-neutral-grey/80 px-3.5 py-2 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
            @click="resetRecording"
          >
            <RotateCcw class="h-3.5 w-3.5" />
            Clear
          </button>
        </div>

        <button
          type="button"
          class="rounded-xl px-4 py-2 text-xs font-semibold text-neutral-muted hover:text-navy"
          @click="emit('close')"
        >
          Close
        </button>
      </div>
    </div>
  </div>
</template>

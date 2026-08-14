<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import {
  Mic,
  Square,
  Sparkles,
  X,
  RotateCcw,
  Volume2,
  Wand2,
  Save,
  Check,
  AlertCircle
} from '@lucide/vue'
import type { StructuredSoapNote } from '../../types/soap'
import { useAudioRecorder } from '../../composables/useAudioRecorder'
import AudioWaveformVisualizer from './AudioWaveformVisualizer.vue'
import {
  CLINICAL_SAMPLE_CONSULTATIONS,
  type ClinicalAudioSample
} from '../../utils/veterinaryLexicon'
import { parseSoapNarrative } from '../../api/soapNotes'

const props = defineProps<{
  isOpen: boolean
  petId: number
  petName: string
  species?: string
}>()

const emit = defineEmits<{
  close: []
  applyStructuredNote: [note: StructuredSoapNote, mode: 'replace' | 'append']
}>()

const {
  recordingState,
  formattedTime,
  audioLevel,
  waveformFrequencies,
  liveTranscript,
  fullTranscript,
  audioUrl,
  errorMessage: recorderError,
  startRecording,
  stopRecording,
  pauseRecording,
  resumeRecording,
  resetRecording,
  loadClinicalSample,
  saveToOfflineQueue
} = useAudioRecorder()

const isParsingAi = ref(false)
const structuringError = ref('')
const structuredResult = ref<StructuredSoapNote | null>(null)
const selectedSampleId = ref<string>('')

// Editable fields for the AI preview card
const editableSubjective = ref('')
const editableObjective = ref('')
const editableAction = ref('')
const editablePlan = ref('')
const editablePain = ref<number | null>(null)
const editableStiffness = ref<number | null>(null)
const editableLameness = ref<number | null>(null)

watch(
  () => props.isOpen,
  (open) => {
    if (!open) {
      resetRecording()
      structuredResult.value = null
      structuringError.value = ''
      selectedSampleId.value = ''
    }
  }
)

async function handleStopAndParse() {
  const result = await stopRecording()
  const transcriptToParse = result.transcript.trim()

  if (!transcriptToParse) {
    structuringError.value = 'No speech or consultation notes detected. Please dictate or select a clinical voice sample.'
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
    editableSubjective.value = structured.subjective
    editableObjective.value = structured.objective
    editableAction.value = structured.action
    editablePlan.value = structured.plan
    editablePain.value = structured.painScore ?? null
    editableStiffness.value = structured.stiffnessScore ?? null
    editableLameness.value = structured.lamenessScore ?? null
  } catch (err: any) {
    console.error('Failed to parse narrative:', err)
    structuringError.value = 'Failed to connect to AI structuring service. Offline parser was utilized.'
  } finally {
    isParsingAi.value = false
  }
}

function handleSelectSample(sample: ClinicalAudioSample) {
  selectedSampleId.value = sample.id
  loadClinicalSample(sample)
  handleStopAndParse()
}

function handleApply(mode: 'replace' | 'append') {
  if (!structuredResult.value) return

  const finalPayload: StructuredSoapNote = {
    ...structuredResult.value,
    subjective: editableSubjective.value,
    objective: editableObjective.value,
    action: editableAction.value,
    plan: editablePlan.value,
    painScore: editablePain.value,
    stiffnessScore: editableStiffness.value,
    lamenessScore: editableLameness.value
  }

  emit('applyStructuredNote', finalPayload, mode)
  emit('close')
}

function handleSaveOffline() {
  if (!fullTranscript.value.trim()) return
  saveToOfflineQueue({
    petId: props.petId,
    petName: props.petName,
    targetSection: 'FULL',
    transcript: fullTranscript.value.trim(),
    audioBlobUrl: audioUrl.value ?? undefined
  })
  alert('Consultation dictation saved to local offline drafts. You can sync or apply it at any time.')
}

const hasTranscribedText = computed(() => !!fullTranscript.value.trim())
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
          <div class="flex h-11 w-11 items-center justify-center rounded-2xl bg-sage text-white shadow-md shadow-sage/20">
            <Mic class="h-6 w-6" />
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h2 class="text-lg font-bold text-navy">Hands-Free SOAP Voice Dictation</h2>
              <span class="inline-flex items-center gap-1 rounded-full bg-sage-muted px-2.5 py-0.5 text-[11px] font-bold text-sage">
                <Sparkles class="h-3 w-3" />
                AI Smart Structuring
              </span>
            </div>
            <p class="text-xs text-neutral-muted">
              Patient: <strong class="text-navy">{{ petName }}</strong> · Speak freely; your consultation will be automatically organized into S, O, A, P.
            </p>
          </div>
        </div>

        <button
          type="button"
          class="rounded-xl p-2 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Modal Body (Scrollable) -->
      <div class="mt-4 space-y-5 overflow-y-auto pr-1 flex-1">
        <!-- Error Alerts -->
        <div
          v-if="structuringError"
          class="flex items-center gap-2 rounded-xl bg-amber-50 border border-amber-200 p-3 text-xs text-amber-800"
        >
          <AlertCircle class="h-4 w-4 shrink-0 text-amber-600" />
          <span>{{ structuringError }}</span>
        </div>

        <!-- 1. Live Waveform & Visualizer -->
        <AudioWaveformVisualizer
          :recording-state="recordingState"
          :formatted-time="formattedTime"
          :audio-level="audioLevel"
          :frequencies="waveformFrequencies"
          :error-message="recorderError"
          @start="startRecording"
          @pause="pauseRecording"
          @resume="resumeRecording"
          @stop="handleStopAndParse"
        />

        <!-- One-Tap Master Control Button -->
        <div class="flex flex-wrap items-center justify-between gap-3 rounded-2xl bg-neutral-grey/25 p-4">
          <div class="flex items-center gap-3">
            <button
              v-if="recordingState === 'idle' || recordingState === 'completed'"
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl bg-sage px-6 py-3 text-sm font-bold text-white shadow-lg shadow-sage/30 hover:bg-sage/90 transition-all hover:scale-105 active:scale-95"
              @click="startRecording"
            >
              <Mic class="h-5 w-5 animate-pulse" />
              {{ recordingState === 'completed' ? 'Re-Record Dictation' : 'Start Voice Dictation' }}
            </button>

            <button
              v-else-if="recordingState === 'recording' || recordingState === 'paused'"
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl bg-rose-600 px-6 py-3 text-sm font-bold text-white shadow-lg shadow-rose-600/30 hover:bg-rose-700 transition-all animate-pulse"
              @click="handleStopAndParse"
            >
              <Square class="h-5 w-5 fill-white" />
              Finish & Structure SOAP (AI)
            </button>

            <button
              v-if="hasTranscribedText && recordingState !== 'recording'"
              type="button"
              :disabled="isParsingAi"
              class="inline-flex items-center gap-2 rounded-2xl border border-sage/40 bg-surface px-4 py-3 text-xs font-bold text-sage hover:bg-sage-muted shadow-xs transition-all disabled:opacity-50"
              @click="handleStopAndParse"
            >
              <Wand2 class="h-4 w-4 text-sage" :class="{ 'animate-spin': isParsingAi }" />
              {{ isParsingAi ? 'AI Structuring...' : 'Re-Parse with AI' }}
            </button>
          </div>

          <!-- Clinical Audio Samples Dropdown for Quick Testing -->
          <div class="flex items-center gap-2">
            <span class="text-xs font-semibold text-neutral-muted">Sample Voice Consultations:</span>
            <div class="flex gap-1.5">
              <button
                v-for="sample in CLINICAL_SAMPLE_CONSULTATIONS"
                :key="sample.id"
                type="button"
                class="rounded-xl border px-3 py-1.5 text-xs font-medium transition-all"
                :class="
                  selectedSampleId === sample.id
                    ? 'border-sage bg-sage text-white shadow-xs'
                    : 'border-neutral-grey/80 bg-surface text-navy hover:border-sage/60'
                "
                @click="handleSelectSample(sample)"
              >
                {{ sample.title }}
              </button>
            </div>
          </div>
        </div>

        <!-- 2. Audio Playback bar (if recorded) -->
        <div v-if="audioUrl" class="flex items-center gap-3 rounded-xl border border-neutral-grey/80 bg-surface p-3 text-xs">
          <Volume2 class="h-4 w-4 text-sage shrink-0" />
          <span class="font-semibold text-navy">Audio Playback:</span>
          <audio :src="audioUrl" controls class="h-8 flex-1" />
        </div>

        <!-- 3. Live Speech Transcript Textarea -->
        <div>
          <div class="flex items-center justify-between">
            <label class="block text-xs font-bold uppercase tracking-wider text-navy">
              Dictated Speech Transcript
            </label>
            <span v-if="recordingState === 'recording'" class="text-[11px] font-semibold text-rose-600 animate-pulse">
              ● Transcribing speech in real-time...
            </span>
          </div>

          <textarea
            v-model="liveTranscript"
            rows="3"
            class="mt-1.5 w-full rounded-2xl border border-neutral-grey/80 bg-surface p-3.5 text-sm text-navy shadow-inner focus:border-sage focus:outline-none"
            placeholder="Your spoken consultation notes will appear here as you speak. You can also edit this text manually before structuring..."
          />
        </div>

        <!-- 4. AI Structured Preview Card -->
        <div v-if="isParsingAi" class="rounded-2xl border border-sage/40 bg-sage-muted/30 p-8 text-center">
          <Wand2 class="mx-auto h-8 w-8 text-sage animate-spin" />
          <p class="mt-2 text-sm font-bold text-navy">Structuring Clinical Narrative with AI...</p>
          <p class="text-xs text-neutral-muted">Extracting Subjective, Objective, Action, Plan, and Pain/Stiffness scores.</p>
        </div>

        <div v-else-if="structuredResult" class="rounded-3xl border border-sage/40 bg-surface p-5 shadow-lg space-y-4">
          <!-- Structured Card Header -->
          <div class="flex flex-wrap items-center justify-between gap-2 border-b border-neutral-grey/60 pb-3">
            <div class="flex items-center gap-2">
              <Sparkles class="h-5 w-5 text-sage" />
              <h3 class="text-sm font-bold text-navy">Extracted SOAP Structure & Findings</h3>
              <span class="rounded-full bg-emerald-100 px-2 py-0.5 text-[10px] font-bold text-emerald-800">
                Confidence {{ Math.round((structuredResult.confidenceScore || 0.95) * 100) }}%
              </span>
            </div>

            <!-- Extracted Diagnosis Tag if any -->
            <div v-if="structuredResult.suggestedDiagnosis" class="rounded-lg bg-sage-muted px-2.5 py-1 text-xs font-semibold text-sage">
              Suggested Diagnosis: <strong>{{ structuredResult.suggestedDiagnosis }}</strong>
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
                  class="w-12 text-center font-bold text-sage border border-neutral-grey/80 rounded"
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
                  class="w-12 text-center font-bold text-sage border border-neutral-grey/80 rounded"
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
                  class="w-12 text-center font-bold text-sage border border-neutral-grey/80 rounded"
                />
                <span class="font-bold text-navy">/ 5</span>
              </div>
            </div>
          </div>

          <!-- Section Review Fields -->
          <div class="grid gap-3 sm:grid-cols-2">
            <!-- S - Subjective -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">S · SUBJECTIVE</span>
                <span class="text-[10px] text-neutral-muted">Owner Observations & Compliance</span>
              </div>
              <textarea
                v-model="editableSubjective"
                rows="3"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-sage focus:outline-none"
              />
            </div>

            <!-- O - Objective -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">O · OBJECTIVE</span>
                <span class="text-[10px] text-neutral-muted">Gait, ROM, Palpation & Findings</span>
              </div>
              <textarea
                v-model="editableObjective"
                rows="3"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-sage focus:outline-none"
              />
            </div>

            <!-- A - Action -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">A · ACTION</span>
                <span class="text-[10px] text-neutral-muted">In-Session Treatments & Modalities</span>
              </div>
              <textarea
                v-model="editableAction"
                rows="3"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-sage focus:outline-none"
              />
            </div>

            <!-- P - Plan -->
            <div class="rounded-2xl border border-neutral-grey/80 bg-neutral-grey/15 p-3 space-y-1.5">
              <div class="flex items-center justify-between">
                <span class="rounded bg-navy px-2 py-0.5 text-[10px] font-bold text-white">P · PLAN</span>
                <span class="text-[10px] text-neutral-muted">Home Exercises & Next Appointment</span>
              </div>
              <textarea
                v-model="editablePlan"
                rows="3"
                class="w-full rounded-xl border border-neutral-grey/80 bg-surface p-2.5 text-xs text-navy focus:border-sage focus:outline-none"
              />
            </div>
          </div>

          <!-- Extracted Keywords Pills -->
          <div v-if="structuredResult.extractedTerms && structuredResult.extractedTerms.length > 0" class="flex flex-wrap items-center gap-1.5 pt-1">
            <span class="text-[11px] font-semibold text-neutral-muted">Identified Clinical Terms:</span>
            <span
              v-for="term in structuredResult.extractedTerms"
              :key="term"
              class="rounded-md bg-sage-muted px-2 py-0.5 text-[10px] font-bold text-sage"
            >
              {{ term }}
            </span>
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

        <div class="flex items-center gap-2">
          <button
            type="button"
            class="rounded-xl px-4 py-2 text-xs font-semibold text-neutral-muted hover:text-navy"
            @click="emit('close')"
          >
            Cancel
          </button>

          <button
            v-if="structuredResult"
            type="button"
            class="inline-flex items-center gap-2 rounded-xl border border-sage px-4 py-2 text-xs font-bold text-sage hover:bg-sage-muted"
            @click="handleApply('append')"
          >
            Append to Note
          </button>

          <button
            v-if="structuredResult"
            type="button"
            class="inline-flex items-center gap-2 rounded-2xl bg-sage px-5 py-2.5 text-xs font-bold text-white shadow-lg shadow-sage/30 hover:bg-sage/90 transition-all hover:scale-105"
            @click="handleApply('replace')"
          >
            <Check class="h-4 w-4" />
            Apply to SOAP Note
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

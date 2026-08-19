<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import {
  X,
  Plus,
  Trash2,
  CheckCircle,
  Share2,
  Import,
  MessageSquareQuote,
  RotateCcw,
  Loader2,
  Sparkles,
  Undo2,
  Mic,
  Volume2,
  Download,
  Copy
} from '@lucide/vue'
import type { CreateSoapNoteRequest, CustomMetricItem, OwnerSubjectiveNote, SoapNote, StructuredSoapNote } from '../../types/soap'
import { fetchOwnerSubjectiveNotes, fetchSoapNotesByPet, createSoapNote, updateSoapNote, parseSoapNarrative } from '../../api/soapNotes'
import { polishSoapSection, getAiConfigStatus, type AiConfigStatus } from '../../api/soapAi'
import { useVoiceSessionStore } from '../../store/voiceSession'
import VoiceDictationButton from '../soap/VoiceDictationButton.vue'
import VoiceSoapDictationModal from '../soap/VoiceSoapDictationModal.vue'

const props = defineProps<{
  petId: number
  petName: string
  isOpen: boolean
  editingNote?: SoapNote | null
}>()

const emit = defineEmits<{
  close: []
  created: [note: any]
  updated: [noteId: number, payload: any]
}>()

const activeTab = ref<'S' | 'O' | 'A' | 'P'>('S')
const voiceSessionStore = useVoiceSessionStore()

function switchTab(tab: 'S' | 'O' | 'A' | 'P') {
  activeTab.value = tab
}

const currentNoteId = ref<number | null>(null)
const autoSaveStatus = ref<string>('')
const isAutoSaving = ref(false)
const rawTranscript = ref<string>('')
const audioUrl = ref<string>('')
const isReSummarizing = ref(false)
const aiSourceNotice = ref<string>('')
const copiedNotice = ref(false)

const sessionDate = ref<string>(new Date().toISOString().slice(0, 10))
const subjective = ref<string>('')
const objective = ref<string>('')
const action = ref<string>('')
const plan = ref<string>('')

// Built-in editable scores
const stiffnessScore = ref<number | null>(3)
const painScore = ref<number | null>(2)
const lamenessScore = ref<number | null>(1)

// Dynamic extensible custom metrics
const customMetrics = ref<CustomMetricItem[]>([
  { name: 'Stifle Extension ROM', value: 130, minScale: 0, maxScale: 180, unitOrDescriptor: 'deg' },
  { name: 'Thigh Circumference', value: 38, minScale: 10, maxScale: 80, unitOrDescriptor: 'cm' },
])

const shareWithOwner = ref(true)
const updateDiagnosis = ref(false)
const diagnosisText = ref('')
const submitting = ref(false)
const errorMessage = ref('')

const ownerNotes = ref<OwnerSubjectiveNote[]>([])
const loadingOwnerNotes = ref(false)

async function loadOwnerNotes() {
  if (!props.petId) return
  loadingOwnerNotes.value = true
  try {
    ownerNotes.value = await fetchOwnerSubjectiveNotes(props.petId)
  } finally {
    loadingOwnerNotes.value = false
  }
}

async function loadPreviousPlan() {
  if (!props.petId || props.editingNote) return
  try {
    const existing = await fetchSoapNotesByPet(props.petId)
    if (existing && existing.length > 0 && existing[0].plan) {
      plan.value = existing[0].plan
    }
  } catch {
    // Ignore error
  }
}

watch(
  () => props.isOpen,
  (val) => {
    if (val) {
      loadOwnerNotes()
      autoSaveStatus.value = ''
      aiSourceNotice.value = ''

      // 1. Check if we have a pending background voice session completed for this pet
      if (voiceSessionStore.pendingReviewNote && voiceSessionStore.pendingReviewNote.petId === props.petId) {
        const pending = voiceSessionStore.pendingReviewNote
        currentNoteId.value = null
        sessionDate.value = new Date().toISOString().slice(0, 10)
        subjective.value = pending.structuredNote.subjective || ''
        objective.value = pending.structuredNote.objective || ''
        action.value = pending.structuredNote.action || ''
        plan.value = pending.structuredNote.plan || ''
        stiffnessScore.value = pending.structuredNote.stiffnessScore ?? 3
        painScore.value = pending.structuredNote.painScore ?? 2
        lamenessScore.value = pending.structuredNote.lamenessScore ?? 1
        if (pending.structuredNote.customMetrics && pending.structuredNote.customMetrics.length > 0) {
          customMetrics.value = pending.structuredNote.customMetrics.map(cm => ({
            name: cm.name,
            value: cm.value,
            minScale: cm.minScale ?? 0,
            maxScale: cm.maxScale ?? 180,
            unitOrDescriptor: cm.unitOrDescriptor
          }))
        }
        if (pending.structuredNote.suggestedDiagnosis) {
          updateDiagnosis.value = true
          diagnosisText.value = pending.structuredNote.suggestedDiagnosis
        }
        rawTranscript.value = pending.rawTranscript || ''
        audioUrl.value = pending.audioUrl || ''
        aiSourceNotice.value = 'Populated automatically from your Voice Session Memo.'
        activeTab.value = 'S'
        voiceSessionStore.clearPendingReview()
      } else if (props.editingNote) {
        currentNoteId.value = props.editingNote.soapNoteId
        sessionDate.value = props.editingNote.sessionDate ? props.editingNote.sessionDate.slice(0, 10) : new Date().toISOString().slice(0, 10)
        subjective.value = props.editingNote.subjective || ''
        objective.value = props.editingNote.objective || ''
        action.value = props.editingNote.action || ''
        plan.value = props.editingNote.plan || ''
        stiffnessScore.value = props.editingNote.stiffnessScore ?? null
        painScore.value = props.editingNote.painScore ?? null
        lamenessScore.value = props.editingNote.lamenessScore ?? null
        customMetrics.value = props.editingNote.customMetrics ? JSON.parse(JSON.stringify(props.editingNote.customMetrics)) : []
        shareWithOwner.value = props.editingNote.isSharedWithOwner ?? true
        rawTranscript.value = props.editingNote.rawTranscript || ''
        audioUrl.value = props.editingNote.audioUrl || ''
        activeTab.value = 'S'
      } else {
        currentNoteId.value = null
        sessionDate.value = new Date().toISOString().slice(0, 10)
        subjective.value = ''
        objective.value = ''
        action.value = ''
        plan.value = ''
        rawTranscript.value = ''
        audioUrl.value = ''
        stiffnessScore.value = 3
        painScore.value = 2
        lamenessScore.value = 1
        customMetrics.value = [
          { name: 'Stifle Extension ROM', value: 130, minScale: 0, maxScale: 180, unitOrDescriptor: 'deg' },
          { name: 'Thigh Circumference', value: 38, minScale: 10, maxScale: 80, unitOrDescriptor: 'cm' },
        ]
        shareWithOwner.value = true
        activeTab.value = 'S'
        loadPreviousPlan()
      }
    }
  },
  { immediate: true }
)

function importOwnerNote(note: OwnerSubjectiveNote) {
  const dateFormatted = new Date(note.noteDate).toLocaleDateString()
  const snippet = `[Owner Update (${note.ownerName} on ${dateFormatted})]: "${note.notes}"`
  if (!subjective.value.trim()) {
    subjective.value = snippet
  } else {
    subjective.value += `\n\n${snippet}`
  }
}

const newMetricName = ref('')
const newMetricValue = ref<number>(0)
const newMetricMin = ref<number>(0)
const newMetricMax = ref<number>(100)
const newMetricUnit = ref('')
const showAddMetric = ref(false)

// Preset exercises & modalities for quick insertion into Action section
const PRESET_EXERCISES_MODALITIES = [
  { name: 'Passive Range of Motion (PROM)', category: 'Mobility', defaultReps: '10 reps x 3 sets' },
  { name: 'Stifle Flexion & Extension', category: 'Mobility', defaultReps: '10 reps x 2 sets' },
  { name: 'Myofascial Soft Tissue Release', category: 'Modality', defaultReps: '15 mins' },
  { name: 'Laser Therapy / Photobiomodulation', category: 'Modality', defaultReps: '4 J/cm²' },
  { name: 'Underwater Treadmill (UWTM)', category: 'Hydro', defaultReps: '15 mins @ 1.2 mph' },
  { name: 'Cavaletti Rails Walkover', category: 'Exercise', defaultReps: '5 laps x 10 rails' },
  { name: 'Airex Balance Disc Standing', category: 'Balance', defaultReps: '30s x 3 sets' },
  { name: 'Sit-to-Stand Squats', category: 'Strength', defaultReps: '10 reps x 2 sets' },
  { name: 'Cryotherapy / Cold Pack', category: 'Modality', defaultReps: '10 mins' },
]

// Preset treatment plans & protocols for quick insertion into Plan section
const PRESET_TREATMENT_PLANS = [
  { name: 'Phase 1: Reduce Pain & Inflammation Protocol', frequency: '3x weekly sessions' },
  { name: 'Phase 2: Restore ROM & Normal Gait Protocol', frequency: '2x weekly sessions' },
  { name: 'Phase 3: Build Muscle Strength & Core Stability', frequency: '1-2x weekly sessions' },
  { name: 'Phase 4: Home Maintenance & Prevention', frequency: 'Re-evaluate in 4 weeks' },
  { name: 'Home Rehab Program: Daily PROM & Balance Disc', frequency: '2x daily at home' },
  { name: 'Hydrotherapy Schedule: UWTM Sessions', frequency: '2x weekly sessions' },
]

function insertExerciseToAction(item: { name: string; defaultReps: string }) {
  const line = `• ${item.name} (${item.defaultReps})`
  if (!action.value.trim()) {
    action.value = line
  } else {
    action.value += `\n${line}`
  }
}

function insertPlanToPlan(item: { name: string; frequency: string }) {
  const line = `• ${item.name} [Target Frequency: ${item.frequency}]`
  if (!plan.value.trim()) {
    plan.value = line
  } else {
    plan.value += `\n${line}`
  }
}

function addCustomMetric() {
  if (!newMetricName.value.trim()) return
  customMetrics.value.push({
    name: newMetricName.value.trim(),
    value: newMetricValue.value,
    minScale: newMetricMin.value,
    maxScale: newMetricMax.value,
    unitOrDescriptor: newMetricUnit.value.trim() || undefined,
  })
  newMetricName.value = ''
  newMetricValue.value = 0
  newMetricUnit.value = ''
  showAddMetric.value = false
}

function removeCustomMetric(index: number) {
  customMetrics.value.splice(index, 1)
}

function formatPausePunctuation(existing: string, chunk: string, pauseSeconds: number = 0): string {
  const trimmed = chunk.trim()
  if (!trimmed) return existing
  if (!existing.trim()) {
    return trimmed.charAt(0).toUpperCase() + trimmed.slice(1)
  }
  if (pauseSeconds >= 2.0 && !/[.!?]$/.test(existing.trim())) {
    return `${existing.trim()}. ${trimmed.charAt(0).toUpperCase() + trimmed.slice(1)}`
  }
  return `${existing.trim()} ${trimmed}`
}

function handleSubjectiveDictationChunk(chunk: string, pauseSeconds: number = 0) {
  subjective.value = formatPausePunctuation(subjective.value, chunk, pauseSeconds)
  autoSaveNote()
}

function handleObjectiveDictationChunk(chunk: string, pauseSeconds: number = 0) {
  objective.value = formatPausePunctuation(objective.value, chunk, pauseSeconds)
  autoSaveNote()
}

function handleActionDictationChunk(chunk: string, pauseSeconds: number = 0) {
  action.value = formatPausePunctuation(action.value, chunk, pauseSeconds)
  autoSaveNote()
}

function handlePlanDictationChunk(chunk: string, pauseSeconds: number = 0) {
  plan.value = formatPausePunctuation(plan.value, chunk, pauseSeconds)
  autoSaveNote()
}

const transcriptionEngine = ref<'browser' | 'cloud'>('browser')
const aiConfig = ref<AiConfigStatus | null>(null)
const polishingSection = ref<string | null>(null)
const sectionHistory = ref<Record<string, string>>({})
const lastCorrections = ref<Record<string, string[]>>({})

async function checkAiConfig() {
  aiConfig.value = await getAiConfigStatus()
}

onMounted(() => {
  checkAiConfig()
  const savedEngine = localStorage.getItem('movewell_dictation_engine')
  if (savedEngine === 'cloud' || savedEngine === 'browser') {
    transcriptionEngine.value = savedEngine
  }
})

function setTranscriptionEngine(engine: 'browser' | 'cloud') {
  transcriptionEngine.value = engine
  localStorage.setItem('movewell_dictation_engine', engine)
}

async function handlePolishSection(sectionKey: 'Subjective' | 'Objective' | 'Action' | 'Plan' | 'Diagnosis') {
  let targetRef = subjective
  if (sectionKey === 'Objective') targetRef = objective
  else if (sectionKey === 'Action') targetRef = action
  else if (sectionKey === 'Plan') targetRef = plan
  else if (sectionKey === 'Diagnosis') targetRef = diagnosisText

  const currentVal = targetRef.value.trim()
  if (!currentVal) return

  // Save current text for 1-click undo/revert
  sectionHistory.value[sectionKey] = currentVal
  polishingSection.value = sectionKey

  try {
    const res = await polishSoapSection({
      sectionName: sectionKey,
      rawText: currentVal,
      petName: props.petName,
      species: 'Canine',
      condition: diagnosisText.value || 'Rehab Assessment'
    })

    if (res && res.polishedText) {
      targetRef.value = res.polishedText
      if (res.correctionsMade && res.correctionsMade.length > 0) {
        lastCorrections.value[sectionKey] = res.correctionsMade
      }
      autoSaveNote()
    }
  } catch (err) {
    console.warn('AI Polish failed:', err)
  } finally {
    polishingSection.value = null
  }
}

function revertPolishedSection(sectionKey: string) {
  if (sectionHistory.value[sectionKey]) {
    if (sectionKey === 'Subjective') subjective.value = sectionHistory.value[sectionKey]
    else if (sectionKey === 'Objective') objective.value = sectionHistory.value[sectionKey]
    else if (sectionKey === 'Action') action.value = sectionHistory.value[sectionKey]
    else if (sectionKey === 'Plan') plan.value = sectionHistory.value[sectionKey]
    else if (sectionKey === 'Diagnosis') diagnosisText.value = sectionHistory.value[sectionKey]

    delete sectionHistory.value[sectionKey]
    delete lastCorrections.value[sectionKey]
    autoSaveNote()
  }
}

const showVoiceDictationModal = ref(false)

function handleApplyStructuredNote(note: StructuredSoapNote, mode: 'replace' | 'append') {
  if (mode === 'replace') {
    if (note.subjective) subjective.value = note.subjective
    if (note.objective) objective.value = note.objective
    if (note.action) action.value = note.action
    if (note.plan) plan.value = note.plan
    if (note.painScore !== null && note.painScore !== undefined) painScore.value = note.painScore
    if (note.stiffnessScore !== null && note.stiffnessScore !== undefined) stiffnessScore.value = note.stiffnessScore
    if (note.lamenessScore !== null && note.lamenessScore !== undefined) lamenessScore.value = note.lamenessScore
    if (note.suggestedDiagnosis) {
      updateDiagnosis.value = true
      diagnosisText.value = note.suggestedDiagnosis
    }
    if (note.customMetrics && note.customMetrics.length > 0) {
      customMetrics.value = note.customMetrics.map(cm => ({
        name: cm.name,
        value: cm.value,
        minScale: cm.minScale ?? 0,
        maxScale: cm.maxScale ?? 180,
        unitOrDescriptor: cm.unitOrDescriptor
      }))
    }
  } else {
    // Append mode
    if (note.subjective) subjective.value = (subjective.value ? subjective.value + '\n\n' : '') + note.subjective
    if (note.objective) objective.value = (objective.value ? objective.value + '\n\n' : '') + note.objective
    if (note.action) action.value = (action.value ? action.value + '\n\n' : '') + note.action
    if (note.plan) plan.value = (plan.value ? plan.value + '\n\n' : '') + note.plan
    if (note.painScore !== null && note.painScore !== undefined) painScore.value = note.painScore
    if (note.stiffnessScore !== null && note.stiffnessScore !== undefined) stiffnessScore.value = note.stiffnessScore
    if (note.lamenessScore !== null && note.lamenessScore !== undefined) lamenessScore.value = note.lamenessScore
    if (note.suggestedDiagnosis) {
      updateDiagnosis.value = true
      diagnosisText.value = note.suggestedDiagnosis
    }
  }

  // Trigger auto-save immediately to persist generated structure
  autoSaveNote()
}

async function handleReSummarizeFromRaw() {
  if (!rawTranscript.value.trim()) return
  isReSummarizing.value = true
  try {
    const res = await parseSoapNarrative({
      transcript: rawTranscript.value.trim(),
      petId: props.petId,
      petName: props.petName,
      species: 'Canine'
    })
    if (res) {
      if (res.subjective) subjective.value = res.subjective
      if (res.objective) objective.value = res.objective
      if (res.action) action.value = res.action
      if (res.plan) plan.value = res.plan
      if (res.painScore !== null && res.painScore !== undefined) painScore.value = res.painScore
      if (res.stiffnessScore !== null && res.stiffnessScore !== undefined) stiffnessScore.value = res.stiffnessScore
      if (res.lamenessScore !== null && res.lamenessScore !== undefined) lamenessScore.value = res.lamenessScore
      if (res.customMetrics && res.customMetrics.length > 0) {
        customMetrics.value = res.customMetrics.map(cm => ({
          name: cm.name,
          value: cm.value,
          minScale: cm.minScale ?? 0,
          maxScale: cm.maxScale ?? 180,
          unitOrDescriptor: cm.unitOrDescriptor
        }))
      }
      aiSourceNotice.value = 'SOAP Note re-summarized with AI from your raw transcript.'
      activeTab.value = 'S'
      autoSaveNote()
    }
  } catch (err) {
    console.warn('Re-summarize failed:', err)
  } finally {
    isReSummarizing.value = false
  }
}

function copyRawTranscript() {
  if (!rawTranscript.value) return
  navigator.clipboard.writeText(rawTranscript.value)
  copiedNotice.value = true
  setTimeout(() => { copiedNotice.value = false }, 2500)
}

function handleInsertRawTranscript(rawText: string, targetSection: 'Subjective' | 'Objective' | 'Action' | 'Plan' | 'All') {
  if (!rawText.trim()) return

  if (targetSection === 'Subjective') {
    subjective.value = (subjective.value ? subjective.value + '\n\n' : '') + rawText
    switchTab('S')
  } else if (targetSection === 'Objective') {
    objective.value = (objective.value ? objective.value + '\n\n' : '') + rawText
    switchTab('O')
  } else if (targetSection === 'Action') {
    action.value = (action.value ? action.value + '\n\n' : '') + rawText
    switchTab('A')
  } else if (targetSection === 'Plan') {
    plan.value = (plan.value ? plan.value + '\n\n' : '') + rawText
    switchTab('P')
  } else if (targetSection === 'All') {
    subjective.value = (subjective.value ? subjective.value + '\n\n' : '') + rawText
    switchTab('S')
  }

  autoSaveNote()
}

async function autoSaveNote() {
  if (!props.petId) return
  // Don't auto-save if everything is completely empty
  if (!subjective.value.trim() && !objective.value.trim() && !action.value.trim() && !plan.value.trim()) {
    return
  }

  isAutoSaving.value = true
  try {
    const payload: CreateSoapNoteRequest = {
      sessionDate: sessionDate.value,
      subjective: subjective.value,
      objective: objective.value,
      action: action.value,
      plan: plan.value,
      stiffnessScore: stiffnessScore.value,
      painScore: painScore.value,
      lamenessScore: lamenessScore.value,
      customMetrics: customMetrics.value,
      shareWithOwner: shareWithOwner.value,
      diagnosisUpdate: updateDiagnosis.value && diagnosisText.value.trim() ? diagnosisText.value.trim() : undefined,
      audioUrl: audioUrl.value || undefined,
      rawTranscript: rawTranscript.value || undefined,
    }

    if (currentNoteId.value) {
      await updateSoapNote(currentNoteId.value, payload)
    } else {
      const created = await createSoapNote(props.petId, payload)
      currentNoteId.value = created.soapNoteId
    }

    const now = new Date()
    const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    autoSaveStatus.value = `Auto-saved at ${timeStr}`
  } catch (err) {
    console.warn('Auto-save SOAP note error:', err)
  } finally {
    isAutoSaving.value = false
  }
}

async function handleSubmit() {
  if (!subjective.value.trim() && !objective.value.trim() && !action.value.trim() && !plan.value.trim() && !rawTranscript.value.trim()) {
    errorMessage.value = 'Please complete at least one section of the SOAP note or record a transcript.'
    return
  }

  submitting.value = true
  errorMessage.value = ''

  const payload: CreateSoapNoteRequest = {
    sessionDate: sessionDate.value,
    subjective: subjective.value,
    objective: objective.value,
    action: action.value,
    plan: plan.value,
    stiffnessScore: stiffnessScore.value,
    painScore: painScore.value,
    lamenessScore: lamenessScore.value,
    customMetrics: customMetrics.value,
    shareWithOwner: shareWithOwner.value,
    diagnosisUpdate: updateDiagnosis.value && diagnosisText.value.trim() ? diagnosisText.value.trim() : undefined,
    audioUrl: audioUrl.value || undefined,
    rawTranscript: rawTranscript.value || undefined,
  }

  try {
    const targetId = currentNoteId.value || (props.editingNote ? props.editingNote.soapNoteId : null)
    if (targetId) {
      const updated = await updateSoapNote(targetId, payload)
      emit('updated', targetId, updated)
    } else {
      const created = await createSoapNote(props.petId, payload)
      emit('created', created)
    }
    emit('close')
  } catch (err: any) {
    console.error('Save SOAP note error:', err)
    errorMessage.value = err.message || 'Failed to save SOAP note.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-navy/60 p-4 backdrop-blur-sm"
  >
    <div class="relative w-full max-w-3xl rounded-2xl bg-surface p-6 shadow-2xl">
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-4">
        <div>
          <div class="flex items-center gap-2">
            <h2 class="text-xl font-bold text-navy">{{ editingNote ? 'Edit Clinical SOAP Assessment' : 'New Clinical SOAP Assessment' }}</h2>
            <span
              v-if="isAutoSaving"
              class="inline-flex items-center gap-1 rounded-full bg-neutral-grey/50 px-2.5 py-0.5 text-xs text-neutral-muted"
            >
              <Loader2 class="h-3 w-3 animate-spin text-sage" />
              Saving...
            </span>
            <span
              v-else-if="autoSaveStatus"
              class="inline-flex items-center gap-1 rounded-full bg-sage-muted/70 px-2.5 py-0.5 text-xs font-semibold text-sage animate-fade-in"
            >
              <CheckCircle class="h-3 w-3 text-sage" />
              {{ autoSaveStatus }}
            </span>
          </div>
          <p class="text-xs text-neutral-muted">Patient: {{ petName }} · Date: {{ sessionDate }}</p>
        </div>

        <div class="flex items-center gap-3">
          <!-- Transcription Engine Mode Toggle -->
          <div class="flex items-center gap-1 rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-0.5 text-[11px]">
            <button
              type="button"
              class="rounded-lg px-2.5 py-1 font-bold transition-all"
              :class="transcriptionEngine === 'browser' ? 'bg-surface text-navy shadow-xs' : 'text-neutral-muted hover:text-navy'"
              title="Instant zero-latency in-browser speech recognition ($0 Cost)"
              @click="setTranscriptionEngine('browser')"
            >
              ⚡ Browser STT
            </button>
            <button
              type="button"
              class="rounded-lg px-2.5 py-1 font-bold transition-all"
              :class="transcriptionEngine === 'cloud' ? 'bg-surface text-navy shadow-xs' : 'text-neutral-muted hover:text-navy'"
              title="High-precision Cloud AI Audio transcription (Gemini)"
              @click="setTranscriptionEngine('cloud')"
            >
              ☁️ Cloud AI Audio
            </button>
          </div>

          <button
            type="button"
            class="rounded-lg p-1.5 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
            @click="emit('close')"
          >
            <X class="h-5 w-5" />
          </button>
        </div>
      </div>

      <!-- SOAP Tabs -->
      <div class="mt-4 flex gap-2 border-b border-neutral-grey/80 pb-2">
        <button
          type="button"
          class="flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-colors"
          :class="
            activeTab === 'S'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-neutral-grey/40 text-navy hover:bg-neutral-grey/70'
          "
          @click="switchTab('S')"
        >
          <span class="rounded bg-white/20 px-1.5 py-0.5 text-[10px]">S</span>
          Subjective
        </button>

        <button
          type="button"
          class="flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-colors"
          :class="
            activeTab === 'O'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-neutral-grey/40 text-navy hover:bg-neutral-grey/70'
          "
          @click="switchTab('O')"
        >
          <span class="rounded bg-white/20 px-1.5 py-0.5 text-[10px]">O</span>
          Objective & Metrics
        </button>

        <button
          type="button"
          class="flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-colors"
          :class="
            activeTab === 'A'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-neutral-grey/40 text-navy hover:bg-neutral-grey/70'
          "
          @click="switchTab('A')"
        >
          <span class="rounded bg-white/20 px-1.5 py-0.5 text-[10px]">A</span>
          Action & Treatment
        </button>

        <button
          type="button"
          class="flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-colors"
          :class="
            activeTab === 'P'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-neutral-grey/40 text-navy hover:bg-neutral-grey/70'
          "
          @click="switchTab('P')"
        >
          <span class="rounded bg-white/20 px-1.5 py-0.5 text-[10px]">P</span>
          Plan & Follow-up
        </button>

        <!-- Full Session AI Dictation Master Button -->
        <button
          type="button"
          class="ml-auto inline-flex items-center gap-1.5 rounded-xl border border-purple-300 bg-purple-50 px-3.5 py-1.5 text-xs font-bold text-purple-700 hover:bg-purple-100 shadow-xs transition-all hover:scale-105 active:scale-95"
          title="Dictate a full consultation session to auto-fill Subjective, Objective, Action, Plan, and Scores with AI"
          @click="showVoiceDictationModal = true"
        >
          <Mic class="h-3.5 w-3.5 text-purple-600 animate-pulse" />
          <span>Full SOAP Note</span>
        </button>
      </div>

      <!-- AI Source / Audio Attached Notice Banner -->
      <div
        v-if="aiSourceNotice || audioUrl || rawTranscript"
        class="mt-3 flex flex-wrap items-center justify-between gap-2 rounded-xl border border-purple-200 bg-purple-50/70 p-3 text-xs text-purple-900"
      >
        <div class="flex items-center gap-2">
          <Sparkles class="h-4 w-4 shrink-0 text-purple-600" />
          <span>
            <strong v-if="aiSourceNotice">{{ aiSourceNotice }}</strong>
            <span v-else>Voice Session memo attached to this assessment.</span>
          </span>
        </div>
        <button
          type="button"
          class="inline-flex items-center gap-1 font-bold text-purple-700 hover:underline text-[11px]"
          @click="activeTab === 'RAW' ? switchTab('S') : switchTab('RAW')"
        >
          {{ activeTab === 'RAW' ? '← View Structured SOAP Note' : 'View Raw Speech & Audio Memo →' }}
        </button>
      </div>

      <!-- Tab Contents -->
      <form @submit.prevent="handleSubmit" class="mt-4 space-y-4">
        <!-- Error Alert -->
        <div v-if="errorMessage" class="rounded-xl bg-danger-red/10 p-3 text-xs font-semibold text-danger-red">
          {{ errorMessage }}
        </div>

        <!-- S - SUBJECTIVE -->
        <div v-show="activeTab === 'S'" class="space-y-4">
          <!-- Recent Owner Submitted Notes Panel -->
          <div v-if="ownerNotes.length > 0" class="rounded-xl border border-sage/40 bg-sage-muted/20 p-4">
            <div class="flex items-center justify-between">
              <h4 class="flex items-center gap-1.5 text-xs font-bold text-navy">
                <MessageSquareQuote class="h-4 w-4 text-sage" />
                Recent Notes Submitted by Owner
              </h4>
              <span class="text-[10px] font-bold text-sage bg-sage/10 px-2 py-0.5 rounded-full">
                {{ ownerNotes.length }} note(s) available
              </span>
            </div>
            <div class="mt-2.5 space-y-2 max-h-40 overflow-y-auto pr-1">
              <div
                v-for="note in ownerNotes"
                :key="note.ownerSubjectiveNoteId"
                class="flex items-start justify-between gap-3 rounded-lg border border-neutral-grey/60 bg-surface p-2.5 text-xs"
              >
                <div>
                  <div class="flex items-center gap-2">
                    <span class="font-bold text-navy">{{ note.ownerName }}</span>
                    <span class="text-[10px] text-neutral-muted">{{ new Date(note.noteDate).toLocaleDateString() }}</span>
                  </div>
                  <p class="mt-1 text-navy leading-normal italic">"{{ note.notes }}"</p>
                </div>
                <button
                  type="button"
                  class="inline-flex shrink-0 items-center gap-1 rounded-lg border border-sage/40 bg-sage-muted px-2.5 py-1 text-[11px] font-bold text-sage hover:bg-sage hover:text-white"
                  @click="importOwnerNote(note)"
                >
                  <Import class="h-3 w-3" />
                  Import
                </button>
              </div>
            </div>
          </div>

          <div>
            <div class="flex items-center justify-between">
              <label class="block text-xs font-semibold text-navy">
                Subjective Findings (Owner Observations & Feedback)
              </label>
              <div class="flex items-center gap-1.5">
                <button
                  v-if="subjective.trim()"
                  type="button"
                  :disabled="polishingSection === 'Subjective'"
                  class="inline-flex items-center gap-1 rounded-lg border border-purple-200 bg-purple-50 px-2 py-1 text-xs font-bold text-purple-700 hover:bg-purple-100 hover:border-purple-300 transition-all disabled:opacity-50"
                  title="Contextually correct medical terms and polish notes with AI"
                  @click="handlePolishSection('Subjective')"
                >
                  <Loader2 v-if="polishingSection === 'Subjective'" class="h-3 w-3 animate-spin text-purple-600" />
                  <Sparkles v-else class="h-3 w-3 text-purple-600" />
                  {{ polishingSection === 'Subjective' ? 'Polishing...' : 'AI Polish' }}
                </button>
                <button
                  v-if="sectionHistory['Subjective']"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-1.5 py-1 text-[11px] font-semibold text-neutral-muted hover:text-navy"
                  title="Revert back to pre-polished text"
                  @click="revertPolishedSection('Subjective')"
                >
                  <Undo2 class="h-3 w-3" />
                  Revert
                </button>
                <button
                  v-if="subjective"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1 text-xs text-neutral-muted hover:border-danger-red/40 hover:bg-danger-red/10 hover:text-danger-red transition-all"
                  title="Clear Subjective text"
                  @click="subjective = ''"
                >
                  <RotateCcw class="h-3 w-3" />
                  Clear
                </button>
                <VoiceDictationButton
                  section-label="Subjective"
                  button-text="Dictate Subjective"
                  :engine="transcriptionEngine"
                  :pet-name="petName"
                  species="Canine"
                  @transcript-chunk="handleSubjectiveDictationChunk"
                  @dictation-finished="autoSaveNote"
                />
              </div>
            </div>
            <p class="mt-0.5 text-[11px] text-neutral-muted">
              Record changes reported by the owner, home exercise compliance, energy/appetite levels, and any concerns.
            </p>
            <div v-if="lastCorrections['Subjective']" class="mt-1 flex flex-wrap gap-1">
              <span
                v-for="corr in lastCorrections['Subjective']"
                :key="corr"
                class="inline-flex items-center gap-1 rounded-md bg-purple-100/70 px-1.5 py-0.5 text-[10px] font-semibold text-purple-800"
              >
                ✨ {{ corr }}
              </span>
            </div>
            <textarea
              v-model="subjective"
              rows="5"
              class="mt-2 w-full rounded-xl border border-neutral-grey/80 bg-surface p-3 text-sm text-navy focus:border-sage focus:outline-none"
              placeholder="e.g. Owner reports Buddy completed 80% of exercises. Noticeably less stiff in mornings..."
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-navy">Session Date</label>
            <input
              type="date"
              v-model="sessionDate"
              class="mt-1 rounded-xl border border-neutral-grey/80 bg-surface px-3 py-2 text-sm text-navy focus:border-sage focus:outline-none"
            />
          </div>
        </div>

        <!-- O - OBJECTIVE & METRICS -->
        <div v-show="activeTab === 'O'" class="space-y-5">
          <div>
            <div class="flex items-center justify-between">
              <label class="block text-xs font-semibold text-navy">Objective Examination Notes</label>
              <div class="flex items-center gap-1.5">
                <button
                  v-if="objective.trim()"
                  type="button"
                  :disabled="polishingSection === 'Objective'"
                  class="inline-flex items-center gap-1 rounded-lg border border-purple-200 bg-purple-50 px-2 py-1 text-xs font-bold text-purple-700 hover:bg-purple-100 hover:border-purple-300 transition-all disabled:opacity-50"
                  title="Contextually correct medical terms and polish notes with AI"
                  @click="handlePolishSection('Objective')"
                >
                  <Loader2 v-if="polishingSection === 'Objective'" class="h-3 w-3 animate-spin text-purple-600" />
                  <Sparkles v-else class="h-3 w-3 text-purple-600" />
                  {{ polishingSection === 'Objective' ? 'Polishing...' : 'AI Polish' }}
                </button>
                <button
                  v-if="sectionHistory['Objective']"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-1.5 py-1 text-[11px] font-semibold text-neutral-muted hover:text-navy"
                  title="Revert back to pre-polished text"
                  @click="revertPolishedSection('Objective')"
                >
                  <Undo2 class="h-3 w-3" />
                  Revert
                </button>
                <button
                  v-if="objective"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1 text-xs text-neutral-muted hover:border-danger-red/40 hover:bg-danger-red/10 hover:text-danger-red transition-all"
                  title="Clear Objective text"
                  @click="objective = ''"
                >
                  <RotateCcw class="h-3 w-3" />
                  Clear
                </button>
                <VoiceDictationButton
                  section-label="Objective"
                  button-text="Dictate Objective"
                  :engine="transcriptionEngine"
                  :pet-name="petName"
                  species="Canine"
                  @transcript-chunk="handleObjectiveDictationChunk"
                  @dictation-finished="autoSaveNote"
                />
              </div>
            </div>
            <div v-if="lastCorrections['Objective']" class="mt-1 flex flex-wrap gap-1">
              <span
                v-for="corr in lastCorrections['Objective']"
                :key="corr"
                class="inline-flex items-center gap-1 rounded-md bg-purple-100/70 px-1.5 py-0.5 text-[10px] font-semibold text-purple-800"
              >
                ✨ {{ corr }}
              </span>
            </div>
            <textarea
              v-model="objective"
              rows="3"
              class="mt-1 w-full rounded-xl border border-neutral-grey/80 bg-surface p-3 text-sm text-navy focus:border-sage focus:outline-none"
              placeholder="e.g. Palpation soreness over right stifling joint, reduced stride length, muscle atrophy..."
            />
          </div>

          <!-- Primary Scores (Editable Sliders/Ratings) -->
          <div class="rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-4 space-y-4">
            <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Clinical Rating Scales (Editable)</h4>
            
            <div class="grid gap-4 sm:grid-cols-3">
              <div>
                <div class="flex justify-between text-xs">
                  <span class="font-semibold text-navy">Pain Score</span>
                  <span class="font-bold text-sage">{{ painScore }}/10</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="10"
                  v-model.number="painScore"
                  class="mt-2 w-full accent-sage"
                />
              </div>

              <div>
                <div class="flex justify-between text-xs">
                  <span class="font-semibold text-navy">Stiffness Score</span>
                  <span class="font-bold text-sage">{{ stiffnessScore }}/10</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="10"
                  v-model.number="stiffnessScore"
                  class="mt-2 w-full accent-sage"
                />
              </div>

              <div>
                <div class="flex justify-between text-xs">
                  <span class="font-semibold text-navy">Lameness Grade</span>
                  <span class="font-bold text-sage">{{ lamenessScore }}/5</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="5"
                  v-model.number="lamenessScore"
                  class="mt-2 w-full accent-sage"
                />
              </div>
            </div>
          </div>

          <!-- Dynamic Extensible Custom Metrics -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface p-4">
            <div class="flex items-center justify-between">
              <div>
                <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Custom Clinical Metrics</h4>
                <p class="text-[11px] text-neutral-muted">Add ROM, girth measurements, or custom rating scales.</p>
              </div>
              <button
                type="button"
                class="inline-flex items-center gap-1.5 rounded-lg border border-sage/40 bg-sage-muted px-2.5 py-1 text-xs font-bold text-sage hover:bg-sage hover:text-white"
                @click="showAddMetric = !showAddMetric"
              >
                <Plus class="h-3.5 w-3.5" />
                Add Metric
              </button>
            </div>

            <!-- New Metric Form -->
            <div v-if="showAddMetric" class="mt-3 grid gap-3 rounded-xl bg-neutral-grey/30 p-3 sm:grid-cols-4">
              <div class="relative flex items-center">
                <input
                  type="text"
                  v-model="newMetricName"
                  placeholder="Metric Name (e.g. ROM)"
                  class="w-full rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 pr-8 text-xs text-navy"
                />
                <div class="absolute right-1">
                  <VoiceDictationButton
                    section-label="Metric Name"
                    button-text="Dictate"
                    :compact="true"
                    @transcript-chunk="(chunk, pause) => newMetricName = formatPausePunctuation(newMetricName, chunk, pause)"
                    @dictation-finished="autoSaveNote"
                  />
                </div>
              </div>
              <input
                type="number"
                v-model.number="newMetricValue"
                placeholder="Value"
                class="rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 text-xs text-navy"
              />
              <input
                type="text"
                v-model="newMetricUnit"
                placeholder="Unit (deg, cm, %)"
                class="rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 text-xs text-navy"
              />
              <button
                type="button"
                class="rounded-lg bg-sage py-1.5 text-xs font-bold text-white hover:bg-sage/90"
                @click="addCustomMetric"
              >
                Confirm Add
              </button>
            </div>

            <!-- Custom Metrics List -->
            <ul class="mt-3 divide-y divide-neutral-grey/60">
              <li
                v-for="(metric, idx) in customMetrics"
                :key="idx"
                class="flex items-center justify-between py-2 text-xs"
              >
                <div class="flex items-center gap-2">
                  <span class="font-semibold text-navy">{{ metric.name }}:</span>
                  <input
                    type="number"
                    v-model.number="metric.value"
                    class="w-20 rounded border border-neutral-grey/80 bg-surface px-2 py-0.5 text-navy"
                  />
                  <span class="text-neutral-muted">{{ metric.unitOrDescriptor ?? '' }}</span>
                </div>
                <button
                  type="button"
                  class="text-neutral-muted hover:text-danger-red"
                  @click="removeCustomMetric(idx)"
                >
                  <Trash2 class="h-4 w-4" />
                </button>
              </li>
            </ul>
          </div>
        </div>

        <!-- A - ACTION & TREATMENT -->
        <div v-show="activeTab === 'A'" class="space-y-4">
          <!-- Quick Select Exercises & Modalities Library -->
          <div class="rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-3.5 space-y-2.5">
            <div class="flex items-center justify-between">
              <span class="text-xs font-bold text-navy uppercase tracking-wider">Quick Select Exercises & Modalities</span>
              <span class="text-[11px] text-neutral-muted">Click any item to append to Action notes</span>
            </div>

            <div class="flex flex-wrap gap-1.5">
              <button
                v-for="item in PRESET_EXERCISES_MODALITIES"
                :key="item.name"
                type="button"
                class="inline-flex items-center gap-1 rounded-lg border border-sage/40 bg-surface px-2.5 py-1 text-xs font-medium text-navy hover:bg-sage hover:text-white transition-colors"
                @click="insertExerciseToAction(item)"
              >
                <Plus class="h-3 w-3 text-sage group-hover:text-white" />
                {{ item.name }}
              </button>
            </div>
          </div>

          <div>
            <div class="flex items-center justify-between">
              <label class="block text-xs font-semibold text-navy">
                Action (Treatment Modalities & In-Session Exercises)
              </label>
              <div class="flex items-center gap-1.5">
                <button
                  v-if="action.trim()"
                  type="button"
                  :disabled="polishingSection === 'Action'"
                  class="inline-flex items-center gap-1 rounded-lg border border-purple-200 bg-purple-50 px-2 py-1 text-xs font-bold text-purple-700 hover:bg-purple-100 hover:border-purple-300 transition-all disabled:opacity-50"
                  title="Contextually correct medical terms and polish notes with AI"
                  @click="handlePolishSection('Action')"
                >
                  <Loader2 v-if="polishingSection === 'Action'" class="h-3 w-3 animate-spin text-purple-600" />
                  <Sparkles v-else class="h-3 w-3 text-purple-600" />
                  {{ polishingSection === 'Action' ? 'Polishing...' : 'AI Polish' }}
                </button>
                <button
                  v-if="sectionHistory['Action']"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-1.5 py-1 text-[11px] font-semibold text-neutral-muted hover:text-navy"
                  title="Revert back to pre-polished text"
                  @click="revertPolishedSection('Action')"
                >
                  <Undo2 class="h-3 w-3" />
                  Revert
                </button>
                <button
                  v-if="action"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1 text-xs text-neutral-muted hover:border-danger-red/40 hover:bg-danger-red/10 hover:text-danger-red transition-all"
                  title="Clear Action text"
                  @click="action = ''"
                >
                  <RotateCcw class="h-3 w-3" />
                  Clear
                </button>
                <VoiceDictationButton
                  section-label="Action"
                  button-text="Dictate Action"
                  :engine="transcriptionEngine"
                  :pet-name="petName"
                  species="Canine"
                  @transcript-chunk="handleActionDictationChunk"
                  @dictation-finished="autoSaveNote"
                />
              </div>
            </div>
            <p class="mt-0.5 text-[11px] text-neutral-muted">
              Document manual therapies, laser/hydro treatments, specific areas treated, and in-session exercise reps.
            </p>
            <div v-if="lastCorrections['Action']" class="mt-1 flex flex-wrap gap-1">
              <span
                v-for="corr in lastCorrections['Action']"
                :key="corr"
                class="inline-flex items-center gap-1 rounded-md bg-purple-100/70 px-1.5 py-0.5 text-[10px] font-semibold text-purple-800"
              >
                ✨ {{ corr }}
              </span>
            </div>
            <textarea
              v-model="action"
              rows="5"
              class="mt-2 w-full rounded-xl border border-neutral-grey/80 bg-surface p-3 text-sm text-navy focus:border-sage focus:outline-none"
              placeholder="e.g. Myofascial release (15 mins) on lumbar spine. Laser therapy to right stifle (4J/cm2). Cavaletti rails (3x10 reps)..."
            />
          </div>
        </div>

        <!-- P - PLAN & FOLLOW-UP -->
        <div v-show="activeTab === 'P'" class="space-y-4">
          <!-- Quick Select Treatment Plans & Protocols -->
          <div class="rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-3.5 space-y-2.5">
            <div class="flex items-center justify-between">
              <span class="text-xs font-bold text-navy uppercase tracking-wider">Quick Select Treatment Plans & Protocols</span>
              <span class="text-[11px] text-neutral-muted">Click any protocol to append to Plan notes</span>
            </div>

            <div class="flex flex-wrap gap-1.5">
              <button
                v-for="item in PRESET_TREATMENT_PLANS"
                :key="item.name"
                type="button"
                class="inline-flex items-center gap-1 rounded-lg border border-sage/40 bg-surface px-2.5 py-1 text-xs font-medium text-navy hover:bg-sage hover:text-white transition-colors"
                @click="insertPlanToPlan(item)"
              >
                <Plus class="h-3 w-3 text-sage group-hover:text-white" />
                {{ item.name }}
              </button>
            </div>
          </div>

          <div>
            <div class="flex items-center justify-between">
              <label class="block text-xs font-semibold text-navy">
                Plan (Future Session Focus & Home Program Adjustments)
              </label>
              <div class="flex items-center gap-1.5">
                <button
                  v-if="plan.trim()"
                  type="button"
                  :disabled="polishingSection === 'Plan'"
                  class="inline-flex items-center gap-1 rounded-lg border border-purple-200 bg-purple-50 px-2 py-1 text-xs font-bold text-purple-700 hover:bg-purple-100 hover:border-purple-300 transition-all disabled:opacity-50"
                  title="Contextually correct medical terms and polish notes with AI"
                  @click="handlePolishSection('Plan')"
                >
                  <Loader2 v-if="polishingSection === 'Plan'" class="h-3 w-3 animate-spin text-purple-600" />
                  <Sparkles v-else class="h-3 w-3 text-purple-600" />
                  {{ polishingSection === 'Plan' ? 'Polishing...' : 'AI Polish' }}
                </button>
                <button
                  v-if="sectionHistory['Plan']"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-1.5 py-1 text-[11px] font-semibold text-neutral-muted hover:text-navy"
                  title="Revert back to pre-polished text"
                  @click="revertPolishedSection('Plan')"
                >
                  <Undo2 class="h-3 w-3" />
                  Revert
                </button>
                <button
                  v-if="plan"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1 text-xs text-neutral-muted hover:border-danger-red/40 hover:bg-danger-red/10 hover:text-danger-red transition-all"
                  title="Clear Plan text"
                  @click="plan = ''"
                >
                  <RotateCcw class="h-3 w-3" />
                  Clear
                </button>
                <VoiceDictationButton
                  section-label="Plan"
                  button-text="Dictate Plan"
                  :engine="transcriptionEngine"
                  :pet-name="petName"
                  species="Canine"
                  @transcript-chunk="handlePlanDictationChunk"
                  @dictation-finished="autoSaveNote"
                />
              </div>
            </div>
            <div v-if="lastCorrections['Plan']" class="mt-1 flex flex-wrap gap-1">
              <span
                v-for="corr in lastCorrections['Plan']"
                :key="corr"
                class="inline-flex items-center gap-1 rounded-md bg-purple-100/70 px-1.5 py-0.5 text-[10px] font-semibold text-purple-800"
              >
                ✨ {{ corr }}
              </span>
            </div>
            <textarea
              v-model="plan"
              rows="4"
              class="mt-1 w-full rounded-xl border border-neutral-grey/80 bg-surface p-3 text-sm text-navy focus:border-sage focus:outline-none"
              placeholder="e.g. Continue home routine. Increase Cavaletti height next session. Recommended visit frequency: 2x weekly..."
            />
          </div>

          <!-- Medical History Diagnosis Update Option -->
          <div class="rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-3">
            <div class="flex items-center justify-between">
              <label class="flex items-center gap-2 text-xs font-semibold text-navy">
                <input type="checkbox" v-model="updateDiagnosis" class="rounded accent-sage" />
                Update Primary Diagnosis / Condition in Patient's Profile
              </label>
              <div class="flex items-center gap-1.5">
                <button
                  v-if="updateDiagnosis && diagnosisText.trim()"
                  type="button"
                  :disabled="polishingSection === 'Diagnosis'"
                  class="inline-flex items-center gap-1 rounded-lg border border-purple-200 bg-purple-50 px-2 py-1 text-xs font-bold text-purple-700 hover:bg-purple-100 hover:border-purple-300 transition-all disabled:opacity-50"
                  title="Contextually correct medical terms and polish diagnosis with AI"
                  @click="handlePolishSection('Diagnosis')"
                >
                  <Loader2 v-if="polishingSection === 'Diagnosis'" class="h-3 w-3 animate-spin text-purple-600" />
                  <Sparkles v-else class="h-3 w-3 text-purple-600" />
                  AI Polish
                </button>
                <button
                  v-if="updateDiagnosis && diagnosisText"
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1 text-xs text-neutral-muted hover:border-danger-red/40 hover:bg-danger-red/10 hover:text-danger-red transition-all"
                  title="Clear Diagnosis text"
                  @click="diagnosisText = ''"
                >
                  <RotateCcw class="h-3 w-3" />
                  Clear
                </button>
                <VoiceDictationButton
                  v-if="updateDiagnosis"
                  section-label="Diagnosis"
                  button-text="Dictate Diagnosis"
                  :compact="true"
                  :engine="transcriptionEngine"
                  :pet-name="petName"
                  species="Canine"
                  @transcript-chunk="(chunk, pause) => diagnosisText = formatPausePunctuation(diagnosisText, chunk, pause)"
                  @dictation-finished="autoSaveNote"
                />
              </div>
            </div>
            <input
              v-if="updateDiagnosis"
              type="text"
              v-model="diagnosisText"
              placeholder="Enter updated primary diagnosis..."
              class="mt-2 w-full rounded-lg border border-neutral-grey/80 bg-surface px-3 py-2 text-xs text-navy focus:border-sage focus:outline-none"
            />
          </div>

          <!-- Share with Owner Toggle -->
          <div class="rounded-xl border border-sage/30 bg-sage-muted/30 p-3 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <Share2 class="h-4 w-4 text-sage" />
              <div>
                <p class="text-xs font-bold text-navy">Publish & Share Report with Pet Owner</p>
                <p class="text-[11px] text-neutral-muted">Owner can access this clinical report in the Owner App under Saved Reports.</p>
              </div>
            </div>
            <input type="checkbox" v-model="shareWithOwner" class="h-4 w-4 rounded accent-sage" />
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-between border-t border-neutral-grey/80 pt-4">
          <div class="flex gap-2">
            <button
              v-if="activeTab !== 'S'"
              type="button"
              class="rounded-xl border border-neutral-grey/80 px-4 py-2 text-xs font-bold text-navy hover:bg-neutral-grey/40"
              @click="switchTab(activeTab === 'P' ? 'A' : activeTab === 'A' ? 'O' : 'S')"
            >
              Previous Section
            </button>
            <button
              v-if="activeTab !== 'P'"
              type="button"
              class="rounded-xl bg-navy/10 px-4 py-2 text-xs font-bold text-navy hover:bg-navy/20"
              @click="switchTab(activeTab === 'S' ? 'O' : activeTab === 'O' ? 'A' : 'P')"
            >
              Next Section
            </button>
          </div>

          <div class="flex items-center gap-3">
            <button
              type="button"
              class="rounded-xl px-4 py-2 text-xs font-semibold text-neutral-muted hover:text-navy"
              @click="emit('close')"
            >
              Cancel
            </button>
            <button
              type="submit"
              :disabled="submitting"
              class="inline-flex items-center gap-2 rounded-xl bg-sage px-5 py-2.5 text-xs font-bold text-white shadow-sm hover:bg-sage/90 disabled:opacity-50"
            >
              <CheckCircle class="h-4 w-4" />
              {{ submitting ? 'Saving Note...' : 'Save Clinical Note' }}
            </button>
          </div>
        </div>
      </form>

      <!-- Full Session AI Dictation Modal -->
      <VoiceSoapDictationModal
        :is-open="showVoiceDictationModal"
        :pet-id="petId"
        :pet-name="petName"
        species="Canine"
        @close="showVoiceDictationModal = false"
        @apply-structured-note="handleApplyStructuredNote"
        @insert-raw-transcript="handleInsertRawTranscript"
      />
    </div>
  </div>
</template>

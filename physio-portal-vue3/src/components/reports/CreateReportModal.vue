<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import {
  X,
  FileText,
  Download,
  Save,
  Share2,
  Sparkles,
  Activity,
  FileCheck2,
  Calendar,
  Clock,
  MessageSquarePlus,
  TrendingUp,
  Dumbbell,
  Stethoscope,
  Pencil,
  Plus,
  Trash2,
  Check,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { Pet } from '../../types/pet'
import type { CreateReportPayload, ReferencedReportSession } from '../../types/soap'
import { REPORT_TYPES } from '../../data/reportsDemo'
import { fetchAppointments } from '../../api/appointments'
import { fetchSoapNotesByPet } from '../../api/soapNotes'
import { getRehabProgramsByPet } from '../../api/rehab-programs'
import { getPetProgress } from '../../api/progress'

interface SelectableSession {
  id: string
  date: string
  sessionType: string
  sessionNotes: string
  selected: boolean
  clinicianComment: string
  showCommentInput: boolean
}

const props = defineProps<{
  patients: Pet[]
  initialPetId: number | null
  initialType?: string | null
  saving?: boolean
  downloading?: boolean
}>()

const emit = defineEmits<{
  close: []
  saveAndDownload: [payload: CreateReportPayload]
  saveOnly: [payload: CreateReportPayload]
  quickDownload: [petId: number, options: {
    type: string
    customTitle: string
    summary: string
    dischargeStatus?: string
    maintenancePlan?: string
    veterinarianNotes?: string
    periodFrom?: string
    periodTo?: string
    referencedSessions?: ReferencedReportSession[]
  }]
}>()

const selectedPetId = ref<number | null>(props.initialPetId)
const selectedTypeId = ref<'progress' | 'discharge' | 'home-program' | 'soap'>(
  (props.initialType as any) || 'progress'
)

// Active tab in modal builder
const activeSection = ref<'scope' | 'sessions' | 'narrative'>('scope')

// Dates & Care Period
const periodFrom = ref('')
const periodTo = ref(new Date().toISOString().slice(0, 10))
const activePeriodPreset = ref<'14days' | '30days' | '60days' | 'all' | 'custom'>('30days')

// Content
const title = ref('')
const summary = ref('')
const dischargeStatus = ref('Rehabilitation Goals Achieved — Discharged to Home Maintenance')
const maintenancePlan = ref('')
const veterinarianNotes = ref('')
const shareWithOwner = ref(true)

interface SynthesisDetailItem {
  id: string
  label: string
  detail: string
  selected: boolean
  isEditing?: boolean
}

const activeSynthesisCategory = ref<'exercises' | 'outcomes' | 'timeline' | 'vet'>('exercises')

const exerciseDetails = ref<SynthesisDetailItem[]>([])
const outcomeDetails = ref<SynthesisDetailItem[]>([])
const vetGuidanceDetails = ref<SynthesisDetailItem[]>([
  {
    id: 'vet-clearance',
    label: 'Lifestyle Activity Clearance',
    detail: 'Patient is cleared for ongoing controlled, low-impact daily lifestyle activities and leash walks.',
    selected: true,
  },
  {
    id: 'vet-monitoring',
    label: 'Clinical Precautions & Flare-Up Signs',
    detail: 'Advise immediate veterinary physiotherapy reassessment if acute lameness, joint warmth, or rising hesitation recurs.',
    selected: true,
  },
  {
    id: 'vet-recheck',
    label: 'Recommended Veterinary Recheck Interval',
    detail: 'Recommend routine clinical wellness checkup and joint range-of-motion evaluation in 6 months.',
    selected: false,
  },
])

// Patient Clinical Sessions
const loadingSessions = ref(false)
const availableSessions = ref<SelectableSession[]>([])

const selectedPatient = computed(() =>
  props.patients.find((p) => p.petId === selectedPetId.value) ?? props.patients[0] ?? null
)

const selectedSessionsCount = computed(
  () => availableSessions.value.filter((s) => s.selected).length
)

// Initialize dates preset
function applyPeriodPreset(preset: '14days' | '30days' | '60days' | 'all') {
  activePeriodPreset.value = preset
  const today = new Date()
  periodTo.value = today.toISOString().slice(0, 10)

  if (preset === '14days') {
    const from = new Date(today.getTime() - 14 * 24 * 60 * 60 * 1000)
    periodFrom.value = from.toISOString().slice(0, 10)
  } else if (preset === '30days') {
    const from = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000)
    periodFrom.value = from.toISOString().slice(0, 10)
  } else if (preset === '60days') {
    const from = new Date(today.getTime() - 60 * 24 * 60 * 60 * 1000)
    periodFrom.value = from.toISOString().slice(0, 10)
  } else if (preset === 'all') {
    periodFrom.value = ''
  }

  // Auto-select sessions that fall within this range
  autoSelectSessionsInRange()
}

function autoSelectSessionsInRange() {
  if (availableSessions.value.length === 0) return
  const fromTime = periodFrom.value ? new Date(periodFrom.value).getTime() : 0
  const toTime = periodTo.value ? new Date(periodTo.value + 'T23:59:59').getTime() : Infinity

  availableSessions.value.forEach((s) => {
    const sTime = new Date(s.date).getTime()
    s.selected = sTime >= fromTime && sTime <= toTime
  })
}

// Fetch all collected data for patient (Appointments, SOAP, Rehab program, Progress logs)
async function loadPatientClinicalData() {
  const petId = selectedPetId.value || selectedPatient.value?.petId
  if (!petId) return

  loadingSessions.value = true
  const combined: SelectableSession[] = []

  try {
    // 1. Fetch appointments
    const appts = await fetchAppointments(undefined, undefined, petId).catch(() => [])
    if (appts && appts.length > 0) {
      appts.forEach((a) => {
        combined.push({
          id: `appt-${a.appointmentId}`,
          date: a.scheduledDateTime,
          sessionType: 'Physiotherapy Consultation',
          sessionNotes: a.clinicianNotes || a.clientNotes || 'Clinical appointment and physical rehabilitation treatment.',
          selected: true,
          clinicianComment: '',
          showCommentInput: false,
        })
      })
    }

    // 2. Fetch SOAP notes
    const soapNotes = await fetchSoapNotesByPet(petId).catch(() => [])
    if (soapNotes && soapNotes.length > 0) {
      soapNotes.forEach((s) => {
        // Only add if not duplicate appointment date
        const sDate = s.sessionDate.slice(0, 10)
        const existing = combined.find((c) => c.date.slice(0, 10) === sDate)
        if (existing) {
          existing.sessionType = 'Clinical Assessment (SOAP)'
          if (s.objective) existing.sessionNotes = `Objective: ${s.objective.slice(0, 120)}... Action: ${s.action || 'Therapy performed'}`
        } else {
          combined.push({
            id: `soap-${s.soapNoteId}`,
            date: s.sessionDate,
            sessionType: 'Clinical SOAP Session',
            sessionNotes: `Objective: ${s.objective ? s.objective.slice(0, 100) : 'Assessment completed'}. Plan: ${s.plan || 'Continue home protocol'}`,
            selected: true,
            clinicianComment: '',
            showCommentInput: false,
          })
        }
      })
    }

    // 3. Fallback demo sessions if no backend entries
    if (combined.length === 0) {
      combined.push(
        {
          id: 'demo-1',
          date: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(),
          sessionType: 'Hydrotherapy & Joint Mobilization',
          sessionNotes: `Underwater treadmill (12 mins, 1.8 km/h). Passive ROM stifle extension measured at 135°. Good weight tolerance.`,
          selected: true,
          clinicianComment: 'Owner reports reduced morning hesitation when rising.',
          showCommentInput: true,
        },
        {
          id: 'demo-2',
          date: new Date(Date.now() - 11 * 24 * 60 * 60 * 1000).toISOString(),
          sessionType: 'Laser Therapy & Cavaletti Training',
          sessionNotes: `Class IV therapeutic laser applied to stifle (4J/cm²). Cavaletti rails x 3 sets. Symmetrical gait observed at walk.`,
          selected: true,
          clinicianComment: 'Mild post-session fatigue resolved within 2 hours.',
          showCommentInput: false,
        },
        {
          id: 'demo-3',
          date: new Date(Date.now() - 25 * 24 * 60 * 60 * 1000).toISOString(),
          sessionType: 'Initial Physiotherapy Assessment',
          sessionNotes: `Baseline gait evaluation: Grade 3/5 lameness. Stifle extension PROM limited to 118°. Pain score 6/10 on palpation.`,
          selected: true,
          clinicianComment: 'Established Phase 1 rehabilitation protocol and home safety guidelines.',
          showCommentInput: false,
        }
      )
    }

    // 4. Fetch Rehab Programs to synthesize active exercises
    const programs = await getRehabProgramsByPet(petId).catch(() => [])
    if (programs && programs.length > 0 && programs[0].exercises?.length) {
      exerciseDetails.value = programs[0].exercises.map((e: any, idx: number) => ({
        id: `ex-${e.rehabProgramExerciseId || idx}`,
        label: e.title || 'Prescribed Exercise',
        detail: `${e.title || 'Exercise'} (${e.sets ?? 2} sets × ${e.repetitions ?? 10} reps, ${e.frequencyPerDay ?? 2}x daily)`,
        selected: true,
      }))
    } else {
      exerciseDetails.value = [
        {
          id: 'ex-1',
          label: 'Cavaletti Rails',
          detail: 'Cavaletti Rails (2 sets × 10 reps, 2x daily)',
          selected: true,
        },
        {
          id: 'ex-2',
          label: 'Sit-to-Stand Squats',
          detail: 'Sit-to-Stand Isometric Squats (3 sets × 8 reps, 2x daily)',
          selected: true,
        },
        {
          id: 'ex-3',
          label: 'Controlled Leash Walks',
          detail: 'Controlled Flat Leash Walk (20-25 minutes, 2x daily)',
          selected: true,
        },
        {
          id: 'ex-4',
          label: 'PROM Stretches',
          detail: 'Passive Range of Motion (PROM) Stretches (10 repetitions post-walk)',
          selected: true,
        },
      ]
    }

    // 5. Fetch pet progress metrics
    const progress = await getPetProgress(petId).catch(() => null)
    const completedSessions = progress?.totalCompletedSessions ?? 6
    const trackedDays = progress?.totalTrackedDays ?? 12
    outcomeDetails.value = [
      {
        id: 'out-pain',
        label: 'Pain Score Reduction',
        detail: 'Pain Score: Reduced from baseline 6/10 to 2/10 at current review.',
        selected: true,
      },
      {
        id: 'out-mobility',
        label: 'Functional Gait & Mobility',
        detail: 'Mobility: Symmetrical weight-bearing restored at walk and trot (Mobility score: 8.5/10).',
        selected: true,
      },
      {
        id: 'out-sessions',
        label: 'Therapy Attendance',
        detail: `Clinical Sessions: Successfully attended and completed ${completedSessions} in-clinic therapy sessions.`,
        selected: true,
      },
      {
        id: 'out-compliance',
        label: 'Home Program Adherence',
        detail: `Home Tracking: Active compliance maintained over ${trackedDays} logged tracking days.`,
        selected: true,
      },
    ]
  } finally {
    // Sort chronologically descending
    combined.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
    availableSessions.value = combined
    loadingSessions.value = false
    autoSelectSessionsInRange()
  }
}

// Initialize default title without pre-filling narrative
function initReportForm() {
  const pet = selectedPatient.value
  const petName = pet?.petName || 'Patient'
  const type = selectedTypeId.value

  title.value = type === 'discharge'
    ? `${petName} - Rehabilitation Discharge Summary`
    : type === 'home-program'
      ? `${petName} - Home Exercise & Care Protocol`
      : type === 'soap'
        ? `${petName} - SOAP Clinical Assessment Summary`
        : `${petName} - Clinical Progress & Rehabilitation Report`

  // Keep narrative and plan text boxes completely blank - no canned or prefilled text
  summary.value = ''
  maintenancePlan.value = ''
  veterinarianNotes.value = ''
}

function clearNarrative() {
  summary.value = ''
}

function toggleAllCategoryItems(category: 'exercises' | 'outcomes' | 'vet' | 'timeline') {
  if (category === 'exercises') {
    const all = exerciseDetails.value.every((e) => e.selected)
    exerciseDetails.value.forEach((e) => (e.selected = !all))
  } else if (category === 'outcomes') {
    const all = outcomeDetails.value.every((o) => o.selected)
    outcomeDetails.value.forEach((o) => (o.selected = !all))
  } else if (category === 'vet') {
    const all = vetGuidanceDetails.value.every((v) => v.selected)
    vetGuidanceDetails.value.forEach((v) => (v.selected = !all))
  } else if (category === 'timeline') {
    toggleSelectAllSessions()
  }
}

function insertCheckedSynthesisDetails(categoryMode: 'active' | 'all' = 'active') {
  const blocks: string[] = []

  const shouldInclude = (cat: 'exercises' | 'outcomes' | 'timeline' | 'vet') =>
    categoryMode === 'all' || activeSynthesisCategory.value === cat

  // 1. Recovery Metrics
  if (shouldInclude('outcomes')) {
    const selected = outcomeDetails.value.filter((o) => o.selected)
    if (selected.length > 0) {
      const lines = selected.map((o) => `• ${o.detail}`).join('\n')
      blocks.push(`Objective Recovery & Outcome Measures:\n${lines}`)
    }
  }

  // 2. Prescribed Exercises
  if (shouldInclude('exercises')) {
    const selected = exerciseDetails.value.filter((e) => e.selected)
    if (selected.length > 0) {
      const lines = selected.map((e) => `• ${e.detail}`).join('\n')
      blocks.push(`Prescribed Active Home Exercise Routine:\n${lines}`)
    }
  }

  // 3. Referenced Sessions
  if (shouldInclude('timeline')) {
    const selected = availableSessions.value.filter((s) => s.selected)
    if (selected.length > 0) {
      const lines = selected.map((s) => {
        const d = new Date(s.date).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' })
        const comment = s.clinicianComment ? ` [Clinician Note: "${s.clinicianComment}"]` : ''
        return `• ${d} (${s.sessionType}): ${s.sessionNotes}${comment}`
      }).join('\n')
      blocks.push(`Referenced Clinical Session Milestones:\n${lines}`)
    }
  }

  // 4. Vet Guidance
  if (shouldInclude('vet')) {
    const selected = vetGuidanceDetails.value.filter((v) => v.selected)
    if (selected.length > 0) {
      const lines = selected.map((v) => `• ${v.label}: ${v.detail}`).join('\n')
      blocks.push(`Veterinary & Long-Term Care Directives:\n${lines}`)
    }
  }

  if (blocks.length === 0) return

  const combinedText = blocks.join('\n\n')
  summary.value = summary.value.trim()
    ? `${summary.value.trim()}\n\n${combinedText}`
    : combinedText
}

function toggleSelectAllSessions() {
  const allSelected = availableSessions.value.every((s) => s.selected)
  availableSessions.value.forEach((s) => (s.selected = !allSelected))
}

function addCustomVetDirective() {
  const newId = `vet-custom-${Date.now()}`
  vetGuidanceDetails.value.push({
    id: newId,
    label: 'Custom Care Directive',
    detail: 'Enter custom instruction or clinical guidance...',
    selected: true,
    isEditing: true,
  })
}

function removeVetDirective(id: string) {
  vetGuidanceDetails.value = vetGuidanceDetails.value.filter((v) => v.id !== id)
}

watch(
  () => [selectedPetId.value, selectedTypeId.value],
  () => {
    initReportForm()
    loadPatientClinicalData()
  },
  { immediate: true }
)

onMounted(() => {
  applyPeriodPreset('30days')
})

function getPayload(): CreateReportPayload {
  const petId = Number(selectedPetId.value || selectedPatient.value?.petId || 1)
  const normalizedType =
    selectedTypeId.value === 'discharge'
      ? 'DISCHARGE_SUMMARY'
      : selectedTypeId.value === 'home-program'
        ? 'OWNER_HOME_PROGRAM'
        : selectedTypeId.value === 'soap'
          ? 'SOAP_SESSION'
          : 'PROGRESS_REPORT'

  const referenced = availableSessions.value
    .filter((s) => s.selected)
    .map((s) => ({
      date: s.date,
      sessionType: s.sessionType,
      sessionNotes: s.sessionNotes,
      clinicianComment: s.clinicianComment.trim() || undefined,
    }))

  return {
    petId,
    reportType: normalizedType,
    title: title.value.trim() || `${selectedPatient.value?.petName || 'Patient'} - Clinical Report`,
    summary: summary.value.trim(),
    dischargeStatus: selectedTypeId.value === 'discharge' ? dischargeStatus.value : undefined,
    maintenancePlan: maintenancePlan.value.trim() || undefined,
    veterinarianNotes: veterinarianNotes.value.trim() || undefined,
    shareWithOwner: shareWithOwner.value,
    periodFrom: periodFrom.value || undefined,
    periodTo: periodTo.value || undefined,
    referencedSessions: referenced.length > 0 ? referenced : undefined,
  }
}

function handleSaveAndDownload() {
  emit('saveAndDownload', getPayload())
}

function handleSaveOnly() {
  emit('saveOnly', getPayload())
}

function handleQuickDownload() {
  const petId = Number(selectedPetId.value || selectedPatient.value?.petId || 1)
  const payload = getPayload()
  emit('quickDownload', petId, {
    type: selectedTypeId.value,
    customTitle: payload.title,
    summary: payload.summary || '',
    dischargeStatus: payload.dischargeStatus,
    maintenancePlan: payload.maintenancePlan,
    veterinarianNotes: payload.veterinarianNotes,
    periodFrom: payload.periodFrom,
    periodTo: payload.periodTo,
    referencedSessions: payload.referencedSessions,
  })
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card flex max-h-[94vh] w-full max-w-4xl flex-col overflow-hidden shadow-2xl animate-in fade-in zoom-in-95 duration-150">
      <!-- Header -->
      <div class="flex items-start justify-between border-b border-neutral-grey/80 p-5">
        <div class="flex items-center gap-3">
          <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage">
            <Sparkles class="h-6 w-6" :stroke-width="1.75" />
          </div>
          <div>
            <h3 class="text-base font-bold text-navy">Generate Clinical Report</h3>
            <p class="text-xs text-neutral-muted">
              Synthesize collected patient visits, therapy notes, outcome trends, and home exercise protocols.
            </p>
          </div>
        </div>
        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted transition-colors hover:bg-surface hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Navigation Steps/Tabs -->
      <div class="flex items-center border-b border-neutral-grey/80 bg-surface/60 px-5 text-xs font-semibold">
        <button
          type="button"
          class="flex items-center gap-1.5 border-b-2 py-3 px-3 transition-colors"
          :class="activeSection === 'scope' ? 'border-sage text-navy font-bold' : 'border-transparent text-neutral-muted hover:text-navy'"
          @click="activeSection = 'scope'"
        >
          <Calendar class="h-3.5 w-3.5" />
          1. Scope & Care Period
        </button>
        <button
          type="button"
          class="flex items-center gap-1.5 border-b-2 py-3 px-3 transition-colors"
          :class="activeSection === 'sessions' ? 'border-sage text-navy font-bold' : 'border-transparent text-neutral-muted hover:text-navy'"
          @click="activeSection = 'sessions'"
        >
          <Clock class="h-3.5 w-3.5" />
          2. Previous Sessions & Comments
          <span
            v-if="selectedSessionsCount > 0"
            class="rounded-full bg-sage/20 px-1.5 py-0.2 text-[10px] font-bold text-sage"
          >
            {{ selectedSessionsCount }}
          </span>
        </button>
        <button
          type="button"
          class="flex items-center gap-1.5 border-b-2 py-3 px-3 transition-colors"
          :class="activeSection === 'narrative' ? 'border-sage text-navy font-bold' : 'border-transparent text-neutral-muted hover:text-navy'"
          @click="activeSection = 'narrative'"
        >
          <FileText class="h-3.5 w-3.5" />
          3. Clinical Narrative & Export
        </button>
      </div>

      <!-- Form Body (Scrollable) -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6">

        <!-- ================= SECTION 1: SCOPE & CARE PERIOD ================= -->
        <div v-show="activeSection === 'scope'" class="space-y-5 animate-in fade-in duration-100">
          <div class="grid gap-4 sm:grid-cols-2">
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-navy mb-1.5">
                Target Patient
              </label>
              <select
                v-model="selectedPetId"
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
              >
                <option v-for="p in patients" :key="p.petId" :value="p.petId">
                  {{ p.petName }} ({{ p.species }} · Owner: {{ p.ownerName }})
                </option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-navy mb-1.5">
                Report Purpose & Category
              </label>
              <div class="grid grid-cols-2 gap-1.5">
                <button
                  v-for="rt in REPORT_TYPES"
                  :key="rt.id"
                  type="button"
                  class="rounded-lg border px-2.5 py-2 text-left text-xs font-semibold transition-all"
                  :class="
                    selectedTypeId === rt.id
                      ? 'border-sage bg-sage text-white shadow-sm'
                      : 'border-neutral-grey/80 bg-surface text-navy hover:border-sage/60'
                  "
                  @click="selectedTypeId = rt.id"
                >
                  <div class="truncate">{{ rt.label }}</div>
                  <div class="text-[10px] opacity-80 font-normal truncate">{{ rt.badge }}</div>
                </button>
              </div>
            </div>
          </div>

          <!-- Care Period & Dates Card -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface/40 p-4 space-y-3">
            <div class="flex flex-wrap items-center justify-between gap-2">
              <div class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-navy">
                <Calendar class="h-4 w-4 text-sage" />
                Treatment Period Covered
              </div>
              <div class="flex items-center gap-1 text-xs">
                <button
                  type="button"
                  class="rounded-md px-2 py-1 text-[11px] font-semibold transition-colors"
                  :class="activePeriodPreset === '14days' ? 'bg-sage text-white' : 'bg-neutral-grey/60 text-navy hover:bg-neutral-grey'"
                  @click="applyPeriodPreset('14days')"
                >
                  Last 14d
                </button>
                <button
                  type="button"
                  class="rounded-md px-2 py-1 text-[11px] font-semibold transition-colors"
                  :class="activePeriodPreset === '30days' ? 'bg-sage text-white' : 'bg-neutral-grey/60 text-navy hover:bg-neutral-grey'"
                  @click="applyPeriodPreset('30days')"
                >
                  Last 30d
                </button>
                <button
                  type="button"
                  class="rounded-md px-2 py-1 text-[11px] font-semibold transition-colors"
                  :class="activePeriodPreset === '60days' ? 'bg-sage text-white' : 'bg-neutral-grey/60 text-navy hover:bg-neutral-grey'"
                  @click="applyPeriodPreset('60days')"
                >
                  Last 60d
                </button>
                <button
                  type="button"
                  class="rounded-md px-2 py-1 text-[11px] font-semibold transition-colors"
                  :class="activePeriodPreset === 'all' ? 'bg-sage text-white' : 'bg-neutral-grey/60 text-navy hover:bg-neutral-grey'"
                  @click="applyPeriodPreset('all')"
                >
                  All-Time
                </button>
              </div>
            </div>

            <div class="grid gap-3 sm:grid-cols-2">
              <div>
                <label class="block text-[11px] font-semibold text-neutral-muted mb-1">Start Date</label>
                <input
                  v-model="periodFrom"
                  type="date"
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-1.5 text-xs text-navy outline-none focus:border-sage"
                  @change="activePeriodPreset = 'custom'; autoSelectSessionsInRange()"
                />
              </div>
              <div>
                <label class="block text-[11px] font-semibold text-neutral-muted mb-1">End Date</label>
                <input
                  v-model="periodTo"
                  type="date"
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-1.5 text-xs text-navy outline-none focus:border-sage"
                  @change="activePeriodPreset = 'custom'; autoSelectSessionsInRange()"
                />
              </div>
            </div>
            <p class="text-[11px] text-neutral-muted">
              This date range will appear prominently in the official report header as the treatment review scope.
            </p>
          </div>

          <!-- Document Title -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-navy mb-1.5">
              Report Document Title
            </label>
            <input
              v-model="title"
              type="text"
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage font-medium"
            />
          </div>

          <div class="flex justify-end pt-2">
            <BaseButton size="sm" @click="activeSection = 'sessions'">
              Next: Reference Previous Sessions &rarr;
            </BaseButton>
          </div>
        </div>

        <!-- ================= SECTION 2: PREVIOUS SESSIONS & COMMENTS ================= -->
        <div v-show="activeSection === 'sessions'" class="space-y-4 animate-in fade-in duration-100">
          <div class="flex flex-wrap items-center justify-between gap-2 border-b border-neutral-grey/60 pb-3">
            <div>
              <h4 class="text-xs font-bold uppercase tracking-wider text-navy">
                Reference Previous Appointments & Therapy Sessions
              </h4>
              <p class="text-[11px] text-neutral-muted">
                Select which clinical sessions to cite in this document and add your clinician observations.
              </p>
            </div>
            <div class="flex items-center gap-2">
              <button
                type="button"
                class="text-xs font-semibold text-sage hover:underline"
                @click="toggleSelectAllSessions"
              >
                {{ availableSessions.every((s) => s.selected) ? 'Deselect All' : 'Select All' }}
              </button>
            </div>
          </div>

          <!-- Session List -->
          <div class="space-y-2.5">
            <div
              v-for="session in availableSessions"
              :key="session.id"
              class="rounded-xl border p-3.5 transition-all text-xs"
              :class="session.selected ? 'border-sage/60 bg-sage-muted/20' : 'border-neutral-grey/70 bg-surface/50 opacity-70'"
            >
              <div class="flex items-start justify-between gap-3">
                <label class="flex items-start gap-2.5 cursor-pointer flex-1">
                  <input
                    v-model="session.selected"
                    type="checkbox"
                    class="mt-0.5 h-4 w-4 rounded text-sage focus:ring-sage"
                  />
                  <div>
                    <div class="flex flex-wrap items-center gap-2">
                      <span class="font-bold text-navy">
                        {{ new Date(session.date).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) }}
                      </span>
                      <span class="rounded bg-navy/5 px-2 py-0.5 text-[10px] font-semibold text-navy">
                        {{ session.sessionType }}
                      </span>
                    </div>
                    <p class="mt-1 text-neutral-muted leading-relaxed">
                      {{ session.sessionNotes }}
                    </p>
                  </div>
                </label>

                <button
                  type="button"
                  class="shrink-0 rounded-lg border border-neutral-grey px-2 py-1 text-[11px] font-semibold text-navy hover:border-sage hover:text-sage transition-colors flex items-center gap-1"
                  @click="session.showCommentInput = !session.showCommentInput"
                >
                  <MessageSquarePlus class="h-3 w-3" />
                  {{ session.clinicianComment ? 'Edit Comment' : 'Add Comment' }}
                </button>
              </div>

              <!-- Inline Clinician Comment Field -->
              <div v-if="session.showCommentInput || session.clinicianComment" class="mt-2.5 pt-2 border-t border-neutral-grey/40">
                <div class="flex items-center justify-between mb-1">
                  <label class="text-[11px] font-bold text-sage flex items-center gap-1">
                    Clinician Observation / Comment on this Session:
                  </label>
                  <span v-if="session.clinicianComment" class="text-[10px] text-emerald-600 font-semibold">Saved</span>
                </div>
                <input
                  v-model="session.clinicianComment"
                  type="text"
                  placeholder="e.g. Excellent stifle extension tolerance; owner reports no residual stiffness..."
                  class="w-full rounded-lg border border-neutral-grey bg-white px-2.5 py-1.5 text-xs text-navy outline-none focus:border-sage placeholder:text-neutral-muted/70"
                />
              </div>
            </div>

            <div v-if="availableSessions.length === 0 && !loadingSessions" class="rounded-xl border border-neutral-grey/80 p-8 text-center text-xs text-neutral-muted">
              No recorded appointments found for this patient.
            </div>
          </div>

          <div class="flex items-center justify-between pt-2">
            <BaseButton size="sm" variant="secondary" @click="activeSection = 'scope'">
              &larr; Back: Scope
            </BaseButton>
            <BaseButton size="sm" @click="activeSection = 'narrative'">
              Next: Clinical Narrative &rarr;
            </BaseButton>
          </div>
        </div>

        <!-- ================= SECTION 3: NARRATIVE & RECOMMENDATIONS ================= -->
        <div v-show="activeSection === 'narrative'" class="space-y-5 animate-in fade-in duration-100">
          <!-- Smart Clinical Synthesis Tools with Granular Checkboxes -->
          <div class="rounded-xl border border-sage/30 bg-surface/70 p-4 space-y-3 shadow-xs">
            <div class="flex flex-wrap items-center justify-between gap-2 border-b border-neutral-grey/60 pb-2.5">
              <div class="flex items-center gap-2">
                <div class="flex h-7 w-7 items-center justify-center rounded-lg bg-sage/10 text-sage">
                  <Sparkles class="h-4 w-4" />
                </div>
                <div>
                  <h4 class="text-xs font-bold uppercase tracking-wider text-navy">
                    Smart Clinical Synthesis Tools
                  </h4>
                  <p class="text-[11px] text-neutral-muted">
                    Check the specific collected items you want included, then insert them directly into your editable report narrative.
                  </p>
                </div>
              </div>
              <div class="flex items-center gap-1.5">
                <button
                  type="button"
                  class="rounded-lg border border-neutral-grey bg-white px-2.5 py-1 text-xs font-semibold text-navy hover:border-sage hover:text-sage transition-all"
                  @click="insertCheckedSynthesisDetails('all')"
                >
                  Insert All Categories
                </button>
              </div>
            </div>

            <!-- Category Tabs -->
            <div class="flex flex-wrap items-center gap-1.5 pt-0.5">
              <button
                type="button"
                class="rounded-lg px-2.5 py-1.5 text-xs font-semibold transition-all flex items-center gap-1.5 border"
                :class="
                  activeSynthesisCategory === 'exercises'
                    ? 'border-sage bg-sage text-white shadow-xs'
                    : 'border-neutral-grey/80 bg-white text-navy hover:border-sage/60'
                "
                @click="activeSynthesisCategory = 'exercises'"
              >
                <Dumbbell class="h-3.5 w-3.5" />
                Prescribed Exercises ({{ exerciseDetails.filter((e) => e.selected).length }}/{{ exerciseDetails.length }})
              </button>

              <button
                type="button"
                class="rounded-lg px-2.5 py-1.5 text-xs font-semibold transition-all flex items-center gap-1.5 border"
                :class="
                  activeSynthesisCategory === 'outcomes'
                    ? 'border-sage bg-sage text-white shadow-xs'
                    : 'border-neutral-grey/80 bg-white text-navy hover:border-sage/60'
                "
                @click="activeSynthesisCategory = 'outcomes'"
              >
                <TrendingUp class="h-3.5 w-3.5" />
                Outcome Metrics ({{ outcomeDetails.filter((o) => o.selected).length }}/{{ outcomeDetails.length }})
              </button>

              <button
                type="button"
                class="rounded-lg px-2.5 py-1.5 text-xs font-semibold transition-all flex items-center gap-1.5 border"
                :class="
                  activeSynthesisCategory === 'timeline'
                    ? 'border-sage bg-sage text-white shadow-xs'
                    : 'border-neutral-grey/80 bg-white text-navy hover:border-sage/60'
                "
                @click="activeSynthesisCategory = 'timeline'"
              >
                <Clock class="h-3.5 w-3.5" />
                Session Milestones ({{ availableSessions.filter((s) => s.selected).length }}/{{ availableSessions.length }})
              </button>

              <button
                type="button"
                class="rounded-lg px-2.5 py-1.5 text-xs font-semibold transition-all flex items-center gap-1.5 border"
                :class="
                  activeSynthesisCategory === 'vet'
                    ? 'border-sage bg-sage text-white shadow-xs'
                    : 'border-neutral-grey/80 bg-white text-navy hover:border-sage/60'
                "
                @click="activeSynthesisCategory = 'vet'"
              >
                <Stethoscope class="h-3.5 w-3.5" />
                Vet & Care Directives ({{ vetGuidanceDetails.filter((v) => v.selected).length }}/{{ vetGuidanceDetails.length }})
              </button>
            </div>

            <!-- Category Items Checklist -->
            <div class="rounded-lg border border-neutral-grey/60 bg-white p-3 space-y-2 max-h-64 overflow-y-auto">
              <!-- Exercises Checklist -->
              <div v-if="activeSynthesisCategory === 'exercises'" class="space-y-2">
                <div
                  v-for="item in exerciseDetails"
                  :key="item.id"
                  class="rounded-lg border border-neutral-grey/50 p-2.5 bg-surface/30 text-xs transition-all"
                >
                  <div class="flex items-start justify-between gap-2">
                    <label class="flex items-start gap-2.5 cursor-pointer flex-1">
                      <input
                        v-model="item.selected"
                        type="checkbox"
                        class="mt-0.5 h-3.5 w-3.5 rounded text-sage focus:ring-sage"
                      />
                      <div v-if="!item.isEditing" class="flex-1">
                        <span class="font-bold text-navy">{{ item.label }}</span>
                        <span class="ml-1 text-[11px] text-neutral-muted leading-relaxed">— {{ item.detail }}</span>
                      </div>
                    </label>

                    <button
                      type="button"
                      class="rounded px-2 py-0.5 text-[11px] font-semibold border border-neutral-grey/70 text-navy hover:border-sage hover:text-sage transition-colors flex items-center gap-1 shrink-0"
                      @click="item.isEditing = !item.isEditing"
                    >
                      <Pencil v-if="!item.isEditing" class="h-3 w-3" />
                      <Check v-else class="h-3 w-3 text-emerald-600" />
                      {{ item.isEditing ? 'Done' : 'Edit' }}
                    </button>
                  </div>

                  <div v-if="item.isEditing" class="mt-2 pt-2 border-t border-neutral-grey/40 space-y-1.5">
                    <input
                      v-model="item.detail"
                      type="text"
                      placeholder="e.g. Cavaletti Rails (2 sets × 10 reps, 2x daily)"
                      class="w-full rounded border border-neutral-grey bg-white px-2 py-1 text-xs text-navy outline-none focus:border-sage"
                    />
                  </div>
                </div>
                <div v-if="exerciseDetails.length === 0" class="text-xs text-neutral-muted italic p-2">
                  No active exercises found for this patient.
                </div>
              </div>

              <!-- Outcomes Checklist -->
              <div v-else-if="activeSynthesisCategory === 'outcomes'" class="space-y-2">
                <div
                  v-for="item in outcomeDetails"
                  :key="item.id"
                  class="rounded-lg border border-neutral-grey/50 p-2.5 bg-surface/30 text-xs transition-all"
                >
                  <div class="flex items-start justify-between gap-2">
                    <label class="flex items-start gap-2.5 cursor-pointer flex-1">
                      <input
                        v-model="item.selected"
                        type="checkbox"
                        class="mt-0.5 h-3.5 w-3.5 rounded text-sage focus:ring-sage"
                      />
                      <div v-if="!item.isEditing" class="flex-1">
                        <span class="font-bold text-navy">{{ item.label }}:</span>
                        <span class="ml-1 text-[11px] text-neutral-muted leading-relaxed">{{ item.detail }}</span>
                      </div>
                    </label>

                    <button
                      type="button"
                      class="rounded px-2 py-0.5 text-[11px] font-semibold border border-neutral-grey/70 text-navy hover:border-sage hover:text-sage transition-colors flex items-center gap-1 shrink-0"
                      @click="item.isEditing = !item.isEditing"
                    >
                      <Pencil v-if="!item.isEditing" class="h-3 w-3" />
                      <Check v-else class="h-3 w-3 text-emerald-600" />
                      {{ item.isEditing ? 'Done' : 'Edit' }}
                    </button>
                  </div>

                  <div v-if="item.isEditing" class="mt-2 pt-2 border-t border-neutral-grey/40 space-y-1.5">
                    <input
                      v-model="item.detail"
                      type="text"
                      class="w-full rounded border border-neutral-grey bg-white px-2 py-1 text-xs text-navy outline-none focus:border-sage"
                    />
                  </div>
                </div>
              </div>

              <!-- Sessions Timeline Checklist -->
              <div v-else-if="activeSynthesisCategory === 'timeline'" class="space-y-1.5">
                <label
                  v-for="s in availableSessions"
                  :key="s.id"
                  class="flex items-start gap-2.5 rounded-md p-1.5 hover:bg-surface/70 cursor-pointer text-xs transition-colors"
                >
                  <input
                    v-model="s.selected"
                    type="checkbox"
                    class="mt-0.5 h-3.5 w-3.5 rounded text-sage focus:ring-sage"
                  />
                  <div class="flex-1">
                    <span class="font-bold text-navy">
                      {{ new Date(s.date).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) }}
                    </span>
                    <span class="ml-1 rounded bg-navy/5 px-1.5 py-0.2 text-[10px] font-semibold text-navy">
                      {{ s.sessionType }}
                    </span>
                    <p class="text-[11px] text-neutral-muted mt-0.5 leading-relaxed">
                      {{ s.sessionNotes }}
                      <span v-if="s.clinicianComment" class="italic text-sage font-medium">
                        (Note: "{{ s.clinicianComment }}")
                      </span>
                    </p>
                  </div>
                </label>
                <div v-if="availableSessions.length === 0" class="text-xs text-neutral-muted italic p-2">
                  No sessions recorded for this patient.
                </div>
              </div>

              <!-- Vet Guidance Checklist (Fully Editable) -->
              <div v-else-if="activeSynthesisCategory === 'vet'" class="space-y-2">
                <div
                  v-for="item in vetGuidanceDetails"
                  :key="item.id"
                  class="rounded-lg border border-neutral-grey/50 p-2.5 bg-surface/30 text-xs transition-all"
                >
                  <div class="flex items-start justify-between gap-2">
                    <label class="flex items-start gap-2.5 cursor-pointer flex-1">
                      <input
                        v-model="item.selected"
                        type="checkbox"
                        class="mt-0.5 h-3.5 w-3.5 rounded text-sage focus:ring-sage"
                      />
                      <div v-if="!item.isEditing" class="flex-1">
                        <span class="font-bold text-navy">{{ item.label }}:</span>
                        <span class="ml-1.5 text-[11px] text-neutral-muted leading-relaxed">{{ item.detail }}</span>
                      </div>
                    </label>

                    <div class="flex items-center gap-1 shrink-0">
                      <button
                        type="button"
                        class="rounded px-2 py-0.5 text-[11px] font-semibold border border-neutral-grey/70 text-navy hover:border-sage hover:text-sage transition-colors flex items-center gap-1"
                        @click="item.isEditing = !item.isEditing"
                      >
                        <Pencil v-if="!item.isEditing" class="h-3 w-3" />
                        <Check v-else class="h-3 w-3 text-emerald-600" />
                        {{ item.isEditing ? 'Done' : 'Edit' }}
                      </button>
                      <button
                        v-if="item.id.startsWith('vet-custom-')"
                        type="button"
                        class="rounded p-1 text-neutral-muted hover:text-red-500 transition-colors"
                        title="Delete directive"
                        @click="removeVetDirective(item.id)"
                      >
                        <Trash2 class="h-3 w-3" />
                      </button>
                    </div>
                  </div>

                  <!-- Inline Editing Fields for Directive -->
                  <div v-if="item.isEditing" class="mt-2 pt-2 border-t border-neutral-grey/40 space-y-2">
                    <div>
                      <label class="block text-[10px] font-bold uppercase text-neutral-muted mb-0.5">Directive Category Title</label>
                      <input
                        v-model="item.label"
                        type="text"
                        placeholder="e.g. Hydrotherapy Recheck / Medication Cue"
                        class="w-full rounded border border-neutral-grey bg-white px-2 py-1 text-xs text-navy outline-none focus:border-sage font-semibold"
                      />
                    </div>
                    <div>
                      <label class="block text-[10px] font-bold uppercase text-neutral-muted mb-0.5">Guidance / Instructions</label>
                      <textarea
                        v-model="item.detail"
                        rows="2"
                        placeholder="Enter directive instructions for the veterinarian or owner..."
                        class="w-full rounded border border-neutral-grey bg-white p-2 text-xs text-navy outline-none focus:border-sage leading-relaxed"
                      ></textarea>
                    </div>
                  </div>
                </div>

                <!-- Add Custom Directive Button -->
                <button
                  type="button"
                  class="w-full rounded-lg border border-dashed border-sage/60 py-2 text-xs font-semibold text-sage hover:bg-sage/5 transition-colors flex items-center justify-center gap-1.5"
                  @click="addCustomVetDirective"
                >
                  <Plus class="h-3.5 w-3.5" />
                  Add Custom Vet & Care Directive
                </button>
              </div>
            </div>

            <!-- Sub-actions per category -->
            <div class="flex flex-wrap items-center justify-between gap-2 pt-1 text-xs">
              <button
                type="button"
                class="text-[11px] font-semibold text-sage hover:underline"
                @click="toggleAllCategoryItems(activeSynthesisCategory)"
              >
                Toggle Select All in Category
              </button>

              <button
                type="button"
                class="rounded-lg bg-sage px-3 py-1.5 text-xs font-bold text-white shadow-xs hover:bg-sage/90 transition-colors flex items-center gap-1.5"
                @click="insertCheckedSynthesisDetails('active')"
              >
                <Sparkles class="h-3.5 w-3.5" />
                Insert Checked Details into Narrative
              </button>
            </div>
          </div>

          <!-- Clinical Summary Narrative (Fully Editable) -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <div>
                <label class="text-xs font-bold uppercase tracking-wider text-navy">
                  Clinical Examination & Progress Narrative (Editable)
                </label>
                <p class="text-[11px] text-neutral-muted">
                  The inserted details appear below and remain 100% editable before saving or downloading.
                </p>
              </div>
              <button
                v-if="summary"
                type="button"
                class="text-[11px] font-semibold text-neutral-muted hover:text-red-600 inline-flex items-center gap-1 transition-colors"
                @click="clearNarrative"
              >
                Clear Text
              </button>
            </div>
            <textarea
              v-model="summary"
              rows="6"
              placeholder="Clinical evaluation findings, functional gait response, session progress... Insert checked items above or write bespoke narrative."
              class="w-full rounded-lg border border-neutral-grey bg-surface p-3 text-xs leading-relaxed text-navy outline-none focus:border-sage font-normal"
            ></textarea>
          </div>

          <!-- Discharge Summary Specifics -->
          <div v-if="selectedTypeId === 'discharge'" class="rounded-xl border border-sage/30 bg-surface p-4 space-y-4">
            <div class="flex items-center gap-2">
              <FileCheck2 class="h-4 w-4 text-sage" />
              <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Discharge & Outcome Disposition</h4>
            </div>

            <div>
              <label class="block text-[11px] font-bold text-neutral-muted mb-1">Discharge Status</label>
              <select
                v-model="dischargeStatus"
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-1.5 text-xs text-navy outline-none focus:border-sage"
              >
                <option value="Rehabilitation Goals Achieved — Discharged to Home Maintenance">
                  Rehabilitation Goals Achieved — Discharged to Home Maintenance
                </option>
                <option value="Full Functional Recovery — Routine Annual Checkup">
                  Full Functional Recovery — Routine Annual Checkup
                </option>
                <option value="Discharged with Chronic Maintenance Protocol">
                  Discharged with Chronic Maintenance Protocol
                </option>
                <option value="Referred to Veterinary Surgeon for Further Evaluation">
                  Referred to Veterinary Surgeon for Further Evaluation
                </option>
              </select>
            </div>

            <div>
              <label class="block text-[11px] font-bold text-neutral-muted mb-1">Prescribed Long-Term Maintenance Plan</label>
              <textarea
                v-model="maintenancePlan"
                rows="3"
                class="w-full rounded-lg border border-neutral-grey bg-surface p-2.5 text-xs text-navy outline-none focus:border-sage"
              ></textarea>
            </div>

            <div>
              <label class="block text-[11px] font-bold text-neutral-muted mb-1">Referring Veterinarian Instructions</label>
              <textarea
                v-model="veterinarianNotes"
                rows="2"
                class="w-full rounded-lg border border-neutral-grey bg-surface p-2.5 text-xs text-navy outline-none focus:border-sage"
              ></textarea>
            </div>
          </div>

          <!-- Home Program Specifics -->
          <div v-else-if="selectedTypeId === 'home-program'" class="rounded-xl border border-sage/30 bg-surface p-4 space-y-3">
            <div class="flex items-center gap-2">
              <Activity class="h-4 w-4 text-sage" />
              <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Prescribed Home Routine & Cues</h4>
            </div>
            <div>
              <label class="block text-[11px] font-bold text-neutral-muted mb-1">Prescribed Exercises & Technique Guidelines</label>
              <textarea
                v-model="maintenancePlan"
                rows="4"
                class="w-full rounded-lg border border-neutral-grey bg-surface p-2.5 text-xs text-navy outline-none focus:border-sage"
              ></textarea>
            </div>
          </div>

          <!-- Publish to Owner App Checkbox -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface/50 p-3.5 flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600">
                <Share2 class="h-4 w-4" />
              </div>
              <div>
                <p class="text-xs font-bold text-navy">Publish to Pet Owner App</p>
                <p class="text-[11px] text-neutral-muted">Make this clinical summary and PDF immediately visible in the Owner Portal.</p>
              </div>
            </div>
            <label class="relative inline-flex cursor-pointer items-center">
              <input v-model="shareWithOwner" type="checkbox" class="peer sr-only" />
              <div class="peer h-5 w-9 rounded-full bg-neutral-grey/80 after:absolute after:left-[2px] after:top-[2px] after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-all after:content-[''] peer-checked:bg-sage peer-checked:after:translate-x-full peer-focus:outline-none"></div>
            </label>
          </div>

          <div class="flex items-center justify-between pt-2">
            <BaseButton size="sm" variant="secondary" @click="activeSection = 'sessions'">
              &larr; Back: Sessions
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Footer Actions -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-neutral-grey/80 bg-surface/50 p-4">
        <BaseButton size="sm" variant="secondary" @click="emit('close')">
          Cancel
        </BaseButton>

        <div class="flex flex-wrap items-center gap-2">
          <BaseButton
            size="sm"
            variant="secondary"
            :loading="downloading"
            @click="handleQuickDownload"
          >
            <Download class="h-3.5 w-3.5" />
            Quick Download PDF
          </BaseButton>
          <BaseButton
            size="sm"
            variant="secondary"
            :loading="saving"
            @click="handleSaveOnly"
          >
            <Save class="h-3.5 w-3.5" />
            Save to Reports
          </BaseButton>
          <BaseButton
            size="sm"
            variant="accent"
            :loading="saving || downloading"
            @click="handleSaveAndDownload"
          >
            <FileText class="h-3.5 w-3.5" />
            Save & Download PDF
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

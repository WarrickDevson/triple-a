<script setup lang="ts">
import { ref, watch } from 'vue'
import { X, Plus, Trash2, CheckCircle, Share2, Import, MessageSquareQuote } from '@lucide/vue'
import type { CreateSoapNoteRequest, CustomMetricItem, OwnerSubjectiveNote } from '../../types/soap'
import { fetchOwnerSubjectiveNotes } from '../../api/soapNotes'

const props = defineProps<{
  petId: number
  petName: string
  isOpen: boolean
}>()

const emit = defineEmits<{
  close: []
  created: [note: any]
}>()

const activeTab = ref<'S' | 'O' | 'A' | 'P'>('S')

const sessionDate = ref<string>(new Date().toISOString().slice(0, 10))
const subjective = ref<string>('')
const objective = ref<string>('')
const action = ref<string>('')
const plan = ref<string>('')

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

watch(
  () => props.isOpen,
  (val) => {
    if (val) loadOwnerNotes()
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

// Built-in editable scores
const stiffnessScore = ref<number | null>(3)
const painScore = ref<number | null>(2)
const lamenessScore = ref<number | null>(1)

// Dynamic extensible custom metrics
const customMetrics = ref<CustomMetricItem[]>([
  { name: 'Stifle Extension ROM', value: 130, minScale: 0, maxScale: 180, unitOrDescriptor: 'deg' },
  { name: 'Thigh Circumference', value: 38, minScale: 10, maxScale: 80, unitOrDescriptor: 'cm' },
])

const newMetricName = ref('')
const newMetricValue = ref<number>(0)
const newMetricMin = ref<number>(0)
const newMetricMax = ref<number>(100)
const newMetricUnit = ref('')
const showAddMetric = ref(false)

const updateDiagnosis = ref(false)
const diagnosisText = ref('')
const shareWithOwner = ref(true)

const submitting = ref(false)
const errorMessage = ref('')

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

async function handleSubmit() {
  if (!subjective.value.trim() && !objective.value.trim() && !action.value.trim() && !plan.value.trim()) {
    errorMessage.value = 'Please complete at least one section of the SOAP note.'
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
  }

  emit('created', payload)
  submitting.value = false
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
          <h2 class="text-xl font-bold text-navy">New Clinical SOAP Assessment</h2>
          <p class="text-xs text-neutral-muted">Patient: {{ petName }} · Date: {{ sessionDate }}</p>
        </div>
        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
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
          @click="activeTab = 'S'"
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
          @click="activeTab = 'O'"
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
          @click="activeTab = 'A'"
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
          @click="activeTab = 'P'"
        >
          <span class="rounded bg-white/20 px-1.5 py-0.5 text-[10px]">P</span>
          Plan & Follow-up
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
            <label class="block text-xs font-semibold text-navy">
              Subjective Findings (Owner Observations & Feedback)
            </label>
            <p class="mt-0.5 text-[11px] text-neutral-muted">
              Record changes reported by the owner, home exercise compliance, energy/appetite levels, and any concerns.
            </p>
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
            <label class="block text-xs font-semibold text-navy">Objective Examination Notes</label>
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
              <input
                type="text"
                v-model="newMetricName"
                placeholder="Metric Name (e.g. ROM)"
                class="rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 text-xs text-navy"
              />
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
          <div>
            <label class="block text-xs font-semibold text-navy">
              Action (Treatment Modalities & In-Session Exercises)
            </label>
            <p class="mt-0.5 text-[11px] text-neutral-muted">
              Document manual therapies, laser/hydro treatments, specific areas treated, and in-session exercise reps.
            </p>
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
          <div>
            <label class="block text-xs font-semibold text-navy">
              Plan (Future Session Focus & Home Program Adjustments)
            </label>
            <textarea
              v-model="plan"
              rows="4"
              class="mt-1 w-full rounded-xl border border-neutral-grey/80 bg-surface p-3 text-sm text-navy focus:border-sage focus:outline-none"
              placeholder="e.g. Continue home routine. Increase Cavaletti height next session. Recommended visit frequency: 2x weekly..."
            />
          </div>

          <!-- Medical History Diagnosis Update Option -->
          <div class="rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-3">
            <label class="flex items-center gap-2 text-xs font-semibold text-navy">
              <input type="checkbox" v-model="updateDiagnosis" class="rounded accent-sage" />
              Update Primary Diagnosis / Condition in Patient's Profile
            </label>
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
              @click="activeTab = activeTab === 'P' ? 'A' : activeTab === 'A' ? 'O' : 'S'"
            >
              Previous Section
            </button>
            <button
              v-if="activeTab !== 'P'"
              type="button"
              class="rounded-xl bg-navy/10 px-4 py-2 text-xs font-bold text-navy hover:bg-navy/20"
              @click="activeTab = activeTab === 'S' ? 'O' : activeTab === 'O' ? 'A' : 'P'"
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
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Check, Dumbbell, Search, X } from '@lucide/vue'
import PlanDetailsSidebar from '../components/plans/PlanDetailsSidebar.vue'
import PlanPatientHeader from '../components/plans/PlanPatientHeader.vue'
import PlanPhaseDetail from '../components/plans/PlanPhaseDetail.vue'
import PlanPhasesSidebar from '../components/plans/PlanPhasesSidebar.vue'
import PlanTabs from '../components/plans/PlanTabs.vue'
import PlanGoalsTab from '../components/plans/PlanGoalsTab.vue'
import PlanExercisesTab from '../components/plans/PlanExercisesTab.vue'
import PlanNotesTab from '../components/plans/PlanNotesTab.vue'
import PlanProgressTab from '../components/plans/PlanProgressTab.vue'
import PlanDocumentsTab from '../components/plans/PlanDocumentsTab.vue'
import AddPhaseModal from '../components/plans/AddPhaseModal.vue'
import EditPhaseModal from '../components/plans/EditPhaseModal.vue'
import BaseButton from '../components/BaseButton.vue'
import { useTreatmentPlan } from '../composables/useTreatmentPlan'
import { DEFAULT_PHASES, type PlanPhase } from '../data/planDemo'
import { usePatientsStore } from '../store/patients'
import { useExercisesStore } from '../store/exercises'
import type { Exercise } from '../types/exercise'

const patientsStore = usePatientsStore()
const exercisesStore = useExercisesStore()
const route = useRoute()
const router = useRouter()

const activeTab = ref<'overview' | 'goals' | 'exercises' | 'notes' | 'progress' | 'documents'>('overview')
const activePhaseId = ref(1)
const planPhases = ref<PlanPhase[]>([...DEFAULT_PHASES])

const showCreateModal = ref(false)
const showAddPhaseModal = ref(false)
const showEditPhaseModal = ref(false)
const selectedPhaseToEdit = ref<PlanPhase | null>(null)
const showStubModal = ref(false)
const stubMessage = ref('')
const createForm = reactive({ title: '', startDate: new Date().toISOString().slice(0, 10) })

const showAddExerciseModal = ref(false)
const exerciseSearchQuery = ref('')
const selectedExerciseForPlan = ref<Exercise | null>(null)
const exerciseForm = reactive({
  sets: 3,
  repetitions: 10,
  frequencyPerDay: 1,
})
const addingExercise = ref(false)

const selectedPetId = computed(() => {
  const param = route.params.petId
  if (param) return Number(param)
  return patientsStore.patients[0]?.petId ?? null
})

const selectedPatient = computed(() => {
  if (!selectedPetId.value) return null
  return patientsStore.getPatientById(selectedPetId.value)
})

const plan = useTreatmentPlan(() => selectedPetId.value)

const program = computed(() => plan.program.value)
const planLoading = computed(() => plan.loading.value)
const hasProgram = computed(() => plan.hasProgram.value)

const activePhase = computed(
  () => planPhases.value.find((p: PlanPhase) => p.id === activePhaseId.value) ?? planPhases.value[0]!,
)

const phaseExercises = computed(() => {
  if (!program.value) return []
  return program.value.exercises.filter((ex, index) => {
    const assignedPhase = ex.phaseId ?? ((index % planPhases.value.length) + 1)
    return assignedPhase === activePhaseId.value
  })
})

const availableExercises = computed(() => {
  const query = exerciseSearchQuery.value.trim().toLowerCase()
  let list = exercisesStore.exercises
  if (query) {
    list = list.filter(
      (e: Exercise) =>
        e.title.toLowerCase().includes(query) ||
        e.shortDescription?.toLowerCase().includes(query) ||
        e.targetSpecies?.toLowerCase().includes(query),
    )
  }
  return list
})

onMounted(async () => {
  await Promise.all([
    patientsStore.fetchClinicPatients().catch(() => undefined),
    exercisesStore.fetchExercises().catch(() => undefined),
  ])
  syncRoute()
})

watch(() => patientsStore.patients, syncRoute, { deep: true })

function syncRoute() {
  if (patientsStore.patients.length === 0) return
  const paramId = route.params.petId ? Number(route.params.petId) : null
  if (!paramId || !patientsStore.getPatientById(paramId)) {
    router.replace({
      name: 'treatment-plan-detail',
      params: { petId: patientsStore.patients[0]!.petId },
    })
  }
}

function selectPatient(petId: number) {
  router.push({ name: 'treatment-plan-detail', params: { petId } })
}

async function createPlan() {
  if (!selectedPetId.value || !createForm.title) return
  await plan.createProgram(selectedPetId.value, createForm.title, createForm.startDate)
  showCreateModal.value = false
}

function openAddExerciseModal() {
  selectedExerciseForPlan.value = null
  exerciseSearchQuery.value = ''
  exerciseForm.sets = 3
  exerciseForm.repetitions = 10
  exerciseForm.frequencyPerDay = 1
  showAddExerciseModal.value = true
}

async function addExerciseToPlan() {
  if (!selectedPetId.value || !selectedExerciseForPlan.value) return
  addingExercise.value = true
  try {
    const existing = (program.value?.exercises ?? []).map((e, index) => ({
      exerciseId: e.exerciseId,
      sets: e.sets,
      repetitions: e.repetitions,
      frequencyPerDay: e.frequencyPerDay,
      phaseId: e.phaseId ?? ((index % planPhases.value.length) + 1),
    }))

    const newEx = {
      exerciseId: selectedExerciseForPlan.value.exerciseId,
      sets: Number(exerciseForm.sets) || 3,
      repetitions: Number(exerciseForm.repetitions) || 10,
      frequencyPerDay: Number(exerciseForm.frequencyPerDay) || 1,
      phaseId: activePhaseId.value,
    }

    const title = program.value?.programTitle ?? 'Rehabilitation Program'
    const startDate = program.value?.startDate ?? new Date().toISOString().slice(0, 10)

    await plan.createProgram(selectedPetId.value, title, startDate, [...existing, newEx])
    showAddExerciseModal.value = false
    selectedExerciseForPlan.value = null
  } catch {
    // silent catch
  } finally {
    addingExercise.value = false
  }
}

async function removeExerciseFromPlan(exerciseId: number) {
  if (!selectedPetId.value || !program.value) return
  const remaining = program.value.exercises
    .filter((e) => e.exerciseId !== exerciseId)
    .map((e, index) => ({
      exerciseId: e.exerciseId,
      sets: e.sets,
      repetitions: e.repetitions,
      frequencyPerDay: e.frequencyPerDay,
      phaseId: e.phaseId ?? ((index % planPhases.value.length) + 1),
    }))

  await plan.createProgram(selectedPetId.value, program.value.programTitle, program.value.startDate, remaining)
}

function handleAddPhase(newPhase: { title: string; goals: string[] }) {
  const nextId = planPhases.value.length + 1
  planPhases.value.push({
    id: nextId,
    label: `Phase ${nextId}`,
    title: newPhase.title,
    goals: newPhase.goals,
  })
  activePhaseId.value = nextId
}

function openEditPhaseModal() {
  selectedPhaseToEdit.value = activePhase.value
  showEditPhaseModal.value = true
}

function handleSavePhase(phaseId: number, data: { title: string; goals: string[] }) {
  const p = planPhases.value.find((phase: PlanPhase) => phase.id === phaseId)
  if (p) {
    p.title = data.title
    p.goals = data.goals
  }
}

async function handleSaveNotes(notesText: string) {
  if (!selectedPetId.value || !program.value) return
  const currentExercises = program.value.exercises.map((e, index) => ({
    exerciseId: e.exerciseId,
    sets: e.sets,
    repetitions: e.repetitions,
    frequencyPerDay: e.frequencyPerDay,
    phaseId: e.phaseId ?? ((index % planPhases.value.length) + 1),
  }))
  await plan.createProgram(selectedPetId.value, program.value.programTitle, program.value.startDate, currentExercises)
  if (program.value) {
    program.value.notes = notesText
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center gap-3">
      <label class="text-sm font-medium text-navy">Patient</label>
      <select
        :value="selectedPetId ?? ''"
        class="rounded-lg border border-neutral-grey bg-white px-3 py-2 text-sm text-navy outline-none focus:border-sage"
        @change="selectPatient(Number(($event.target as HTMLSelectElement).value))"
      >
        <option v-for="pet in patientsStore.patients" :key="pet.petId" :value="pet.petId">
          {{ pet.petName }} ({{ pet.ownerName }})
        </option>
      </select>
    </div>

    <div v-if="planLoading" class="py-16 text-center text-sm text-neutral-muted">
      Loading treatment plan...
    </div>

    <template v-else-if="selectedPatient">
      <PlanPatientHeader :patient="selectedPatient" :program="program" />

      <section class="portal-card overflow-hidden">
        <PlanTabs v-model:active-tab="activeTab" />

        <div v-if="!hasProgram" class="empty-state m-4 py-16">
          <p class="text-sm text-neutral-muted">No treatment plan for this patient yet.</p>
          <BaseButton class="mt-4" size="sm" @click="showCreateModal = true">
            Create Treatment Plan
          </BaseButton>
        </div>

        <template v-else>
          <!-- Overview Tab -->
          <div v-if="activeTab === 'overview'" class="grid gap-4 p-4 xl:grid-cols-[220px_minmax(0,1fr)_240px]">
            <PlanPhasesSidebar
              :phases="planPhases"
              :active-phase-id="activePhaseId"
              @update:active-phase-id="activePhaseId = $event"
              @add-phase="showAddPhaseModal = true"
            />
            <PlanPhaseDetail
              :phase="activePhase"
              :exercises="phaseExercises"
              @edit-phase="openEditPhaseModal"
              @add-exercise="openAddExerciseModal"
            />
            <PlanDetailsSidebar
              :program="program"
              @add-note="activeTab = 'notes'"
            />
          </div>

          <!-- Goals Tab -->
          <PlanGoalsTab
            v-else-if="activeTab === 'goals'"
            :patient="selectedPatient"
            :phases="planPhases"
            :active-phase-id="activePhaseId"
          />

          <!-- Exercises Tab -->
          <PlanExercisesTab
            v-else-if="activeTab === 'exercises'"
            :program="program"
            :phases="planPhases"
            @add-exercise="openAddExerciseModal"
            @remove-exercise="removeExerciseFromPlan"
            @edit-prescription="openAddExerciseModal"
          />

          <!-- Notes Tab -->
          <PlanNotesTab
            v-else-if="activeTab === 'notes'"
            :patient="selectedPatient"
            :program="program"
            @save-notes="handleSaveNotes"
          />

          <!-- Progress Tab -->
          <PlanProgressTab
            v-else-if="activeTab === 'progress'"
            :patient="selectedPatient"
          />

          <!-- Documents Tab -->
          <PlanDocumentsTab
            v-else-if="activeTab === 'documents'"
            :patient="selectedPatient"
          />
        </template>
      </section>
    </template>

    <!-- Create Treatment Plan Modal -->
    <div
      v-if="showCreateModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
      @click.self="showCreateModal = false"
    >
      <div class="portal-card w-full max-w-md p-6 shadow-xl">
        <h3 class="text-lg font-bold text-navy">Create Treatment Plan</h3>
        <form class="mt-4 space-y-4" @submit.prevent="createPlan">
          <label class="block">
            <span class="text-sm font-medium text-navy">Plan title</span>
            <input
              v-model="createForm.title"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage"
              placeholder="e.g. Post-surgery rehabilitation"
            />
          </label>
          <label class="block">
            <span class="text-sm font-medium text-navy">Start date</span>
            <input
              v-model="createForm.startDate"
              type="date"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage"
            />
          </label>
          <div class="flex gap-3 pt-2">
            <BaseButton type="button" variant="secondary" class="flex-1" @click="showCreateModal = false">
              Cancel
            </BaseButton>
            <BaseButton type="submit" class="flex-1">Create</BaseButton>
          </div>
        </form>
      </div>
    </div>

    <!-- Phase Modals -->
    <AddPhaseModal
      :open="showAddPhaseModal"
      @close="showAddPhaseModal = false"
      @add="handleAddPhase"
    />
    <EditPhaseModal
      :open="showEditPhaseModal"
      :phase="selectedPhaseToEdit"
      @close="showEditPhaseModal = false"
      @save="handleSavePhase"
    />

    <!-- Add Exercise Selection Modal -->
    <div
      v-if="showAddExerciseModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
      @click.self="showAddExerciseModal = false"
    >
      <div class="portal-card flex max-h-[90vh] w-full max-w-2xl flex-col p-6 overflow-hidden shadow-2xl">
        <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-3">
          <div>
            <h3 class="text-lg font-bold text-navy">Add Exercise to Plan</h3>
            <p class="text-xs text-neutral-muted">Select an exercise from the library to prescribe</p>
          </div>
          <button
            type="button"
            class="text-neutral-muted hover:text-navy"
            @click="showAddExerciseModal = false"
          >
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="mt-4 space-y-4 overflow-y-auto flex-1 pr-1">
          <!-- Search input -->
          <div class="relative">
            <Search class="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
            <input
              v-model="exerciseSearchQuery"
              type="search"
              placeholder="Search exercise library by title, species, description..."
              class="w-full rounded-xl border border-neutral-grey bg-white py-2 pl-10 pr-4 text-sm text-navy outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
            />
          </div>

          <!-- Exercises list grid -->
          <div v-if="availableExercises.length === 0" class="py-8 text-center text-xs text-neutral-muted">
            No exercises match your search.
          </div>
          <div v-else class="grid gap-2.5 sm:grid-cols-2 max-h-[260px] overflow-y-auto pr-1">
            <div
              v-for="exercise in availableExercises"
              :key="exercise.exerciseId"
              class="flex cursor-pointer items-start gap-3 rounded-xl border p-3 transition-all"
              :class="
                selectedExerciseForPlan?.exerciseId === exercise.exerciseId
                  ? 'border-sage bg-sage/5 ring-2 ring-sage/20'
                  : 'border-neutral-grey/60 bg-white hover:border-sage/40'
              "
              @click="selectedExerciseForPlan = exercise"
            >
              <div class="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-sage/15 text-sage">
                <img
                  v-if="exercise.steps?.find((s: any) => s.imageUrl)?.imageUrl"
                  :src="exercise.steps.find((s: any) => s.imageUrl)!.imageUrl!"
                  :alt="exercise.title"
                  class="h-full w-full object-cover"
                />
                <Dumbbell v-else class="h-5 w-5" />
              </div>
              <div class="min-w-0 flex-1">
                <div class="flex items-center justify-between">
                  <p class="text-xs font-bold text-navy truncate">{{ exercise.title }}</p>
                  <Check
                    v-if="selectedExerciseForPlan?.exerciseId === exercise.exerciseId"
                    class="h-4 w-4 shrink-0 text-sage"
                  />
                </div>
                <p class="text-[11px] text-neutral-muted line-clamp-2 mt-0.5">
                  {{ exercise.shortDescription || 'Targeted rehabilitation exercise' }}
                </p>
                <span
                  v-if="exercise.targetSpecies"
                  class="mt-1 inline-block rounded bg-navy/5 px-1.5 py-0.5 text-[9px] font-semibold uppercase text-navy/70"
                >
                  {{ exercise.targetSpecies }}
                </span>
              </div>
            </div>
          </div>

          <!-- Prescription settings form -->
          <div v-if="selectedExerciseForPlan" class="rounded-xl border border-sage/30 bg-surface p-4 space-y-3">
            <p class="text-xs font-bold uppercase tracking-wider text-sage">
              Prescription Details for {{ selectedExerciseForPlan.title }}
            </p>
            <div class="grid grid-cols-3 gap-3">
              <label class="block">
                <span class="text-xs font-medium text-navy">Sets</span>
                <input
                  v-model.number="exerciseForm.sets"
                  type="number"
                  min="1"
                  max="20"
                  class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-1.5 text-sm"
                />
              </label>
              <label class="block">
                <span class="text-xs font-medium text-navy">Repetitions</span>
                <input
                  v-model.number="exerciseForm.repetitions"
                  type="number"
                  min="1"
                  max="100"
                  class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-1.5 text-sm"
                />
              </label>
              <label class="block">
                <span class="text-xs font-medium text-navy">Frequency/Day</span>
                <input
                  v-model.number="exerciseForm.frequencyPerDay"
                  type="number"
                  min="1"
                  max="10"
                  class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-1.5 text-sm"
                />
              </label>
            </div>
          </div>
        </div>

        <div class="mt-4 flex gap-3 border-t border-neutral-grey/60 pt-3">
          <BaseButton
            type="button"
            variant="secondary"
            class="flex-1"
            @click="showAddExerciseModal = false"
          >
            Cancel
          </BaseButton>
          <BaseButton
            type="button"
            class="flex-1"
            :disabled="!selectedExerciseForPlan || addingExercise"
            @click="addExerciseToPlan"
          >
            {{ addingExercise ? 'Adding...' : 'Add to Treatment Plan' }}
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Stub Modal -->
    <div
      v-if="showStubModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
      @click.self="showStubModal = false"
    >
      <div class="portal-card max-w-sm p-6 text-center">
        <p class="text-sm text-neutral-muted">{{ stubMessage }}</p>
        <BaseButton class="mt-4" size="sm" @click="showStubModal = false">Close</BaseButton>
      </div>
    </div>
  </div>
</template>

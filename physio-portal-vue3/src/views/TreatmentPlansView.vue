<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PlanDetailsSidebar from '../components/plans/PlanDetailsSidebar.vue'
import PlanPatientHeader from '../components/plans/PlanPatientHeader.vue'
import PlanPhaseDetail from '../components/plans/PlanPhaseDetail.vue'
import PlanPhasesSidebar from '../components/plans/PlanPhasesSidebar.vue'
import PlanTabs from '../components/plans/PlanTabs.vue'
import BaseButton from '../components/BaseButton.vue'
import { useTreatmentPlan } from '../composables/useTreatmentPlan'
import { DEFAULT_PHASES } from '../data/planDemo'
import { usePatientsStore } from '../store/patients'

const patientsStore = usePatientsStore()
const route = useRoute()
const router = useRouter()

const activeTab = ref<'overview' | 'goals' | 'exercises' | 'notes' | 'progress' | 'documents'>('overview')
const activePhaseId = ref(1)
const showCreateModal = ref(false)
const showStubModal = ref(false)
const stubMessage = ref('')
const createForm = reactive({ title: '', startDate: new Date().toISOString().slice(0, 10) })

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
  () => DEFAULT_PHASES.find((p) => p.id === activePhaseId.value) ?? DEFAULT_PHASES[0]!,
)

onMounted(async () => {
  await patientsStore.fetchClinicPatients().catch(() => undefined)
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

function showStub(message: string) {
  stubMessage.value = message
  showStubModal.value = true
}

async function createPlan() {
  if (!selectedPetId.value || !createForm.title) return
  await plan.createProgram(selectedPetId.value, createForm.title, createForm.startDate)
  showCreateModal.value = false
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

        <div v-if="activeTab !== 'overview'" class="empty-state m-4 py-16">
          <p class="text-sm text-neutral-muted capitalize">{{ activeTab }} view coming soon.</p>
        </div>

        <div v-else-if="!hasProgram" class="empty-state m-4 py-16">
          <p class="text-sm text-neutral-muted">No treatment plan for this patient yet.</p>
          <BaseButton class="mt-4" size="sm" @click="showCreateModal = true">
            Create Treatment Plan
          </BaseButton>
        </div>

        <div v-else class="grid gap-4 p-4 xl:grid-cols-[220px_minmax(0,1fr)_240px]">
          <PlanPhasesSidebar
            :phases="DEFAULT_PHASES"
            :active-phase-id="activePhaseId"
            @update:active-phase-id="activePhaseId = $event"
            @add-phase="showStub('Phase management coming soon.')"
          />
          <PlanPhaseDetail
            :phase="activePhase"
            :program="program"
            @edit-phase="showStub('Edit phase coming soon.')"
          />
          <PlanDetailsSidebar
            :program="program"
            @add-note="showStub('Notes editing coming soon.')"
          />
        </div>
      </section>
    </template>

    <div
      v-if="showCreateModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
      @click.self="showCreateModal = false"
    >
      <div class="portal-card w-full max-w-md p-6">
        <h3 class="text-lg font-bold text-navy">Create Treatment Plan</h3>
        <form class="mt-4 space-y-4" @submit.prevent="createPlan">
          <label class="block">
            <span class="text-sm font-medium text-navy">Plan title</span>
            <input
              v-model="createForm.title"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
              placeholder="e.g. Post-surgery rehabilitation"
            />
          </label>
          <label class="block">
            <span class="text-sm font-medium text-navy">Start date</span>
            <input
              v-model="createForm.startDate"
              type="date"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
            />
          </label>
          <div class="flex gap-3">
            <BaseButton type="button" variant="secondary" class="flex-1" @click="showCreateModal = false">
              Cancel
            </BaseButton>
            <BaseButton type="submit" class="flex-1">Create</BaseButton>
          </div>
        </form>
      </div>
    </div>

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

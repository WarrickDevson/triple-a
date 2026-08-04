<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AddPatientModal from '../components/patients/AddPatientModal.vue'
import PatientDetailPanel from '../components/patients/PatientDetailPanel.vue'
import PatientListPanel from '../components/patients/PatientListPanel.vue'
import PatientOutcomePanel from '../components/patients/PatientOutcomePanel.vue'
import { usePatientDetail } from '../composables/usePatientDetail'
import { usePatientsStore } from '../store/patients'

const patientsStore = usePatientsStore()
const route = useRoute()
const router = useRouter()

const mobileView = ref<'list' | 'detail'>('list')
const showAddModal = ref(false)

const selectedPetId = computed(() => {
  const param = route.params.petId
  if (param) return Number(param)
  return patientsStore.patients[0]?.petId ?? null
})

const selectedPatient = computed(() => {
  if (!selectedPetId.value) return null
  return patientsStore.getPatientById(selectedPetId.value)
})

const detail = usePatientDetail(() => selectedPatient.value)

const demoMeta = computed(() => detail.demoMeta.value)
const activeProgram = computed(() => detail.activeProgram.value)
const nextAppointment = computed(() => detail.nextAppointment.value)
const progressPercent = computed(() => detail.progressPercent.value)
const detailLoading = computed(() => detail.loading.value)
const patientProgress = computed(() => detail.progress.value)
const latestVideo = computed(() => detail.latestVideo.value)

onMounted(async () => {
  await patientsStore.fetchClinicPatients().catch(() => undefined)
  syncRouteSelection()
})

watch(
  () => patientsStore.patients,
  () => syncRouteSelection(),
  { deep: true },
)

function syncRouteSelection() {
  if (patientsStore.patients.length === 0) return

  const paramId = route.params.petId ? Number(route.params.petId) : null
  const exists = paramId ? patientsStore.getPatientById(paramId) : null

  if (!exists) {
    const firstId = patientsStore.patients[0]!.petId
    router.replace({ name: 'patient-detail', params: { petId: firstId } })
  }
}

function selectPatient(petId: number) {
  router.push({ name: 'patient-detail', params: { petId } })
  mobileView.value = 'detail'
}

function backToList() {
  mobileView.value = 'list'
}

function onPatientCreated(petId: number) {
  selectPatient(petId)
}
</script>

<template>
  <div class="grid gap-4 xl:grid-cols-[280px_minmax(0,1fr)_320px]">
    <AddPatientModal
      v-if="showAddModal"
      @close="showAddModal = false"
      @created="onPatientCreated"
    />
    <div
      class="min-h-[420px] xl:min-h-[calc(100vh-10rem)]"
      :class="mobileView === 'detail' ? 'hidden xl:block' : 'block'"
    >
      <PatientListPanel
        :patients="patientsStore.patients"
        :selected-pet-id="selectedPetId"
        :loading="patientsStore.loading"
        @select="selectPatient"
        @add="showAddModal = true"
      />
    </div>

    <div
      class="min-h-[420px] xl:min-h-[calc(100vh-10rem)]"
      :class="mobileView === 'list' ? 'hidden xl:block' : 'block'"
    >
      <PatientDetailPanel
        :patient="selectedPatient"
        :demo-meta="demoMeta"
        :active-program="activeProgram"
        :next-appointment="nextAppointment"
        :progress-percent="progressPercent"
        :loading="detailLoading"
        :show-back="mobileView === 'detail'"
        @back="backToList"
      />
    </div>

    <div
      class="min-h-0 xl:min-h-[calc(100vh-10rem)]"
      :class="mobileView === 'list' ? 'hidden xl:block' : 'block xl:block'"
    >
      <PatientOutcomePanel
        :progress="patientProgress"
        :latest-video="latestVideo"
        :loading="detailLoading"
      />
    </div>
  </div>
</template>

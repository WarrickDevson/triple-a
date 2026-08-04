<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PatientListPanel from '../components/patients/PatientListPanel.vue'
import PatientOutcomePanel from '../components/patients/PatientOutcomePanel.vue'
import ProgressDetailPanel from '../components/progress/ProgressDetailPanel.vue'
import ProgressSummaryStrip from '../components/progress/ProgressSummaryStrip.vue'
import { usePetProgress } from '../composables/usePetProgress'
import { usePatientsStore } from '../store/patients'

const patientsStore = usePatientsStore()
const route = useRoute()
const router = useRouter()

const mobileView = ref<'list' | 'detail'>('list')

const selectedPetId = computed(() => {
  const param = route.params.petId
  if (param) return Number(param)
  return patientsStore.patients[0]?.petId ?? null
})

const selectedPatient = computed(() => {
  if (!selectedPetId.value) return null
  return patientsStore.getPatientById(selectedPetId.value)
})

const progressData = usePetProgress(() => selectedPetId.value)

const progress = computed(() => progressData.progress.value)
const latestVideo = computed(() => progressData.latestVideo.value)
const loading = computed(() => progressData.loading.value)

onMounted(async () => {
  await patientsStore.fetchClinicPatients().catch(() => undefined)
  syncRoute()
})

watch(() => patientsStore.patients, syncRoute, { deep: true })

function syncRoute() {
  if (patientsStore.patients.length === 0) return
  const paramId = route.params.petId ? Number(route.params.petId) : null
  if (!paramId || !patientsStore.getPatientById(paramId)) {
    router.replace({ name: 'progress-detail', params: { petId: patientsStore.patients[0]!.petId } })
  }
}

function selectPatient(petId: number) {
  router.push({ name: 'progress-detail', params: { petId } })
  mobileView.value = 'detail'
}
</script>

<template>
  <div class="space-y-4">
    <ProgressSummaryStrip />

    <div class="grid gap-4 xl:grid-cols-[280px_minmax(0,1fr)_280px]">
      <div
        class="min-h-[600px]"
        :class="mobileView === 'detail' ? 'hidden xl:block' : 'block'"
      >
        <PatientListPanel
          :patients="patientsStore.patients"
          :selected-pet-id="selectedPetId"
          :loading="patientsStore.loading"
          @select="selectPatient"
        />
      </div>

      <div
        class="min-h-[600px]"
        :class="mobileView === 'list' ? 'hidden xl:block' : 'block'"
      >
        <ProgressDetailPanel
          :patient="selectedPatient"
          :progress="progress"
          :loading="loading"
          :show-back="mobileView === 'detail'"
          @back="mobileView = 'list'"
        />
      </div>

      <div class="min-h-0 hidden xl:block">
        <PatientOutcomePanel
          :progress="progress"
          :latest-video="latestVideo"
          :loading="loading"
        />
      </div>
    </div>
  </div>
</template>

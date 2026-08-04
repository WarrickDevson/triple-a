<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import ReportsPatientList from '../components/reports/ReportsPatientList.vue'
import ReportTypesPanel from '../components/reports/ReportTypesPanel.vue'
import RecentReportsPanel from '../components/reports/RecentReportsPanel.vue'
import BaseButton from '../components/BaseButton.vue'
import { downloadPetReport } from '../api/reports'
import { demoReportHistory } from '../data/reportsDemo'
import { usePatientsStore } from '../store/patients'

const patientsStore = usePatientsStore()
const selectedPetId = ref<number | null>(null)
const downloading = ref(false)
const showStubModal = ref(false)
const stubMessage = ref('')

const selectedPatient = computed(() => {
  if (!selectedPetId.value) return null
  return patientsStore.getPatientById(selectedPetId.value)
})

onMounted(async () => {
  await patientsStore.fetchClinicPatients().catch(() => undefined)
  if (patientsStore.patients[0]) {
    selectedPetId.value = patientsStore.patients[0].petId
  }
})

async function downloadReport() {
  if (!selectedPetId.value) return
  downloading.value = true
  try {
    await downloadPetReport(selectedPetId.value)
  } catch {
    showStub('Unable to download report. Please try again.')
  } finally {
    downloading.value = false
  }
}

function showStub(message: string) {
  stubMessage.value = message
  showStubModal.value = true
}
</script>

<template>
  <div class="grid gap-4 xl:grid-cols-[260px_minmax(0,1fr)_280px]">
    <ReportsPatientList
      :patients="patientsStore.patients"
      :selected-pet-id="selectedPetId"
      :loading="patientsStore.loading"
      @select="selectedPetId = $event"
    />
    <ReportTypesPanel
      :patient="selectedPatient"
      :downloading="downloading"
      @download="downloadReport"
      @stub="showStub"
    />
    <RecentReportsPanel :reports="demoReportHistory" />
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
</template>

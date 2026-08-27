<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Sparkles, CheckCircle2, FileText } from '@lucide/vue'
import ReportsPatientList from '../components/reports/ReportsPatientList.vue'
import ReportTypesPanel from '../components/reports/ReportTypesPanel.vue'
import RecentReportsPanel from '../components/reports/RecentReportsPanel.vue'
import CreateReportModal from '../components/reports/CreateReportModal.vue'
import ReportDetailModal from '../components/reports/ReportDetailModal.vue'
import BaseButton from '../components/BaseButton.vue'
import {
  downloadPetReport,
  downloadSharedReport,
  fetchRecentReports,
  createReport,
  deleteSharedReport,
  shareDocument,
  type DownloadReportOptions,
} from '../api/reports'
import type { CreateReportPayload, SharedReport } from '../types/soap'
import { usePatientsStore } from '../store/patients'

const patientsStore = usePatientsStore()
const selectedPetId = ref<number | null>(null)
const recentReports = ref<SharedReport[]>([])
const reportsLoading = ref(false)
const downloading = ref(false)
const saving = ref(false)

// Modals
const showCreateModal = ref(false)
const createModalType = ref<string | null>(null)
const selectedDetailReport = ref<SharedReport | null>(null)

// Toast notification
const toastMessage = ref<string | null>(null)
let toastTimeout: any = null

function showToast(message: string) {
  toastMessage.value = message
  if (toastTimeout) clearTimeout(toastTimeout)
  toastTimeout = setTimeout(() => {
    toastMessage.value = null
  }, 4000)
}

const selectedPatient = computed(() => {
  if (!selectedPetId.value) return null
  return patientsStore.getPatientById(selectedPetId.value)
})

async function loadReports() {
  reportsLoading.value = true
  try {
    const list = await fetchRecentReports()
    recentReports.value = list
  } catch {
    // Keep existing
  } finally {
    reportsLoading.value = false
  }
}

onMounted(async () => {
  await Promise.all([
    patientsStore.fetchClinicPatients().catch(() => undefined),
    loadReports(),
  ])
  if (patientsStore.patients[0]) {
    selectedPetId.value = patientsStore.patients[0].petId
  }
})

// Open create modal with pre-selected type
function openCreateModal(typeId: string = 'progress') {
  createModalType.value = typeId
  showCreateModal.value = true
}

// Quick download from report card
async function handleQuickDownload(typeId: string) {
  if (!selectedPetId.value) return
  downloading.value = true
  try {
    const targetPatient = selectedPatient.value
    await downloadPetReport(selectedPetId.value, { type: typeId, patient: targetPatient })
    showToast('Report PDF downloaded successfully.')
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Unable to download report. Please try again.')
  } finally {
    downloading.value = false
  }
}

// Custom quick download from modal
async function handleCustomQuickDownload(petId: number, options: DownloadReportOptions) {
  downloading.value = true
  try {
    const targetPatient = patientsStore.getPatientById(petId) || selectedPatient.value
    await downloadPetReport(petId, { ...options, patient: targetPatient })
    showToast('Customized PDF downloaded.')
    showCreateModal.value = false
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Download failed.')
  } finally {
    downloading.value = false
  }
}

// Save to reports database & download PDF
async function handleSaveAndDownload(payload: CreateReportPayload) {
  saving.value = true
  downloading.value = true
  try {
    const targetPatient = patientsStore.getPatientById(payload.petId) || selectedPatient.value
    const created = await createReport(payload, targetPatient)
    recentReports.value = [created, ...recentReports.value.filter((r) => r.sharedReportId !== created.sharedReportId)]
    showToast(`Report "${payload.title}" saved and published.`)
    showCreateModal.value = false

    // Trigger PDF download
    await downloadPetReport(payload.petId, {
      type: payload.reportType.toLowerCase().replace('_', '-'),
      customTitle: payload.title,
      summary: payload.summary,
      dischargeStatus: payload.dischargeStatus,
      maintenancePlan: payload.maintenancePlan,
      veterinarianNotes: payload.veterinarianNotes,
      patient: targetPatient,
    })
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Failed to save and generate report.')
  } finally {
    saving.value = false
    downloading.value = false
  }
}

// Save only (no PDF download)
async function handleSaveOnly(payload: CreateReportPayload) {
  saving.value = true
  try {
    const targetPatient = patientsStore.getPatientById(payload.petId) || selectedPatient.value
    const created = await createReport(payload, targetPatient)
    recentReports.value = [created, ...recentReports.value.filter((r) => r.sharedReportId !== created.sharedReportId)]
    showToast(`Report "${payload.title}" saved to existing reports.`)
    showCreateModal.value = false
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Failed to save report.')
  } finally {
    saving.value = false
  }
}

// View details for a report
function handleViewReport(report: SharedReport) {
  selectedDetailReport.value = report
}

// Download existing saved report
async function handleDownloadExistingReport(report: SharedReport) {
  downloading.value = true
  try {
    await downloadSharedReport(report.sharedReportId, `${report.title.replace(/\s+/g, '_')}.pdf`)
    showToast(`Downloaded "${report.title}".`)
  } catch {
    showToast('Unable to download report.')
  } finally {
    downloading.value = false
  }
}

// Toggle share with owner
async function handleToggleShare(report: SharedReport) {
  const currentStatus = report.isActive !== false
  const newStatus = !currentStatus
  report.isActive = newStatus

  try {
    if (newStatus) {
      await shareDocument(report.petId, {
        title: report.title,
        reportType: report.reportType,
        summary: report.summary || undefined,
        soapNoteId: report.soapNoteId || undefined,
      })
      showToast(`Report shared with ${report.ownerName || 'owner'}.`)
    } else {
      await deleteSharedReport(report.sharedReportId)
      showToast('Report unpublished from owner portal.')
    }
  } catch {
    showToast(`Report share status updated.`)
  }
}

// Delete report
async function handleDeleteReport(reportId: number) {
  try {
    await deleteSharedReport(reportId)
    recentReports.value = recentReports.value.filter((r) => r.sharedReportId !== reportId)
    selectedDetailReport.value = null
    showToast('Report deleted successfully.')
  } catch {
    showToast('Failed to delete report.')
  }
}
</script>

<template>
  <div class="space-y-4">
    <!-- Top Action Banner -->
    <div class="portal-card p-4 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-lg font-bold text-navy flex items-center gap-2">
          <FileText class="h-5 w-5 text-sage" />
          Clinical Reports & Summaries
        </h1>
        <p class="text-xs text-neutral-muted">
          Generate, customize, view summaries, and export clinical PDFs (Progress Reports, Discharge Summaries, Owner Home Programs, and SOAP Assessments).
        </p>
      </div>

      <div class="flex items-center gap-2">
        <BaseButton
          size="sm"
          variant="accent"
          @click="openCreateModal('progress')"
        >
          <Sparkles class="h-4 w-4" />
          + Generate New Report
        </BaseButton>
      </div>
    </div>

    <!-- 3-Column Layout: Patient List | Generator Cards | Existing Reports List -->
    <div class="grid gap-4 xl:grid-cols-[260px_minmax(0,1fr)_340px]">
      <ReportsPatientList
        :patients="patientsStore.patients"
        :selected-pet-id="selectedPetId"
        :loading="patientsStore.loading"
        @select="selectedPetId = $event"
      />

      <ReportTypesPanel
        :patient="selectedPatient"
        :downloading="downloading"
        @customize="openCreateModal($event)"
        @quick-download="handleQuickDownload($event)"
        @open-new="openCreateModal('progress')"
      />

      <RecentReportsPanel
        :reports="recentReports"
        :selected-pet-id="selectedPetId"
        :loading="reportsLoading"
        @view="handleViewReport"
        @download="handleDownloadExistingReport"
        @create-new="openCreateModal('progress')"
      />
    </div>

    <!-- Create Report Modal -->
    <CreateReportModal
      v-if="showCreateModal"
      :patients="patientsStore.patients"
      :initial-pet-id="selectedPetId"
      :initial-type="createModalType"
      :saving="saving"
      :downloading="downloading"
      @close="showCreateModal = false"
      @save-and-download="handleSaveAndDownload"
      @save-only="handleSaveOnly"
      @quick-download="handleCustomQuickDownload"
    />

    <!-- View Report Detail & Summary Modal -->
    <ReportDetailModal
      v-if="selectedDetailReport"
      :report="selectedDetailReport"
      :downloading="downloading"
      @close="selectedDetailReport = null"
      @download="handleDownloadExistingReport"
      @toggle-share="handleToggleShare"
      @delete="handleDeleteReport"
    />

    <!-- Floating Toast Notification -->
    <Transition
      enter-active-class="transform transition ease-out duration-200"
      enter-from-class="translate-y-2 opacity-0"
      enter-to-class="translate-y-0 opacity-100"
      leave-active-class="transition ease-in duration-150"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="toastMessage"
        class="fixed bottom-6 right-6 z-50 flex items-center gap-2 rounded-xl bg-navy px-4 py-3 text-xs font-semibold text-white shadow-xl border border-sage/40"
      >
        <CheckCircle2 class="h-4 w-4 text-sage shrink-0" />
        <span>{{ toastMessage }}</span>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { FileText, Download, Eye, Plus } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { downloadPetReport } from '../../api/reports'
import { useDocumentsStore } from '../../store/documents'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patient: Pet
}>()

const documentsStore = useDocumentsStore()

const patientDocs = computed(() =>
  documentsStore.documents.filter(
    (d) => d.petName.toLowerCase() === props.patient.petName.toLowerCase(),
  ),
)

async function handleDownloadReport() {
  try {
    await downloadPetReport(props.patient.petId)
  } catch {
    documentsStore.showToast('Downloading clinical PDF report...')
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Clinical File Records</h4>
      <div class="flex gap-2">
        <BaseButton size="sm" variant="secondary" @click="documentsStore.openUpload">
          <Plus class="h-3.5 w-3.5" />
          Upload File
        </BaseButton>
        <BaseButton size="sm" @click="handleDownloadReport">
          <Download class="h-3.5 w-3.5" />
          Download PDF Report
        </BaseButton>
      </div>
    </div>

    <div v-if="patientDocs.length === 0" class="portal-card p-6 text-center">
      <FileText class="mx-auto h-8 w-8 text-neutral-muted/60 mb-2" />
      <p class="text-xs font-semibold text-navy">No records uploaded for {{ patient.petName }} yet.</p>
      <p class="text-[11px] text-neutral-muted mt-0.5">Generate a clinical progress PDF report or upload referral letters.</p>
      <BaseButton class="mt-3" size="sm" @click="handleDownloadReport">
        <Download class="h-3.5 w-3.5" />
        Generate PDF Report
      </BaseButton>
    </div>

    <div v-else class="space-y-2">
      <div
        v-for="doc in patientDocs"
        :key="doc.id"
        class="portal-card p-3 flex items-center justify-between gap-3 hover:bg-surface/80 transition-colors"
      >
        <div class="flex items-center gap-3 min-w-0">
          <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-sage-muted text-sage">
            <FileText class="h-4 w-4" />
          </div>
          <div class="min-w-0">
            <p class="text-xs font-bold text-navy truncate">{{ doc.name }}</p>
            <p class="text-[10px] text-neutral-muted">{{ doc.category }} · Uploaded {{ doc.uploadedAt }}</p>
          </div>
        </div>

        <div class="flex items-center gap-2 shrink-0 text-xs font-semibold">
          <button
            type="button"
            class="text-sage hover:text-navy inline-flex items-center gap-1"
            @click="documentsStore.openPreview(doc)"
          >
            <Eye class="h-3.5 w-3.5" />
            Preview
          </button>
          <button
            type="button"
            class="text-sage hover:text-navy inline-flex items-center gap-1"
            @click="documentsStore.downloadDocument(doc)"
          >
            <Download class="h-3.5 w-3.5" />
            Download
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

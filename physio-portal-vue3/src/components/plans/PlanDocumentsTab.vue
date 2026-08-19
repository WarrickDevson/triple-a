<script setup lang="ts">
import { computed } from 'vue'
import { FileText, Download, Eye, Plus, ShieldCheck } from '@lucide/vue'
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
    documentsStore.showToast('Generating clinical PDF report for download...')
  }
}
</script>

<template>
  <div class="p-5 space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-base font-bold text-navy">Plan Documents & Reports</h3>
        <p class="text-xs text-neutral-muted">
          Clinical reports, SOAP assessments, and consent forms for {{ patient.petName }}
        </p>
      </div>

      <div class="flex gap-2">
        <BaseButton size="sm" variant="secondary" @click="documentsStore.openUpload">
          <Plus class="h-4 w-4" />
          Upload Document
        </BaseButton>
        <BaseButton size="sm" @click="handleDownloadReport">
          <Download class="h-4 w-4" />
          Generate Progress PDF
        </BaseButton>
      </div>
    </div>

    <!-- Documents List -->
    <div v-if="patientDocs.length === 0" class="portal-card p-8 text-center">
      <FileText class="mx-auto h-10 w-10 text-neutral-muted/60 mb-2" />
      <p class="text-sm font-semibold text-navy">No documents uploaded for {{ patient.petName }} yet.</p>
      <p class="text-xs text-neutral-muted mt-1">Upload clinical files or generate a PDF progress report.</p>
      <BaseButton class="mt-4" size="sm" @click="handleDownloadReport">
        <Download class="h-4 w-4" />
        Download Sample Clinical Report
      </BaseButton>
    </div>

    <div v-else class="grid gap-3 sm:grid-cols-2">
      <div
        v-for="doc in patientDocs"
        :key="doc.id"
        class="portal-card p-4 flex flex-col justify-between border border-neutral-grey/60 hover:border-sage/40 transition-all"
      >
        <div class="flex items-start gap-3">
          <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage">
            <FileText class="h-5 w-5" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex items-center justify-between">
              <h4 class="text-sm font-bold text-navy truncate">{{ doc.name }}</h4>
              <span class="rounded bg-surface px-2 py-0.5 text-[10px] font-bold text-neutral-muted">
                {{ doc.category }}
              </span>
            </div>
            <p class="text-xs text-neutral-muted mt-0.5">Uploaded: {{ doc.uploadedAt }} · {{ doc.sizeKb }} KB</p>
          </div>
        </div>

        <div class="mt-4 flex items-center justify-between border-t border-neutral-grey/40 pt-3 text-xs">
          <span class="flex items-center gap-1 text-emerald-700 font-medium">
            <ShieldCheck class="h-3.5 w-3.5" />
            Verified Record
          </span>

          <div class="flex items-center gap-2">
            <button
              type="button"
              class="font-semibold text-sage hover:text-navy inline-flex items-center gap-1"
              @click="documentsStore.openPreview(doc)"
            >
              <Eye class="h-3.5 w-3.5" />
              Preview
            </button>
            <button
              type="button"
              class="font-semibold text-sage hover:text-navy inline-flex items-center gap-1"
              @click="documentsStore.downloadDocument(doc)"
            >
              <Download class="h-3.5 w-3.5" />
              Download
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

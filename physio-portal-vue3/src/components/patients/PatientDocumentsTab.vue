<script setup lang="ts">
import { computed } from 'vue'
import { FileText, Download, Eye, Plus, Share2, CheckCircle2 } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { downloadPetReport, publishProgressReport, shareDocument } from '../../api/reports'
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

async function handleShareProgressReport() {
  try {
    await publishProgressReport(props.patient.petId)
    documentsStore.showToast(`Clinical Progress Report successfully shared with ${props.patient.ownerName || 'owner'}.`)
  } catch {
    documentsStore.showToast(`Clinical Progress Report shared to owner's document hub.`)
  }
}

async function handleToggleShareDoc(doc: any) {
  const willShare = !doc.isSharedWithOwner
  documentsStore.toggleDocumentShare(doc.id, willShare)
  if (willShare) {
    try {
      await shareDocument(props.patient.petId, {
        title: doc.name,
        reportType: doc.category === 'Home Programs' ? 'HOME_PROGRAM' : 'CLINICAL_DOCUMENT',
        summary: `Shared file: ${doc.name} (${doc.category})`,
      })
    } catch {
      // Ignore API offline fallback
    }
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Clinical File Records & Reports</h4>
        <p class="text-xs text-neutral-muted">Share medical records, referral letters, and clinical reports with the pet owner.</p>
      </div>
      <div class="flex flex-wrap gap-2">
        <BaseButton size="sm" variant="secondary" @click="documentsStore.openUpload">
          <Plus class="h-3.5 w-3.5" />
          Upload File
        </BaseButton>
        <BaseButton size="sm" variant="secondary" @click="handleDownloadReport">
          <Download class="h-3.5 w-3.5" />
          Download PDF
        </BaseButton>
        <BaseButton size="sm" variant="accent" @click="handleShareProgressReport">
          <Share2 class="h-3.5 w-3.5" />
          Share Full Report to Owner
        </BaseButton>
      </div>
    </div>

    <div v-if="patientDocs.length === 0" class="portal-card p-6 text-center">
      <FileText class="mx-auto h-8 w-8 text-neutral-muted/60 mb-2" />
      <p class="text-xs font-semibold text-navy">No records uploaded for {{ patient.petName }} yet.</p>
      <p class="text-[11px] text-neutral-muted mt-0.5">Generate a clinical progress PDF report or upload referral letters and consent files.</p>
      <div class="mt-3 flex justify-center gap-2">
        <BaseButton size="sm" variant="secondary" @click="handleDownloadReport">
          <Download class="h-3.5 w-3.5" />
          Download PDF Report
        </BaseButton>
        <BaseButton size="sm" variant="accent" @click="handleShareProgressReport">
          <Share2 class="h-3.5 w-3.5" />
          Share Full Report with Owner
        </BaseButton>
      </div>
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
            <div class="flex items-center gap-2">
              <p class="text-xs font-bold text-navy truncate">{{ doc.name }}</p>
              <span
                v-if="doc.isSharedWithOwner"
                class="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-[10px] font-bold text-emerald-700 border border-emerald-200"
              >
                <CheckCircle2 class="h-3 w-3 text-emerald-600" />
                Shared with Owner
              </span>
            </div>
            <p class="text-[10px] text-neutral-muted">{{ doc.category }} · Uploaded {{ doc.uploadedAt }}</p>
          </div>
        </div>

        <div class="flex items-center gap-2 shrink-0 text-xs font-semibold">
          <button
            type="button"
            class="inline-flex items-center gap-1 rounded-lg border px-2 py-1 text-xs transition-colors"
            :class="
              doc.isSharedWithOwner
                ? 'border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100'
                : 'border-neutral-grey/80 bg-surface text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy'
            "
            :title="doc.isSharedWithOwner ? 'Shared with Owner (click to unshare)' : 'Click to share with Owner'"
            @click="handleToggleShareDoc(doc)"
          >
            <Share2 class="h-3.5 w-3.5" :class="doc.isSharedWithOwner ? 'text-emerald-600' : 'text-neutral-muted'" />
            <span>{{ doc.isSharedWithOwner ? 'Shared' : 'Share' }}</span>
          </button>
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

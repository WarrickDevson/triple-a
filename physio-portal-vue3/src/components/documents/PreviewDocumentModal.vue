<script setup lang="ts">
import { computed } from 'vue'
import { Download, ExternalLink, FileText, X, ShieldCheck } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { formatFileSize, type DocumentItem } from '../../data/documentsDemo'
import { useDocumentsStore } from '../../store/documents'

const props = defineProps<{
  open: boolean
  document: DocumentItem | null
}>()

const emit = defineEmits<{
  close: []
}>()

const documentsStore = useDocumentsStore()

const isImage = computed(() => {
  if (!props.document?.fileDataUrl && !props.document?.fileType) return false
  const type = props.document.fileType?.toLowerCase() || ''
  const url = props.document.fileDataUrl?.toLowerCase() || ''
  return type.startsWith('image/') || url.startsWith('data:image/')
})

const isPdf = computed(() => {
  if (!props.document?.fileDataUrl && !props.document?.fileType) return false
  const type = props.document.fileType?.toLowerCase() || ''
  const url = props.document.fileDataUrl?.toLowerCase() || ''
  return type.includes('pdf') || url.startsWith('data:application/pdf')
})

function handleDownload() {
  if (props.document) {
    documentsStore.downloadDocument(props.document)
  }
}

function handleOpenNewTab() {
  if (!props.document) return

  if (props.document.fileDataUrl) {
    const win = window.open()
    if (win) {
      win.document.write(
        `<iframe src="${props.document.fileDataUrl}" frameborder="0" style="border:0; top:0px; left:0px; bottom:0px; right:0px; width:100%; height:100%;" allowfullscreen></iframe>`,
      )
    }
  } else {
    // Generate text print preview
    const win = window.open('', '_blank')
    if (win) {
      win.document.write(`
        <html>
          <head>
            <title>${props.document.name}</title>
            <style>
              body { font-family: system-ui, sans-serif; padding: 40px; color: #1e293b; max-width: 800px; margin: 0 auto; }
              h1 { color: #0f172a; border-bottom: 2px solid #cbd5e1; padding-bottom: 12px; }
              .meta { background: #f8fafc; padding: 16px; border-radius: 8px; margin-bottom: 24px; }
              .meta dt { color: #64748b; font-size: 12px; font-weight: 600; text-transform: uppercase; }
              .meta dd { font-weight: 600; font-size: 14px; margin: 0 0 8px 0; }
              .content { line-height: 1.6; font-size: 15px; background: #ffffff; border: 1px solid #e2e8f0; padding: 24px; border-radius: 8px; }
            </style>
          </head>
          <body>
            <h1>${props.document.name}</h1>
            <div class="meta">
              <dt>Patient</dt><dd>${props.document.petName}</dd>
              <dt>Owner</dt><dd>${props.document.ownerName}</dd>
              <dt>Category</dt><dd>${props.document.category}</dd>
              <dt>Uploaded</dt><dd>${props.document.uploadedAt}</dd>
            </div>
            <div class="content">
              <h3>Clinical Document Record</h3>
              <p>This is a verified clinical rehabilitation record for <strong>${props.document.petName}</strong> under the care of Triple A Veterinary Physiotherapy Clinic.</p>
              <p>Category: <strong>${props.document.category}</strong></p>
              <p>Status: Verified & Confidential</p>
            </div>
            <script>window.print();<\/script>
          </body>
        </html>
      `)
    }
  }
}
</script>

<template>
  <div
    v-if="open && document"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div
      class="portal-card flex h-[85vh] w-full max-w-4xl flex-col overflow-hidden shadow-2xl animate-in fade-in zoom-in-95"
    >
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-neutral-grey/80 px-6 py-4">
        <div class="flex items-center gap-3">
          <div
            class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage"
          >
            <FileText class="h-5 w-5" />
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h2 class="text-base font-bold text-navy">{{ document.name }}</h2>
              <span
                class="rounded-full bg-sage-muted px-2.5 py-0.5 text-xs font-bold text-sage"
              >
                {{ document.category }}
              </span>
            </div>
            <p class="text-xs text-neutral-muted">
              Patient: <strong class="text-navy">{{ document.petName }}</strong> · Owner:
              {{ document.ownerName }} · Uploaded: {{ document.uploadedAt }} ·
              {{ formatFileSize(document.sizeKb) }}
            </p>
          </div>
        </div>

        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted hover:bg-surface hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Preview Canvas Body -->
      <div class="flex-1 overflow-y-auto bg-surface/60 p-6">
        <!-- Image Preview -->
        <div v-if="isImage" class="flex h-full items-center justify-center">
          <img
            :src="document.fileDataUrl"
            :alt="document.name"
            class="max-h-full max-w-full rounded-xl border border-neutral-grey/80 object-contain shadow-md"
          />
        </div>

        <!-- PDF Preview -->
        <div v-else-if="isPdf" class="h-full w-full rounded-xl overflow-hidden border border-neutral-grey/80 bg-white">
          <iframe :src="document.fileDataUrl" class="h-full w-full border-0"></iframe>
        </div>

        <!-- Default Document Card Preview -->
        <div v-else class="mx-auto max-w-2xl">
          <div class="portal-card p-8 shadow-sm">
            <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-4">
              <div>
                <span class="text-xs font-bold uppercase tracking-wider text-sage">
                  Triple A Veterinary Physiotherapy
                </span>
                <h3 class="text-xl font-extrabold text-navy mt-1">{{ document.name }}</h3>
              </div>
              <div class="flex items-center gap-1 text-xs font-semibold text-emerald-700 bg-emerald-50 px-2.5 py-1 rounded-lg border border-emerald-200">
                <ShieldCheck class="h-4 w-4" />
                Verified Record
              </div>
            </div>

            <div class="mt-6 grid grid-cols-2 gap-4 rounded-xl bg-surface p-4 text-xs">
              <div>
                <p class="text-neutral-muted font-medium">Patient Name</p>
                <p class="text-sm font-bold text-navy mt-0.5">{{ document.petName }}</p>
              </div>
              <div>
                <p class="text-neutral-muted font-medium">Owner</p>
                <p class="text-sm font-bold text-navy mt-0.5">{{ document.ownerName }}</p>
              </div>
              <div>
                <p class="text-neutral-muted font-medium">Category</p>
                <p class="text-sm font-bold text-navy mt-0.5">{{ document.category }}</p>
              </div>
              <div>
                <p class="text-neutral-muted font-medium">Date Uploaded</p>
                <p class="text-sm font-bold text-navy mt-0.5">{{ document.uploadedAt }}</p>
              </div>
            </div>

            <div class="mt-6 space-y-3 text-sm text-neutral-dark">
              <h4 class="font-bold text-navy border-b border-neutral-grey/40 pb-1">
                Clinical Overview & Content Summary
              </h4>
              <p class="text-xs leading-relaxed text-neutral-muted">
                {{ document.contentSummary || 'This document contains confidential medical records, rehabilitation protocols, or consent agreements pertaining to ' + document.petName + '. Access is restricted to authorized clinic staff.' }}
              </p>
              <div class="mt-4 rounded-lg border border-amber-200 bg-amber-50/60 p-3 text-xs text-amber-900">
                <p class="font-bold flex items-center gap-1">
                  💡 Download Available
                </p>
                <p class="mt-0.5">Click the Download button below to export this record locally as a text or file document.</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Action Footer -->
      <div class="flex items-center justify-between border-t border-neutral-grey/80 px-6 py-4 bg-white">
        <p class="text-xs text-neutral-muted">
          Document ID: #DOC-{{ document.id }}
        </p>

        <div class="flex items-center gap-2">
          <BaseButton size="sm" variant="secondary" @click="handleOpenNewTab">
            <ExternalLink class="h-4 w-4" />
            Open / Print
          </BaseButton>
          <BaseButton size="sm" variant="accent" @click="handleDownload">
            <Download class="h-4 w-4" />
            Download Document
          </BaseButton>
          <BaseButton size="sm" variant="secondary" @click="emit('close')">
            Close
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

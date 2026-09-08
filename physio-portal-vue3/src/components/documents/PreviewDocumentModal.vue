<script setup lang="ts">
import { computed, ref } from 'vue'
import { Download, ExternalLink, FileText, X, ShieldCheck, ZoomIn, ZoomOut, RotateCw } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { formatFileSize, type DocumentItem } from '../../data/documentsDemo'
import { useDocumentsStore } from '../../store/documents'
import { API_BASE_URL } from '../../api/client'

const props = defineProps<{
  open: boolean
  document: DocumentItem | null
}>()

const emit = defineEmits<{
  close: []
}>()

const documentsStore = useDocumentsStore()
const imgZoom = ref(1)

const resolvedPreviewUrl = computed(() => {
  if (!props.document) return ''
  const direct = props.document.fileDataUrl || props.document.fileUrl
  if (!direct) return ''
  if (
    direct.startsWith('data:') ||
    direct.startsWith('blob:') ||
    direct.startsWith('http://') ||
    direct.startsWith('https://')
  ) {
    return direct
  }
  const base = API_BASE_URL.replace(/\/+$/, '')
  return `${base}${direct.startsWith('/') ? direct : `/${direct}`}`
})

const isImage = computed(() => {
  if (!props.document) return false
  const type = props.document.fileType?.toLowerCase() || ''
  const url = (resolvedPreviewUrl.value || props.document.name || '').toLowerCase()
  return (
    type.startsWith('image/') ||
    url.startsWith('data:image/') ||
    /\.(png|jpe?g|webp|gif|svg|bmp)(\?.*)?$/i.test(url)
  )
})

const isPdf = computed(() => {
  if (!props.document) return false
  const type = props.document.fileType?.toLowerCase() || ''
  const url = (resolvedPreviewUrl.value || props.document.name || '').toLowerCase()
  return (
    type.includes('pdf') ||
    url.startsWith('data:application/pdf') ||
    /\.pdf(\?.*)?$/i.test(url)
  )
})

function zoomIn() {
  if (imgZoom.value < 3) imgZoom.value = Number((imgZoom.value + 0.25).toFixed(2))
}

function zoomOut() {
  if (imgZoom.value > 0.5) imgZoom.value = Number((imgZoom.value - 0.25).toFixed(2))
}

function resetZoom() {
  imgZoom.value = 1
}

function handleDownload() {
  if (props.document) {
    documentsStore.downloadDocument(props.document)
  }
}

function handleOpenNewTab() {
  if (!props.document) return

  if (resolvedPreviewUrl.value) {
    window.open(resolvedPreviewUrl.value, '_blank')
    return
  }

  // Generate high-fidelity clinical print / new tab preview
  const win = window.open('', '_blank')
  if (win) {
    const doc = props.document
    win.document.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <meta charset="utf-8">
          <title>${doc.name} — Triple A Veterinary Physiotherapy</title>
          <style>
            body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; padding: 40px; color: #0f172a; max-width: 800px; margin: 0 auto; background: #fff; }
            .header { border-bottom: 2px solid #0f172a; padding-bottom: 16px; margin-bottom: 24px; display: flex; justify-content: space-between; align-items: flex-end; }
            .clinic-title { font-size: 20px; font-weight: 800; color: #1e3a8a; letter-spacing: -0.5px; margin: 0; }
            .clinic-sub { font-size: 12px; color: #64748b; margin-top: 4px; }
            .badge { background: #ecfdf5; color: #065f46; font-size: 12px; font-weight: 700; padding: 4px 10px; border-radius: 6px; border: 1px solid #a7f3d0; }
            .doc-title { font-size: 24px; font-weight: 800; margin: 16px 0 8px 0; color: #0f172a; }
            .meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; background: #f8fafc; padding: 18px; border-radius: 10px; margin: 20px 0; border: 1px solid #e2e8f0; }
            .meta-item dt { color: #64748b; font-size: 11px; font-weight: 700; text-transform: uppercase; margin-bottom: 4px; }
            .meta-item dd { font-size: 14px; font-weight: 600; color: #0f172a; margin: 0; }
            .section { margin-top: 24px; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px; }
            .section h4 { font-size: 13px; text-transform: uppercase; color: #475569; margin: 0 0 12px 0; letter-spacing: 0.5px; border-bottom: 1px solid #f1f5f9; padding-bottom: 8px; }
            .section p { font-size: 14px; line-height: 1.65; color: #334155; margin: 0; }
            .footer { margin-top: 40px; padding-top: 16px; border-top: 1px solid #e2e8f0; font-size: 11px; color: #94a3b8; display: flex; justify-content: space-between; }
          </style>
        </head>
        <body>
          <div class="header">
            <div>
              <h1 class="clinic-title">TRIPLE A VETERINARY PHYSIOTHERAPY</h1>
              <div class="clinic-sub">Canine & Equine Rehabilitation · Clinical Records</div>
            </div>
            <span class="badge">VERIFIED RECORD</span>
          </div>

          <div class="doc-title">${doc.name}</div>

          <div class="meta-grid">
            <div class="meta-item"><dt>Patient Name</dt><dd>${doc.petName}</dd></div>
            <div class="meta-item"><dt>Owner</dt><dd>${doc.ownerName}</dd></div>
            <div class="meta-item"><dt>Category</dt><dd>${doc.category}</dd></div>
            <div class="meta-item"><dt>Date Documented</dt><dd>${doc.uploadedAt}</dd></div>
          </div>

          <div class="section">
            <h4>Clinical Overview & Record Details</h4>
            <p>${doc.contentSummary || 'This verified clinical record pertains to the rehabilitation and physical management of ' + doc.petName + '. Managed under veterinary physiotherapy standards with authorized client access.'}</p>
          </div>

          <div class="footer">
            <span>Triple A Veterinary Physiotherapy Management System</span>
            <span>Document ID: #DOC-${doc.id}</span>
          </div>
          <script>
            window.onload = function() { window.print(); };
          <\/script>
        </body>
      </html>
    `)
    win.document.close()
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
      <div class="flex items-center justify-between border-b border-neutral-grey/80 px-6 py-4 bg-white">
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

        <div class="flex items-center gap-2">
          <!-- Image Zoom Controls -->
          <div v-if="isImage && resolvedPreviewUrl" class="mr-2 flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1">
            <button
              type="button"
              class="rounded p-1 text-neutral-muted hover:text-navy disabled:opacity-40"
              :disabled="imgZoom <= 0.5"
              title="Zoom Out"
              @click="zoomOut"
            >
              <ZoomOut class="h-3.5 w-3.5" />
            </button>
            <span class="text-[11px] font-bold text-navy w-10 text-center">{{ Math.round(imgZoom * 100) }}%</span>
            <button
              type="button"
              class="rounded p-1 text-neutral-muted hover:text-navy disabled:opacity-40"
              :disabled="imgZoom >= 3"
              title="Zoom In"
              @click="zoomIn"
            >
              <ZoomIn class="h-3.5 w-3.5" />
            </button>
            <button
              type="button"
              class="rounded p-1 text-neutral-muted hover:text-navy"
              title="Reset Zoom"
              @click="resetZoom"
            >
              <RotateCw class="h-3 w-3" />
            </button>
          </div>

          <button
            type="button"
            class="rounded-lg p-1.5 text-neutral-muted hover:bg-surface hover:text-navy transition-colors"
            @click="emit('close')"
          >
            <X class="h-5 w-5" />
          </button>
        </div>
      </div>

      <!-- Preview Canvas Body -->
      <div class="flex-1 overflow-y-auto bg-surface/70 p-6 flex flex-col">
        <!-- Image Preview -->
        <div v-if="isImage && resolvedPreviewUrl" class="flex-1 flex flex-col items-center justify-center overflow-auto p-2">
          <div class="relative overflow-hidden rounded-xl border border-neutral-grey/80 bg-white/80 p-2 shadow-md">
            <img
              :src="resolvedPreviewUrl"
              :alt="document.name"
              class="max-h-[60vh] max-w-full rounded-lg object-contain transition-transform duration-150"
              :style="{ transform: `scale(${imgZoom})` }"
            />
          </div>
          <div class="mt-3 flex items-center gap-2">
            <a
              :href="resolvedPreviewUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-xs text-sage hover:text-navy font-semibold inline-flex items-center gap-1"
            >
              <ExternalLink class="h-3.5 w-3.5" />
              View full image in new tab
            </a>
          </div>
        </div>

        <!-- PDF Preview -->
        <div v-else-if="isPdf && resolvedPreviewUrl" class="flex-1 flex flex-col h-full w-full rounded-xl overflow-hidden border border-neutral-grey/80 bg-white shadow-sm">
          <div class="flex items-center justify-between border-b border-neutral-grey/60 bg-neutral-50 px-4 py-2 text-xs text-neutral-muted">
            <span>Embedded Document Preview</span>
            <a
              :href="resolvedPreviewUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="font-semibold text-sage hover:text-navy inline-flex items-center gap-1"
            >
              <ExternalLink class="h-3.5 w-3.5" />
              Open in standalone tab
            </a>
          </div>
          <iframe
            :src="resolvedPreviewUrl"
            class="flex-1 w-full h-full min-h-[500px] border-0"
            title="Document PDF Preview"
          />
        </div>

        <!-- Default Document Record Card Preview -->
        <div v-else class="mx-auto my-auto w-full max-w-2xl">
          <div class="portal-card p-8 shadow-sm border border-neutral-grey/80 bg-white">
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
                <p class="text-neutral-muted font-medium">Date Uploaded / Documented</p>
                <p class="text-sm font-bold text-navy mt-0.5">{{ document.uploadedAt }}</p>
              </div>
            </div>

            <div class="mt-6 space-y-3 text-sm text-neutral-dark">
              <h4 class="font-bold text-navy border-b border-neutral-grey/40 pb-1">
                Clinical Overview & Content Summary
              </h4>
              <p class="text-xs leading-relaxed text-neutral-muted">
                {{ document.contentSummary || 'This document contains verified medical records, rehabilitation protocols, or consent agreements pertaining to ' + document.petName + '. Access is restricted to authorized clinic staff and the pet owner.' }}
              </p>
              <div class="mt-4 rounded-lg border border-sage/20 bg-sage-muted/30 p-3.5 text-xs text-navy flex items-start gap-2.5">
                <ShieldCheck class="h-4 w-4 text-sage shrink-0 mt-0.5" />
                <div>
                  <p class="font-bold">Clinic Verified Document</p>
                  <p class="mt-0.5 text-neutral-muted">You can open a formatted printable copy or download this clinical document record locally using the actions below.</p>
                </div>
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

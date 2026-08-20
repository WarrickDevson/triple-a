<script setup lang="ts">
import { computed, ref } from 'vue'
import { Download, Eye, Plus, Trash2, Share2, CheckCircle2 } from '@lucide/vue'
import {
  categoryCount,
  DOCUMENT_CATEGORIES,
  formatFileSize,
  type DocumentCategory,
} from '../data/documentsDemo'
import PreviewDocumentModal from '../components/documents/PreviewDocumentModal.vue'
import UploadDocumentModal from '../components/documents/UploadDocumentModal.vue'
import BaseButton from '../components/BaseButton.vue'
import { useDocumentsStore } from '../store/documents'

const documentsStore = useDocumentsStore()

const search = ref('')
const category = ref<DocumentCategory | 'All'>('All')

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  return documentsStore.documents.filter((doc) => {
    const matchesCategory = category.value === 'All' || doc.category === category.value
    const matchesSearch =
      !q ||
      doc.name.toLowerCase().includes(q) ||
      doc.petName.toLowerCase().includes(q) ||
      doc.ownerName.toLowerCase().includes(q)
    return matchesCategory && matchesSearch
  })
})
</script>

<template>
  <div class="space-y-4">
    <!-- Toast Notification Banner -->
    <div
      v-if="documentsStore.notificationMessage"
      class="fixed bottom-6 right-6 z-50 flex items-center gap-2 rounded-xl bg-navy px-4 py-3 text-sm font-medium text-white shadow-xl animate-in slide-in-from-bottom-5"
    >
      <span>✨</span>
      <span>{{ documentsStore.notificationMessage }}</span>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_260px]">
      <section class="portal-card overflow-hidden">
        <div class="flex flex-wrap items-center gap-3 border-b border-neutral-grey/80 p-4">
          <input
            v-model="search"
            type="search"
            placeholder="Search documents by title, pet, owner..."
            class="min-w-[220px] flex-1 rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
          />
          <select
            v-model="category"
            class="rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
          >
            <option value="All">All categories</option>
            <option v-for="cat in DOCUMENT_CATEGORIES" :key="cat" :value="cat">{{ cat }}</option>
          </select>
          <BaseButton size="sm" variant="accent" @click="documentsStore.openUpload">
            <Plus class="h-4 w-4" />
            Upload Document
          </BaseButton>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full min-w-[640px] text-left text-sm">
            <thead>
              <tr class="border-b border-neutral-grey/80 text-xs font-semibold uppercase tracking-wide text-neutral-muted">
                <th class="px-4 py-3">Document</th>
                <th class="px-4 py-3">Patient</th>
                <th class="px-4 py-3">Category</th>
                <th class="px-4 py-3">Uploaded</th>
                <th class="px-4 py-3">Size</th>
                <th class="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filtered.length === 0">
                <td colspan="6" class="empty-state py-16 text-center text-sm text-neutral-muted">
                  No documents match your search or filter.
                </td>
              </tr>
              <tr
                v-for="doc in filtered"
                :key="doc.id"
                class="border-b border-neutral-grey/60 transition-colors hover:bg-surface/80"
              >
                <td class="px-4 py-3 font-semibold text-navy">
                  <div class="flex items-center gap-2">
                    <span>{{ doc.name }}</span>
                    <span v-if="doc.fileDataUrl" class="rounded bg-sage-muted px-1.5 py-0.5 text-[10px] font-bold text-sage">
                      Uploaded File
                    </span>
                    <span v-if="doc.isSharedWithOwner" class="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-[10px] font-bold text-emerald-700 border border-emerald-200">
                      <CheckCircle2 class="h-3 w-3 text-emerald-600" />
                      Shared
                    </span>
                  </div>
                </td>
                <td class="px-4 py-3 text-neutral-muted">
                  <span class="font-medium text-navy">{{ doc.petName }}</span>
                  <span class="block text-xs text-neutral-muted">{{ doc.ownerName }}</span>
                </td>
                <td class="px-4 py-3">
                  <span class="rounded-full bg-surface px-2.5 py-1 text-xs font-semibold text-navy border border-neutral-grey/60">
                    {{ doc.category }}
                  </span>
                </td>
                <td class="px-4 py-3 text-neutral-muted">
                  {{ doc.uploadedAt }}
                </td>
                <td class="px-4 py-3 text-neutral-muted">{{ formatFileSize(doc.sizeKb) }}</td>
                <td class="px-4 py-3">
                  <div class="flex items-center gap-1">
                    <button
                      type="button"
                      class="rounded-lg p-2 transition-colors"
                      :class="doc.isSharedWithOwner ? 'text-emerald-600 hover:bg-emerald-50' : 'text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy'"
                      :title="doc.isSharedWithOwner ? 'Shared with Owner (click to unshare)' : 'Click to share with Owner'"
                      @click="documentsStore.toggleDocumentShare(doc.id)"
                    >
                      <Share2 class="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      class="rounded-lg p-2 text-sage hover:bg-sage-muted transition-colors"
                      title="Preview Document"
                      @click="documentsStore.openPreview(doc)"
                    >
                      <Eye class="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      class="rounded-lg p-2 text-sage hover:bg-sage-muted transition-colors"
                      title="Download Document"
                      @click="documentsStore.downloadDocument(doc)"
                    >
                      <Download class="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      class="rounded-lg p-2 text-neutral-muted hover:bg-red-50 hover:text-red-600 transition-colors"
                      title="Delete Document"
                      @click="documentsStore.deleteDocument(doc.id)"
                    >
                      <Trash2 class="h-4 w-4" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <div class="space-y-4">
        <section class="portal-card p-4">
          <h3 class="text-sm font-bold text-navy">Categories Summary</h3>
          <ul class="mt-3 space-y-2 text-sm">
            <li class="flex justify-between border-b border-neutral-grey/40 pb-2">
              <span class="font-medium text-navy">All Documents</span>
              <span class="font-semibold text-navy">{{ categoryCount(documentsStore.documents, 'All') }}</span>
            </li>
            <li v-for="cat in DOCUMENT_CATEGORIES" :key="cat" class="flex justify-between text-xs">
              <span class="text-neutral-muted">{{ cat }}</span>
              <span class="font-semibold text-navy">{{ categoryCount(documentsStore.documents, cat) }}</span>
            </li>
          </ul>
        </section>

        <button
          type="button"
          class="flex w-full items-center justify-center gap-2 rounded-xl border border-dashed border-sage/60 py-3.5 text-sm font-bold text-sage hover:bg-sage-muted/30 transition-colors"
          @click="documentsStore.openUpload"
        >
          <Plus class="h-4 w-4" />
          Upload New Document
        </button>
      </div>
    </div>

    <!-- Modals -->
    <UploadDocumentModal
      :open="documentsStore.isUploadOpen"
      @close="documentsStore.closeUpload"
    />

    <PreviewDocumentModal
      :open="documentsStore.isPreviewOpen"
      :document="documentsStore.selectedDocument"
      @close="documentsStore.closePreview"
    />
  </div>
</template>

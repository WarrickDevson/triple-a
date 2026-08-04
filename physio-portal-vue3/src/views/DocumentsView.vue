<script setup lang="ts">
import { computed, ref } from 'vue'
import { Download, Eye } from '@lucide/vue'
import {
  categoryCount,
  demoDocuments,
  DOCUMENT_CATEGORIES,
  formatFileSize,
  type DocumentCategory,
} from '../data/documentsDemo'
import BaseButton from '../components/BaseButton.vue'

const search = ref('')
const category = ref<DocumentCategory | 'All'>('All')
const showStubModal = ref(false)
const stubMessage = ref('')

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  return demoDocuments.filter((doc) => {
    const matchesCategory = category.value === 'All' || doc.category === category.value
    const matchesSearch =
      !q ||
      doc.name.toLowerCase().includes(q) ||
      doc.petName.toLowerCase().includes(q) ||
      doc.ownerName.toLowerCase().includes(q)
    return matchesCategory && matchesSearch
  })
})

function showStub(message: string) {
  stubMessage.value = message
  showStubModal.value = true
}
</script>

<template>
  <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_240px]">
    <section class="portal-card overflow-hidden">
      <div class="flex flex-wrap items-center gap-3 border-b border-neutral-grey/80 p-4">
        <input
          v-model="search"
          type="search"
          placeholder="Search documents..."
          class="min-w-[200px] flex-1 rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
        />
        <select
          v-model="category"
          class="rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
        >
          <option value="All">All categories</option>
          <option v-for="cat in DOCUMENT_CATEGORIES" :key="cat" :value="cat">{{ cat }}</option>
        </select>
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
                No documents match your search.
              </td>
            </tr>
            <tr
              v-for="doc in filtered"
              :key="doc.id"
              class="border-b border-neutral-grey/60 transition-colors hover:bg-surface"
            >
              <td class="px-4 py-3 font-medium text-navy">{{ doc.name }}</td>
              <td class="px-4 py-3 text-neutral-muted">{{ doc.petName }}</td>
              <td class="px-4 py-3 text-neutral-muted">{{ doc.category }}</td>
              <td class="px-4 py-3 text-neutral-muted">
                {{ new Date(doc.uploadedAt).toLocaleDateString() }}
              </td>
              <td class="px-4 py-3 text-neutral-muted">{{ formatFileSize(doc.sizeKb) }}</td>
              <td class="px-4 py-3">
                <div class="flex gap-2">
                  <button
                    type="button"
                    class="rounded-lg p-1.5 text-sage hover:bg-sage-muted"
                    title="View"
                    @click="showStub('Document preview coming soon.')"
                  >
                    <Eye class="h-4 w-4" />
                  </button>
                  <button
                    type="button"
                    class="rounded-lg p-1.5 text-sage hover:bg-sage-muted"
                    title="Download"
                    @click="showStub('Document download coming soon.')"
                  >
                    <Download class="h-4 w-4" />
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
        <h3 class="text-sm font-bold text-navy">Categories</h3>
        <ul class="mt-3 space-y-2 text-sm">
          <li class="flex justify-between">
            <span class="text-neutral-muted">All</span>
            <span class="font-semibold text-navy">{{ categoryCount(demoDocuments, 'All') }}</span>
          </li>
          <li v-for="cat in DOCUMENT_CATEGORIES" :key="cat" class="flex justify-between">
            <span class="text-neutral-muted">{{ cat }}</span>
            <span class="font-semibold text-navy">{{ categoryCount(demoDocuments, cat) }}</span>
          </li>
        </ul>
      </section>

      <button
        type="button"
        class="w-full rounded-xl border border-dashed border-neutral-grey py-3 text-sm font-semibold text-sage hover:bg-surface"
        @click="showStub('Document upload coming soon.')"
      >
        Upload Document
      </button>
    </div>
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

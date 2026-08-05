<script setup lang="ts">
import { computed, ref } from 'vue'
import { Plus, Star } from '@lucide/vue'
import { formatMessageTime, loadStarredThreadIds } from '../../data/messageDemo'
import { usePatientsStore } from '../../store/patients'
import type { MessageThread } from '../../types/message'

const props = defineProps<{
  threads: MessageThread[]
  selectedPetId: number | null
  loading?: boolean
}>()

const emit = defineEmits<{
  select: [petId: number]
}>()

const patientsStore = usePatientsStore()
const search = ref('')
const filterTab = ref<'all' | 'unread' | 'starred'>('all')
const starredIds = ref(loadStarredThreadIds())
const showNewChatSelect = ref(false)

const existingPetIds = computed(() => new Set(props.threads.map((t) => t.petId)))

const availableNewPatients = computed(() =>
  patientsStore.patients.filter((p) => !existingPetIds.value.has(p.petId)),
)

const filteredThreads = computed(() => {
  const query = search.value.trim().toLowerCase()
  return props.threads.filter((thread) => {
    const matchesSearch =
      !query ||
      thread.petName.toLowerCase().includes(query) ||
      thread.ownerName.toLowerCase().includes(query) ||
      (thread.lastMessagePreview?.toLowerCase().includes(query) ?? false)

    if (filterTab.value === 'unread') return matchesSearch && thread.unreadCount > 0
    if (filterTab.value === 'starred') {
      return matchesSearch && starredIds.value.includes(thread.messageThreadId)
    }
    return matchesSearch
  })
})

function selectThread(petId: number) {
  showNewChatSelect.value = false
  emit('select', petId)
}
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <div class="flex items-center justify-between gap-2 mb-3">
        <h2 class="text-sm font-bold text-navy">Messages</h2>
        <button
          v-if="availableNewPatients.length > 0"
          type="button"
          class="inline-flex items-center gap-1 text-xs font-semibold text-sage hover:text-navy"
          @click="showNewChatSelect = !showNewChatSelect"
        >
          <Plus class="h-3.5 w-3.5" :stroke-width="2" />
          {{ showNewChatSelect ? 'Cancel' : 'New Message' }}
        </button>
      </div>

      <div v-if="showNewChatSelect" class="mb-3 rounded-lg border border-sage/40 bg-sage-muted/20 p-2">
        <p class="mb-1 text-xs font-semibold text-navy">Start chat with patient:</p>
        <div class="max-h-36 overflow-y-auto space-y-1">
          <button
            v-for="patient in availableNewPatients"
            :key="patient.petId"
            type="button"
            class="flex w-full items-center justify-between rounded px-2 py-1.5 text-xs text-navy hover:bg-white"
            @click="selectThread(patient.petId)"
          >
            <span class="font-medium">{{ patient.petName }}</span>
            <span class="text-[10px] text-neutral-muted">Owner: {{ patient.ownerName }}</span>
          </button>
        </div>
      </div>

      <input
        v-model="search"
        type="search"
        placeholder="Search conversations..."
        class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
      />
      <div class="mt-3 flex gap-1">
        <button
          v-for="tab in ['all', 'unread', 'starred'] as const"
          :key="tab"
          type="button"
          class="rounded-lg px-3 py-1.5 text-xs font-semibold capitalize transition-colors"
          :class="
            filterTab === tab
              ? 'bg-sage-muted text-navy'
              : 'text-neutral-muted hover:bg-surface'
          "
          @click="filterTab = tab"
        >
          {{ tab }}
          <span v-if="tab === 'unread' && threads.some((t) => t.unreadCount > 0)">
            ({{ threads.filter((t) => t.unreadCount > 0).length }})
          </span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="p-6 text-sm text-neutral-muted">Loading conversations...</div>
    <ul v-else class="flex-1 overflow-y-auto">
      <li v-for="thread in filteredThreads" :key="thread.messageThreadId">
        <button
          type="button"
          class="flex w-full items-start gap-3 border-b border-neutral-grey/60 px-4 py-3 text-left transition-colors hover:bg-surface"
          :class="selectedPetId === thread.petId ? 'bg-sage-muted/40' : ''"
          @click="selectThread(thread.petId)"
        >
          <div
            class="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-sage-muted text-xs font-bold text-sage"
          >
            {{ thread.petName.slice(0, 2).toUpperCase() }}
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex items-center justify-between gap-2">
              <p class="truncate text-sm font-semibold text-navy">{{ thread.petName }}</p>
              <span class="shrink-0 text-[10px] text-neutral-muted">
                {{ formatMessageTime(thread.lastMessageAt) }}
              </span>
            </div>
            <p class="truncate text-xs text-neutral-muted">Owner: {{ thread.ownerName }}</p>
            <p class="mt-0.5 truncate text-xs text-neutral-muted">
              {{ thread.lastMessagePreview || 'No messages yet' }}
            </p>
          </div>
          <div class="flex flex-col items-end gap-1">
            <Star
              v-if="starredIds.includes(thread.messageThreadId)"
              class="h-3.5 w-3.5 fill-accent-amber text-accent-amber"
            />
            <span
              v-if="thread.unreadCount > 0"
              class="flex h-5 min-w-5 items-center justify-center rounded-full bg-sage px-1.5 text-[10px] font-bold text-white"
            >
              {{ thread.unreadCount }}
            </span>
          </div>
        </button>
      </li>
    </ul>
  </section>
</template>

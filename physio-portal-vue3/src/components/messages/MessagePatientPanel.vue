<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import {
  Calendar,
  ClipboardList,
  FileText,
  Maximize2,
  Video,
} from '@lucide/vue'
import ImageViewerModal from '../common/ImageViewerModal.vue'
import { resolveMediaUrl } from '../../api/videos'
import type { MessageThread } from '../../types/message'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  thread: MessageThread | null
  patient: Pet | null
}>()

const emit = defineEmits<{
  comingSoon: [message: string]
}>()

const viewerModal = ref<{
  url: string
  title: string
  subtitle?: string | null
} | null>(null)

function openViewer(url: string, title: string, subtitle?: string | null) {
  const resolved = resolveMediaUrl(url)
  if (resolved) {
    viewerModal.value = { url: resolved, title, subtitle }
  }
}

const petDp = computed(() => props.thread?.petProfilePictureUrl ?? props.patient?.profilePictureUrl ?? null)
const ownerDp = computed(() => props.thread?.ownerProfilePictureUrl ?? props.patient?.ownerProfilePictureUrl ?? null)

const quickActions = computed(() => {
  const petId = props.thread?.petId ?? props.patient?.petId ?? null
  return [
    { label: 'Update Plan', icon: ClipboardList, route: petId ? 'treatment-plan-detail' : 'treatment-plans', routeParam: petId },
    { label: 'Schedule Appointment', icon: Calendar, route: 'appointments' },
    { label: 'Add Clinical Note', icon: FileText, route: petId ? 'patient-detail' : 'patients', routeParam: petId },
    { label: 'Exercise Library Video', icon: Video, route: 'exercises' },
  ]
})
</script>

<template>
  <div class="space-y-4">
    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Patient Info</h3>
      <div v-if="thread || patient" class="mt-4 space-y-3">
        <!-- Pet Profile Picture -->
        <div class="flex items-center gap-3">
          <div
            class="relative flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-full bg-sage-muted text-sm font-bold text-sage ring-2 ring-sage/20"
            :class="petDp ? 'cursor-pointer hover:ring-sage' : ''"
            :title="petDp ? 'Click to view full photo' : ''"
            @click="petDp ? openViewer(petDp, (thread?.petName ?? patient?.petName ?? 'Pet'), (patient?.breed || patient?.species)) : null"
          >
            <img
              v-if="petDp"
              :src="resolveMediaUrl(petDp)!"
              :alt="thread?.petName ?? patient?.petName"
              class="h-full w-full object-cover transition hover:scale-105"
            />
            <span v-else>
              {{ (thread?.petName ?? patient?.petName ?? 'P').slice(0, 2).toUpperCase() }}
            </span>
          </div>
          <div class="min-w-0 flex-1">
            <p class="font-bold text-navy truncate">{{ thread?.petName ?? patient?.petName }}</p>
            <p class="text-xs text-neutral-muted truncate">{{ patient?.breed || patient?.species || 'Patient' }}</p>
            <button
              v-if="petDp"
              type="button"
              class="mt-0.5 inline-flex items-center gap-1 text-[10px] font-semibold text-sage hover:underline"
              @click="openViewer(petDp, (thread?.petName ?? patient?.petName ?? 'Pet'), (patient?.breed || patient?.species))"
            >
              <Maximize2 class="h-2.5 w-2.5" />
              View Pet DP
            </button>
          </div>
        </div>

        <!-- Owner Profile Picture Card -->
        <div class="flex items-center gap-2.5 rounded-xl border border-neutral-grey/70 bg-surface/60 p-2.5">
          <div
            class="relative flex h-9 w-9 shrink-0 items-center justify-center overflow-hidden rounded-full bg-sage-muted text-xs font-bold text-sage ring-1 ring-sage/20"
            :class="ownerDp ? 'cursor-pointer hover:ring-sage' : ''"
            :title="ownerDp ? 'Click to view owner photo' : ''"
            @click="ownerDp ? openViewer(ownerDp, (thread?.ownerName ?? patient?.ownerName ?? 'Owner'), 'Pet Owner') : null"
          >
            <img
              v-if="ownerDp"
              :src="resolveMediaUrl(ownerDp)!"
              :alt="thread?.ownerName ?? patient?.ownerName"
              class="h-full w-full object-cover transition hover:scale-105"
            />
            <span v-else>
              {{ (thread?.ownerName ?? patient?.ownerName ?? 'O').slice(0, 2).toUpperCase() }}
            </span>
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-1">
              <p class="font-semibold text-xs text-navy truncate">{{ thread?.ownerName ?? patient?.ownerName }}</p>
              <span class="rounded bg-sage-muted px-1 py-0.2 text-[8px] font-bold text-sage">Owner</span>
            </div>
            <button
              v-if="ownerDp"
              type="button"
              class="mt-0.5 inline-flex items-center gap-1 text-[10px] font-semibold text-sage hover:underline"
              @click="openViewer(ownerDp, (thread?.ownerName ?? patient?.ownerName ?? 'Owner'), 'Pet Owner')"
            >
              <Maximize2 class="h-2.5 w-2.5" />
              View Owner DP
            </button>
          </div>
        </div>

        <RouterLink
          v-if="patient?.petId || thread?.petId"
          :to="{ name: 'patient-detail', params: { petId: String(patient?.petId ?? thread?.petId) } }"
          class="portal-card-link mt-2 inline-block text-xs font-semibold text-sage hover:text-navy"
        >
          View Full Profile →
        </RouterLink>
      </div>
      <p v-else class="mt-4 text-sm text-neutral-muted">No patient selected.</p>
    </section>

    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Quick Actions</h3>
      <div class="mt-3 space-y-2">
        <template v-for="action in quickActions" :key="action.label">
          <RouterLink
            v-if="action.route && action.routeParam"
            :to="{ name: action.route, params: { petId: action.routeParam } }"
            class="flex items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
          >
            <component :is="action.icon" class="h-4 w-4 text-sage" :stroke-width="1.75" />
            {{ action.label }}
          </RouterLink>
          <RouterLink
            v-else-if="action.route"
            :to="{ name: action.route }"
            class="flex items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
          >
            <component :is="action.icon" class="h-4 w-4 text-sage" :stroke-width="1.75" />
            {{ action.label }}
          </RouterLink>
          <button
            v-else
            type="button"
            class="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
            @click="emit('comingSoon', `${action.label} coming soon.`)"
          >
            <component :is="action.icon" class="h-4 w-4 text-sage" :stroke-width="1.75" />
            {{ action.label }}
          </button>
        </template>
      </div>
    </section>
  </div>

  <ImageViewerModal
    v-if="viewerModal"
    :image-url="viewerModal.url"
    :title="viewerModal.title"
    :subtitle="viewerModal.subtitle"
    @close="viewerModal = null"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Play } from '@lucide/vue'
import PatientProgressChart from '../PatientProgressChart.vue'
import { OUTCOME_MEASURES } from '../../data/patientDemo'
import { resolveMediaUrl } from '../../api/videos'
import type { PetProgressSummary } from '../../types/dashboard'
import type { VideoSubmission } from '../../types/video'

const props = defineProps<{
  progress: PetProgressSummary | null
  latestVideo: VideoSubmission | null
  loading?: boolean
}>()

const outcomeRows = computed(() => {
  const logs = props.progress?.logs ?? []
  const latest = logs[logs.length - 1]

  return OUTCOME_MEASURES.map((measure) => {
    const value = latest ? latest[measure.field] : null
    const previous = logs.length >= 2 ? logs[logs.length - 2][measure.field] : null
    const trend =
      value != null && previous != null ? value - previous : 0
    const improving = measure.field === 'painScore' ? trend < 0 : trend > 0
    return {
      label: measure.label,
      value: value ?? '—',
      status: improving ? 'Improving' : trend === 0 ? 'Stable' : 'At Risk',
    }
  })
})

const videoUrl = computed(() =>
  resolveMediaUrl(
    props.latestVideo?.processedVideoStreamingUrl ?? props.latestVideo?.rawVideoStorageUrl ?? null,
  ),
)
</script>

<template>
  <div class="space-y-4">
    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Outcome Measures</h3>
      <div v-if="loading" class="mt-4 text-sm text-neutral-muted">Loading measures...</div>
      <ul v-else class="mt-4 space-y-3">
        <li
          v-for="row in outcomeRows"
          :key="row.label"
          class="flex items-center justify-between gap-3 border-b border-neutral-grey/60 pb-3 last:border-0 last:pb-0"
        >
          <div>
            <p class="text-sm font-medium text-navy">{{ row.label }}</p>
            <p class="text-xs text-neutral-muted">{{ row.status }}</p>
          </div>
          <p class="text-sm font-bold text-navy">{{ row.value }}</p>
        </li>
      </ul>
    </section>

    <section class="portal-card overflow-hidden p-4">
      <h3 class="text-sm font-bold text-navy">Latest Owner Upload</h3>
      <div v-if="latestVideo" class="mt-4">
        <div class="relative overflow-hidden rounded-xl bg-navy/5">
          <video
            v-if="videoUrl"
            :src="videoUrl"
            class="aspect-video w-full object-cover"
            controls
            preload="metadata"
          />
          <div
            v-else
            class="flex aspect-video items-center justify-center bg-navy/10 text-neutral-muted"
          >
            <Play class="h-10 w-10" :stroke-width="1.5" />
          </div>
        </div>
        <p class="mt-3 text-sm font-semibold text-navy">
          {{ latestVideo.exerciseTitle || latestVideo.title || 'General Progress Video' }}
        </p>
        <p v-if="latestVideo.notes" class="mt-1 text-xs italic text-navy/80">
          "{{ latestVideo.notes }}"
        </p>
        <p class="text-xs text-neutral-muted">
          {{ new Date(latestVideo.createdDate).toLocaleString() }}
        </p>
      </div>
      <div v-else class="empty-state mt-4 py-8">
        <p class="text-sm text-neutral-muted">No owner video uploads yet.</p>
      </div>
    </section>

    <PatientProgressChart v-if="progress && progress.logs.length > 0" :progress="progress" />
  </div>
</template>

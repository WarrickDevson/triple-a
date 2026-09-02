<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { Play, CheckCircle2, Clock, Send, Edit3, Loader2, MessageSquare, Video, Trash2 } from '@lucide/vue'
import PatientProgressChart from '../PatientProgressChart.vue'
import BaseButton from '../BaseButton.vue'
import { OUTCOME_MEASURES } from '../../data/patientDemo'
import { resolveMediaUrl, reviewVideo, deleteVideo } from '../../api/videos'
import type { PetProgressSummary } from '../../types/dashboard'
import type { VideoSubmission } from '../../types/video'
import { getVideoTitle } from '../../types/video'

const emit = defineEmits<{
  (e: 'video-deleted', videoId: number): void
  (e: 'refresh'): void
}>()

const route = useRoute()

const props = withDefaults(
  defineProps<{
    progress: PetProgressSummary | null
    latestVideo?: VideoSubmission | null
    videos?: VideoSubmission[]
    loading?: boolean
  }>(),
  {
    latestVideo: null,
    videos: () => [],
    loading: false,
  },
)

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

// Video History Computation
const allVideos = computed<VideoSubmission[]>(() => {
  if (props.videos && props.videos.length > 0) {
    return [...props.videos].sort(
      (a, b) => new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime(),
    )
  }
  return props.latestVideo ? [props.latestVideo] : []
})

const selectedVideoId = ref<number | null>(null)

watch(
  () => [allVideos.value, route.query.videoId],
  () => {
    const vids = allVideos.value
    if (vids.length === 0) {
      selectedVideoId.value = null
      return
    }

    const queryVideoId = route.query.videoId ? Number(route.query.videoId) : null
    if (queryVideoId && vids.some((v) => v.videoSubmissionId === queryVideoId)) {
      selectedVideoId.value = queryVideoId
      return
    }

    if (!selectedVideoId.value || !vids.some((v) => v.videoSubmissionId === selectedVideoId.value)) {
      selectedVideoId.value = vids[0].videoSubmissionId
    }
  },
  { immediate: true },
)

const currentVideo = computed(() => {
  if (!selectedVideoId.value) return allVideos.value[0] ?? null
  return allVideos.value.find((v) => v.videoSubmissionId === selectedVideoId.value) ?? allVideos.value[0] ?? null
})

const videoUrl = computed(() =>
  resolveMediaUrl(
    currentVideo.value?.processedVideoStreamingUrl ?? currentVideo.value?.rawVideoStorageUrl ?? null,
  ),
)

// Review Form State
const feedbackText = ref('')
const isEditing = ref(false)
const isSubmitting = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

watch(
  () => currentVideo.value,
  (video) => {
    if (video?.physioFeedbackNotes) {
      feedbackText.value = video.physioFeedbackNotes
      isEditing.value = false
    } else {
      feedbackText.value = ''
      isEditing.value = false
    }
    successMessage.value = ''
    errorMessage.value = ''
  },
  { immediate: true },
)

async function handleSaveReview() {
  if (!currentVideo.value || !feedbackText.value.trim()) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const updated = await reviewVideo(currentVideo.value.videoSubmissionId, {
      feedbackNotes: feedbackText.value.trim(),
    })
    currentVideo.value.isReviewed = true
    currentVideo.value.physioFeedbackNotes = updated.physioFeedbackNotes || feedbackText.value.trim()
    isEditing.value = false
    successMessage.value = 'Review submitted and sent to pet owner.'
    emit('refresh')
    setTimeout(() => {
      successMessage.value = ''
    }, 4000)
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to submit review. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}

async function handleDeleteVideo() {
  if (!currentVideo.value) return
  if (!confirm(`Are you sure you want to delete "${getVideoTitle(currentVideo.value)}"? This video will be removed.`)) {
    return
  }
  try {
    isSubmitting.value = true
    await deleteVideo(currentVideo.value.videoSubmissionId)
    emit('video-deleted', currentVideo.value.videoSubmissionId)
    emit('refresh')
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to delete video submission.'
  } finally {
    isSubmitting.value = false
  }
}
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
      <div class="flex items-center justify-between gap-2">
        <div>
          <h3 class="text-sm font-bold text-navy flex items-center gap-1.5">
            <Video class="h-4 w-4 text-sage" />
            Owner Video Submissions
          </h3>
          <p v-if="allVideos.length > 0" class="text-[11px] text-neutral-muted">
            {{ allVideos.length }} total submission{{ allVideos.length > 1 ? 's' : '' }}
          </p>
        </div>
        <span
          v-if="currentVideo"
          class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold"
          :class="
            currentVideo.isReviewed
              ? 'bg-emerald-100 text-emerald-800'
              : 'bg-amber-100 text-amber-800'
          "
        >
          <component :is="currentVideo.isReviewed ? CheckCircle2 : Clock" class="h-3 w-3" />
          {{ currentVideo.isReviewed ? 'Reviewed' : 'Pending Review' }}
        </span>
      </div>

      <!-- Video History Selector -->
      <div v-if="allVideos.length > 1" class="mt-3">
        <label class="block text-[10px] font-bold uppercase tracking-wider text-neutral-muted mb-1">
          Select Video to Review:
        </label>
        <select
          v-model="selectedVideoId"
          class="portal-input w-full text-xs font-semibold py-1.5 px-2.5 bg-surface border-neutral-grey rounded-lg text-navy"
        >
          <option
            v-for="(vid, idx) in allVideos"
            :key="vid.videoSubmissionId"
            :value="vid.videoSubmissionId"
          >
            {{ idx === 0 ? '★ Latest: ' : '' }}{{ getVideoTitle(vid) }} — {{ new Date(vid.createdDate).toLocaleDateString() }} ({{ vid.isReviewed ? '✓ Reviewed' : '⏳ Pending' }})
          </option>
        </select>
      </div>

      <div v-if="currentVideo" class="mt-4 space-y-3">
        <div class="relative overflow-hidden rounded-xl bg-navy/5">
          <video
            v-if="videoUrl"
            :key="videoUrl"
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

        <div>
          <div class="flex items-center justify-between">
            <p class="text-sm font-semibold text-navy">
              {{ getVideoTitle(currentVideo) }}
            </p>
            <button
              type="button"
              class="inline-flex items-center gap-1 rounded-md px-2 py-0.5 text-[11px] font-bold text-alert-red hover:bg-rose-50 transition"
              title="Delete this video submission"
              @click="handleDeleteVideo"
            >
              <Trash2 class="h-3.5 w-3.5" />
              Delete
            </button>
          </div>
          <p v-if="currentVideo.notes && currentVideo.notes !== getVideoTitle(currentVideo)" class="mt-1 rounded-lg bg-surface p-2 text-xs italic text-navy/90 border border-neutral-grey/80">
            <span class="font-semibold not-italic text-neutral-muted">Owner Note:</span> "{{ currentVideo.notes }}"
          </p>
          <p class="mt-1 text-[11px] text-neutral-muted">
            Uploaded {{ new Date(currentVideo.createdDate).toLocaleString() }}
          </p>
        </div>

        <!-- Physio Review Section -->
        <div class="mt-4 pt-1">
          <!-- Pending or Editing Review Form -->
          <div
            v-if="!currentVideo.isReviewed || isEditing"
            class="rounded-xl border-2 border-sage/50 bg-sage-muted/30 p-3.5 shadow-xs space-y-3"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <div class="flex h-7 w-7 items-center justify-center rounded-lg bg-sage text-white shadow-2xs">
                  <MessageSquare class="h-4 w-4" />
                </div>
                <div>
                  <h4 class="text-xs font-bold uppercase tracking-wider text-navy">
                    Physiotherapist Review & Feedback
                  </h4>
                  <p class="text-[11px] text-neutral-muted">
                    {{ isEditing ? 'Update clinical feedback for this video' : 'Provide movement feedback and advice for the pet owner' }}
                  </p>
                </div>
              </div>

              <span
                class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold"
                :class="currentVideo.isReviewed ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'"
              >
                <component :is="currentVideo.isReviewed ? CheckCircle2 : Clock" class="h-3 w-3" />
                {{ currentVideo.isReviewed ? 'Editing Review' : 'Pending Review' }}
              </span>
            </div>

            <div class="space-y-1.5">
              <textarea
                v-model="feedbackText"
                rows="4"
                class="w-full rounded-xl border-2 border-sage/40 bg-white p-3 text-xs font-medium text-navy placeholder:text-neutral-muted/70 focus:border-sage focus:ring-2 focus:ring-sage/20 outline-none shadow-xs transition-all"
                placeholder="Type your clinical assessment here (e.g. Range of motion, gait analysis, posture adjustments, or praise for the owner)..."
              />
            </div>

            <div v-if="errorMessage" class="rounded-lg bg-rose-50 border border-rose-200 p-2 text-xs font-medium text-rose-700">
              {{ errorMessage }}
            </div>
            <div v-if="successMessage" class="rounded-lg bg-emerald-50 border border-emerald-200 p-2 text-xs font-medium text-emerald-800">
              {{ successMessage }}
            </div>

            <div class="flex items-center justify-end gap-2 pt-1">
              <button
                v-if="currentVideo.isReviewed"
                type="button"
                class="rounded-lg px-3 py-1.5 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 transition"
                @click="isEditing = false"
              >
                Cancel
              </button>
              <BaseButton
                size="sm"
                variant="accent"
                :disabled="!feedbackText.trim() || isSubmitting"
                @click="handleSaveReview"
              >
                <Loader2 v-if="isSubmitting" class="h-3.5 w-3.5 animate-spin" />
                <Send v-else class="h-3.5 w-3.5" />
                {{ currentVideo.isReviewed ? 'Update Feedback' : 'Send Feedback to Owner' }}
              </BaseButton>
            </div>
          </div>

          <!-- Existing Review Display (When Reviewed and not editing) -->
          <div
            v-else
            class="rounded-xl border-2 border-emerald-500/30 bg-emerald-50/40 p-3.5 shadow-2xs space-y-2.5"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-1.5 font-bold text-emerald-800 text-xs">
                <CheckCircle2 class="h-4 w-4 text-emerald-600" />
                Clinical Feedback Sent to Owner
              </div>
              <button
                type="button"
                class="inline-flex items-center gap-1 rounded-lg bg-white px-2.5 py-1 text-xs font-bold text-sage border border-sage/40 hover:bg-sage hover:text-white shadow-2xs transition"
                @click="isEditing = true"
              >
                <Edit3 class="h-3 w-3" />
                Edit Feedback
              </button>
            </div>
            <div class="rounded-lg bg-white p-3 border border-emerald-200/80 text-xs text-navy">
              <p class="font-medium whitespace-pre-wrap leading-relaxed">{{ currentVideo.physioFeedbackNotes }}</p>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="empty-state mt-4 py-8">
        <p class="text-sm text-neutral-muted">No owner video uploads yet.</p>
      </div>
    </section>

    <PatientProgressChart v-if="progress && progress.logs.length > 0" :progress="progress" />
  </div>
</template>

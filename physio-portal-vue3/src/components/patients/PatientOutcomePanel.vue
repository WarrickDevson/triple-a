<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Play, CheckCircle2, Clock, Send, Edit3, Loader2, MessageSquare } from '@lucide/vue'
import PatientProgressChart from '../PatientProgressChart.vue'
import BaseButton from '../BaseButton.vue'
import { OUTCOME_MEASURES } from '../../data/patientDemo'
import { resolveMediaUrl, reviewVideo } from '../../api/videos'
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

// Review Form State
const feedbackText = ref('')
const isEditing = ref(false)
const isSubmitting = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

watch(
  () => props.latestVideo,
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
  if (!props.latestVideo || !feedbackText.value.trim()) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const updated = await reviewVideo(props.latestVideo.videoSubmissionId, {
      feedbackNotes: feedbackText.value.trim(),
    })
    props.latestVideo.isReviewed = true
    props.latestVideo.physioFeedbackNotes = updated.physioFeedbackNotes || feedbackText.value.trim()
    isEditing.value = false
    successMessage.value = 'Review submitted and sent to pet owner.'
    setTimeout(() => {
      successMessage.value = ''
    }, 4000)
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to submit review. Please try again.'
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
        <h3 class="text-sm font-bold text-navy">Latest Owner Upload</h3>
        <span
          v-if="latestVideo"
          class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold"
          :class="
            latestVideo.isReviewed
              ? 'bg-emerald-100 text-emerald-800'
              : 'bg-amber-100 text-amber-800'
          "
        >
          <component :is="latestVideo.isReviewed ? CheckCircle2 : Clock" class="h-3 w-3" />
          {{ latestVideo.isReviewed ? 'Reviewed' : 'Pending Review' }}
        </span>
      </div>

      <div v-if="latestVideo" class="mt-4 space-y-3">
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

        <div>
          <p class="text-sm font-semibold text-navy">
            {{ latestVideo.exerciseTitle || latestVideo.title || 'General Progress Video' }}
          </p>
          <p v-if="latestVideo.notes" class="mt-1 rounded-lg bg-surface p-2 text-xs italic text-navy/90 border border-neutral-grey/80">
            <span class="font-semibold not-italic text-neutral-muted">Owner Note:</span> "{{ latestVideo.notes }}"
          </p>
          <p class="mt-1 text-[11px] text-neutral-muted">
            Uploaded {{ new Date(latestVideo.createdDate).toLocaleString() }}
          </p>
        </div>

        <!-- Physio Review Section -->
        <div class="border-t border-neutral-grey/80 pt-3">
          <div class="flex items-center justify-between mb-2">
            <h4 class="text-xs font-bold uppercase tracking-wider text-navy flex items-center gap-1.5">
              <MessageSquare class="h-3.5 w-3.5 text-sage" />
              Physiotherapist Review & Feedback
            </h4>
            <button
              v-if="latestVideo.isReviewed && !isEditing"
              type="button"
              class="inline-flex items-center gap-1 text-[11px] font-semibold text-sage hover:underline"
              @click="isEditing = true"
            >
              <Edit3 class="h-3 w-3" />
              Edit Feedback
            </button>
          </div>

          <!-- Existing Review Display -->
          <div
            v-if="latestVideo.isReviewed && !isEditing"
            class="rounded-xl border border-sage/30 bg-sage-muted/40 p-3 text-xs text-navy"
          >
            <p class="font-medium whitespace-pre-wrap">{{ latestVideo.physioFeedbackNotes }}</p>
          </div>

          <!-- Review Input Form (Pending or Editing) -->
          <div v-else class="space-y-2">
            <textarea
              v-model="feedbackText"
              rows="3"
              class="portal-input w-full text-xs"
              placeholder="Provide clinical feedback on exercise form, movement quality, or progression instructions..."
            />

            <div v-if="errorMessage" class="text-xs font-medium text-rose-600">
              {{ errorMessage }}
            </div>
            <div v-if="successMessage" class="text-xs font-medium text-emerald-700">
              {{ successMessage }}
            </div>

            <div class="flex items-center justify-end gap-2 pt-1">
              <button
                v-if="latestVideo.isReviewed"
                type="button"
                class="rounded-lg px-2.5 py-1.5 text-xs font-medium text-neutral-muted hover:bg-neutral-grey/40"
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
                {{ latestVideo.isReviewed ? 'Update Feedback' : 'Send Review to Owner' }}
              </BaseButton>
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

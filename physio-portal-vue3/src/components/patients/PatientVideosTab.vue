<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import {
  Play,
  CheckCircle2,
  Clock,
  Search,
  Calendar,
  X,
  Send,
  Edit3,
  Loader2,
  Video,
  MessageSquare,
  Sparkles,
  Trash2,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { resolveMediaUrl, reviewVideo, deleteVideo, updateVideo } from '../../api/videos'
import { getVideoTitle, type VideoSubmission } from '../../types/video'
import type { Pet } from '../../types/pet'

const props = withDefaults(
  defineProps<{
    patient: Pet
    videos?: VideoSubmission[]
    loading?: boolean
  }>(),
  {
    videos: () => [],
    loading: false,
  },
)

const emit = defineEmits<{
  (e: 'video-deleted', videoId: number): void
  (e: 'refresh'): void
}>()

const route = useRoute()

// Filter and Search States
const searchQuery = ref('')
const statusFilter = ref<'all' | 'pending' | 'reviewed'>('all')
const selectedExercise = ref<string>('all')
const sortOrder = ref<'desc' | 'asc'>('desc')
const startDate = ref<string>('')
const endDate = ref<string>('')

// Active Video for Review Modal
const activeModalVideo = ref<VideoSubmission | null>(null)
const feedbackText = ref('')
const isSubmitting = ref(false)
const modalSuccess = ref('')
const modalError = ref('')

// Check if route has query videoId to auto-open modal
watch(
  () => [props.videos, route.query.videoId, route.query.tab],
  () => {
    if (route.query.tab === 'videos' && route.query.videoId && props.videos.length > 0) {
      const match = props.videos.find((v) => v.videoSubmissionId === Number(route.query.videoId))
      if (match) {
        openReviewModal(match)
      }
    }
  },
  { immediate: true },
)

// Unique Exercise Titles for dropdown
const exerciseOptions = computed(() => {
  const exercises = new Set<string>()
  for (const v of props.videos) {
    if (v.exerciseTitle && v.exerciseTitle.trim()) {
      exercises.add(v.exerciseTitle.trim())
    }
  }
  return Array.from(exercises)
})

// Counts
const totalCount = computed(() => props.videos.length)
const pendingCount = computed(() => props.videos.filter((v) => !v.isReviewed).length)
const reviewedCount = computed(() => props.videos.filter((v) => v.isReviewed).length)

// Filtered Videos List
const filteredVideos = computed(() => {
  return props.videos
    .filter((v) => {
      // Status filter
      if (statusFilter.value === 'pending' && v.isReviewed) return false
      if (statusFilter.value === 'reviewed' && !v.isReviewed) return false

      // Exercise filter
      if (selectedExercise.value !== 'all' && v.exerciseTitle !== selectedExercise.value) {
        return false
      }

      // Date range filter
      if (startDate.value) {
        const vDate = new Date(v.createdDate).setHours(0, 0, 0, 0)
        const sDate = new Date(startDate.value).setHours(0, 0, 0, 0)
        if (vDate < sDate) return false
      }
      if (endDate.value) {
        const vDate = new Date(v.createdDate).setHours(23, 59, 59, 999)
        const eDate = new Date(endDate.value).setHours(23, 59, 59, 999)
        if (vDate > eDate) return false
      }

      // Search Query filter (matches title, exerciseTitle, notes, or ID)
      if (searchQuery.value.trim()) {
        const q = searchQuery.value.trim().toLowerCase()
        const title = (v.title || '').toLowerCase()
        const exTitle = (v.exerciseTitle || '').toLowerCase()
        const notes = (v.notes || '').toLowerCase()
        const idStr = v.videoSubmissionId.toString()

        return (
          title.includes(q) ||
          exTitle.includes(q) ||
          notes.includes(q) ||
          idStr.includes(q)
        )
      }

      return true
    })
    .sort((a, b) => {
      const timeA = new Date(a.createdDate).getTime()
      const timeB = new Date(b.createdDate).getTime()
      return sortOrder.value === 'desc' ? timeB - timeA : timeA - timeB
    })
})

const hasActiveFilters = computed(() => {
  return (
    searchQuery.value.trim() !== '' ||
    statusFilter.value !== 'all' ||
    selectedExercise.value !== 'all' ||
    startDate.value !== '' ||
    endDate.value !== '' ||
    sortOrder.value !== 'desc'
  )
})

function clearFilters() {
  searchQuery.value = ''
  statusFilter.value = 'all'
  selectedExercise.value = 'all'
  startDate.value = ''
  endDate.value = ''
  sortOrder.value = 'desc'
}

function openReviewModal(video: VideoSubmission) {
  activeModalVideo.value = video
  feedbackText.value = video.physioFeedbackNotes || ''
  modalSuccess.value = ''
  modalError.value = ''
}

function closeModal() {
  activeModalVideo.value = null
  feedbackText.value = ''
  modalSuccess.value = ''
  modalError.value = ''
}

async function handleModalSaveReview() {
  if (!activeModalVideo.value || !feedbackText.value.trim()) return

  isSubmitting.value = true
  modalError.value = ''
  modalSuccess.value = ''

  try {
    const updated = await reviewVideo(activeModalVideo.value.videoSubmissionId, {
      feedbackNotes: feedbackText.value.trim(),
    })
    activeModalVideo.value.isReviewed = true
    activeModalVideo.value.physioFeedbackNotes = updated.physioFeedbackNotes || feedbackText.value.trim()
    modalSuccess.value = 'Feedback submitted and sent to pet owner.'
    emit('refresh')
    setTimeout(() => {
      modalSuccess.value = ''
    }, 4000)
  } catch (err: any) {
    modalError.value = err?.response?.data?.message || 'Failed to submit review. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}

// Delete Video
const isDeleting = ref(false)
async function handleDeleteVideo(video: VideoSubmission) {
  if (!confirm(`Are you sure you want to delete "${getVideoTitle(video)}"? This submission will be removed.`)) {
    return
  }
  try {
    isDeleting.value = true
    await deleteVideo(video.videoSubmissionId)
    if (activeModalVideo.value?.videoSubmissionId === video.videoSubmissionId) {
      closeModal()
    }
    emit('video-deleted', video.videoSubmissionId)
    emit('refresh')
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Failed to delete video submission.')
  } finally {
    isDeleting.value = false
  }
}

// Edit Video Details Modal
const activeEditVideo = ref<VideoSubmission | null>(null)
const editTitle = ref('')
const editNotes = ref('')
const isEditSaving = ref(false)
const editError = ref('')

function openEditModal(video: VideoSubmission) {
  activeEditVideo.value = video
  editTitle.value = video.title || ''
  editNotes.value = video.notes || ''
  editError.value = ''
}

function closeEditModal() {
  activeEditVideo.value = null
  editTitle.value = ''
  editNotes.value = ''
  editError.value = ''
}

async function handleSaveEditVideo() {
  if (!activeEditVideo.value) return
  isEditSaving.value = true
  editError.value = ''
  try {
    const updated = await updateVideo(activeEditVideo.value.videoSubmissionId, {
      title: editTitle.value.trim() || null,
      notes: editNotes.value.trim() || null,
    })
    activeEditVideo.value.title = updated.title
    activeEditVideo.value.notes = updated.notes
    if (activeModalVideo.value?.videoSubmissionId === activeEditVideo.value.videoSubmissionId) {
      activeModalVideo.value.title = updated.title
      activeModalVideo.value.notes = updated.notes
    }
    closeEditModal()
    emit('refresh')
  } catch (err: any) {
    editError.value = err?.response?.data?.message || 'Failed to update video details.'
  } finally {
    isEditSaving.value = false
  }
}
</script>

<template>
  <div class="space-y-4">
    <!-- Header Stats Banner -->
    <div class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-neutral-grey/70 bg-surface p-3.5">
      <div class="flex items-center gap-2.5">
        <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-sage/20 text-sage">
          <Video class="h-5 w-5" />
        </div>
        <div>
          <h3 class="text-sm font-bold text-navy">Video Submissions Library</h3>
          <p class="text-xs text-neutral-muted">
            All movement recordings submitted by {{ patient.petName }}'s owner.
          </p>
        </div>
      </div>
      <div class="flex items-center gap-2 text-xs">
        <span class="rounded-lg bg-surface px-2.5 py-1 font-semibold text-navy border border-neutral-grey">
          Total: <strong class="text-navy font-bold">{{ totalCount }}</strong>
        </span>
        <span class="rounded-lg bg-amber-50 px-2.5 py-1 font-semibold text-amber-800 border border-amber-200">
          Pending: <strong class="font-bold">{{ pendingCount }}</strong>
        </span>
        <span class="rounded-lg bg-emerald-50 px-2.5 py-1 font-semibold text-emerald-800 border border-emerald-200">
          Reviewed: <strong class="font-bold">{{ reviewedCount }}</strong>
        </span>
      </div>
    </div>

    <!-- Search & Filter Controls Card -->
    <div class="rounded-xl border border-neutral-grey/80 bg-white p-3.5 shadow-2xs space-y-3">
      <!-- Search input and main filters -->
      <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <!-- Search bar -->
        <div class="relative lg:col-span-2">
          <Search class="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search by title, notes, or exercise..."
            class="portal-input w-full pl-9 text-xs"
          />
          <button
            v-if="searchQuery"
            type="button"
            class="absolute right-2.5 top-1/2 -translate-y-1/2 text-neutral-muted hover:text-navy"
            @click="searchQuery = ''"
          >
            <X class="h-3.5 w-3.5" />
          </button>
        </div>

        <!-- Exercise Filter Dropdown -->
        <div>
          <select v-model="selectedExercise" class="portal-input w-full text-xs">
            <option value="all">All Exercises</option>
            <option v-for="ex in exerciseOptions" :key="ex" :value="ex">
              {{ ex }}
            </option>
          </select>
        </div>

        <!-- Sort Order Dropdown -->
        <div>
          <select v-model="sortOrder" class="portal-input w-full text-xs">
            <option value="desc">Newest First</option>
            <option value="asc">Oldest First</option>
          </select>
        </div>
      </div>

      <!-- Secondary Filters: Status Tabs & Date Range -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-neutral-grey/50 pt-2.5 text-xs">
        <!-- Status Pills -->
        <div class="flex gap-1.5">
          <button
            type="button"
            class="rounded-lg px-2.5 py-1 font-semibold transition"
            :class="
              statusFilter === 'all'
                ? 'bg-navy text-white'
                : 'bg-surface text-neutral-muted hover:bg-neutral-grey/40 border border-neutral-grey'
            "
            @click="statusFilter = 'all'"
          >
            All ({{ totalCount }})
          </button>
          <button
            type="button"
            class="rounded-lg px-2.5 py-1 font-semibold transition flex items-center gap-1"
            :class="
              statusFilter === 'pending'
                ? 'bg-amber-600 text-white'
                : 'bg-amber-50 text-amber-800 hover:bg-amber-100 border border-amber-200'
            "
            @click="statusFilter = 'pending'"
          >
            <Clock class="h-3 w-3" />
            Pending ({{ pendingCount }})
          </button>
          <button
            type="button"
            class="rounded-lg px-2.5 py-1 font-semibold transition flex items-center gap-1"
            :class="
              statusFilter === 'reviewed'
                ? 'bg-emerald-600 text-white'
                : 'bg-emerald-50 text-emerald-800 hover:bg-emerald-100 border border-emerald-200'
            "
            @click="statusFilter = 'reviewed'"
          >
            <CheckCircle2 class="h-3 w-3" />
            Reviewed ({{ reviewedCount }})
          </button>
        </div>

        <!-- Date Filters -->
        <div class="flex flex-wrap items-center gap-2">
          <div class="flex items-center gap-1.5 text-neutral-muted">
            <Calendar class="h-3.5 w-3.5" />
            <span>From:</span>
            <input
              v-model="startDate"
              type="date"
              class="portal-input py-0.5 px-2 text-xs"
            />
          </div>
          <div class="flex items-center gap-1.5 text-neutral-muted">
            <span>To:</span>
            <input
              v-model="endDate"
              type="date"
              class="portal-input py-0.5 px-2 text-xs"
            />
          </div>

          <button
            v-if="hasActiveFilters"
            type="button"
            class="inline-flex items-center gap-1 text-[11px] font-bold text-alert-red hover:underline ml-1"
            @click="clearFilters"
          >
            <X class="h-3 w-3" />
            Clear Filters
          </button>
        </div>
      </div>
    </div>

    <!-- Video Submissions List -->
    <div v-if="loading" class="py-12 text-center text-sm text-neutral-muted">
      <Loader2 class="mx-auto h-6 w-6 animate-spin text-sage mb-2" />
      Loading video submissions...
    </div>

    <div v-else-if="filteredVideos.length === 0" class="empty-state py-12 text-center">
      <Video class="mx-auto h-10 w-10 text-neutral-muted/60 mb-2" />
      <p class="text-sm font-semibold text-navy">No video submissions found</p>
      <p class="text-xs text-neutral-muted mt-1">
        {{ hasActiveFilters ? 'No submissions match your search and filter criteria.' : 'No videos have been uploaded for this patient yet.' }}
      </p>
      <BaseButton
        v-if="hasActiveFilters"
        size="sm"
        variant="secondary"
        class="mt-3 text-xs"
        @click="clearFilters"
      >
        Reset Filters
      </BaseButton>
    </div>

    <!-- Videos Grid / List -->
    <div v-else class="space-y-3">
      <div
        v-for="video in filteredVideos"
        :key="video.videoSubmissionId"
        class="portal-card overflow-hidden transition hover:shadow-md border border-neutral-grey/80 bg-white"
      >
        <div class="grid gap-4 p-4 sm:grid-cols-[180px_1fr]">
          <!-- Video Preview Card / Player -->
          <div class="relative overflow-hidden rounded-xl bg-navy/5 aspect-video sm:aspect-auto sm:h-full flex items-center justify-center">
            <video
              v-if="resolveMediaUrl(video.processedVideoStreamingUrl || video.rawVideoStorageUrl)"
              :src="resolveMediaUrl(video.processedVideoStreamingUrl || video.rawVideoStorageUrl)!"
              class="h-full w-full object-cover rounded-xl"
              preload="metadata"
              controls
            />
            <div
              v-else
              class="flex h-full w-full items-center justify-center bg-navy/10 text-neutral-muted"
            >
              <Play class="h-8 w-8" :stroke-width="1.5" />
            </div>
          </div>

          <!-- Video Details and Review State -->
          <div class="flex flex-col justify-between space-y-2.5">
            <div>
              <div class="flex flex-wrap items-center justify-between gap-2">
                <div class="flex items-center gap-2">
                  <h4 class="text-sm font-bold text-navy">
                    {{ getVideoTitle(video) }}
                  </h4>
                  <span class="rounded bg-neutral-grey/40 px-1.5 py-0.5 text-[10px] font-semibold text-neutral-muted">
                    #{{ video.videoSubmissionId }}
                  </span>
                </div>

                <!-- Status Badge -->
                <span
                  class="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[11px] font-bold"
                  :class="
                    video.isReviewed
                      ? 'bg-emerald-100 text-emerald-800 border border-emerald-300/60'
                      : 'bg-amber-100 text-amber-800 border border-amber-300/60'
                  "
                >
                  <component :is="video.isReviewed ? CheckCircle2 : Clock" class="h-3.5 w-3.5" />
                  {{ video.isReviewed ? 'Reviewed' : 'Pending Review' }}
                </span>
              </div>

              <!-- Exercise Tag and Timestamp -->
              <div class="mt-1 flex flex-wrap items-center gap-3 text-xs text-neutral-muted">
                <span v-if="video.exerciseTitle" class="inline-flex items-center gap-1 font-medium text-sage">
                  <Sparkles class="h-3.5 w-3.5" />
                  {{ video.exerciseTitle }}
                </span>
                <span>
                  Uploaded {{ new Date(video.createdDate).toLocaleDateString() }} at {{ new Date(video.createdDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                </span>
              </div>

              <!-- Owner's Note Box -->
              <div
                v-if="video.notes"
                class="mt-2.5 rounded-lg border border-neutral-grey/80 bg-surface/80 p-2.5 text-xs text-navy"
              >
                <p class="font-semibold text-neutral-muted text-[11px] mb-0.5">Owner Note:</p>
                <p class="italic text-navy/90">"{{ video.notes }}"</p>
              </div>

              <!-- Physio Clinical Feedback Quote -->
              <div
                v-if="video.isReviewed && video.physioFeedbackNotes"
                class="mt-2.5 rounded-lg border border-sage/30 bg-sage-muted/30 p-2.5 text-xs text-navy"
              >
                <div class="flex items-center gap-1.5 font-bold text-sage text-[11px] mb-0.5">
                  <MessageSquare class="h-3 w-3" />
                  Clinical Feedback:
                </div>
                <p class="font-medium whitespace-pre-wrap">{{ video.physioFeedbackNotes }}</p>
              </div>
            </div>

            <!-- Action Bar -->
            <div class="flex items-center justify-between border-t border-neutral-grey/60 pt-2.5">
              <div class="flex items-center gap-1">
                <button
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy transition"
                  title="Edit title or notes"
                  @click="openEditModal(video)"
                >
                  <Edit3 class="h-3.5 w-3.5" />
                  Edit
                </button>
                <button
                  type="button"
                  class="inline-flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-semibold text-alert-red/80 hover:bg-rose-50 hover:text-alert-red transition"
                  title="Delete video submission"
                  @click="handleDeleteVideo(video)"
                >
                  <Trash2 class="h-3.5 w-3.5" />
                  Delete
                </button>
              </div>

              <BaseButton
                size="sm"
                variant="accent"
                class="text-xs"
                @click="openReviewModal(video)"
              >
                <Edit3 class="h-3.5 w-3.5" />
                {{ video.isReviewed ? 'Edit Clinical Feedback' : 'Watch & Review' }}
              </BaseButton>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Review & Video Watcher Modal -->
    <div
      v-if="activeModalVideo"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-xs"
      @click.self="closeModal"
    >
      <div class="portal-card max-h-[90vh] w-full max-w-2xl overflow-y-auto bg-white p-5 shadow-xl rounded-2xl">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-3">
          <div class="flex-1 pr-3">
            <div class="flex items-center gap-2">
              <h3 class="text-base font-bold text-navy">
                {{ getVideoTitle(activeModalVideo) }}
              </h3>
              <button
                type="button"
                class="rounded-md p-1 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
                title="Edit title & notes"
                @click="openEditModal(activeModalVideo)"
              >
                <Edit3 class="h-3.5 w-3.5" />
              </button>
            </div>
            <p class="text-xs text-neutral-muted">
              Uploaded {{ new Date(activeModalVideo.createdDate).toLocaleString() }} · #{{ activeModalVideo.videoSubmissionId }}
            </p>
          </div>
          <button
            type="button"
            class="rounded-lg p-1.5 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
            @click="closeModal"
          >
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="mt-4 space-y-4">
          <!-- Main Modal Video Player -->
          <div class="overflow-hidden rounded-xl bg-black aspect-video flex items-center justify-center">
            <video
              v-if="resolveMediaUrl(activeModalVideo.processedVideoStreamingUrl || activeModalVideo.rawVideoStorageUrl)"
              :src="resolveMediaUrl(activeModalVideo.processedVideoStreamingUrl || activeModalVideo.rawVideoStorageUrl)!"
              class="h-full w-full"
              controls
              autoplay
            />
            <div v-else class="text-white text-xs">Video unavailable</div>
          </div>

          <!-- Owner Notes in Modal -->
          <div v-if="activeModalVideo.notes" class="rounded-xl border border-neutral-grey bg-surface p-3 text-xs flex items-start justify-between gap-2">
            <div>
              <span class="font-bold text-neutral-muted">Owner Note:</span>
              <p class="mt-0.5 italic text-navy font-medium">"{{ activeModalVideo.notes }}"</p>
            </div>
            <button
              type="button"
              class="text-[11px] font-semibold text-sage hover:underline"
              @click="openEditModal(activeModalVideo)"
            >
              Edit
            </button>
          </div>

          <!-- Review Input Form -->
          <div class="rounded-xl border-2 border-sage/50 bg-sage-muted/30 p-4 shadow-xs space-y-3">
            <div class="flex items-center justify-between">
              <label class="block text-xs font-bold uppercase tracking-wider text-navy flex items-center gap-1.5">
                <MessageSquare class="h-4 w-4 text-sage" />
                Clinical Feedback & Progression Notes
              </label>
              <span
                class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold"
                :class="activeModalVideo.isReviewed ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'"
              >
                <component :is="activeModalVideo.isReviewed ? CheckCircle2 : Clock" class="h-3 w-3" />
                {{ activeModalVideo.isReviewed ? 'Reviewed' : 'Pending Review' }}
              </span>
            </div>

            <textarea
              v-model="feedbackText"
              rows="4"
              class="w-full rounded-xl border-2 border-sage/40 bg-white p-3 text-xs font-medium text-navy placeholder:text-neutral-muted/70 focus:border-sage focus:ring-2 focus:ring-sage/20 outline-none shadow-xs transition-all"
              placeholder="Enter clinical observations, exercise form guidance, gait analysis, or progression advice for the owner..."
            />

            <div v-if="modalError" class="rounded-lg bg-rose-50 border border-rose-200 p-2 text-xs font-medium text-rose-700">
              {{ modalError }}
            </div>
            <div v-if="modalSuccess" class="rounded-lg bg-emerald-50 border border-emerald-200 p-2 text-xs font-medium text-emerald-800">
              {{ modalSuccess }}
            </div>

            <div class="flex items-center justify-between pt-1">
              <button
                type="button"
                class="inline-flex items-center gap-1 text-xs font-semibold text-alert-red hover:underline"
                @click="handleDeleteVideo(activeModalVideo)"
              >
                <Trash2 class="h-3.5 w-3.5" />
                Delete Video
              </button>

              <div class="flex items-center gap-2">
                <button
                  type="button"
                  class="rounded-lg px-3 py-1.5 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 transition"
                  @click="closeModal"
                >
                  Close
                </button>
                <BaseButton
                  size="sm"
                  variant="accent"
                  :disabled="!feedbackText.trim() || isSubmitting"
                  @click="handleModalSaveReview"
                >
                  <Loader2 v-if="isSubmitting" class="h-3.5 w-3.5 animate-spin" />
                  <Send v-else class="h-3.5 w-3.5" />
                  {{ activeModalVideo.isReviewed ? 'Update Feedback' : 'Send Feedback to Owner' }}
                </BaseButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit Video Details Modal -->
    <div
      v-if="activeEditVideo"
      class="fixed inset-0 z-60 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-xs"
      @click.self="closeEditModal"
    >
      <div class="portal-card max-h-[90vh] w-full max-w-md overflow-y-auto bg-white p-5 shadow-xl rounded-2xl space-y-4">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-3">
          <h3 class="text-base font-bold text-navy">Edit Video Submission</h3>
          <button
            type="button"
            class="rounded-lg p-1.5 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
            @click="closeEditModal"
          >
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="space-y-3 text-xs">
          <div>
            <label class="block font-bold text-navy mb-1">Title / Caption</label>
            <input
              v-model="editTitle"
              type="text"
              class="portal-input w-full"
              placeholder="e.g. Walking in garden, Standing exercise..."
            />
          </div>

          <div>
            <label class="block font-bold text-navy mb-1">Owner Notes</label>
            <textarea
              v-model="editNotes"
              rows="3"
              class="portal-input w-full"
              placeholder="Observations submitted with this video..."
            />
          </div>

          <div v-if="editError" class="rounded-lg bg-rose-50 border border-rose-200 p-2 text-rose-700">
            {{ editError }}
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 border-t border-neutral-grey/80 pt-3">
          <button
            type="button"
            class="rounded-lg px-3 py-1.5 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 transition"
            @click="closeEditModal"
          >
            Cancel
          </button>
          <BaseButton
            size="sm"
            variant="accent"
            :disabled="isEditSaving"
            @click="handleSaveEditVideo"
          >
            <Loader2 v-if="isEditSaving" class="h-3.5 w-3.5 animate-spin" />
            Save Changes
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

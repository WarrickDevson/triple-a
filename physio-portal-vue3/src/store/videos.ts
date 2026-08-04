import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchPendingVideos, reviewVideo } from '../api/videos'
import type { VideoSubmission } from '../types/video'

export const useVideosStore = defineStore('videos', () => {
  const pendingVideos = ref<VideoSubmission[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadPending() {
    loading.value = true
    error.value = null
    try {
      pendingVideos.value = await fetchPendingVideos()
    } catch {
      error.value = 'Unable to load pending video reviews.'
    } finally {
      loading.value = false
    }
  }

  async function submitReview(videoSubmissionId: number, feedbackNotes: string) {
    await reviewVideo(videoSubmissionId, { feedbackNotes })
    pendingVideos.value = pendingVideos.value.filter((v) => v.videoSubmissionId !== videoSubmissionId)
  }

  return {
    pendingVideos,
    loading,
    error,
    loadPending,
    submitReview,
  }
})

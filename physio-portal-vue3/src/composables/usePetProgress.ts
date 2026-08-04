import { ref, watch } from 'vue'
import { getPetProgress } from '../api/progress'
import { getPetVideos } from '../api/videos'
import type { PetProgressSummary } from '../types/dashboard'
import type { VideoSubmission } from '../types/video'

export function usePetProgress(petId: () => number | null) {
  const progress = ref<PetProgressSummary | null>(null)
  const latestVideo = ref<VideoSubmission | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function load(id: number) {
    loading.value = true
    error.value = null
    progress.value = null
    latestVideo.value = null

    try {
      const [progressResult, videosResult] = await Promise.allSettled([
        getPetProgress(id),
        getPetVideos(id),
      ])

      if (progressResult.status === 'fulfilled') progress.value = progressResult.value
      if (videosResult.status === 'fulfilled' && videosResult.value.length > 0) {
        latestVideo.value = [...videosResult.value].sort(
          (a, b) => new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime(),
        )[0]!
      }
    } catch {
      error.value = 'Unable to load progress data.'
    } finally {
      loading.value = false
    }
  }

  watch(
    () => petId(),
    (id) => {
      if (id) load(id)
    },
    { immediate: true },
  )

  return { progress, latestVideo, loading, error, reload: () => {
    const id = petId()
    if (id) return load(id)
  } }
}

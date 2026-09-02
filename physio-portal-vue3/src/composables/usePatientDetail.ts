import { computed, ref, watch } from 'vue'
import { fetchAppointments } from '../api/appointments'
import { getPetProgress } from '../api/progress'
import { getRehabProgramsByPet } from '../api/rehab-programs'
import { getPetVideos } from '../api/videos'
import { getPatientDemoMeta } from '../data/patientDemo'
import type { Appointment } from '../types/appointment'
import type { PetProgressSummary } from '../types/dashboard'
import type { RehabProgram } from '../types/exercise'
import type { Pet } from '../types/pet'
import type { VideoSubmission } from '../types/video'

export function usePatientDetail(pet: () => Pet | null) {
  const loading = ref(false)
  const error = ref<string | null>(null)
  const progress = ref<PetProgressSummary | null>(null)
  const programs = ref<RehabProgram[]>([])
  const appointments = ref<Appointment[]>([])
  const videos = ref<VideoSubmission[]>([])

  const demoMeta = computed(() => {
    const current = pet()
    if (!current) return null
    return getPatientDemoMeta(current.petId, current.species)
  })

  const activeProgram = computed(() => {
    if (programs.value.length === 0) return null
    return [...programs.value].sort(
      (a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime(),
    )[0]
  })

  const nextAppointment = computed(() => {
    const current = pet()
    if (!current) return null
    const now = Date.now()
    return appointments.value
      .filter(
        (a) =>
          a.petId === current.petId &&
          new Date(a.scheduledDateTime).getTime() >= now &&
          a.appointmentStatus.toLowerCase() !== 'cancelled',
      )
      .sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime())[0] ?? null
  })

  const latestVideo = computed(() => {
    if (videos.value.length === 0) return null
    return [...videos.value].sort(
      (a, b) => new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime(),
    )[0]
  })

  const progressPercent = computed(() => {
    if (progress.value && progress.value.totalTrackedDays > 0) {
      const completed = progress.value.logs.filter((l) => l.isCompleted).length
      const rate = Math.round((completed / progress.value.totalTrackedDays) * 100)
      return Math.min(100, Math.max(0, rate))
    }
    return 0
  })

  async function loadDetail(petId: number) {
    loading.value = true
    error.value = null
    progress.value = null
    programs.value = []
    appointments.value = []
    videos.value = []

    try {
      const [progressResult, programsResult, appointmentsResult, videosResult] =
        await Promise.allSettled([
          getPetProgress(petId),
          getRehabProgramsByPet(petId),
          fetchAppointments(),
          getPetVideos(petId),
        ])

      if (progressResult.status === 'fulfilled') progress.value = progressResult.value
      if (programsResult.status === 'fulfilled') programs.value = programsResult.value
      if (appointmentsResult.status === 'fulfilled') appointments.value = appointmentsResult.value
      if (videosResult.status === 'fulfilled') videos.value = videosResult.value
    } catch {
      error.value = 'Unable to load patient details.'
    } finally {
      loading.value = false
    }
  }

  watch(
    () => pet()?.petId,
    (petId) => {
      if (petId) loadDetail(petId)
    },
    { immediate: true },
  )

  return {
    loading,
    error,
    progress,
    programs,
    activeProgram,
    nextAppointment,
    latestVideo,
    videos,
    demoMeta,
    progressPercent,
    reload: () => {
      const id = pet()?.petId
      if (id) return loadDetail(id)
    },
  }
}

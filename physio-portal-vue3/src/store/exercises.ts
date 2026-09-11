import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  createExercise,
  customizeExercise as apiCustomizeExercise,
  deleteExercise as apiDeleteExercise,
  getExercises,
  toggleActiveExercise as apiToggleActiveExercise,
  updateExercise as apiUpdateExercise,
  uploadExerciseMedia,
} from '../api/exercises'
import type { CreateExerciseRequest, Exercise } from '../types/exercise'

const FAVOURITES_KEY = 'triple-a-exercise-favourites'

function loadFavourites(): number[] {
  try {
    const raw = localStorage.getItem(FAVOURITES_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw) as number[]
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

function saveFavourites(ids: number[]) {
  localStorage.setItem(FAVOURITES_KEY, JSON.stringify(ids))
}

export const useExercisesStore = defineStore('exercises', () => {
  const exercises = ref<Exercise[]>([])
  const favourites = ref<number[]>(loadFavourites())
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchExercises(species?: string, condition?: string, force = false) {
    if (exercises.value.length > 0 && !force && !species && !condition) {
      return exercises.value
    }

    loading.value = true
    error.value = null
    try {
      exercises.value = await getExercises(species, condition)
      return exercises.value
    } catch {
      error.value = 'Unable to load exercises.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  async function addExercise(request: CreateExerciseRequest): Promise<Exercise> {
    loading.value = true
    error.value = null
    try {
      const created = await createExercise(request)
      exercises.value = [created, ...exercises.value]
      return created
    } finally {
      loading.value = false
    }
  }

  async function editExercise(id: number, request: CreateExerciseRequest): Promise<Exercise> {
    loading.value = true
    error.value = null
    try {
      const updated = await apiUpdateExercise(id, request)
      const index = exercises.value.findIndex((e) => e.exerciseId === id)
      if (index !== -1) {
        exercises.value[index] = updated
      }
      return updated
    } finally {
      loading.value = false
    }
  }

  async function customizeExercise(id: number): Promise<Exercise> {
    loading.value = true
    error.value = null
    try {
      const cloned = await apiCustomizeExercise(id)
      // Update the base exercise status in the list
      const baseIndex = exercises.value.findIndex((e) => e.exerciseId === id)
      if (baseIndex !== -1) {
        exercises.value[baseIndex] = {
          ...exercises.value[baseIndex],
          hasCustomOverride: true,
          customExerciseId: cloned.exerciseId,
          isCustomActive: true,
        }
      }
      // Also add the new cloned custom exercise to the list if not present
      if (!exercises.value.some((e) => e.exerciseId === cloned.exerciseId)) {
        exercises.value = [cloned, ...exercises.value]
      }
      return cloned
    } finally {
      loading.value = false
    }
  }

  async function toggleActive(id: number): Promise<boolean> {
    try {
      const res = await apiToggleActiveExercise(id)
      // Update any exercise matching this id or with this base id
      exercises.value = exercises.value.map((e) => {
        if (e.exerciseId === res.exerciseId || e.customExerciseId === res.exerciseId || (res.baseExerciseId && e.exerciseId === res.baseExerciseId)) {
          return {
            ...e,
            isCustomActive: res.isActiveForOwners,
            isActiveForOwners: res.isActiveForOwners,
          }
        }
        return e
      })
      return res.isActiveForOwners
    } catch (err) {
      console.error('Failed to toggle exercise active state', err)
      throw err
    }
  }

  async function removeExercise(id: number): Promise<void> {
    loading.value = true
    try {
      await apiDeleteExercise(id)
      exercises.value = exercises.value.filter((e) => e.exerciseId !== id)
    } finally {
      loading.value = false
    }
  }

  async function uploadMedia(file: File) {
    return await uploadExerciseMedia(file)
  }

  function isFavourite(exerciseId: number) {
    return favourites.value.includes(exerciseId)
  }

  function toggleFavourite(exerciseId: number) {
    if (favourites.value.includes(exerciseId)) {
      favourites.value = favourites.value.filter((id) => id !== exerciseId)
    } else {
      favourites.value = [...favourites.value, exerciseId]
    }
    saveFavourites(favourites.value)
  }

  return {
    exercises,
    favourites,
    loading,
    error,
    fetchExercises,
    addExercise,
    editExercise,
    customizeExercise,
    toggleActive,
    removeExercise,
    uploadMedia,
    isFavourite,
    toggleFavourite,
  }
})


import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getExercises } from '../api/exercises'
import type { Exercise } from '../types/exercise'

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
    isFavourite,
    toggleFavourite,
  }
})

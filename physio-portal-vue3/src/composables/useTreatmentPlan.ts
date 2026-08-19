import { computed, ref, watch } from 'vue'
import { createRehabProgram, getRehabProgramsByPet } from '../api/rehab-programs'
import type { CreateRehabProgramExercise, RehabProgram } from '../types/exercise'

export function useTreatmentPlan(petId: () => number | null) {
  const program = ref<RehabProgram | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadProgram(id: number) {
    loading.value = true
    error.value = null
    program.value = null
    try {
      const programs = await getRehabProgramsByPet(id)
      program.value =
        programs.length > 0
          ? [...programs].sort(
              (a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime(),
            )[0]!
          : null
    } catch {
      error.value = 'Unable to load treatment plan.'
    } finally {
      loading.value = false
    }
  }

  async function createProgram(
    id: number,
    title: string,
    startDate: string,
    exercises: CreateRehabProgramExercise[] = [],
  ) {
    loading.value = true
    error.value = null
    try {
      program.value = await createRehabProgram({
        petId: id,
        programTitle: title,
        startDate,
        exercises,
      })
    } catch {
      error.value = 'Unable to save treatment plan.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  watch(
    () => petId(),
    (id) => {
      if (id) loadProgram(id)
    },
    { immediate: true },
  )

  const hasProgram = computed(() => program.value !== null)

  return {
    program,
    loading,
    error,
    hasProgram,
    reload: () => {
      const id = petId()
      if (id) return loadProgram(id)
    },
    createProgram,
  }
}

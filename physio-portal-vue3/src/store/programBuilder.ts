import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { getExercises } from '../api/exercises'
import { createRehabProgram, getRehabProgramsByPet } from '../api/rehab-programs'
import type { Exercise, ProgramBuilderExercise, RehabProgram } from '../types/exercise'

export const useProgramBuilderStore = defineStore('programBuilder', () => {
  const library = ref<Exercise[]>([])
  const selectedExercises = ref<ProgramBuilderExercise[]>([])
  const currentProgram = ref<RehabProgram | null>(null)
  const searchQuery = ref('')
  const speciesFilter = ref('')
  const programTitle = ref('')
  const programNotes = ref('')
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  const filteredLibrary = computed(() => {
    const query = searchQuery.value.trim().toLowerCase()
    return library.value.filter((exercise) => {
      const matchesQuery =
        !query ||
        exercise.title.toLowerCase().includes(query) ||
        exercise.shortDescription?.toLowerCase().includes(query)
      const matchesSpecies =
        !speciesFilter.value ||
        exercise.targetSpecies === speciesFilter.value ||
        exercise.targetSpecies === null
      const alreadySelected = selectedExercises.value.some((e) => e.exerciseId === exercise.exerciseId)
      return matchesQuery && matchesSpecies && !alreadySelected
    })
  })

  async function loadLibrary(species?: string) {
    loading.value = true
    error.value = null
    try {
      library.value = await getExercises(species)
      if (species) speciesFilter.value = species
    } catch {
      error.value = 'Unable to load exercise library.'
    } finally {
      loading.value = false
    }
  }

  async function loadExistingProgram(petId: number) {
    loading.value = true
    error.value = null
    try {
      const programs = await getRehabProgramsByPet(petId)
      currentProgram.value = programs[0] ?? null
      if (currentProgram.value) {
        programTitle.value = `${currentProgram.value.programTitle} (Revised)`
        programNotes.value = currentProgram.value.notes ?? ''
        selectedExercises.value = currentProgram.value.exercises.map((exercise) => ({
          exerciseId: exercise.exerciseId,
          title: exercise.title,
          shortDescription: exercise.shortDescription,
          repetitions: exercise.repetitions,
          sets: exercise.sets,
          frequencyPerDay: exercise.frequencyPerDay,
        }))
      }
    } catch {
      error.value = 'Unable to load existing program.'
    } finally {
      loading.value = false
    }
  }

  function addExercise(exercise: Exercise) {
    selectedExercises.value.push({
      exerciseId: exercise.exerciseId,
      title: exercise.title,
      shortDescription: exercise.shortDescription,
      repetitions: 10,
      sets: 3,
      frequencyPerDay: 1,
    })
  }

  function removeExercise(exerciseId: number) {
    selectedExercises.value = selectedExercises.value.filter((e) => e.exerciseId !== exerciseId)
  }

  function moveExercise(fromIndex: number, toIndex: number) {
    if (toIndex < 0 || toIndex >= selectedExercises.value.length) return
    const items = [...selectedExercises.value]
    const [moved] = items.splice(fromIndex, 1)
    items.splice(toIndex, 0, moved)
    selectedExercises.value = items
  }

  function reset(petName?: string) {
    selectedExercises.value = []
    currentProgram.value = null
    searchQuery.value = ''
    programTitle.value = petName ? `${petName} Recovery Program` : ''
    programNotes.value = ''
    error.value = null
  }

  async function saveProgram(petId: number) {
    if (selectedExercises.value.length === 0) {
      error.value = 'Add at least one exercise to the program.'
      return null
    }

    saving.value = true
    error.value = null
    try {
      const today = new Date().toISOString().slice(0, 10)
      const program = await createRehabProgram({
        petId,
        programTitle: programTitle.value.trim() || 'Rehabilitation Program',
        startDate: today,
        notes: programNotes.value.trim() || undefined,
        exercises: selectedExercises.value.map((exercise) => ({
          exerciseId: exercise.exerciseId,
          repetitions: exercise.repetitions,
          sets: exercise.sets,
          frequencyPerDay: exercise.frequencyPerDay,
        })),
      })
      currentProgram.value = program
      return program
    } catch {
      error.value = 'Unable to save rehabilitation program.'
      return null
    } finally {
      saving.value = false
    }
  }

  return {
    library,
    selectedExercises,
    currentProgram,
    searchQuery,
    speciesFilter,
    programTitle,
    programNotes,
    loading,
    saving,
    error,
    filteredLibrary,
    loadLibrary,
    loadExistingProgram,
    addExercise,
    removeExercise,
    moveExercise,
    reset,
    saveProgram,
  }
})

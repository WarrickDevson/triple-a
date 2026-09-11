<script setup lang="ts">
import ExerciseCard from './ExerciseCard.vue'
import type { Exercise } from '../../types/exercise'

defineProps<{
  exercises: Exercise[]
  favouriteIds: number[]
  loading?: boolean
}>()

const emit = defineEmits<{
  toggleFavourite: [exerciseId: number]
  edit: [exercise: Exercise]
  customize: [exercise: Exercise]
  preview: [exercise: Exercise]
  toggleActive: [exerciseId: number]
}>()
</script>

<template>
  <div v-if="loading" class="py-16 text-center text-sm text-neutral-muted">Loading exercises...</div>
  <div v-else-if="exercises.length === 0" class="empty-state py-16">
    <p class="text-sm text-neutral-muted">No exercises match your filters.</p>
  </div>
  <div v-else class="grid gap-4 sm:gap-6 grid-cols-1 sm:grid-cols-[repeat(auto-fill,minmax(290px,1fr))]">
    <ExerciseCard
      v-for="exercise in exercises"
      :key="exercise.exerciseId"
      :exercise="exercise"
      :is-favourite="favouriteIds.includes(exercise.exerciseId)"
      @toggle-favourite="emit('toggleFavourite', $event)"
      @edit="emit('edit', $event)"
      @customize="emit('customize', $event)"
      @preview="emit('preview', $event)"
      @toggle-active="emit('toggleActive', $event)"
    />
  </div>
</template>

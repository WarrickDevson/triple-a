<script setup lang="ts">
import { ref } from 'vue'
import { Dumbbell } from '@lucide/vue'
import { resolveMediaUrl } from '../../api/videos'
import { getExerciseStatus, statusBadgeClass } from '../../data/planDemo'
import type { RehabProgramExercise } from '../../types/exercise'

defineProps<{
  exercises: RehabProgramExercise[]
}>()

const brokenImages = ref<Record<number, boolean>>({})

function handleImageError(id: number) {
  brokenImages.value[id] = true
}

function exerciseImage(exercise: RehabProgramExercise) {
  const raw = exercise.coverImageUrl || exercise.steps?.find((s) => s.imageUrl)?.imageUrl
  return resolveMediaUrl(raw)
}
</script>

<template>
  <div v-if="!exercises || exercises.length === 0" class="empty-state py-8">
    <p class="text-sm text-neutral-muted">No exercises assigned to this phase yet.</p>
  </div>
  <div v-else class="overflow-x-auto">
    <table class="w-full min-w-[480px] text-left text-sm">
      <thead>
        <tr class="border-b border-neutral-grey/80 text-xs font-semibold uppercase tracking-wide text-neutral-muted">
          <th class="pb-3 pr-4">Exercise</th>
          <th class="pb-3 pr-4">Sets/Reps</th>
          <th class="pb-3 pr-4">Frequency</th>
          <th class="pb-3">Status</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="(exercise, index) in exercises"
          :key="exercise.rehabProgramExerciseId"
          class="border-b border-neutral-grey/60"
        >
          <td class="py-3 pr-4">
            <div class="flex items-center gap-3">
              <div
                class="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-sage/15 text-[10px] text-sage"
              >
                <img
                  v-if="exerciseImage(exercise) && !brokenImages[exercise.rehabProgramExerciseId]"
                  :src="exerciseImage(exercise)!"
                  :alt="exercise.title"
                  class="h-full w-full object-cover"
                  @error="handleImageError(exercise.rehabProgramExerciseId)"
                />
                <Dumbbell v-else class="h-4 w-4" />
              </div>
              <span class="font-medium text-navy">{{ exercise.title }}</span>
            </div>
          </td>
          <td class="py-3 pr-4 text-neutral-muted">
            {{ exercise.sets }} x {{ exercise.repetitions }}
          </td>
          <td class="py-3 pr-4 text-neutral-muted">
            {{ exercise.frequencyPerDay }}x daily
          </td>
          <td class="py-3">
            <span
              :class="statusBadgeClass(getExerciseStatus(exercise.rehabProgramExerciseId, index))"
            >
              {{ getExerciseStatus(exercise.rehabProgramExerciseId, index) }}
            </span>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

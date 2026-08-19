<script setup lang="ts">
import { Plus } from '@lucide/vue'
import PlanExerciseTable from './PlanExerciseTable.vue'
import type { PlanPhase } from '../../data/planDemo'
import type { RehabProgramExercise } from '../../types/exercise'

defineProps<{
  phase: PlanPhase
  exercises: RehabProgramExercise[]
}>()

const emit = defineEmits<{
  editPhase: []
  addExercise: []
}>()
</script>

<template>
  <section class="portal-card overflow-hidden">
    <div class="flex items-center justify-between border-b border-neutral-grey/80 px-4 py-3">
      <h2 class="text-sm font-bold uppercase tracking-wide text-navy">
        {{ phase.label }}: {{ phase.title }}
      </h2>
      <button
        type="button"
        class="text-xs font-semibold text-sage hover:text-navy"
        @click="emit('editPhase')"
      >
        Edit Phase
      </button>
    </div>

    <div class="p-4">
      <h3 class="text-xs font-bold uppercase tracking-wide text-neutral-muted">Goals</h3>
      <ul class="mt-3 space-y-2">
        <li
          v-for="goal in phase.goals"
          :key="goal"
          class="flex items-start gap-2 text-sm text-navy"
        >
          <span class="mt-0.5 h-4 w-4 shrink-0 rounded border border-sage bg-sage-muted" />
          {{ goal }}
        </li>
      </ul>

      <div class="mt-6">
        <PlanExerciseTable :exercises="exercises" />
      </div>

      <button
        type="button"
        class="mt-4 inline-flex items-center gap-2 text-sm font-semibold text-sage hover:text-navy"
        @click="emit('addExercise')"
      >
        <Plus class="h-4 w-4" />
        Add Exercise
      </button>
    </div>
  </section>
</template>

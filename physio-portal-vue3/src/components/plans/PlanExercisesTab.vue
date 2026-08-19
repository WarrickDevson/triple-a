<script setup lang="ts">
import { Dumbbell, Plus, Trash2, Edit3, Repeat } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { PlanPhase } from '../../data/planDemo'
import type { RehabProgram, RehabProgramExercise } from '../../types/exercise'

defineProps<{
  program: RehabProgram | null
  phases: PlanPhase[]
}>()

const emit = defineEmits<{
  addExercise: []
  removeExercise: [exerciseId: number]
  editPrescription: [exercise: RehabProgramExercise]
}>()

function getPhaseLabel(phaseId?: number) {
  if (!phaseId) return 'Phase 1'
  return `Phase ${phaseId}`
}
</script>

<template>
  <div class="p-5 space-y-5">
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-base font-bold text-navy">Prescribed Exercises</h3>
        <p class="text-xs text-neutral-muted">
          {{ program?.exercises?.length ?? 0 }} exercises assigned across active rehab phases
        </p>
      </div>
      <BaseButton size="sm" @click="emit('addExercise')">
        <Plus class="h-4 w-4" />
        Add Exercise to Plan
      </BaseButton>
    </div>

    <!-- Exercises Grid / List -->
    <div v-if="!program?.exercises || program.exercises.length === 0" class="empty-state py-12">
      <p class="text-sm text-neutral-muted">No exercises assigned to this plan yet.</p>
      <BaseButton class="mt-4" size="sm" @click="emit('addExercise')">
        Prescribe First Exercise
      </BaseButton>
    </div>

    <div v-else class="grid gap-4 sm:grid-cols-2">
      <div
        v-for="exercise in program.exercises"
        :key="exercise.exerciseId"
        class="portal-card flex flex-col justify-between p-4 border border-neutral-grey/60 hover:border-sage/40 transition-all shadow-sm"
      >
        <div>
          <!-- Header info -->
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-center gap-3">
              <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-sage/15 text-sage">
                <Dumbbell class="h-6 w-6" />
              </div>
              <div>
                <h4 class="text-sm font-bold text-navy">{{ exercise.title }}</h4>
                <span class="inline-block mt-0.5 rounded bg-sage-muted px-2 py-0.5 text-[10px] font-bold text-sage">
                  {{ getPhaseLabel(exercise.phaseId) }}
                </span>
              </div>
            </div>

            <button
              type="button"
              class="text-neutral-muted hover:text-alert-red transition-colors p-1"
              title="Remove exercise from plan"
              @click="emit('removeExercise', exercise.exerciseId)"
            >
              <Trash2 class="h-4 w-4" />
            </button>
          </div>

          <!-- Description & Target muscles -->
          <p class="mt-3 text-xs text-neutral-muted line-clamp-2">
            Targeted rehabilitation exercise protocol
          </p>

          <!-- Prescription Badge Stats -->
          <div class="mt-4 grid grid-cols-3 gap-2 rounded-xl bg-surface p-2.5 text-center text-xs">
            <div>
              <p class="text-[10px] font-semibold text-neutral-muted uppercase">Sets</p>
              <p class="font-extrabold text-navy mt-0.5">{{ exercise.sets }}</p>
            </div>
            <div>
              <p class="text-[10px] font-semibold text-neutral-muted uppercase">Reps</p>
              <p class="font-extrabold text-navy mt-0.5">{{ exercise.repetitions }}</p>
            </div>
            <div>
              <p class="text-[10px] font-semibold text-neutral-muted uppercase">Freq / Day</p>
              <p class="font-extrabold text-sage mt-0.5">{{ exercise.frequencyPerDay }}x</p>
            </div>
          </div>
        </div>

        <div class="mt-4 flex items-center justify-between border-t border-neutral-grey/40 pt-3 text-xs">
          <span class="flex items-center gap-1 text-neutral-muted">
            <Repeat class="h-3.5 w-3.5 text-sage" />
            Daily Home Protocol
          </span>
          <button
            type="button"
            class="inline-flex items-center gap-1 font-semibold text-sage hover:text-navy"
            @click="emit('editPrescription', exercise)"
          >
            <Edit3 class="h-3.5 w-3.5" />
            Edit Sets/Reps
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

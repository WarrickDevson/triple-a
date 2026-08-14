<script setup lang="ts">
import { useRouter } from 'vue-router'
import { Dumbbell, Calendar, ArrowRight } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { RehabProgram } from '../../types/exercise'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patient: Pet
  activeProgram: RehabProgram | null
}>()

const router = useRouter()

function goToFullPlan() {
  router.push({ name: 'treatment-plan-detail', params: { petId: props.patient.petId } })
}
</script>

<template>
  <div class="space-y-4">
    <!-- Summary Header Card -->
    <div class="rounded-xl border border-neutral-grey/60 bg-surface p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <span class="rounded bg-sage-muted px-2.5 py-0.5 text-[10px] font-extrabold uppercase tracking-wider text-sage">
          Active Rehabilitation Plan
        </span>
        <h3 class="text-base font-bold text-navy mt-1">
          {{ activeProgram?.programTitle || 'Standard Post-Op Rehabilitation Protocol' }}
        </h3>
        <p class="text-xs text-neutral-muted flex items-center gap-1.5 mt-1">
          <Calendar class="h-3.5 w-3.5" />
          Started {{ activeProgram?.startDate || 'Recent' }} · Phase 2 of 4 (Active Strength Building)
        </p>
      </div>

      <BaseButton size="sm" @click="goToFullPlan">
        View Full Plan Details
        <ArrowRight class="h-4 w-4" />
      </BaseButton>
    </div>

    <!-- Prescribed Exercises Preview -->
    <div class="portal-card overflow-hidden">
      <div class="flex items-center justify-between border-b border-neutral-grey/60 px-4 py-3 bg-surface/50">
        <h4 class="text-xs font-bold uppercase tracking-wider text-navy">
          Prescribed Home Exercises ({{ activeProgram?.exercises?.length ?? 0 }})
        </h4>
        <button type="button" class="text-xs font-semibold text-sage hover:underline" @click="goToFullPlan">
          Manage Prescriptions →
        </button>
      </div>

      <div v-if="!activeProgram?.exercises || activeProgram.exercises.length === 0" class="p-8 text-center text-xs text-neutral-muted">
        No active exercises prescribed for this patient.
      </div>

      <div v-else class="divide-y divide-neutral-grey/40">
        <div
          v-for="exercise in activeProgram.exercises"
          :key="exercise.exerciseId"
          class="p-3.5 flex items-center justify-between gap-3 hover:bg-surface/60 transition-colors"
        >
          <div class="flex items-center gap-3">
            <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-sage-muted text-sage">
              <Dumbbell class="h-4 w-4" />
            </div>
            <div>
              <p class="text-xs font-bold text-navy">{{ exercise.title }}</p>
              <p class="text-[11px] text-neutral-muted line-clamp-1">
                Targeted rehabilitation exercise protocol
              </p>
            </div>
          </div>

          <div class="flex items-center gap-3 text-xs font-semibold shrink-0">
            <span class="rounded-md bg-navy/5 px-2 py-1 text-navy">
              {{ exercise.sets }} sets × {{ exercise.repetitions }} reps
            </span>
            <span class="rounded-md bg-sage-muted px-2 py-1 text-sage">
              {{ exercise.frequencyPerDay }}x / day
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Clinical Instructions Note -->
    <div v-if="activeProgram?.notes" class="portal-card p-4 bg-sage-muted/20 border border-sage/30">
      <h4 class="text-xs font-bold uppercase tracking-wider text-sage">Attending Physio Notes</h4>
      <p class="text-xs text-navy/90 leading-relaxed mt-1 whitespace-pre-line">{{ activeProgram.notes }}</p>
    </div>
  </div>
</template>

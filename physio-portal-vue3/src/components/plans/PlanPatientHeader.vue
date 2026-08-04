<script setup lang="ts">
import { computed } from 'vue'
import DonutChart from '../dashboard/DonutChart.vue'
import { getPlanProgressPercent } from '../../data/planDemo'
import type { Pet } from '../../types/pet'
import type { RehabProgram } from '../../types/exercise'

const props = defineProps<{
  patient: Pet
  program: RehabProgram | null
}>()

const progressPercent = computed(() =>
  getPlanProgressPercent(props.program?.exercises.length ?? 0),
)

const ageLabel = computed(() => {
  if (!props.patient.birthDate) return null
  const birth = new Date(props.patient.birthDate)
  const years = Math.floor((Date.now() - birth.getTime()) / (365.25 * 24 * 60 * 60 * 1000))
  return years > 0 ? `${years} yrs` : '< 1 yr'
})
</script>

<template>
  <div class="portal-card flex flex-wrap items-center justify-between gap-4 p-5">
    <div class="flex items-center gap-4">
      <div
        class="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-sage-muted text-lg font-bold text-sage"
      >
        {{ patient.petName.slice(0, 2).toUpperCase() }}
      </div>
      <div>
        <h2 class="text-lg font-bold text-navy">
          Treatment Plan: {{ patient.petName }}
        </h2>
        <p class="text-sm text-neutral-muted">
          {{ patient.breed || patient.species }}
          <span v-if="ageLabel"> · {{ ageLabel }}</span>
          <span v-if="patient.weightKg"> · {{ patient.weightKg }} kg</span>
        </p>
      </div>
    </div>
    <div class="flex items-center gap-3">
      <DonutChart
        :labels="['Complete', 'Remaining']"
        :values="[progressPercent, Math.max(0, 100 - progressPercent)]"
        :colors="['#6b7a4d', '#e5e7e3']"
        cutout="72%"
      >
        <div class="text-center">
          <p class="text-xs font-bold text-navy">{{ progressPercent }}%</p>
        </div>
      </DonutChart>
      <p class="text-xs font-medium text-neutral-muted">Plan Progress</p>
    </div>
  </div>
</template>

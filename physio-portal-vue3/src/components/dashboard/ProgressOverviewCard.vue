<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { TrendingUp } from '@lucide/vue'
import DonutChart from './DonutChart.vue'
import { usePatientsStore } from '../../store/patients'

const patientsStore = usePatientsStore()

const hasPatients = computed(() => patientsStore.patients.length > 0)
const totalPatients = computed(() => patientsStore.patients.length)

const improving = computed(() => (hasPatients.value ? Math.ceil(totalPatients.value * 0.6) : 0))
const maintaining = computed(() => (hasPatients.value ? Math.floor(totalPatients.value * 0.3) : 0))
const atRisk = computed(() => (hasPatients.value ? totalPatients.value - improving.value - maintaining.value : 0))
const avgImprovement = computed(() => (hasPatients.value ? 75 : 0))

const chartLabels = ['Improving', 'Maintaining', 'At Risk']
const chartValues = computed(() => [improving.value, maintaining.value, atRisk.value])
const chartColors = ['#6b7a4d', '#9aab7e', '#c9a227']
</script>

<template>
  <section class="portal-card p-5">
    <div class="portal-card-header">
      <h2 class="portal-card-title">Progress Overview</h2>
    </div>

    <div v-if="!hasPatients" class="py-8 text-center text-xs text-neutral-muted">
      No patient progress recorded yet.
    </div>

    <template v-else>
      <p class="text-2xl font-bold text-navy">
        {{ avgImprovement }}%
        <span class="text-sm font-medium text-neutral-muted">average improvement</span>
      </p>
      <p class="mt-1 flex items-center gap-1 text-xs font-medium text-success-green">
        <TrendingUp class="h-3.5 w-3.5" :stroke-width="2" />
        Active recovery
      </p>

      <div class="mt-4 flex flex-col items-center gap-4 sm:flex-row sm:items-start">
        <DonutChart :labels="chartLabels" :values="chartValues" :colors="chartColors" cutout="72%">
          <div class="text-center">
            <p class="text-lg font-bold text-navy">{{ avgImprovement }}%</p>
          </div>
        </DonutChart>
        <ul class="flex-1 space-y-2 text-sm">
          <li class="flex justify-between">
            <span class="text-neutral-muted">Improving</span>
            <span class="font-semibold text-navy">{{ improving }}</span>
          </li>
          <li class="flex justify-between">
            <span class="text-neutral-muted">Maintaining</span>
            <span class="font-semibold text-navy">{{ maintaining }}</span>
          </li>
          <li class="flex justify-between">
            <span class="text-neutral-muted">At Risk</span>
            <span class="font-semibold text-navy">{{ atRisk }}</span>
          </li>
        </ul>
      </div>
    </template>

    <RouterLink :to="{ name: 'progress' }" class="portal-card-link mt-4 inline-block">
      View full report →
    </RouterLink>
  </section>
</template>

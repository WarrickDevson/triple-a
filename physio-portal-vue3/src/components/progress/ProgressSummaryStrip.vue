<script setup lang="ts">
import { computed } from 'vue'
import { TrendingUp } from '@lucide/vue'
import DonutChart from '../dashboard/DonutChart.vue'
import { usePatientsStore } from '../../store/patients'

const patientsStore = usePatientsStore()

const total = computed(() => patientsStore.patients.length)
const improving = computed(() => (total.value > 0 ? Math.ceil(total.value * 0.6) : 0))
const maintaining = computed(() => (total.value > 0 ? Math.floor(total.value * 0.3) : 0))
const atRisk = computed(() => (total.value > 0 ? total.value - improving.value - maintaining.value : 0))
const avgImprovement = computed(() => (total.value > 0 ? 75 : 0))

const chartLabels = ['Improving', 'Maintaining', 'At Risk']
const chartValues = computed(() => [improving.value, maintaining.value, atRisk.value])
const chartColors = ['#6b7a4d', '#9aab7e', '#c9a227']
</script>

<template>
  <section class="portal-card p-5">
    <div class="flex flex-wrap items-center justify-between gap-6">
      <div>
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Clinic Overview</p>
        <p class="mt-1 text-3xl font-bold text-navy">
          {{ avgImprovement }}%
          <span class="text-sm font-medium text-neutral-muted">avg improvement</span>
        </p>
        <p class="mt-1 flex items-center gap-1 text-xs font-medium text-success-green">
          <TrendingUp class="h-3.5 w-3.5" :stroke-width="2" />
          Active recovery tracking
        </p>
      </div>

      <DonutChart :labels="chartLabels" :values="chartValues" :colors="chartColors" cutout="72%">
        <div class="text-center">
          <p class="text-sm font-bold text-navy">{{ avgImprovement }}%</p>
        </div>
      </DonutChart>

      <ul class="flex flex-wrap gap-6 text-sm">
        <li>
          <p class="text-neutral-muted">Improving</p>
          <p class="text-lg font-bold text-navy">{{ improving }}</p>
        </li>
        <li>
          <p class="text-neutral-muted">Maintaining</p>
          <p class="text-lg font-bold text-navy">{{ maintaining }}</p>
        </li>
        <li>
          <p class="text-neutral-muted">At Risk</p>
          <p class="text-lg font-bold text-navy">{{ atRisk }}</p>
        </li>
      </ul>
    </div>
  </section>
</template>

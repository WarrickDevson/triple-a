<script setup lang="ts">
import { TrendingUp } from '@lucide/vue'
import DonutChart from '../dashboard/DonutChart.vue'
import { demoProgressStats } from '../../data/dashboardDemo'

const chartLabels = ['Improving', 'Maintaining', 'At Risk']
const chartValues = [
  demoProgressStats.improving,
  demoProgressStats.maintaining,
  demoProgressStats.atRisk,
]
const chartColors = ['#6b7a4d', '#9aab7e', '#c9a227']
</script>

<template>
  <section class="portal-card p-5">
    <div class="flex flex-wrap items-center justify-between gap-6">
      <div>
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Clinic Overview</p>
        <p class="mt-1 text-3xl font-bold text-navy">
          {{ demoProgressStats.averageImprovement }}%
          <span class="text-sm font-medium text-neutral-muted">avg improvement</span>
        </p>
        <p class="mt-1 flex items-center gap-1 text-xs font-medium text-success-green">
          <TrendingUp class="h-3.5 w-3.5" :stroke-width="2" />
          {{ demoProgressStats.trend }}
        </p>
      </div>

      <DonutChart :labels="chartLabels" :values="chartValues" :colors="chartColors" cutout="72%">
        <div class="text-center">
          <p class="text-sm font-bold text-navy">{{ demoProgressStats.averageImprovement }}%</p>
        </div>
      </DonutChart>

      <ul class="flex flex-wrap gap-6 text-sm">
        <li>
          <p class="text-neutral-muted">Improving</p>
          <p class="text-lg font-bold text-navy">{{ demoProgressStats.improving }}</p>
        </li>
        <li>
          <p class="text-neutral-muted">Maintaining</p>
          <p class="text-lg font-bold text-navy">{{ demoProgressStats.maintaining }}</p>
        </li>
        <li>
          <p class="text-neutral-muted">At Risk</p>
          <p class="text-lg font-bold text-navy">{{ demoProgressStats.atRisk }}</p>
        </li>
      </ul>
    </div>
  </section>
</template>

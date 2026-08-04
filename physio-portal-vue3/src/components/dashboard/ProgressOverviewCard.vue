<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { TrendingUp } from '@lucide/vue'
import DonutChart from './DonutChart.vue'
import { demoProgressStats } from '../../data/dashboardDemo'

const chartLabels = ['Improving', 'Maintaining', 'At Risk']
const chartValues = computed(() => [
  demoProgressStats.improving,
  demoProgressStats.maintaining,
  demoProgressStats.atRisk,
])
const chartColors = ['#6b7a4d', '#9aab7e', '#c9a227']
</script>

<template>
  <section class="portal-card p-5">
    <div class="portal-card-header">
      <h2 class="portal-card-title">Progress Overview</h2>
    </div>

    <p class="text-2xl font-bold text-navy">
      {{ demoProgressStats.averageImprovement }}%
      <span class="text-sm font-medium text-neutral-muted">average improvement</span>
    </p>
    <p class="mt-1 flex items-center gap-1 text-xs font-medium text-success-green">
      <TrendingUp class="h-3.5 w-3.5" :stroke-width="2" />
      {{ demoProgressStats.trend }}
    </p>

    <div class="mt-4 flex flex-col items-center gap-4 sm:flex-row sm:items-start">
      <DonutChart :labels="chartLabels" :values="chartValues" :colors="chartColors" cutout="72%">
        <div class="text-center">
          <p class="text-lg font-bold text-navy">{{ demoProgressStats.averageImprovement }}%</p>
        </div>
      </DonutChart>
      <ul class="flex-1 space-y-2 text-sm">
        <li class="flex justify-between">
          <span class="text-neutral-muted">Improving</span>
          <span class="font-semibold text-navy">{{ demoProgressStats.improving }}</span>
        </li>
        <li class="flex justify-between">
          <span class="text-neutral-muted">Maintaining</span>
          <span class="font-semibold text-navy">{{ demoProgressStats.maintaining }}</span>
        </li>
        <li class="flex justify-between">
          <span class="text-neutral-muted">At Risk</span>
          <span class="font-semibold text-navy">{{ demoProgressStats.atRisk }}</span>
        </li>
      </ul>
    </div>

    <RouterLink :to="{ name: 'progress' }" class="portal-card-link mt-4 inline-block">
      View full report →
    </RouterLink>
  </section>
</template>

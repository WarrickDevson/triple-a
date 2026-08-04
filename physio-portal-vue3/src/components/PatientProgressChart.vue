<script setup lang="ts">
import {
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LineElement,
  LinearScale,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js'
import { computed } from 'vue'
import { Line } from 'vue-chartjs'
import type { PetProgressSummary } from '../types/dashboard'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

const props = defineProps<{
  progress: PetProgressSummary | null
}>()

const chartData = computed(() => {
  const logs = props.progress?.logs ?? []
  return {
    labels: logs.map((log) => log.logDate),
    datasets: [
      {
        label: 'Pain',
        borderColor: '#D90429',
        backgroundColor: '#D90429',
        data: logs.map((log) => log.painScore),
        tension: 0.3,
      },
      {
        label: 'Mobility',
        borderColor: '#2D8B57',
        backgroundColor: '#2D8B57',
        data: logs.map((log) => log.mobilityScore),
        tension: 0.3,
      },
      {
        label: 'Energy',
        borderColor: '#1E6E8E',
        backgroundColor: '#1E6E8E',
        data: logs.map((log) => log.energyScore),
        tension: 0.3,
      },
    ],
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    y: {
      min: 0,
      max: 10,
      ticks: { stepSize: 1 },
    },
  },
}
</script>

<template>
  <div class="panel p-6">
    <div class="mb-4 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h3 class="font-display text-lg font-bold text-primary-dark">Program Progression</h3>
        <p class="mt-1 text-sm text-neutral-dark/70">
          {{ progress?.totalCompletedSessions ?? 0 }} completed sessions across
          {{ progress?.totalTrackedDays ?? 0 }} tracked days
        </p>
      </div>
    </div>

    <div v-if="!progress || progress.logs.length === 0" class="empty-state py-12">
      <p class="text-sm text-neutral-dark/70">
        No tracking data yet. Progress charts will appear once owners complete daily routines.
      </p>
    </div>

    <div v-else class="h-72">
      <Line :data="chartData" :options="chartOptions" />
    </div>
  </div>
</template>

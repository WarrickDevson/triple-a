<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { Chart, DoughnutController, ArcElement, Tooltip, Legend } from 'chart.js'

Chart.register(DoughnutController, ArcElement, Tooltip, Legend)

const props = defineProps<{
  labels: string[]
  values: number[]
  colors: string[]
  cutout?: string
}>()

const canvasRef = ref<HTMLCanvasElement | null>(null)
let chart: Chart | null = null

function renderChart() {
  if (!canvasRef.value) return
  chart?.destroy()
  chart = new Chart(canvasRef.value, {
    type: 'doughnut',
    data: {
      labels: props.labels,
      datasets: [
        {
          data: props.values,
          backgroundColor: props.colors,
          borderWidth: 0,
          hoverOffset: 4,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      cutout: props.cutout ?? '68%',
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: '#0a1a2e',
          padding: 10,
          cornerRadius: 8,
        },
      },
    },
  })
}

onMounted(renderChart)
onUnmounted(() => chart?.destroy())
watch(() => [props.labels, props.values, props.colors], renderChart, { deep: true })
</script>

<template>
  <div class="relative mx-auto aspect-square w-full max-w-[140px]">
    <canvas ref="canvasRef" />
    <div v-if="$slots.default" class="pointer-events-none absolute inset-0 flex items-center justify-center">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import DonutChart from './DonutChart.vue'
import { usePatientsStore } from '../../store/patients'

const props = defineProps<{
  patientCount: number
}>()

const patientsStore = usePatientsStore()

const speciesBreakdown = computed(() => {
  if (props.patientCount === 0 || patientsStore.patients.length === 0) {
    return []
  }
  const counts: Record<string, number> = {}
  for (const p of patientsStore.patients) {
    const s = p.species || 'Other'
    counts[s] = (counts[s] || 0) + 1
  }
  const total = patientsStore.patients.length
  const colors: Record<string, string> = {
    Canine: '#2c3e50',
    Equine: '#5a7865',
    Feline: '#c9a227',
  }
  return Object.entries(counts).map(([label, count]) => ({
    label,
    value: Math.round((count / total) * 100),
    color: colors[label] || '#718096',
  }))
})

const chartLabels = computed(() => speciesBreakdown.value.map((s) => s.label))
const chartValues = computed(() => speciesBreakdown.value.map((s) => s.value))
const chartColors = computed(() => speciesBreakdown.value.map((s) => s.color))
</script>

<template>
  <section class="portal-card p-5">
    <div class="portal-card-header">
      <h2 class="portal-card-title">Patients in Care</h2>
    </div>

    <p class="text-2xl font-bold text-navy">
      {{ patientCount }}
      <span class="text-sm font-medium text-neutral-muted">active patients</span>
    </p>

    <div v-if="patientCount === 0" class="py-8 text-center text-xs text-neutral-muted">
      No active patients registered yet.
    </div>

    <div v-else class="mt-4 flex flex-col items-center gap-4 sm:flex-row sm:items-start">
      <DonutChart :labels="chartLabels" :values="chartValues" :colors="chartColors" />
      <ul class="flex-1 space-y-2 text-sm">
        <li
          v-for="item in speciesBreakdown"
          :key="item.label"
          class="flex items-center justify-between gap-2"
        >
          <span class="flex items-center gap-2 text-neutral-dark">
            <span class="h-2.5 w-2.5 rounded-full" :style="{ backgroundColor: item.color }" />
            {{ item.label }}
          </span>
          <span class="font-semibold text-navy">{{ item.value }}%</span>
        </li>
      </ul>
    </div>

    <RouterLink :to="{ name: 'patients' }" class="portal-card-link mt-4 inline-block">
      View all patients →
    </RouterLink>
  </section>
</template>

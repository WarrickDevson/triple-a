<script setup lang="ts">
import { computed, ref } from 'vue'
import { Search } from '@lucide/vue'
import { getPatientDemoMeta, statusDotClass } from '../../data/patientDemo'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patients: Pet[]
  selectedPetId: number | null
  loading?: boolean
}>()

const emit = defineEmits<{
  select: [petId: number]
}>()

const search = ref('')

const filteredPatients = computed(() => {
  const query = search.value.trim().toLowerCase()
  return props.patients.filter(
    (p) =>
      !query ||
      p.petName.toLowerCase().includes(query) ||
      p.ownerName.toLowerCase().includes(query),
  )
})
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <h2 class="text-sm font-bold text-navy">Select Patient</h2>
      <div class="relative mt-3">
        <Search class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          v-model="search"
          type="search"
          placeholder="Search patients..."
          class="w-full rounded-lg border border-neutral-grey bg-surface py-2 pl-9 pr-3 text-sm outline-none focus:border-sage"
        />
      </div>
    </div>
    <div v-if="loading" class="p-6 text-sm text-neutral-muted">Loading patients...</div>
    <ul v-else class="flex-1 overflow-y-auto">
      <li v-for="patient in filteredPatients" :key="patient.petId">
        <button
          type="button"
          class="flex w-full items-start gap-3 border-b border-neutral-grey/60 px-4 py-3 text-left transition-colors hover:bg-surface"
          :class="selectedPetId === patient.petId ? 'bg-sage-muted/40' : ''"
          @click="emit('select', patient.petId)"
        >
          <span
            class="mt-1.5 h-2 w-2 shrink-0 rounded-full"
            :class="statusDotClass(getPatientDemoMeta(patient.petId, patient.species).status)"
          />
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-semibold text-navy">{{ patient.petName }}</p>
            <p class="truncate text-xs text-neutral-muted">{{ patient.ownerName }}</p>
          </div>
        </button>
      </li>
    </ul>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { Search } from '@lucide/vue'
import { getPatientDemoMeta, statusDotClass } from '../../data/patientDemo'
import { PET_SPECIES } from '../../types/pet'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patients: Pet[]
  selectedPetId: number | null
  loading?: boolean
}>()

const emit = defineEmits<{
  select: [petId: number]
  add: []
  invite: []
}>()

const search = ref('')
const speciesFilter = ref('All')

const speciesOptions = ['All', ...PET_SPECIES]

const filteredPatients = computed(() => {
  const query = search.value.trim().toLowerCase()
  return props.patients.filter((patient) => {
    const matchesSpecies =
      speciesFilter.value === 'All' ||
      patient.species.toLowerCase() === speciesFilter.value.toLowerCase()
    const matchesSearch =
      !query ||
      patient.petName.toLowerCase().includes(query) ||
      patient.ownerName.toLowerCase().includes(query) ||
      (patient.breed?.toLowerCase().includes(query) ?? false)
    return matchesSpecies && matchesSearch
  })
})

function selectPatient(petId: number) {
  emit('select', petId)
}
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <div class="flex items-center justify-between gap-2">
        <h2 class="text-sm font-bold text-navy">My Patients</h2>
        <div class="flex gap-1.5">
          <button
            type="button"
            class="rounded-lg border border-sage/40 bg-sage-muted/50 px-2.5 py-1.5 text-xs font-semibold text-navy transition-colors hover:bg-sage-muted"
            @click="emit('invite')"
          >
            Invite owner
          </button>
          <button
            type="button"
            class="rounded-lg bg-sage px-3 py-1.5 text-xs font-semibold text-white hover:bg-sage-light"
            @click="emit('add')"
          >
            Add patient
          </button>
        </div>
      </div>
      <div class="relative mt-3">
        <Search class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          v-model="search"
          type="search"
          placeholder="Search patients..."
          class="w-full rounded-lg border border-neutral-grey bg-surface py-2 pl-9 pr-3 text-sm outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
        />
      </div>
      <select
        v-model="speciesFilter"
        class="mt-2 w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
      >
        <option v-for="option in speciesOptions" :key="option" :value="option">
          {{ option === 'All' ? 'All species' : option }}
        </option>
      </select>
    </div>

    <div v-if="loading" class="p-6 text-sm text-neutral-muted">Loading patients...</div>
    <div v-else-if="filteredPatients.length === 0" class="empty-state m-4 py-8">
      <p class="text-sm text-neutral-muted">No patients match your search.</p>
    </div>
    <ul v-else class="flex-1 overflow-y-auto">
      <li v-for="patient in filteredPatients" :key="patient.petId">
        <button
          type="button"
          class="flex w-full items-start gap-3 border-b border-neutral-grey/60 px-4 py-3 text-left transition-colors hover:bg-surface"
          :class="selectedPetId === patient.petId ? 'border-l-[3px] border-l-sage bg-sage-muted/40' : 'border-l-[3px] border-l-transparent'"
          @click="selectPatient(patient.petId)"
        >
          <span
            class="mt-1.5 h-2 w-2 shrink-0 rounded-full"
            :class="statusDotClass(getPatientDemoMeta(patient.petId, patient.species).status)"
          />
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-semibold text-navy">{{ patient.petName }}</p>
            <p class="truncate text-xs text-neutral-muted">
              {{ patient.breed || patient.species }} · {{ patient.ownerName }}
            </p>
          </div>
        </button>
      </li>
    </ul>
  </section>
</template>

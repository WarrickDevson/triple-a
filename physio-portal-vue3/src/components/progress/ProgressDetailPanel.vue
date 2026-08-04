<script setup lang="ts">
import { computed } from 'vue'
import PatientProgressChart from '../PatientProgressChart.vue'
import { getPatientDemoMeta, statusBadgeClass, statusLabel } from '../../data/patientDemo'
import type { Pet } from '../../types/pet'
import type { PetProgressSummary } from '../../types/dashboard'

const props = defineProps<{
  patient: Pet | null
  progress: PetProgressSummary | null
  loading?: boolean
  showBack?: boolean
}>()

const emit = defineEmits<{
  back: []
}>()

const demoMeta = computed(() =>
  props.patient ? getPatientDemoMeta(props.patient.petId, props.patient.species) : null,
)

const completedSessions = computed(() => props.progress?.totalCompletedSessions ?? 0)
const trackedDays = computed(() => props.progress?.totalTrackedDays ?? 0)
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 px-4 py-3">
      <button
        v-if="showBack"
        type="button"
        class="mb-2 text-sm font-semibold text-sage xl:hidden"
        @click="emit('back')"
      >
        ← Back to patients
      </button>
      <div v-if="patient" class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 class="text-sm font-bold text-navy">{{ patient.petName }}</h2>
          <p class="text-xs text-neutral-muted">
            {{ patient.breed || patient.species }} · {{ patient.ownerName }}
          </p>
        </div>
        <span v-if="demoMeta" :class="statusBadgeClass(demoMeta.status)">
          {{ statusLabel(demoMeta.status) }}
        </span>
      </div>
      <p v-else class="text-sm text-neutral-muted">Select a patient to view progress.</p>
    </div>

    <div class="flex-1 overflow-y-auto p-4">
      <div v-if="loading" class="py-16 text-center text-sm text-neutral-muted">
        Loading progress...
      </div>
      <template v-else-if="patient">
        <div class="mb-4 grid gap-3 sm:grid-cols-3">
          <div class="quick-stat">
            <p class="text-xs text-neutral-muted">Completed Sessions</p>
            <p class="text-xl font-bold text-navy">{{ completedSessions }}</p>
          </div>
          <div class="quick-stat">
            <p class="text-xs text-neutral-muted">Tracked Days</p>
            <p class="text-xl font-bold text-navy">{{ trackedDays }}</p>
          </div>
          <div class="quick-stat">
            <p class="text-xs text-neutral-muted">Current Phase</p>
            <p class="text-sm font-bold text-navy">{{ demoMeta?.phaseLabel ?? '—' }}</p>
          </div>
        </div>

        <PatientProgressChart :progress="progress" />
      </template>
      <div v-else class="empty-state py-16">
        <p class="text-sm text-neutral-muted">Choose a patient from the list to view their progress chart.</p>
      </div>
    </div>
  </section>
</template>

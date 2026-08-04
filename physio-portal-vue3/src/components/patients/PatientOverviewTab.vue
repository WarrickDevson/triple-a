<script setup lang="ts">
import { computed } from 'vue'
import DonutChart from '../dashboard/DonutChart.vue'
import type { Appointment } from '../../types/appointment'
import type { PatientDemoMeta } from '../../data/patientDemo'
import { statusBadgeClass, statusLabel } from '../../data/patientDemo'
import type { RehabProgram } from '../../types/exercise'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patient: Pet
  demoMeta: PatientDemoMeta
  activeProgram: RehabProgram | null
  nextAppointment: Appointment | null
  progressPercent: number
}>()

const diagnosis = computed(
  () => props.patient.medicalHistories[0]?.diagnosis ?? 'No diagnosis recorded yet.',
)

const ageLabel = computed(() => {
  if (!props.patient.birthDate) return 'Age unknown'
  const birth = new Date(props.patient.birthDate)
  const years = Math.floor((Date.now() - birth.getTime()) / (365.25 * 24 * 60 * 60 * 1000))
  return years > 0 ? `${years} yrs` : '< 1 yr'
})

function formatDate(value: string) {
  return new Date(value).toLocaleDateString([], {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  })
}

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}
</script>

<template>
  <div class="space-y-5">
    <div class="flex flex-wrap items-start gap-4">
      <div
        class="flex h-16 w-16 shrink-0 items-center justify-center rounded-full bg-sage-muted text-lg font-bold text-sage"
      >
        {{ patient.petName.slice(0, 2).toUpperCase() }}
      </div>
      <div class="min-w-0 flex-1">
        <div class="flex flex-wrap items-center gap-2">
          <h2 class="text-xl font-bold text-navy">{{ patient.petName }}</h2>
          <span :class="statusBadgeClass(demoMeta.status)">{{ statusLabel(demoMeta.status) }}</span>
        </div>
        <p class="mt-1 text-sm text-neutral-muted">
          {{ patient.breed || patient.species }} · Owner: {{ patient.ownerName }}
        </p>
        <p class="mt-1 text-xs text-neutral-muted">
          {{ ageLabel }}
          <span v-if="patient.weightKg"> · {{ patient.weightKg }} kg</span>
        </p>
      </div>
    </div>

    <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      <div v-if="demoMeta.discipline" class="rounded-xl border border-neutral-grey/80 bg-surface px-3 py-2">
        <p class="text-[10px] font-semibold uppercase tracking-wide text-neutral-muted">Discipline</p>
        <p class="text-sm font-medium text-navy">{{ demoMeta.discipline }}</p>
      </div>
      <div v-if="demoMeta.height" class="rounded-xl border border-neutral-grey/80 bg-surface px-3 py-2">
        <p class="text-[10px] font-semibold uppercase tracking-wide text-neutral-muted">Height</p>
        <p class="text-sm font-medium text-navy">{{ demoMeta.height }}</p>
      </div>
      <div v-if="demoMeta.vet" class="rounded-xl border border-neutral-grey/80 bg-surface px-3 py-2">
        <p class="text-[10px] font-semibold uppercase tracking-wide text-neutral-muted">Vet</p>
        <p class="text-sm font-medium text-navy">{{ demoMeta.vet }}</p>
      </div>
      <div v-if="demoMeta.farrier" class="rounded-xl border border-neutral-grey/80 bg-surface px-3 py-2">
        <p class="text-[10px] font-semibold uppercase tracking-wide text-neutral-muted">Farrier</p>
        <p class="text-sm font-medium text-navy">{{ demoMeta.farrier }}</p>
      </div>
      <div v-if="demoMeta.saddleFitter" class="rounded-xl border border-neutral-grey/80 bg-surface px-3 py-2">
        <p class="text-[10px] font-semibold uppercase tracking-wide text-neutral-muted">Saddle Fitter</p>
        <p class="text-sm font-medium text-navy">{{ demoMeta.saddleFitter }}</p>
      </div>
    </div>

    <div class="rounded-xl border border-neutral-grey/80 bg-surface p-4">
      <p class="text-[10px] font-semibold uppercase tracking-wide text-neutral-muted">Diagnosis</p>
      <p class="mt-1 text-sm text-navy">{{ diagnosis }}</p>
    </div>

    <div class="grid gap-4 lg:grid-cols-2">
      <div class="portal-card p-4">
        <p class="text-sm font-bold text-navy">Current Plan</p>
        <div class="mt-4 flex items-center gap-4">
          <DonutChart
            :labels="['Complete', 'Remaining']"
            :values="[progressPercent, Math.max(0, 100 - progressPercent)]"
            :colors="['#6b7a4d', '#e5e7e3']"
            cutout="72%"
          >
            <div class="text-center">
              <p class="text-sm font-bold text-navy">{{ progressPercent }}%</p>
            </div>
          </DonutChart>
          <div class="min-w-0">
            <p class="text-sm font-semibold text-navy">
              {{ activeProgram?.programTitle ?? demoMeta.phaseLabel }}
            </p>
            <p v-if="activeProgram" class="mt-1 text-xs text-neutral-muted">
              Started {{ formatDate(activeProgram.startDate) }}
            </p>
            <p v-else class="mt-1 text-xs text-neutral-muted">{{ demoMeta.phaseLabel }}</p>
            <p v-if="activeProgram?.notes" class="mt-2 text-xs text-neutral-muted line-clamp-2">
              {{ activeProgram.notes }}
            </p>
          </div>
        </div>
      </div>

      <div class="portal-card p-4">
        <p class="text-sm font-bold text-navy">Next Session</p>
        <div v-if="nextAppointment" class="mt-4">
          <p class="text-sm font-semibold text-navy">
            {{ formatDate(nextAppointment.scheduledDateTime) }}
            at {{ formatTime(nextAppointment.scheduledDateTime) }}
          </p>
          <p class="mt-1 text-xs text-neutral-muted">{{ nextAppointment.appointmentStatus }}</p>
          <p v-if="nextAppointment.clinicianNotes" class="mt-2 text-xs text-neutral-muted">
            {{ nextAppointment.clinicianNotes }}
          </p>
        </div>
        <div v-else class="empty-state mt-4 py-6">
          <p class="text-sm text-neutral-muted">No upcoming sessions scheduled.</p>
        </div>
      </div>
    </div>
  </div>
</template>

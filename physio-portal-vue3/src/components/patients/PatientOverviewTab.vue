<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Camera, Trash2 } from '@lucide/vue'
import DonutChart from '../dashboard/DonutChart.vue'
import type { Appointment } from '../../types/appointment'
import type { PatientDemoMeta } from '../../data/patientDemo'
import { statusBadgeClass, statusLabel } from '../../data/patientDemo'
import type { RehabProgram } from '../../types/exercise'
import type { Pet } from '../../types/pet'
import { formatSaDate, formatSaTime } from '../../utils/dateTime'
import { resolveMediaUrl } from '../../api/videos'
import { usePatientsStore } from '../../store/patients'

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
  return formatSaDate(value, {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  })
}

function formatTime(value: string) {
  return formatSaTime(value)
}

const patientsStore = usePatientsStore()
const fileInput = ref<HTMLInputElement | null>(null)
const uploading = ref(false)
const imageError = ref(false)

watch(() => props.patient.profilePictureUrl, () => {
  imageError.value = false
})

async function onPhotoSelected(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  uploading.value = true
  try {
    await patientsStore.uploadPhoto(props.patient.petId, file)
  } catch (_) {
    // handled in store
  } finally {
    uploading.value = false
    target.value = ''
  }
}

async function onRemovePhoto() {
  uploading.value = true
  try {
    await patientsStore.removePhoto(props.patient.petId)
  } catch (_) {
    // handled in store
  } finally {
    uploading.value = false
  }
}
</script>

<template>
  <div class="space-y-5">
    <div class="flex flex-wrap items-start gap-4">
      <div
        class="group relative flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-full bg-sage-muted text-lg font-bold text-sage ring-2 ring-sage/20"
      >
        <img
          v-if="patient.profilePictureUrl && !imageError"
          :src="resolveMediaUrl(patient.profilePictureUrl)!"
          :alt="patient.petName"
          class="h-full w-full object-cover"
          @error="imageError = true"
        />
        <span v-else>
          {{ patient.petName.slice(0, 2).toUpperCase() }}
        </span>

        <!-- Hover overlay for upload/remove -->
        <div class="absolute inset-0 flex items-center justify-center gap-1 bg-navy/60 opacity-0 transition-opacity group-hover:opacity-100">
          <input
            ref="fileInput"
            type="file"
            accept="image/png,image/jpeg,image/webp"
            class="hidden"
            @change="onPhotoSelected"
          />
          <button
            type="button"
            :disabled="uploading"
            class="rounded p-1 text-white hover:bg-white/20"
            title="Upload pet photo"
            @click="fileInput?.click()"
          >
            <Camera class="h-4 w-4" />
          </button>
          <button
            v-if="patient.profilePictureUrl"
            type="button"
            :disabled="uploading"
            class="rounded p-1 text-red-300 hover:bg-white/20 hover:text-red-100"
            title="Remove photo"
            @click="onRemovePhoto"
          >
            <Trash2 class="h-4 w-4" />
          </button>
        </div>

        <div
          v-if="uploading"
          class="absolute inset-0 flex items-center justify-center bg-navy/70 text-xs text-white"
        >
          ...
        </div>
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

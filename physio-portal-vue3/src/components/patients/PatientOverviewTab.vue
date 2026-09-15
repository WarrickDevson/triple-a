<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Camera, Maximize2, Trash2 } from '@lucide/vue'
import DonutChart from '../dashboard/DonutChart.vue'
import ImageViewerModal from '../common/ImageViewerModal.vue'
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
const ownerImageError = ref(false)

const viewerModal = ref<{
  url: string
  title: string
  subtitle?: string | null
} | null>(null)

function openViewer(url: string, title: string, subtitle?: string | null) {
  const resolved = resolveMediaUrl(url)
  if (!resolved) return
  viewerModal.value = { url: resolved, title, subtitle }
}

watch(() => props.patient.profilePictureUrl, () => {
  imageError.value = false
})

watch(() => props.patient.ownerProfilePictureUrl, () => {
  ownerImageError.value = false
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
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="flex items-start gap-4 min-w-0">
        <!-- Pet Profile Picture with Click-to-Zoom and Upload Hover -->
        <div
          class="group relative flex h-16 w-16 shrink-0 cursor-pointer items-center justify-center overflow-hidden rounded-full bg-sage-muted text-lg font-bold text-sage ring-2 ring-sage/20 shadow-sm"
          title="Click to view full photo"
          @click="patient.profilePictureUrl ? openViewer(patient.profilePictureUrl, patient.petName, patient.breed || patient.species) : fileInput?.click()"
        >
          <img
            v-if="patient.profilePictureUrl && !imageError"
            :src="resolveMediaUrl(patient.profilePictureUrl)!"
            :alt="patient.petName"
            class="h-full w-full object-cover transition-transform duration-200 group-hover:scale-105"
            @error="imageError = true"
          />
          <span v-else>
            {{ patient.petName.slice(0, 2).toUpperCase() }}
          </span>

          <!-- Hover overlay for upload/remove/zoom -->
          <div
            class="absolute inset-0 flex items-center justify-center gap-1 bg-navy/70 opacity-0 transition-opacity group-hover:opacity-100"
            @click.stop
          >
            <input
              ref="fileInput"
              type="file"
              accept="image/png,image/jpeg,image/webp"
              class="hidden"
              @change="onPhotoSelected"
            />
            <button
              v-if="patient.profilePictureUrl"
              type="button"
              class="rounded p-1 text-white hover:bg-white/20"
              title="View full screen"
              @click="openViewer(patient.profilePictureUrl, patient.petName, patient.breed || patient.species)"
            >
              <Maximize2 class="h-3.5 w-3.5" />
            </button>
            <button
              type="button"
              :disabled="uploading"
              class="rounded p-1 text-white hover:bg-white/20"
              title="Change pet photo"
              @click="fileInput?.click()"
            >
              <Camera class="h-3.5 w-3.5" />
            </button>
            <button
              v-if="patient.profilePictureUrl"
              type="button"
              :disabled="uploading"
              class="rounded p-1 text-red-300 hover:bg-white/20 hover:text-red-100"
              title="Remove photo"
              @click="onRemovePhoto"
            >
              <Trash2 class="h-3.5 w-3.5" />
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
            {{ patient.breed || patient.species }}
          </p>
          <p class="mt-0.5 text-xs text-neutral-muted">
            {{ ageLabel }}
            <span v-if="patient.weightKg"> · {{ patient.weightKg }} kg</span>
          </p>
        </div>
      </div>

      <!-- Owner Profile Picture Card -->
      <div
        class="flex items-center gap-3 rounded-2xl border border-neutral-grey/80 bg-surface/80 px-3.5 py-2.5 shadow-xs transition hover:border-sage/40"
      >
        <div
          class="relative flex h-11 w-11 shrink-0 items-center justify-center overflow-hidden rounded-full bg-sage-muted text-xs font-bold text-sage ring-2 ring-sage/20 transition hover:ring-sage"
          :class="patient.ownerProfilePictureUrl ? 'cursor-pointer' : ''"
          :title="patient.ownerProfilePictureUrl ? 'Click to view owner photo' : 'Owner avatar'"
          @click="patient.ownerProfilePictureUrl ? openViewer(patient.ownerProfilePictureUrl, patient.ownerName, 'Pet Owner') : null"
        >
          <img
            v-if="patient.ownerProfilePictureUrl && !ownerImageError"
            :src="resolveMediaUrl(patient.ownerProfilePictureUrl)!"
            :alt="patient.ownerName"
            class="h-full w-full object-cover transition-transform duration-200 hover:scale-105"
            @error="ownerImageError = true"
          />
          <span v-else>
            {{ patient.ownerName.slice(0, 2).toUpperCase() }}
          </span>
        </div>
        <div class="min-w-0">
          <div class="flex items-center gap-1.5">
            <p class="truncate text-xs font-bold text-navy">{{ patient.ownerName }}</p>
            <span class="rounded bg-sage-muted px-1.5 py-0.5 text-[9px] font-bold text-sage">Owner</span>
          </div>
          <p class="text-[11px] text-neutral-muted">ID: #{{ patient.ownerId }}</p>
          <button
            v-if="patient.ownerProfilePictureUrl"
            type="button"
            class="mt-0.5 inline-flex items-center gap-1 text-[10px] font-semibold text-sage hover:underline"
            @click="openViewer(patient.ownerProfilePictureUrl, patient.ownerName, 'Pet Owner')"
          >
            <Maximize2 class="h-2.5 w-2.5" />
            View Photo
          </button>
        </div>
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

    <!-- Full Screen Image Preview Modal -->
    <ImageViewerModal
      v-if="viewerModal"
      :image-url="viewerModal.url"
      :title="viewerModal.title"
      :subtitle="viewerModal.subtitle"
      @close="viewerModal = null"
    />
  </div>
</template>

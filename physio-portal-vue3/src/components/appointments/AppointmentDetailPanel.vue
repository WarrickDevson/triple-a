<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import BaseButton from '../BaseButton.vue'
import {
  getAppointmentLocation,
  getAppointmentType,
  statusBadgeClass,
} from '../../data/appointmentDemo'
import type { Appointment } from '../../types/appointment'

const props = defineProps<{
  appointment: Appointment | null
  upcoming: Appointment[]
}>()

const emit = defineEmits<{
  accept: [appointmentId: number]
  reject: [appointmentId: number]
  cancel: [appointmentId: number]
  complete: [appointmentId: number]
  startSoap: [appointment: Appointment]
  reschedule: [appointment: Appointment, newDatetime: string]
}>()

const showReschedule = ref(false)
const rescheduleDate = ref('')
const rescheduleTime = ref('09:00')

const sessionType = computed(() =>
  props.appointment ? getAppointmentType(props.appointment.appointmentId) : null,
)

function parseUtcDate(value: string) {
  const str = value.endsWith('Z') || value.includes('+') ? value : `${value}Z`
  return new Date(str)
}

function formatDateTime(value: string) {
  return parseUtcDate(value).toLocaleString([], {
    timeZone: 'UTC',
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function openReschedule() {
  if (!props.appointment) return
  const d = parseUtcDate(props.appointment.scheduledDateTime)
  const y = d.getUTCFullYear()
  const m = String(d.getUTCMonth() + 1).padStart(2, '0')
  const day = String(d.getUTCDate()).padStart(2, '0')
  const hh = String(d.getUTCHours()).padStart(2, '0')
  const mm = String(d.getUTCMinutes()).padStart(2, '0')
  rescheduleDate.value = `${y}-${m}-${day}`
  rescheduleTime.value = `${hh}:${mm}`
  showReschedule.value = true
}

function submitReschedule() {
  if (!props.appointment || !rescheduleDate.value) return
  const newDatetime = `${rescheduleDate.value}T${rescheduleTime.value}:00Z`
  emit('reschedule', props.appointment, newDatetime)
  showReschedule.value = false
}
</script>

<template>
  <div class="space-y-4">
    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Appointment Details</h3>
      <div v-if="appointment" class="mt-4">
        <div class="flex items-start gap-3">
          <div
            class="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-sage-muted text-sm font-bold text-sage"
          >
            {{ appointment.petName.slice(0, 2).toUpperCase() }}
          </div>
          <div>
            <p class="font-semibold text-navy">{{ appointment.petName }}</p>
            <p class="text-xs text-neutral-muted">{{ sessionType?.label }}</p>
            <span :class="statusBadgeClass(appointment.appointmentStatus)" class="mt-2 inline-block">
              {{ appointment.appointmentStatus }}
            </span>
          </div>
        </div>

        <dl class="mt-4 space-y-2 text-sm">
          <div class="flex justify-between gap-2">
            <dt class="text-neutral-muted">Date & time</dt>
            <dd class="text-right font-medium text-navy">{{ formatDateTime(appointment.scheduledDateTime) }}</dd>
          </div>
          <div class="flex justify-between gap-2">
            <dt class="text-neutral-muted">Location</dt>
            <dd class="text-right font-medium text-navy">
              {{ getAppointmentLocation(appointment.appointmentId) }}
            </dd>
          </div>
          <div class="flex justify-between gap-2">
            <dt class="text-neutral-muted">Therapist</dt>
            <dd class="text-right font-medium text-navy">{{ appointment.physioName }}</dd>
          </div>
          <div class="flex justify-between gap-2">
            <dt class="text-neutral-muted">Owner</dt>
            <dd class="text-right font-medium text-navy">{{ appointment.ownerName }}</dd>
          </div>
        </dl>

        <!-- Owner Notes Box -->
        <div v-if="appointment.clientNotes" class="mt-4 rounded-xl border border-amber-200 bg-amber-50/70 p-3 text-sm">
          <p class="text-xs font-bold text-amber-900 flex items-center gap-1">
            <span class="text-amber-600">📝</span> Owner Notes:
          </p>
          <p class="mt-1 text-xs text-amber-950 font-medium whitespace-pre-line">
            {{ appointment.clientNotes }}
          </p>
        </div>

        <!-- Action Buttons depending on status -->
        <div v-if="appointment.appointmentStatus === 'Requested'" class="mt-4 space-y-2">
          <div class="flex gap-2">
            <BaseButton
              class="flex-1"
              variant="accent"
              size="sm"
              @click="emit('accept', appointment.appointmentId)"
            >
              ✓ Accept Request
            </BaseButton>
            <BaseButton
              class="flex-1"
              variant="danger"
              size="sm"
              @click="emit('reject', appointment.appointmentId)"
            >
              ✕ Reject Request
            </BaseButton>
          </div>
        </div>

        <div v-else-if="appointment.appointmentStatus === 'Scheduled'" class="mt-4 space-y-2">
          <BaseButton
            class="w-full"
            variant="accent"
            size="sm"
            @click="emit('startSoap', appointment)"
          >
            📋 Start SOAP Assessment
          </BaseButton>

          <div class="grid grid-cols-3 gap-2">
            <BaseButton size="sm" variant="secondary" @click="openReschedule">Reschedule</BaseButton>
            <BaseButton size="sm" variant="secondary">Edit</BaseButton>
            <BaseButton
              size="sm"
              variant="danger"
              @click="emit('cancel', appointment.appointmentId)"
            >
              Cancel
            </BaseButton>
          </div>
          <BaseButton
            class="mt-2 w-full"
            variant="accent"
            size="sm"
            @click="emit('complete', appointment.appointmentId)"
          >
            Mark Complete
          </BaseButton>
        </div>

        <div v-if="showReschedule" class="mt-4 rounded-xl border border-neutral-grey/80 bg-surface p-3">
          <p class="text-xs font-semibold text-navy">Reschedule to</p>
          <div class="mt-2 grid grid-cols-2 gap-2">
            <input
              v-model="rescheduleDate"
              type="date"
              class="rounded-lg border border-neutral-grey px-2 py-1.5 text-sm"
            />
            <input
              v-model="rescheduleTime"
              type="time"
              class="rounded-lg border border-neutral-grey px-2 py-1.5 text-sm"
            />
          </div>
          <BaseButton class="mt-2 w-full" size="sm" @click="submitReschedule">Confirm Reschedule</BaseButton>
        </div>
      </div>
      <p v-else class="mt-4 text-sm text-neutral-muted">Select an appointment to view details.</p>
    </section>

    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Upcoming Appointments</h3>
      <ul class="mt-3 space-y-2">
        <li
          v-for="item in upcoming.slice(0, 4)"
          :key="item.appointmentId"
          class="flex justify-between gap-2 border-b border-neutral-grey/60 pb-2 text-sm last:border-0"
        >
          <span class="font-medium text-navy">{{ item.petName }}</span>
          <span class="text-xs text-neutral-muted">
            {{ parseUtcDate(item.scheduledDateTime).toLocaleDateString([], { timeZone: 'UTC', month: 'short', day: 'numeric' }) }}
            {{ parseUtcDate(item.scheduledDateTime).toLocaleTimeString([], { timeZone: 'UTC', hour: '2-digit', minute: '2-digit' }) }}
          </span>
        </li>
      </ul>
      <RouterLink :to="{ name: 'appointments' }" class="portal-card-link mt-3 inline-block">
        View all appointments →
      </RouterLink>
    </section>
  </div>
</template>

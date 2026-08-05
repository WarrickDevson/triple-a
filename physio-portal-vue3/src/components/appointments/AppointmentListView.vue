<script setup lang="ts">
import { getAppointmentLocation, getAppointmentType, statusBadgeClass } from '../../data/appointmentDemo'
import type { Appointment } from '../../types/appointment'

defineProps<{
  appointments: Appointment[]
  selectedId: number | null
}>()

const emit = defineEmits<{
  select: [appointmentId: number]
}>()

function formatDateTime(value: string) {
  const str = value.endsWith('Z') || value.includes('+') ? value : `${value}Z`
  return new Date(str).toLocaleString([], {
    timeZone: 'UTC',
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<template>
  <section class="portal-card overflow-hidden">
    <ul class="divide-y divide-neutral-grey/80">
      <li v-for="appointment in appointments" :key="appointment.appointmentId">
        <button
          type="button"
          class="flex w-full items-start gap-4 px-4 py-4 text-left transition-colors hover:bg-surface"
          :class="selectedId === appointment.appointmentId ? 'bg-sage-muted/40' : ''"
          @click="emit('select', appointment.appointmentId)"
        >
          <div
            class="mt-1 h-3 w-3 shrink-0 rounded-full"
            :style="{ backgroundColor: getAppointmentType(appointment.appointmentId).color }"
          />
          <div class="min-w-0 flex-1">
            <div class="flex flex-wrap items-center gap-2">
              <p class="font-semibold text-navy">{{ appointment.petName }}</p>
              <span :class="statusBadgeClass(appointment.appointmentStatus)">
                {{ appointment.appointmentStatus }}
              </span>
            </div>
            <p class="mt-1 text-sm text-neutral-muted">
              {{ getAppointmentType(appointment.appointmentId).label }}
            </p>
            <p class="text-xs text-neutral-muted">{{ formatDateTime(appointment.scheduledDateTime) }}</p>
            <p class="text-xs text-neutral-muted">
              {{ appointment.ownerName }} · {{ getAppointmentLocation(appointment.appointmentId) }}
            </p>
            <p v-if="appointment.clientNotes" class="mt-1 text-xs font-medium text-amber-800 bg-amber-50/80 rounded px-1.5 py-0.5 inline-block">
              📝 Owner note attached
            </p>
          </div>
        </button>
      </li>
    </ul>
    <div v-if="appointments.length === 0" class="empty-state m-4 py-12">
      <p class="text-sm text-neutral-muted">No appointments in this view.</p>
    </div>
  </section>
</template>

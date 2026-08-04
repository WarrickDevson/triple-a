<script setup lang="ts">
import { computed } from 'vue'
import { MoreVertical } from '@lucide/vue'
import { getAppointmentLocation, getAppointmentType } from '../../data/appointmentDemo'
import type { Appointment } from '../../types/appointment'

const props = defineProps<{
  appointments: Appointment[]
  selectedDate: Date
  selectedId: number | null
  showCancelled: boolean
  showCompleted: boolean
}>()

const emit = defineEmits<{
  select: [appointmentId: number]
}>()

const hours = Array.from({ length: 11 }, (_, i) => 8 + i)

const dayAppointments = computed(() => {
  return props.appointments
    .filter((a) => {
      const d = new Date(a.scheduledDateTime)
      const sameDay =
        d.getFullYear() === props.selectedDate.getFullYear() &&
        d.getMonth() === props.selectedDate.getMonth() &&
        d.getDate() === props.selectedDate.getDate()
      if (!sameDay) return false
      const status = a.appointmentStatus.toLowerCase()
      if (!props.showCancelled && status.includes('cancel')) return false
      if (!props.showCompleted && status.includes('complete')) return false
      return true
    })
    .sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime())
})

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function blockTop(datetime: string) {
  const d = new Date(datetime)
  const minutes = (d.getHours() - 8) * 60 + d.getMinutes()
  return `${(minutes / 600) * 100}%`
}

function blockHeight() {
  return '12%'
}
</script>

<template>
  <section class="portal-card overflow-hidden">
    <div class="border-b border-neutral-grey/80 px-4 py-3">
      <h2 class="text-sm font-bold text-navy">
        {{ selectedDate.toLocaleDateString([], { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }) }}
      </h2>
    </div>

    <div class="relative min-h-[520px] p-4">
      <div class="absolute inset-4 flex">
        <div class="w-12 shrink-0">
          <div
            v-for="hour in hours"
            :key="hour"
            class="h-[60px] text-[10px] text-neutral-muted"
          >
            {{ String(hour).padStart(2, '0') }}:00
          </div>
        </div>
        <div class="relative flex-1 border-l border-neutral-grey/80">
          <div
            v-for="hour in hours"
            :key="`line-${hour}`"
            class="h-[60px] border-b border-neutral-grey/40"
          />
          <button
            v-for="appointment in dayAppointments"
            :key="appointment.appointmentId"
            type="button"
            class="absolute left-2 right-2 overflow-hidden rounded-lg border p-2 text-left shadow-sm transition-shadow hover:shadow-md"
            :class="selectedId === appointment.appointmentId ? 'ring-2 ring-sage' : ''"
            :style="{
              top: blockTop(appointment.scheduledDateTime),
              height: blockHeight(),
              backgroundColor: getAppointmentType(appointment.appointmentId).bg,
              borderColor: getAppointmentType(appointment.appointmentId).color,
            }"
            @click="emit('select', appointment.appointmentId)"
          >
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <p class="truncate text-xs font-bold text-navy">{{ appointment.petName }}</p>
                <p class="truncate text-[10px] text-neutral-muted">
                  {{ getAppointmentType(appointment.appointmentId).label }}
                </p>
                <p class="text-[10px] text-neutral-muted">
                  {{ formatTime(appointment.scheduledDateTime) }} · {{ appointment.physioName }}
                </p>
                <p class="text-[10px] text-neutral-muted">
                  {{ getAppointmentLocation(appointment.appointmentId) }}
                </p>
              </div>
              <MoreVertical class="h-3.5 w-3.5 shrink-0 text-neutral-muted" />
            </div>
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

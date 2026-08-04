<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { DashboardAppointment } from '../../types/dashboard'

defineProps<{
  appointments: DashboardAppointment[]
  loading?: boolean
}>()

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function statusColor(status: string) {
  const normalized = status.toLowerCase()
  if (normalized.includes('complete') || normalized.includes('confirm')) return 'bg-success-green'
  if (normalized.includes('cancel') || normalized.includes('no-show')) return 'bg-neutral-grey'
  return 'bg-accent-amber'
}

const demoTreatments = ['Laser Therapy & Strength', 'Hydrotherapy Session', 'Mobility Assessment', 'TENS & Massage']
</script>

<template>
  <section class="portal-card p-5">
    <div class="portal-card-header">
      <h2 class="portal-card-title">Today's Schedule</h2>
    </div>

    <div v-if="loading" class="py-8 text-center text-sm text-neutral-muted">Loading schedule...</div>
    <ul v-else-if="appointments.length" class="space-y-3">
      <li
        v-for="(appointment, index) in appointments"
        :key="appointment.appointmentId"
        class="flex items-center gap-3 border-b border-neutral-grey/60 pb-3 last:border-0 last:pb-0"
      >
        <span class="w-12 shrink-0 text-sm font-semibold text-navy">{{ formatTime(appointment.scheduledDateTime) }}</span>
        <span class="h-2 w-2 shrink-0 rounded-full" :class="statusColor(appointment.appointmentStatus)" />
        <div class="min-w-0 flex-1">
          <p class="truncate text-sm font-semibold text-navy">{{ appointment.petName }}</p>
          <p class="truncate text-xs text-neutral-muted">
            {{ appointment.ownerName }} · {{ demoTreatments[index % demoTreatments.length] }}
          </p>
        </div>
      </li>
    </ul>
    <div v-else class="empty-state py-6">
      <p class="text-sm text-neutral-muted">No appointments scheduled for today.</p>
    </div>

    <RouterLink :to="{ name: 'appointments' }" class="portal-card-link mt-4 inline-block">
      View full calendar →
    </RouterLink>
  </section>
</template>

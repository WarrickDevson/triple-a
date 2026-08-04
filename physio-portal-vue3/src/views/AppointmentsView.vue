<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { Calendar, ChevronLeft, ChevronRight, Plus } from '@lucide/vue'
import AppointmentDetailPanel from '../components/appointments/AppointmentDetailPanel.vue'
import AppointmentListView from '../components/appointments/AppointmentListView.vue'
import AppointmentTabs from '../components/appointments/AppointmentTabs.vue'
import DaySchedule from '../components/appointments/DaySchedule.vue'
import MiniCalendar from '../components/appointments/MiniCalendar.vue'
import NewAppointmentModal from '../components/appointments/NewAppointmentModal.vue'
import BaseButton from '../components/BaseButton.vue'
import { useAppointmentsStore } from '../store/appointments'
import { usePatientsStore } from '../store/patients'

const appointmentsStore = useAppointmentsStore()
const patientsStore = usePatientsStore()

const activeTab = ref<'calendar' | 'list' | 'waitlist'>('calendar')
const selectedDate = ref(new Date())
const selectedAppointmentId = ref<number | null>(null)
const showNewModal = ref(false)
const showCancelled = ref(false)
const showCompleted = ref(false)

onMounted(async () => {
  await Promise.all([
    loadMonthAppointments(),
    patientsStore.fetchClinicPatients().catch(() => undefined),
  ])
})

watch(selectedDate, () => {
  loadMonthAppointments()
})

function monthRange(date: Date) {
  const from = new Date(date.getFullYear(), date.getMonth(), 1)
  const to = new Date(date.getFullYear(), date.getMonth() + 1, 0, 23, 59, 59)
  return { from: from.toISOString(), to: to.toISOString() }
}

async function loadMonthAppointments() {
  const { from, to } = monthRange(selectedDate.value)
  await appointmentsStore.loadAppointments(from, to)
}

const filteredAppointments = computed(() => {
  return appointmentsStore.appointments.filter((a) => {
    const status = a.appointmentStatus.toLowerCase()
    if (!showCancelled.value && status.includes('cancel')) return false
    if (!showCompleted.value && status.includes('complete')) return false
    return true
  })
})

const listAppointments = computed(() =>
  [...filteredAppointments.value].sort(
    (a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime(),
  ),
)

const selectedAppointment = computed(
  () =>
    appointmentsStore.appointments.find((a) => a.appointmentId === selectedAppointmentId.value) ??
    null,
)

const upcomingAppointments = computed(() => {
  const now = Date.now()
  return appointmentsStore.appointments
    .filter((a) => new Date(a.scheduledDateTime).getTime() >= now)
    .sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime())
})

function goToday() {
  selectedDate.value = new Date()
}

function shiftDay(delta: number) {
  const next = new Date(selectedDate.value)
  next.setDate(next.getDate() + delta)
  selectedDate.value = next
}

async function onCancel(appointmentId: number) {
  await appointmentsStore.cancelAppointment(appointmentId)
}

async function onComplete(appointmentId: number) {
  await appointmentsStore.completeAppointment(appointmentId)
}

async function onReschedule(appointment: { appointmentId: number; petId: number; clientNotes: string | null; clinicianNotes: string | null }, newDatetime: string) {
  await appointmentsStore.scheduleAppointment({
    petId: appointment.petId,
    scheduledDateTime: newDatetime,
    clientNotes: appointment.clientNotes ?? undefined,
    clinicianNotes: appointment.clinicianNotes ?? undefined,
  })
  await appointmentsStore.cancelAppointment(appointment.appointmentId)
  await loadMonthAppointments()
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center gap-3">
      <BaseButton @click="showNewModal = true">
        <Plus class="h-4 w-4" :stroke-width="2" />
        New Appointment
      </BaseButton>
      <div class="ml-auto flex items-center gap-2">
        <BaseButton size="sm" variant="secondary" @click="goToday">
          <Calendar class="h-4 w-4" :stroke-width="1.75" />
          Today
        </BaseButton>
        <button
          type="button"
          class="flex h-9 w-9 items-center justify-center rounded-lg border border-neutral-grey text-navy hover:bg-surface"
          @click="shiftDay(-1)"
        >
          <ChevronLeft class="h-4 w-4" />
        </button>
        <button
          type="button"
          class="flex h-9 w-9 items-center justify-center rounded-lg border border-neutral-grey text-navy hover:bg-surface"
          @click="shiftDay(1)"
        >
          <ChevronRight class="h-4 w-4" />
        </button>
      </div>
    </div>

    <div class="grid gap-4 xl:grid-cols-[240px_minmax(0,1fr)_300px]">
      <div class="min-h-0">
        <MiniCalendar
          v-model:selected-date="selectedDate"
          v-model:show-cancelled="showCancelled"
          v-model:show-completed="showCompleted"
        />
      </div>

      <section class="portal-card min-h-[600px] overflow-hidden">
        <AppointmentTabs v-model:active-tab="activeTab" />
        <div class="p-4">
          <div v-if="activeTab === 'waitlist'" class="empty-state py-16">
            <p class="text-sm text-neutral-muted">Waitlist management coming soon.</p>
          </div>
          <DaySchedule
            v-else-if="activeTab === 'calendar'"
            :appointments="filteredAppointments"
            :selected-date="selectedDate"
            :selected-id="selectedAppointmentId"
            :show-cancelled="showCancelled"
            :show-completed="showCompleted"
            @select="selectedAppointmentId = $event"
          />
          <AppointmentListView
            v-else
            :appointments="listAppointments"
            :selected-id="selectedAppointmentId"
            @select="selectedAppointmentId = $event"
          />
        </div>
      </section>

      <div class="min-h-0">
        <AppointmentDetailPanel
          :appointment="selectedAppointment"
          :upcoming="upcomingAppointments"
          @cancel="onCancel"
          @complete="onComplete"
          @reschedule="onReschedule"
        />
      </div>
    </div>

    <NewAppointmentModal
      :open="showNewModal"
      @close="showNewModal = false"
      @created="loadMonthAppointments"
    />
  </div>
</template>

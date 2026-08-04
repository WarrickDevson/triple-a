import { defineStore } from 'pinia'
import { ref } from 'vue'
import { createAppointment, fetchAppointments, updateAppointmentStatus } from '../api/appointments'
import type { Appointment, CreateAppointmentRequest } from '../types/appointment'

export const useAppointmentsStore = defineStore('appointments', () => {
  const appointments = ref<Appointment[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadAppointments(from?: string, to?: string) {
    loading.value = true
    error.value = null
    try {
      appointments.value = await fetchAppointments(from, to)
    } catch {
      error.value = 'Unable to load appointments.'
    } finally {
      loading.value = false
    }
  }

  async function scheduleAppointment(request: CreateAppointmentRequest) {
    const appointment = await createAppointment(request)
    appointments.value = [...appointments.value, appointment].sort(
      (a, b) =>
        new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime(),
    )
    return appointment
  }

  async function completeAppointment(appointmentId: number, clinicianNotes?: string) {
    const updated = await updateAppointmentStatus(appointmentId, {
      status: 'Completed',
      clinicianNotes,
    })
    appointments.value = appointments.value.map((a) =>
      a.appointmentId === appointmentId ? updated : a,
    )
    return updated
  }

  async function cancelAppointment(appointmentId: number, clinicianNotes?: string) {
    const updated = await updateAppointmentStatus(appointmentId, {
      status: 'Cancelled',
      clinicianNotes,
    })
    appointments.value = appointments.value.map((a) =>
      a.appointmentId === appointmentId ? updated : a,
    )
    return updated
  }

  return {
    appointments,
    loading,
    error,
    loadAppointments,
    scheduleAppointment,
    completeAppointment,
    cancelAppointment,
  }
})

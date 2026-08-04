import { apiClient } from './client'
import type {
  Appointment,
  CreateAppointmentRequest,
  UpdateAppointmentStatusRequest,
} from '../types/appointment'

export async function fetchAppointments(from?: string, to?: string): Promise<Appointment[]> {
  const { data } = await apiClient.get<Appointment[]>('/api/appointments', {
    params: { from, to },
  })
  return data
}

export async function createAppointment(request: CreateAppointmentRequest): Promise<Appointment> {
  const { data } = await apiClient.post<Appointment>('/api/appointments', request)
  return data
}

export async function updateAppointmentStatus(
  appointmentId: number,
  request: UpdateAppointmentStatusRequest,
): Promise<Appointment> {
  const { data } = await apiClient.put<Appointment>(
    `/api/appointments/${appointmentId}/status`,
    request,
  )
  return data
}

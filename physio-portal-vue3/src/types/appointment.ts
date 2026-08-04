export interface Appointment {
  appointmentId: number
  physioId: number
  physioName: string
  ownerId: number
  ownerName: string
  petId: number
  petName: string
  scheduledDateTime: string
  appointmentStatus: string
  clientNotes: string | null
  clinicianNotes: string | null
}

export interface CreateAppointmentRequest {
  petId: number
  scheduledDateTime: string
  clientNotes?: string
  clinicianNotes?: string
}

export interface UpdateAppointmentStatusRequest {
  status: string
  clinicianNotes?: string
}

export interface DashboardAppointment {
  appointmentId: number
  petName: string
  ownerName: string
  scheduledDateTime: string
  appointmentStatus: string
}

export interface PhysioDashboard {
  patientCount: number
  pendingVideoReviews: number
  todaysAppointmentsCount: number
  todaysSchedule: DashboardAppointment[]
}

export interface PetProgressLog {
  logDate: string
  painScore: number | null
  lamenessScore: number | null
  energyScore: number | null
  appetiteScore: number | null
  mobilityScore: number | null
  weightKg: number | null
  isCompleted: boolean
}

export interface PetProgressSummary {
  petId: number
  petName: string
  totalCompletedSessions: number
  totalTrackedDays: number
  logs: PetProgressLog[]
}

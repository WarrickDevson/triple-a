export type AppointmentSessionType =
  | 'Initial Assessment'
  | 'Follow-up Session'
  | 'Hydrotherapy'
  | 'Re-evaluation'
  | 'Home Visit'
  | 'Other'

export interface AppointmentTypeStyle {
  label: AppointmentSessionType
  color: string
  bg: string
}

export const APPOINTMENT_TYPES: AppointmentTypeStyle[] = [
  { label: 'Initial Assessment', color: '#c4845c', bg: '#f5e6d8' },
  { label: 'Follow-up Session', color: '#d4924f', bg: '#fce8d4' },
  { label: 'Hydrotherapy', color: '#5a8fa8', bg: '#dceef5' },
  { label: 'Re-evaluation', color: '#a89068', bg: '#f0ebe0' },
  { label: 'Home Visit', color: '#8b7ab8', bg: '#ebe6f5' },
  { label: 'Other', color: '#6b7280', bg: '#f0f1ee' },
]

const typeByAppointmentId: Record<number, AppointmentSessionType> = {}

export function getAppointmentType(appointmentId: number): AppointmentTypeStyle {
  if (!typeByAppointmentId[appointmentId]) {
    const types = APPOINTMENT_TYPES
    typeByAppointmentId[appointmentId] = types[appointmentId % types.length]!.label
  }
  const label = typeByAppointmentId[appointmentId]!
  return APPOINTMENT_TYPES.find((t) => t.label === label) ?? APPOINTMENT_TYPES[5]!
}

export function getAppointmentLocation(appointmentId: number): string {
  const rooms = ['Consult Room 1', 'Consult Room 2', 'Hydro Pool', 'Home Visit', 'Arena']
  return rooms[appointmentId % rooms.length]!
}

export function statusBadgeClass(status: string) {
  const normalized = status.toLowerCase()
  if (normalized.includes('confirm') || normalized.includes('complete') || normalized.includes('schedule')) {
    return 'status-badge status-badge--improving'
  }
  if (normalized.includes('request') || normalized.includes('pending')) {
    return 'status-badge status-badge--stable'
  }
  if (normalized.includes('cancel') || normalized.includes('reject')) {
    return 'status-badge status-badge--at-risk'
  }
  return 'status-badge status-badge--stable'
}

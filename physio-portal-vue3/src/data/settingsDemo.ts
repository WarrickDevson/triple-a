export interface ClinicSettings {
  clinicName: string
  timezone: string
  defaultAppointmentMinutes: number
}

export interface NotificationSettings {
  emailAppointments: boolean
  emailMessages: boolean
  emailVideoReviews: boolean
  inAppAppointments: boolean
  inAppMessages: boolean
  inAppVideoReviews: boolean
}

const CLINIC_KEY = 'triple-a-clinic-settings'
const NOTIF_KEY = 'triple-a-notification-settings'

export const defaultClinicSettings: ClinicSettings = {
  clinicName: 'Personal Clinic',
  timezone: 'Africa/Johannesburg',
  defaultAppointmentMinutes: 45,
}

export const defaultNotificationSettings: NotificationSettings = {
  emailAppointments: true,
  emailMessages: true,
  emailVideoReviews: false,
  inAppAppointments: true,
  inAppMessages: true,
  inAppVideoReviews: true,
}

export const TIMEZONE_OPTIONS = [
  'Africa/Johannesburg',
  'Africa/Cape_Town',
  'Europe/London',
  'Australia/Sydney',
  'America/New_York',
]

export const APPOINTMENT_DURATIONS = [30, 45, 60, 90]

export function loadClinicSettings(): ClinicSettings {
  try {
    const raw = localStorage.getItem(CLINIC_KEY)
    if (!raw) return { ...defaultClinicSettings }
    return { ...defaultClinicSettings, ...JSON.parse(raw) }
  } catch {
    return { ...defaultClinicSettings }
  }
}

export function saveClinicSettings(settings: ClinicSettings) {
  localStorage.setItem(CLINIC_KEY, JSON.stringify(settings))
}

export function loadNotificationSettings(): NotificationSettings {
  try {
    const raw = localStorage.getItem(NOTIF_KEY)
    if (!raw) return { ...defaultNotificationSettings }
    return { ...defaultNotificationSettings, ...JSON.parse(raw) }
  } catch {
    return { ...defaultNotificationSettings }
  }
}

export function saveNotificationSettings(settings: NotificationSettings) {
  localStorage.setItem(NOTIF_KEY, JSON.stringify(settings))
}

export function displayRole(role?: string) {
  if (!role) return 'Veterinary Physiotherapist'
  return role.replace(/([A-Z])/g, ' $1').trim()
}

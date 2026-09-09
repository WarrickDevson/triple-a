/**
 * South African Timezone Utilities (SAST / Africa/Johannesburg)
 * South Africa Standard Time is UTC+2 year-round (no DST).
 */

export const SA_TIMEZONE = 'Africa/Johannesburg'
export const SA_LOCALE = 'en-ZA'
export const SA_OFFSET_HOURS = 2

/**
 * Parses an API or ISO date string into a Date object.
 * If no timezone is specified, it assumes UTC as returned by the .NET backend.
 */
export function parseDate(input: string | Date | number | null | undefined): Date {
  if (!input) return new Date()
  if (input instanceof Date) return input
  if (typeof input === 'number') return new Date(input)

  const str = input.trim()
  if (str.endsWith('Z') || /[+-]\d{2}:?\d{2}$/.test(str)) {
    return new Date(str)
  }
  return new Date(`${str}Z`)
}

export interface ZonedDateParts {
  year: number
  month: number // 0-indexed (0 = Jan, 11 = Dec) like Date.getMonth()
  day: number
  hour: number
  minute: number
  second: number
}

/**
 * Extracts date & time parts in South Africa Standard Time (Africa/Johannesburg).
 */
export function getZonedParts(
  input: string | Date | number | null | undefined,
  timeZone = SA_TIMEZONE,
): ZonedDateParts {
  const date = parseDate(input)
  const formatter = new Intl.DateTimeFormat(SA_LOCALE, {
    timeZone,
    year: 'numeric',
    month: 'numeric',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
    second: 'numeric',
    hourCycle: 'h23',
  })

  const parts = formatter.formatToParts(date)
  const map: Record<string, number> = {}

  for (const p of parts) {
    if (p.type !== 'literal') {
      map[p.type] = parseInt(p.value, 10)
    }
  }

  return {
    year: map.year ?? date.getUTCFullYear(),
    month: (map.month ?? date.getUTCMonth() + 1) - 1,
    day: map.day ?? date.getUTCDate(),
    hour: map.hour === 24 ? 0 : (map.hour ?? 0),
    minute: map.minute ?? date.getUTCMinutes(),
    second: map.second ?? date.getUTCSeconds(),
  }
}

/**
 * Formats a date & time in South Africa Standard Time (Africa/Johannesburg).
 */
export function formatSaDateTime(
  input: string | Date | number | null | undefined,
  options?: Intl.DateTimeFormatOptions,
): string {
  if (!input) return ''
  const date = parseDate(input)
  return date.toLocaleString(SA_LOCALE, {
    timeZone: SA_TIMEZONE,
    hourCycle: 'h23',
    ...options,
  })
}

/**
 * Formats a date in South Africa Standard Time.
 */
export function formatSaDate(
  input: string | Date | number | null | undefined,
  options?: Intl.DateTimeFormatOptions,
): string {
  if (!input) return ''
  const date = parseDate(input)
  return date.toLocaleDateString(SA_LOCALE, {
    timeZone: SA_TIMEZONE,
    ...options,
  })
}

/**
 * Formats a time in South Africa Standard Time (e.g. "09:30").
 */
export function formatSaTime(
  input: string | Date | number | null | undefined,
  options?: Intl.DateTimeFormatOptions,
): string {
  if (!input) return ''
  const date = parseDate(input)
  return date.toLocaleTimeString(SA_LOCALE, {
    timeZone: SA_TIMEZONE,
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
    ...options,
  })
}

/**
 * Converts a clinician-selected local date (YYYY-MM-DD) and time (HH:mm)
 * from South Africa Standard Time (UTC+2) to a UTC ISO string with 'Z'.
 */
export function saTimeToUtcIso(dateStr: string, timeStr = '00:00'): string {
  const [yearStr, monthStr, dayStr] = dateStr.split('-')
  const [hourStr, minStr] = timeStr.split(':')

  const year = parseInt(yearStr, 10)
  const month = parseInt(monthStr, 10) - 1
  const day = parseInt(dayStr, 10)
  const hour = parseInt(hourStr || '0', 10)
  const min = parseInt(minStr || '0', 10)

  // SAST is UTC+2, so UTC is 2 hours earlier
  const utcMillis = Date.UTC(year, month, day, hour - SA_OFFSET_HOURS, min, 0, 0)
  return new Date(utcMillis).toISOString()
}

/**
 * Returns today's date in South Africa formatted as YYYY-MM-DD.
 */
export function getSaTodayDateString(): string {
  const parts = getZonedParts(new Date())
  const y = String(parts.year).padStart(4, '0')
  const m = String(parts.month + 1).padStart(2, '0')
  const d = String(parts.day).padStart(2, '0')
  return `${y}-${m}-${d}`
}

/**
 * Checks if two dates fall on the same calendar day in South Africa Standard Time.
 */
export function isSameSaDay(
  a: string | Date | number | null | undefined,
  b: string | Date | number | null | undefined,
): boolean {
  if (!a || !b) return false
  const partsA = getZonedParts(a)
  const partsB = getZonedParts(b)
  return (
    partsA.year === partsB.year &&
    partsA.month === partsB.month &&
    partsA.day === partsB.day
  )
}

const STARRED_KEY = 'triple-a-starred-threads'

export function loadStarredThreadIds(): number[] {
  try {
    const raw = localStorage.getItem(STARRED_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw) as number[]
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

export function saveStarredThreadIds(ids: number[]) {
  localStorage.setItem(STARRED_KEY, JSON.stringify(ids))
}

export function toggleStarredThread(threadId: number): number[] {
  const current = loadStarredThreadIds()
  const next = current.includes(threadId)
    ? current.filter((id) => id !== threadId)
    : [...current, threadId]
  saveStarredThreadIds(next)
  return next
}

export function formatMessageTime(value: string | null) {
  if (!value) return ''
  const date = new Date(value)
  const now = new Date()
  const isToday =
    date.getDate() === now.getDate() &&
    date.getMonth() === now.getMonth() &&
    date.getFullYear() === now.getFullYear()
  if (isToday) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  }
  return date.toLocaleDateString([], { month: 'short', day: 'numeric' })
}

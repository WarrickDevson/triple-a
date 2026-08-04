export type TaskPriority = 'low' | 'medium' | 'high'
export type TaskFilter = 'all' | 'open' | 'done' | 'overdue'

export interface TaskItem {
  id: number
  label: string
  date: string
  dueDate: string
  done: boolean
  priority: TaskPriority
  petId?: number
  petName?: string
}

const STORAGE_KEY = 'triple-a-tasks'

export const defaultTasks: TaskItem[] = [
  {
    id: 1,
    label: "Update Bella's treatment plan",
    date: 'Today',
    dueDate: new Date().toISOString().slice(0, 10),
    done: false,
    priority: 'high',
    petId: 1,
    petName: 'Bella',
  },
  {
    id: 2,
    label: 'Recheck: Maverick mobility assessment',
    date: 'Tomorrow',
    dueDate: new Date(Date.now() + 86400000).toISOString().slice(0, 10),
    done: false,
    priority: 'medium',
    petId: 2,
    petName: 'Maverick',
  },
  {
    id: 3,
    label: 'Send progress report to owner — Rocky',
    date: 'Thu',
    dueDate: new Date(Date.now() + 3 * 86400000).toISOString().slice(0, 10),
    done: false,
    priority: 'high',
    petId: 3,
    petName: 'Rocky',
  },
  {
    id: 4,
    label: 'Review exercise video submission',
    date: 'Fri',
    dueDate: new Date(Date.now() + 4 * 86400000).toISOString().slice(0, 10),
    done: true,
    priority: 'medium',
  },
  {
    id: 5,
    label: 'Follow up on Whiskers pain score',
    date: 'Mon',
    dueDate: new Date(Date.now() - 86400000).toISOString().slice(0, 10),
    done: false,
    priority: 'high',
    petId: 4,
    petName: 'Whiskers',
  },
  {
    id: 6,
    label: 'Prepare discharge summary for Maverick',
    date: 'Next week',
    dueDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10),
    done: false,
    priority: 'low',
    petId: 2,
    petName: 'Maverick',
  },
]

export function loadTasks(): TaskItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return defaultTasks.map((t) => ({ ...t }))
    const parsed = JSON.parse(raw) as TaskItem[]
    return Array.isArray(parsed) ? parsed : defaultTasks.map((t) => ({ ...t }))
  } catch {
    return defaultTasks.map((t) => ({ ...t }))
  }
}

export function saveTasks(tasks: TaskItem[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(tasks))
}

export function priorityBadgeClass(priority: TaskPriority) {
  if (priority === 'high') return 'status-badge status-badge--at-risk'
  if (priority === 'medium') return 'status-badge status-badge--stable'
  return 'status-badge status-badge--improving'
}

export function isOverdue(task: TaskItem) {
  if (task.done) return false
  const today = new Date().toISOString().slice(0, 10)
  return task.dueDate < today
}

export function filterTasks(tasks: TaskItem[], filter: TaskFilter, query: string) {
  const q = query.trim().toLowerCase()
  return tasks.filter((task) => {
    const matchesSearch =
      !q ||
      task.label.toLowerCase().includes(q) ||
      (task.petName?.toLowerCase().includes(q) ?? false)
    if (!matchesSearch) return false
    if (filter === 'open') return !task.done
    if (filter === 'done') return task.done
    if (filter === 'overdue') return isOverdue(task)
    return true
  })
}

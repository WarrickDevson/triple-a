<script setup lang="ts">
import { computed, ref } from 'vue'
import { Plus } from '@lucide/vue'
import {
  filterTasks,
  isOverdue,
  loadTasks,
  priorityBadgeClass,
  saveTasks,
  type TaskFilter,
  type TaskItem,
} from '../data/taskDemo'

const tasks = ref<TaskItem[]>(loadTasks())
const filter = ref<TaskFilter>('all')
const search = ref('')
const showAddModal = ref(false)
const newTask = ref({ label: '', dueDate: new Date().toISOString().slice(0, 10), priority: 'medium' as const })

const filtered = computed(() => filterTasks(tasks.value, filter.value, search.value))

const summary = computed(() => ({
  open: tasks.value.filter((t) => !t.done).length,
  done: tasks.value.filter((t) => t.done).length,
  overdue: tasks.value.filter((t) => isOverdue(t)).length,
}))

function persist() {
  saveTasks(tasks.value)
}

function toggleTask(id: number) {
  const task = tasks.value.find((t) => t.id === id)
  if (task) {
    task.done = !task.done
    persist()
  }
}

function addTask() {
  if (!newTask.value.label.trim()) return
  const nextId = Math.max(0, ...tasks.value.map((t) => t.id)) + 1
  tasks.value.unshift({
    id: nextId,
    label: newTask.value.label.trim(),
    date: 'Today',
    dueDate: newTask.value.dueDate,
    done: false,
    priority: newTask.value.priority,
  })
  persist()
  showAddModal.value = false
  newTask.value = { label: '', dueDate: new Date().toISOString().slice(0, 10), priority: 'medium' }
}
</script>

<template>
  <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_260px]">
    <section class="portal-card overflow-hidden">
      <div class="border-b border-neutral-grey/80 p-4">
        <div class="flex flex-wrap items-center gap-3">
          <input
            v-model="search"
            type="search"
            placeholder="Search tasks..."
            class="min-w-[200px] flex-1 rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
          />
          <div class="flex gap-1">
            <button
              v-for="tab in ['all', 'open', 'done', 'overdue'] as const"
              :key="tab"
              type="button"
              class="rounded-lg px-3 py-1.5 text-xs font-semibold capitalize transition-colors"
              :class="filter === tab ? 'bg-sage-muted text-navy' : 'text-neutral-muted hover:bg-surface'"
              @click="filter = tab"
            >
              {{ tab }}
            </button>
          </div>
        </div>
      </div>

      <ul class="divide-y divide-neutral-grey/60">
        <li v-if="filtered.length === 0" class="empty-state py-16">
          <p class="text-sm text-neutral-muted">No tasks match your filters.</p>
        </li>
        <li
          v-for="task in filtered"
          :key="task.id"
          class="flex items-start gap-3 px-4 py-3 transition-colors hover:bg-surface"
        >
          <input
            type="checkbox"
            :checked="task.done"
            class="mt-1 h-4 w-4 rounded border-neutral-grey text-sage focus:ring-sage/30"
            @change="toggleTask(task.id)"
          />
          <div class="min-w-0 flex-1">
            <p class="text-sm font-medium" :class="task.done ? 'text-neutral-muted line-through' : 'text-navy'">
              {{ task.label }}
            </p>
            <div class="mt-1 flex flex-wrap items-center gap-2">
              <span class="text-xs text-neutral-muted">{{ task.date }}</span>
              <span v-if="task.petName" class="text-xs text-sage">{{ task.petName }}</span>
              <span :class="priorityBadgeClass(task.priority)">{{ task.priority }}</span>
              <span v-if="isOverdue(task)" class="status-badge status-badge--at-risk">Overdue</span>
            </div>
          </div>
        </li>
      </ul>
    </section>

    <div class="space-y-4">
      <section class="portal-card p-4">
        <h3 class="text-sm font-bold text-navy">Summary</h3>
        <dl class="mt-3 space-y-2 text-sm">
          <div class="flex justify-between">
            <dt class="text-neutral-muted">Open</dt>
            <dd class="font-bold text-navy">{{ summary.open }}</dd>
          </div>
          <div class="flex justify-between">
            <dt class="text-neutral-muted">Done</dt>
            <dd class="font-bold text-navy">{{ summary.done }}</dd>
          </div>
          <div class="flex justify-between">
            <dt class="text-neutral-muted">Overdue</dt>
            <dd class="font-bold text-alert-red">{{ summary.overdue }}</dd>
          </div>
        </dl>
      </section>

      <button
        type="button"
        class="flex w-full items-center justify-center gap-2 rounded-xl border border-dashed border-neutral-grey py-3 text-sm font-semibold text-sage hover:bg-surface"
        @click="showAddModal = true"
      >
        <Plus class="h-4 w-4" />
        Add Task
      </button>
    </div>
  </div>

  <div
    v-if="showAddModal"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
    @click.self="showAddModal = false"
  >
    <form class="portal-card w-full max-w-md p-6" @submit.prevent="addTask">
      <h3 class="text-lg font-bold text-navy">Add Task</h3>
      <label class="mt-4 block">
        <span class="text-sm font-medium text-navy">Task</span>
        <input
          v-model="newTask.label"
          required
          class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
          placeholder="What needs to be done?"
        />
      </label>
      <label class="mt-3 block">
        <span class="text-sm font-medium text-navy">Due date</span>
        <input v-model="newTask.dueDate" type="date" required class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
      </label>
      <label class="mt-3 block">
        <span class="text-sm font-medium text-navy">Priority</span>
        <select v-model="newTask.priority" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm">
          <option value="low">Low</option>
          <option value="medium">Medium</option>
          <option value="high">High</option>
        </select>
      </label>
      <div class="mt-4 flex gap-3">
        <button type="button" class="flex-1 rounded-xl border border-neutral-grey py-2 text-sm font-semibold" @click="showAddModal = false">
          Cancel
        </button>
        <button type="submit" class="flex-1 rounded-xl bg-sage py-2 text-sm font-semibold text-white">
          Add
        </button>
      </div>
    </form>
  </div>
</template>

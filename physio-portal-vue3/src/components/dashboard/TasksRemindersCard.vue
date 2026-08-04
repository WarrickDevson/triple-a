<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ChevronRight } from '@lucide/vue'
import { loadTasks, saveTasks } from '../../data/taskDemo'

const tasks = ref(loadTasks())

onMounted(() => {
  tasks.value = loadTasks()
})

function toggleTask(id: number) {
  const task = tasks.value.find((t) => t.id === id)
  if (task) {
    task.done = !task.done
    saveTasks(tasks.value)
  }
}
</script>

<template>
  <section class="portal-card p-5">
    <div class="portal-card-header">
      <h2 class="portal-card-title">Tasks & Reminders</h2>
    </div>

    <ul class="space-y-2">
      <li
        v-for="task in tasks.filter((t) => !t.done).slice(0, 4)"
        :key="task.id"
        class="flex items-center gap-3 rounded-lg px-2 py-2 transition-colors hover:bg-surface"
      >
        <input
          type="checkbox"
          :checked="task.done"
          class="h-4 w-4 shrink-0 rounded border-neutral-grey text-sage focus:ring-sage/30"
          @change="toggleTask(task.id)"
        />
        <div class="min-w-0 flex-1">
          <p class="text-sm font-medium" :class="task.done ? 'text-neutral-muted line-through' : 'text-navy'">
            {{ task.label }}
          </p>
          <p class="text-xs text-neutral-muted">{{ task.date }}</p>
        </div>
        <ChevronRight class="h-4 w-4 shrink-0 text-neutral-muted" :stroke-width="1.75" />
      </li>
    </ul>

    <RouterLink :to="{ name: 'tasks' }" class="portal-card-link mt-4 inline-block">
      View all tasks →
    </RouterLink>
  </section>
</template>

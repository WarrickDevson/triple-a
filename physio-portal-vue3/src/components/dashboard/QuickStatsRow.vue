<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { Calendar, ClipboardCheck, FileText, MessageSquare } from '@lucide/vue'

defineProps<{
  appointmentsToday: number
}>()

const stats = [
  {
    label: 'Upcoming Appointments',
    value: '4 today',
    route: 'appointments',
    icon: Calendar,
  },
  {
    label: 'New Messages',
    value: '3 unread',
    route: 'messages',
    icon: MessageSquare,
  },
  {
    label: 'Assessments Due',
    value: '5 pending',
    route: 'progress',
    icon: ClipboardCheck,
  },
  {
    label: 'Reports to Finalise',
    value: '2 pending',
    route: 'reports',
    icon: FileText,
  },
]
</script>

<template>
  <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
    <RouterLink
      v-for="stat in stats"
      :key="stat.label"
      :to="{ name: stat.route }"
      class="quick-stat"
    >
      <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-sage-muted text-sage">
        <component :is="stat.icon" class="h-5 w-5" :stroke-width="1.75" />
      </div>
      <div>
        <p class="text-sm font-semibold text-navy">{{ stat.label }}</p>
        <p class="text-xs text-neutral-muted">
          {{ stat.label === 'Upcoming Appointments' ? `${appointmentsToday || 4} today` : stat.value }}
        </p>
      </div>
    </RouterLink>
  </div>
</template>

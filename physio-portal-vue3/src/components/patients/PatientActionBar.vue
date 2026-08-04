<script setup lang="ts">
import {
  ClipboardList,
  Dumbbell,
  FileBarChart,
  MessageSquare,
  Share2,
  Stethoscope,
  Video,
} from '@lucide/vue'
import { RouterLink } from 'vue-router'

const actions = [
  { label: 'Add Assessment', icon: Stethoscope, route: 'treatment-plans' },
  { label: 'Build / Edit Plan', icon: ClipboardList, route: 'treatment-plans' },
  { label: 'Exercise Library', icon: Dumbbell, route: 'exercises' },
  { label: 'Video Review', icon: Video, route: 'progress' },
  { label: 'Reports', icon: FileBarChart, route: 'reports' },
  { label: 'Message Owner', icon: MessageSquare, route: 'messages' },
  { label: 'Share Plan', icon: Share2, route: 'documents' },
]
</script>

<template>
  <div class="mt-6 grid grid-cols-2 gap-2 sm:grid-cols-4 lg:grid-cols-7">
    <RouterLink
      v-for="action in actions"
      :key="action.label"
      :to="{ name: action.route }"
      class="flex flex-col items-center gap-2 rounded-xl border border-neutral-grey/80 bg-surface p-3 text-center transition-colors hover:border-sage/30 hover:bg-sage-muted/30"
    >
      <component :is="action.icon" class="h-5 w-5 text-sage" :stroke-width="1.75" />
      <span class="text-[10px] font-semibold leading-tight text-navy">{{ action.label }}</span>
    </RouterLink>
  </div>
</template>

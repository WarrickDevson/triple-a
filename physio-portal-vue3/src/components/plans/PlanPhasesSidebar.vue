<script setup lang="ts">
import { Plus } from '@lucide/vue'
import type { PlanPhase } from '../../data/planDemo'

defineProps<{
  phases: PlanPhase[]
  activePhaseId: number
}>()

const emit = defineEmits<{
  'update:activePhaseId': [value: number]
  addPhase: []
}>()
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <h2 class="text-sm font-bold text-navy">Plan Phases</h2>
    </div>
    <ul class="flex-1 overflow-y-auto p-2">
      <li v-for="phase in phases" :key="phase.id">
        <button
          type="button"
          class="w-full rounded-lg px-3 py-3 text-left transition-colors"
          :class="
            activePhaseId === phase.id
              ? 'bg-sage-muted font-semibold text-navy'
              : 'text-neutral-muted hover:bg-surface hover:text-navy'
          "
          @click="emit('update:activePhaseId', phase.id)"
        >
          <p class="text-[10px] font-semibold uppercase tracking-wide text-sage">{{ phase.label }}</p>
          <p class="mt-0.5 text-sm">{{ phase.title }}</p>
        </button>
      </li>
    </ul>
    <div class="border-t border-neutral-grey/80 p-4">
      <button
        type="button"
        class="flex w-full items-center justify-center gap-2 rounded-lg border border-dashed border-neutral-grey py-2 text-sm font-semibold text-sage hover:bg-surface"
        @click="emit('addPhase')"
      >
        <Plus class="h-4 w-4" />
        Add Phase
      </button>
    </div>
  </section>
</template>

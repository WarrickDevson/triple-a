<script setup lang="ts">
import { computed } from 'vue'
import { Plus } from '@lucide/vue'
import { getCategoryLabel } from '../../data/exerciseDemo'
import type { Exercise } from '../../types/exercise'

const props = defineProps<{
  exercises: Exercise[]
  selectedCategory: string
}>()

const emit = defineEmits<{
  'update:selectedCategory': [value: string]
  newExercise: []
}>()

const categories = computed(() => {
  const counts = new Map<string, number>()
  for (const exercise of props.exercises) {
    const label = getCategoryLabel(exercise)
    counts.set(label, (counts.get(label) ?? 0) + 1)
  }
  const items = Array.from(counts.entries())
    .map(([label, count]) => ({ label, count }))
    .sort((a, b) => a.label.localeCompare(b.label))
  return [{ label: 'All Categories', count: props.exercises.length }, ...items]
})
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <h2 class="text-sm font-bold text-navy">Categories</h2>
    </div>
    <ul class="flex-1 overflow-y-auto p-2">
      <li v-for="cat in categories" :key="cat.label">
        <button
          type="button"
          class="flex w-full items-center justify-between rounded-lg px-3 py-2 text-left text-sm transition-colors"
          :class="
            selectedCategory === cat.label
              ? 'bg-sage-muted font-semibold text-navy'
              : 'text-neutral-muted hover:bg-surface hover:text-navy'
          "
          @click="emit('update:selectedCategory', cat.label)"
        >
          <span>{{ cat.label }}</span>
          <span class="text-xs">{{ cat.count }}</span>
        </button>
      </li>
    </ul>
    <div class="border-t border-neutral-grey/80 p-4">
      <p class="text-xs text-neutral-muted">
        Build your own exercise with photos, videos and instructions.
      </p>
      <button
        type="button"
        class="mt-3 flex w-full items-center justify-center gap-2 rounded-lg bg-sage px-3 py-2 text-sm font-semibold text-white transition-colors hover:bg-sage-light"
        @click="emit('newExercise')"
      >
        <Plus class="h-4 w-4" :stroke-width="2" />
        New Exercise
      </button>
    </div>
  </section>
</template>

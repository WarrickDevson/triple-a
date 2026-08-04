<script setup lang="ts">
const tabs = [
  { id: 'all', label: 'All Exercises' },
  { id: 'favourites', label: 'Favourites' },
  { id: 'region', label: 'By Body Region' },
  { id: 'templates', label: 'Templates' },
] as const

defineProps<{
  activeTab: (typeof tabs)[number]['id']
}>()

const emit = defineEmits<{
  'update:activeTab': [value: (typeof tabs)[number]['id']]
}>()
</script>

<template>
  <div class="flex gap-1 overflow-x-auto border-b border-neutral-grey/80">
    <button
      v-for="tab in tabs"
      :key="tab.id"
      type="button"
      class="shrink-0 px-4 py-2.5 text-sm font-semibold transition-colors"
      :class="
        activeTab === tab.id
          ? 'border-b-2 border-sage text-navy'
          : 'text-neutral-muted hover:text-navy'
      "
      @click="emit('update:activeTab', tab.id)"
    >
      {{ tab.label }}
    </button>
  </div>
</template>

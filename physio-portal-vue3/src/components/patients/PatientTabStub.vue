<script setup lang="ts">
defineProps<{
  title: string
  description?: string
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center py-16 text-center">
    <p class="text-base font-semibold text-navy">{{ title }}</p>
    <p v-if="description" class="mt-2 max-w-sm text-sm text-neutral-muted">{{ description }}</p>
    <p class="mt-4 text-xs font-medium uppercase tracking-wider text-sage">Coming soon</p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { Construction } from '@lucide/vue'

const route = useRoute()

const pageTitle = computed(() => (route.meta.title as string) ?? 'Page')
const pageDescription = computed(
  () =>
    (route.meta.description as string) ??
    'This section is being prepared. Check back soon for the full experience.',
)
</script>

<template>
  <section class="portal-card mx-auto max-w-2xl p-10 text-center">
    <div class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-sage-muted text-sage">
      <Construction class="h-8 w-8" :stroke-width="1.5" />
    </div>
    <h2 class="text-xl font-bold text-navy">{{ pageTitle }}</h2>
    <p class="mt-3 text-sm leading-relaxed text-neutral-muted">{{ pageDescription }}</p>
    <p class="mt-6 text-xs font-medium uppercase tracking-wider text-sage">Coming soon</p>
  </section>
</template>

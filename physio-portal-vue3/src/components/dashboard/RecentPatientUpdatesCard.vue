<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { demoPatientUpdates } from '../../data/dashboardDemo'

function badgeClass(status: string) {
  if (status === 'improving') return 'status-badge status-badge--improving'
  if (status === 'at-risk') return 'status-badge status-badge--at-risk'
  return 'status-badge status-badge--stable'
}

function badgeLabel(status: string) {
  if (status === 'improving') return 'Improving'
  if (status === 'at-risk') return 'At Risk'
  return 'Stable'
}
</script>

<template>
  <section class="portal-card p-5">
    <div class="portal-card-header">
      <h2 class="portal-card-title">Recent Patient Updates</h2>
    </div>

    <ul class="space-y-4">
      <li
        v-for="update in demoPatientUpdates"
        :key="update.id"
        class="flex items-start gap-3 border-b border-neutral-grey/60 pb-4 last:border-0 last:pb-0"
      >
        <div
          class="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-sage-muted text-xs font-bold text-sage"
        >
          {{ update.initials }}
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex flex-wrap items-center gap-2">
            <p class="text-sm font-semibold text-navy">{{ update.name }}</p>
            <span class="text-xs text-neutral-muted">{{ update.species }} · {{ update.age }}</span>
          </div>
          <p class="mt-0.5 text-sm text-neutral-muted">{{ update.note }}</p>
          <div class="mt-2 flex flex-wrap items-center gap-2">
            <span :class="badgeClass(update.status)">{{ badgeLabel(update.status) }}</span>
            <span class="text-xs text-neutral-muted">{{ update.timeAgo }}</span>
          </div>
        </div>
      </li>
    </ul>

    <RouterLink :to="{ name: 'patients' }" class="portal-card-link mt-4 inline-block">
      View all updates →
    </RouterLink>
  </section>
</template>

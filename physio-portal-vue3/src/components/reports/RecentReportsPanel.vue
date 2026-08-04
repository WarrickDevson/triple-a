<script setup lang="ts">
import { reportStatusClass, type ReportHistoryItem } from '../../data/reportsDemo'

defineProps<{
  reports: ReportHistoryItem[]
}>()
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <h2 class="text-sm font-bold text-navy">Recent Reports</h2>
    </div>
    <ul class="flex-1 overflow-y-auto">
      <li
        v-for="report in reports"
        :key="report.id"
        class="border-b border-neutral-grey/60 px-4 py-3"
      >
        <div class="flex items-start justify-between gap-2">
          <div class="min-w-0">
            <p class="truncate text-sm font-semibold text-navy">{{ report.reportType }}</p>
            <p class="text-xs text-neutral-muted">{{ report.petName }} · {{ report.ownerName }}</p>
          </div>
          <span :class="reportStatusClass(report.status)">{{ report.status }}</span>
        </div>
        <p class="mt-1 text-[10px] text-neutral-muted">
          {{ new Date(report.generatedAt).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) }}
        </p>
      </li>
    </ul>
  </section>
</template>

<script setup lang="ts">
import { FileDown, FileText, Sparkles, Activity, FileCheck2, ClipboardList } from '@lucide/vue'
import { REPORT_TYPES } from '../../data/reportsDemo'
import type { Pet } from '../../types/pet'

defineProps<{
  patient: Pet | null
  downloading?: boolean
}>()

const emit = defineEmits<{
  customize: [typeId: string]
  quickDownload: [typeId: string]
  openNew: []
}>()

function getIcon(id: string) {
  switch (id) {
    case 'discharge':
      return FileCheck2
    case 'home-program':
      return Activity
    case 'soap':
      return ClipboardList
    default:
      return FileText
  }
}
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden p-4">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-sm font-bold text-navy flex items-center gap-1.5">
          <Sparkles class="h-4 w-4 text-sage" />
          Generate Report Document
        </h2>
        <p v-if="patient" class="mt-0.5 text-xs text-neutral-muted">
          For <strong class="text-navy font-semibold">{{ patient.petName }}</strong> ({{ patient.ownerName }}) · {{ patient.species }}{{ patient.breed ? ` (${patient.breed})` : '' }}
        </p>
        <p v-else class="mt-0.5 text-xs text-neutral-muted">
          Select a patient on the left to generate or customize a clinical report.
        </p>
      </div>

      <button
        type="button"
        class="hidden sm:inline-flex items-center gap-1.5 rounded-lg bg-sage px-3 py-1.5 text-xs font-bold text-white shadow-sm transition-all hover:bg-sage/90"
        :disabled="!patient"
        @click="emit('openNew')"
      >
        <Sparkles class="h-3.5 w-3.5" />
        + Create New Report
      </button>
    </div>

    <!-- Report Type Cards Grid -->
    <div class="mt-4 grid gap-3 sm:grid-cols-2">
      <div
        v-for="report in REPORT_TYPES"
        :key="report.id"
        class="rounded-xl border border-neutral-grey/80 bg-white p-4 flex flex-col justify-between transition-all hover:border-sage hover:shadow-sm"
      >
        <div>
          <div class="flex items-start justify-between gap-2">
            <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage">
              <component :is="getIcon(report.id)" class="h-5 w-5" :stroke-width="1.75" />
            </div>
            <span class="rounded bg-surface px-2 py-0.5 text-[10px] font-bold text-neutral-muted border border-neutral-grey/60">
              {{ report.badge }}
            </span>
          </div>

          <p class="mt-3 text-sm font-bold text-navy">{{ report.label }}</p>
          <p class="mt-1 text-xs text-neutral-muted leading-relaxed">{{ report.description }}</p>
        </div>

        <!-- Card Action Buttons -->
        <div class="mt-4 flex items-center justify-between border-t border-neutral-grey/40 pt-3 text-xs">
          <button
            type="button"
            class="font-bold text-sage hover:text-navy inline-flex items-center gap-1 transition-colors disabled:opacity-50"
            :disabled="!patient || downloading"
            @click="emit('customize', report.id)"
          >
            <Sparkles class="h-3.5 w-3.5" />
            Customize & Generate
          </button>

          <button
            type="button"
            class="inline-flex items-center gap-1 rounded-md border border-neutral-grey/80 bg-surface px-2 py-1 text-[11px] font-semibold text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy transition-colors disabled:opacity-50"
            :disabled="!patient || downloading"
            @click="emit('quickDownload', report.id)"
          >
            <FileDown class="h-3 w-3" />
            Quick PDF
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

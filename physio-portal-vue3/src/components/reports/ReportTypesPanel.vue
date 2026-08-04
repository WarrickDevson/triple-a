<script setup lang="ts">
import { FileDown, FileText } from '@lucide/vue'
import { REPORT_TYPES } from '../../data/reportsDemo'
import type { Pet } from '../../types/pet'

defineProps<{
  patient: Pet | null
  downloading?: boolean
}>()

const emit = defineEmits<{
  download: []
  stub: [message: string]
}>()
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden p-4">
    <h2 class="text-sm font-bold text-navy">Generate Report</h2>
    <p v-if="patient" class="mt-1 text-xs text-neutral-muted">
      For {{ patient.petName }} ({{ patient.ownerName }})
    </p>
    <p v-else class="mt-1 text-xs text-neutral-muted">Select a patient to generate a report.</p>

    <div class="mt-4 space-y-3">
      <div
        v-for="report in REPORT_TYPES"
        :key="report.id"
        class="rounded-xl border border-neutral-grey/80 p-4 transition-colors hover:bg-surface"
      >
        <div class="flex items-start gap-3">
          <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-sage-muted text-sage">
            <FileText class="h-5 w-5" :stroke-width="1.75" />
          </div>
          <div class="min-w-0 flex-1">
            <p class="text-sm font-semibold text-navy">{{ report.label }}</p>
            <p class="mt-0.5 text-xs text-neutral-muted">{{ report.description }}</p>
            <button
              type="button"
              class="mt-3 inline-flex items-center gap-1.5 text-xs font-semibold text-sage hover:text-navy disabled:opacity-50"
              :disabled="!patient || (report.available && downloading)"
              @click="
                report.available
                  ? emit('download')
                  : emit('stub', `${report.label} coming soon.`)
              "
            >
              <FileDown class="h-3.5 w-3.5" />
              {{ report.available ? (downloading ? 'Generating...' : 'Download PDF') : 'Coming soon' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

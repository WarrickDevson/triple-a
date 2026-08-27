<script setup lang="ts">
import { ref, computed } from 'vue'
import { FileText, Download, Eye, Search, Sparkles } from '@lucide/vue'
import type { SharedReport } from '../../types/soap'
import { formatReportType, reportStatusClass } from '../../data/reportsDemo'

const props = defineProps<{
  reports: SharedReport[]
  selectedPetId?: number | null
  loading?: boolean
}>()

const emit = defineEmits<{
  view: [report: SharedReport]
  download: [report: SharedReport]
  createNew: []
}>()

const activeTab = ref<'all' | 'selected'>('all')
const searchQuery = ref('')

const filteredReports = computed(() => {
  let list = props.reports

  if (activeTab.value === 'selected' && props.selectedPetId) {
    list = list.filter((r) => r.petId === props.selectedPetId)
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(
      (r) =>
        r.title.toLowerCase().includes(q) ||
        (r.petName && r.petName.toLowerCase().includes(q)) ||
        (r.summary && r.summary.toLowerCase().includes(q)) ||
        formatReportType(r.reportType).toLowerCase().includes(q)
    )
  }

  return list
})
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <!-- Header -->
    <div class="border-b border-neutral-grey/80 p-4 space-y-3">
      <div class="flex items-center justify-between">
        <h2 class="text-sm font-bold text-navy flex items-center gap-1.5">
          <FileText class="h-4 w-4 text-sage" />
          Existing Reports & Summaries
        </h2>
        <span class="rounded-full bg-surface px-2 py-0.5 text-[10px] font-bold text-neutral-muted">
          {{ filteredReports.length }}
        </span>
      </div>

      <!-- Tabs: All Reports vs Selected Patient -->
      <div class="flex rounded-lg bg-surface p-1 text-xs border border-neutral-grey/60">
        <button
          type="button"
          class="flex-1 rounded-md py-1 font-semibold transition-all"
          :class="
            activeTab === 'all'
              ? 'bg-white text-navy shadow-sm'
              : 'text-neutral-muted hover:text-navy'
          "
          @click="activeTab = 'all'"
        >
          All Clinic ({{ reports.length }})
        </button>
        <button
          type="button"
          class="flex-1 rounded-md py-1 font-semibold transition-all"
          :class="
            activeTab === 'selected'
              ? 'bg-white text-navy shadow-sm'
              : 'text-neutral-muted hover:text-navy'
          "
          @click="activeTab = 'selected'"
        >
          Selected Patient
        </button>
      </div>

      <!-- Search Filter -->
      <div class="relative">
        <Search class="pointer-events-none absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-neutral-muted" />
        <input
          v-model="searchQuery"
          type="search"
          placeholder="Filter reports by title, patient, notes..."
          class="w-full rounded-md border border-neutral-grey bg-surface py-1.5 pl-8 pr-2.5 text-xs outline-none focus:border-sage"
        />
      </div>
    </div>

    <!-- Reports List -->
    <div v-if="loading" class="flex-1 p-6 text-center text-xs text-neutral-muted">
      Loading clinic reports...
    </div>

    <ul v-else-if="filteredReports.length > 0" class="flex-1 overflow-y-auto divide-y divide-neutral-grey/60">
      <li
        v-for="report in filteredReports"
        :key="report.sharedReportId"
        class="group p-3.5 hover:bg-surface/70 transition-colors cursor-pointer"
        @click="emit('view', report)"
      >
        <div class="flex items-start justify-between gap-2">
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-1.5">
              <span class="rounded bg-navy/5 px-1.5 py-0.5 text-[10px] font-bold text-navy">
                {{ formatReportType(report.reportType) }}
              </span>
              <span :class="reportStatusClass(report.isActive !== false ? 'Sent' : 'Draft')">
                {{ report.isActive !== false ? 'Shared' : 'Draft' }}
              </span>
            </div>
            <p class="mt-1 truncate text-xs font-bold text-navy group-hover:text-sage transition-colors">
              {{ report.title }}
            </p>
            <p class="text-[11px] text-neutral-muted truncate mt-0.5">
              Patient: <strong class="text-navy font-semibold">{{ report.petName || 'Patient' }}</strong>
              <span v-if="report.ownerName"> ({{ report.ownerName }})</span>
            </p>
          </div>
        </div>

        <p v-if="report.summary" class="mt-1.5 text-[11px] text-neutral-muted line-clamp-2 leading-snug bg-surface/40 rounded p-1.5 border border-neutral-grey/40">
          {{ report.summary }}
        </p>

        <div class="mt-2.5 flex items-center justify-between text-[10px] text-neutral-muted border-t border-neutral-grey/40 pt-2">
          <span>
            {{ new Date(report.sharedAtUtc).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) }}
          </span>

          <div class="flex items-center gap-2 font-semibold">
            <button
              type="button"
              class="inline-flex items-center gap-1 text-sage hover:text-navy transition-colors"
              @click.stop="emit('view', report)"
            >
              <Eye class="h-3 w-3" />
              View Summary
            </button>
            <button
              type="button"
              class="inline-flex items-center gap-1 text-sage hover:text-navy transition-colors"
              @click.stop="emit('download', report)"
            >
              <Download class="h-3 w-3" />
              PDF
            </button>
          </div>
        </div>
      </li>
    </ul>

    <!-- Empty State -->
    <div v-else class="flex-1 flex flex-col items-center justify-center p-6 text-center">
      <FileText class="h-8 w-8 text-neutral-muted/50 mb-2" />
      <p class="text-xs font-semibold text-navy">No reports found</p>
      <p class="text-[11px] text-neutral-muted mt-0.5">
        {{ searchQuery ? 'Try adjusting your search query.' : 'Generate a report using the templates.' }}
      </p>
      <button
        type="button"
        class="mt-3 text-xs font-semibold text-sage hover:text-navy inline-flex items-center gap-1"
        @click="emit('createNew')"
      >
        <Sparkles class="h-3.5 w-3.5" />
        Generate Report Now
      </button>
    </div>
  </section>
</template>

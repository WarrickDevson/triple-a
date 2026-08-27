<script setup lang="ts">
import { ref, computed } from 'vue'
import {
  X,
  FileText,
  Download,
  Share2,
  Trash2,
  Calendar,
  User,
  CheckCircle2,
  Clock,
  ShieldCheck,
  AlertCircle,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { SharedReport } from '../../types/soap'
import { formatReportType, reportStatusClass } from '../../data/reportsDemo'

const props = defineProps<{
  report: SharedReport | null
  downloading?: boolean
}>()

const emit = defineEmits<{
  close: []
  download: [report: SharedReport]
  toggleShare: [report: SharedReport]
  delete: [reportId: number]
}>()

const showConfirmDelete = ref(false)

const formattedType = computed(() => {
  if (!props.report) return 'Progress Report'
  return formatReportType(props.report.reportType)
})

const isShared = computed(() => {
  if (!props.report) return false
  return props.report.isActive !== false
})
</script>

<template>
  <div
    v-if="report"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden shadow-2xl animate-in fade-in zoom-in-95 duration-150">
      <!-- Modal Header -->
      <div class="flex items-start justify-between border-b border-neutral-grey/80 p-5">
        <div class="flex items-start gap-3">
          <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage">
            <FileText class="h-6 w-6" :stroke-width="1.75" />
          </div>
          <div>
            <div class="flex flex-wrap items-center gap-2">
              <span class="rounded-md bg-navy/5 px-2 py-0.5 text-xs font-bold text-navy">
                {{ formattedType }}
              </span>
              <span :class="reportStatusClass(isShared ? 'Sent' : 'Draft')">
                {{ isShared ? 'Shared with Owner' : 'Internal Clinical Record' }}
              </span>
            </div>
            <h3 class="mt-1 text-base font-bold text-navy">{{ report.title }}</h3>
            <p class="text-xs text-neutral-muted">
              Generated for <strong class="text-navy">{{ report.petName || 'Patient' }}</strong>
              <span v-if="report.ownerName"> ({{ report.ownerName }})</span>
            </p>
          </div>
        </div>
        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted transition-colors hover:bg-surface hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Modal Body (Scrollable) -->
      <div class="flex-1 overflow-y-auto p-6 space-y-5">
        <!-- Metadata grid -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 rounded-xl bg-surface p-3.5 border border-neutral-grey/60 text-xs">
          <div>
            <p class="text-[11px] font-semibold text-neutral-muted flex items-center gap-1">
              <Calendar class="h-3.5 w-3.5 text-sage" /> Date
            </p>
            <p class="mt-0.5 font-bold text-navy">
              {{ new Date(report.sharedAtUtc).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) }}
            </p>
          </div>
          <div>
            <p class="text-[11px] font-semibold text-neutral-muted flex items-center gap-1">
              <User class="h-3.5 w-3.5 text-sage" /> Clinician
            </p>
            <p class="mt-0.5 font-bold text-navy truncate">
              {{ report.sharedByPhysioName || 'Dr. S. Devson' }}
            </p>
          </div>
          <div>
            <p class="text-[11px] font-semibold text-neutral-muted flex items-center gap-1">
              <ShieldCheck class="h-3.5 w-3.5 text-sage" /> Species / Breed
            </p>
            <p class="mt-0.5 font-bold text-navy truncate">
              {{ report.species || 'Canine' }}{{ report.breed ? ` · ${report.breed}` : '' }}
            </p>
          </div>
          <div>
            <p class="text-[11px] font-semibold text-neutral-muted flex items-center gap-1">
              <Clock class="h-3.5 w-3.5 text-sage" /> Record ID
            </p>
            <p class="mt-0.5 font-bold text-navy">
              #REP-{{ report.sharedReportId }}
            </p>
          </div>
        </div>

        <!-- Care Period Banner if present -->
        <div v-if="report.periodFrom || report.periodTo" class="rounded-xl border border-sage/30 bg-sage-muted/30 px-3.5 py-2 flex items-center justify-between text-xs">
          <span class="font-bold text-navy flex items-center gap-1.5">
            <Calendar class="h-3.5 w-3.5 text-sage" />
            Treatment Period Covered:
          </span>
          <span class="font-semibold text-sage">
            {{ report.periodFrom ? new Date(report.periodFrom).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) : 'Initial' }}
            –
            {{ report.periodTo ? new Date(report.periodTo).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) : 'Current' }}
          </span>
        </div>

        <!-- Referenced Sessions Table if present -->
        <div v-if="report.referencedSessions && report.referencedSessions.length > 0" class="space-y-2">
          <h4 class="text-xs font-bold uppercase tracking-wider text-navy flex items-center gap-1.5">
            <Clock class="h-3.5 w-3.5 text-sage" />
            Referenced Clinical Sessions ({{ report.referencedSessions.length }})
          </h4>
          <div class="rounded-xl border border-neutral-grey/80 overflow-hidden text-xs">
            <table class="w-full text-left border-collapse">
              <thead class="bg-surface border-b border-neutral-grey/80 text-[11px] font-bold text-neutral-muted">
                <tr>
                  <th class="p-2.5">Date</th>
                  <th class="p-2.5">Session Type</th>
                  <th class="p-2.5">Clinical Notes</th>
                  <th class="p-2.5">Clinician Comment</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-neutral-grey/60">
                <tr v-for="(sess, idx) in report.referencedSessions" :key="idx" class="hover:bg-surface/50">
                  <td class="p-2.5 font-bold text-navy whitespace-nowrap">
                    {{ new Date(sess.date).toLocaleDateString([], { day: 'numeric', month: 'short', year: 'numeric' }) }}
                  </td>
                  <td class="p-2.5 font-semibold text-sage">
                    {{ sess.sessionType }}
                  </td>
                  <td class="p-2.5 text-neutral-muted">
                    {{ sess.sessionNotes || '—' }}
                  </td>
                  <td class="p-2.5 italic text-navy font-medium">
                    {{ sess.clinicianComment || '—' }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Clinical Summary & Findings -->
        <div class="space-y-2">
          <h4 class="text-xs font-bold uppercase tracking-wider text-navy flex items-center gap-1.5">
            Clinical Summary & Recommendations
          </h4>
          <div class="rounded-xl border border-neutral-grey/80 bg-surface/50 p-4 text-xs text-navy leading-relaxed whitespace-pre-line">
            {{ report.summary || 'No narrative summary entered for this report.' }}
          </div>
        </div>

        <!-- Report Status & Portal Info -->
        <div class="rounded-xl border border-sage/20 bg-sage-muted/30 p-4 flex items-start gap-3">
          <CheckCircle2 class="h-5 w-5 text-sage shrink-0 mt-0.5" />
          <div class="text-xs">
            <p class="font-bold text-navy">Owner Portal Access</p>
            <p class="mt-0.5 text-neutral-muted">
              {{ isShared ? 'This document is visible in the Pet Owner Mobile App under Saved Clinical Reports.' : 'This report is stored as an internal physiotherapy record and is not visible to the pet owner.' }}
            </p>
          </div>
        </div>

        <!-- Delete Confirmation Alert if active -->
        <div v-if="showConfirmDelete" class="rounded-xl border border-red-200 bg-red-50 p-4 text-xs text-red-800 space-y-2">
          <div class="flex items-center gap-2 font-bold">
            <AlertCircle class="h-4 w-4 text-red-600" />
            Are you sure you want to delete this report?
          </div>
          <p class="text-neutral-muted">
            This will remove the report record from the clinic portal and owner app. This action cannot be undone.
          </p>
          <div class="flex justify-end gap-2 pt-1">
            <BaseButton size="sm" variant="secondary" @click="showConfirmDelete = false">
              Cancel
            </BaseButton>
            <BaseButton size="sm" variant="danger" @click="emit('delete', report.sharedReportId)">
              Confirm Delete
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Modal Footer -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-neutral-grey/80 bg-surface/50 p-4">
        <div>
          <button
            v-if="!showConfirmDelete"
            type="button"
            class="inline-flex items-center gap-1.5 text-xs font-semibold text-red-600 hover:text-red-800 transition-colors"
            @click="showConfirmDelete = true"
          >
            <Trash2 class="h-3.5 w-3.5" />
            Delete Report
          </button>
        </div>

        <div class="flex items-center gap-2">
          <BaseButton size="sm" variant="secondary" @click="emit('close')">
            Close
          </BaseButton>
          <BaseButton
            size="sm"
            variant="secondary"
            @click="emit('toggleShare', report)"
          >
            <Share2 class="h-3.5 w-3.5" />
            {{ isShared ? 'Shared with Owner' : 'Share with Owner' }}
          </BaseButton>
          <BaseButton
            size="sm"
            :loading="downloading"
            @click="emit('download', report)"
          >
            <Download class="h-3.5 w-3.5" />
            Download PDF
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

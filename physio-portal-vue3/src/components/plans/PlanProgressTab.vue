<script setup lang="ts">
import { ref } from 'vue'
import { Activity, Award, TrendingUp, CheckCircle, Calendar } from '@lucide/vue'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patient: Pet
}>()

// Sample progress metrics
const complianceRate = ref(88)
const completedSessions = ref(14)
const totalAssignedSessions = ref(16)

const recentLogs = [
  { id: 1, date: '2026-08-12', painScore: 2, mobilityScore: 8, energyScore: 9, isCompleted: true, note: 'Walked very well on leash. Minimal stiffness after exercises.' },
  { id: 2, date: '2026-08-11', painScore: 3, mobilityScore: 7, energyScore: 8, isCompleted: true, note: 'Completed all 3 sets of passive range of motion and sit-to-stands.' },
  { id: 3, date: '2026-08-10', painScore: 3, mobilityScore: 7, energyScore: 8, isCompleted: true, note: 'Good engagement during morning session.' },
  { id: 4, date: '2026-08-09', painScore: 4, mobilityScore: 6, energyScore: 7, isCompleted: false, note: 'Rest day requested due to mild stiffness after long walk.' },
]
</script>

<template>
  <div class="p-5 space-y-6">
    <!-- Stat Highlights -->
    <div class="grid gap-4 sm:grid-cols-4">
      <div class="portal-card p-4">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Compliance Rate</p>
          <Award class="h-5 w-5 text-sage" />
        </div>
        <p class="text-2xl font-extrabold text-navy mt-1">{{ complianceRate }}%</p>
        <div class="w-full bg-neutral-grey/60 h-1.5 rounded-full mt-2 overflow-hidden">
          <div class="bg-sage h-full rounded-full" :style="{ width: `${complianceRate}%` }"></div>
        </div>
      </div>

      <div class="portal-card p-4">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Completed Sessions</p>
          <CheckCircle class="h-5 w-5 text-emerald-600" />
        </div>
        <p class="text-2xl font-extrabold text-navy mt-1">{{ completedSessions }} / {{ totalAssignedSessions }}</p>
        <p class="text-xs text-neutral-muted mt-1">Sessions logged</p>
      </div>

      <div class="portal-card p-4">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Avg Pain Level</p>
          <TrendingUp class="h-5 w-5 text-amber-600" />
        </div>
        <p class="text-2xl font-extrabold text-navy mt-1">2.5 <span class="text-xs font-normal text-neutral-muted">/ 10</span></p>
        <p class="text-xs text-emerald-700 font-semibold mt-1">↓ 45% reduction from baseline</p>
      </div>

      <div class="portal-card p-4">
        <div class="flex items-center justify-between">
          <p class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Mobility Score</p>
          <Activity class="h-5 w-5 text-blue-600" />
        </div>
        <p class="text-2xl font-extrabold text-navy mt-1">7.8 <span class="text-xs font-normal text-neutral-muted">/ 10</span></p>
        <p class="text-xs text-emerald-700 font-semibold mt-1">↑ Steady improvement</p>
      </div>
    </div>

    <!-- Session Log History -->
    <div class="portal-card overflow-hidden">
      <div class="flex items-center justify-between border-b border-neutral-grey/60 px-4 py-3 bg-surface/50">
        <h3 class="text-sm font-bold text-navy">Recent Owner Session Submissions</h3>
        <span class="text-xs font-semibold text-neutral-muted">Logged via Triple A App</span>
      </div>

      <div class="divide-y divide-neutral-grey/40">
        <div
          v-for="log in recentLogs"
          :key="log.id"
          class="p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3 hover:bg-surface/60 transition-colors"
        >
          <div class="space-y-1">
            <div class="flex items-center gap-2">
              <span class="flex items-center gap-1 text-xs font-bold text-navy">
                <Calendar class="h-3.5 w-3.5 text-sage" />
                {{ log.date }}
              </span>
              <span
                class="rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider"
                :class="log.isCompleted ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'"
              >
                {{ log.isCompleted ? 'Session Completed' : 'Session Skipped' }}
              </span>
            </div>
            <p class="text-xs text-neutral-muted italic">"{{ log.note }}"</p>
          </div>

          <div class="flex items-center gap-4 text-xs font-semibold shrink-0">
            <div class="text-center px-2 py-1 bg-surface rounded-lg">
              <span class="text-[10px] text-neutral-muted block uppercase">Pain</span>
              <span class="text-navy font-bold">{{ log.painScore }}/10</span>
            </div>
            <div class="text-center px-2 py-1 bg-surface rounded-lg">
              <span class="text-[10px] text-neutral-muted block uppercase">Mobility</span>
              <span class="text-navy font-bold">{{ log.mobilityScore }}/10</span>
            </div>
            <div class="text-center px-2 py-1 bg-surface rounded-lg">
              <span class="text-[10px] text-neutral-muted block uppercase">Energy</span>
              <span class="text-navy font-bold">{{ log.energyScore }}/10</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

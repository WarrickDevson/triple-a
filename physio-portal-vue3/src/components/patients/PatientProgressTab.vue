<script setup lang="ts">
import { Activity, Award, TrendingUp, CheckCircle } from '@lucide/vue'
import type { Pet } from '../../types/pet'

defineProps<{
  patient: Pet
  progressPercent: number
}>()
</script>

<template>
  <div class="space-y-4">
    <!-- Highlights -->
    <div class="grid gap-3 sm:grid-cols-3">
      <div class="rounded-xl border border-neutral-grey/60 bg-surface p-3.5">
        <div class="flex items-center justify-between">
          <span class="text-[10px] font-bold uppercase tracking-wider text-neutral-muted">Compliance Score</span>
          <Award class="h-4 w-4 text-sage" />
        </div>
        <p class="text-xl font-extrabold text-navy mt-1">{{ progressPercent }}%</p>
        <p class="text-[11px] text-emerald-700 font-semibold mt-0.5">Optimal owner engagement</p>
      </div>

      <div class="rounded-xl border border-neutral-grey/60 bg-surface p-3.5">
        <div class="flex items-center justify-between">
          <span class="text-[10px] font-bold uppercase tracking-wider text-neutral-muted">Pain Reduction</span>
          <TrendingUp class="h-4 w-4 text-amber-600" />
        </div>
        <p class="text-xl font-extrabold text-navy mt-1">2.4 / 10</p>
        <p class="text-[11px] text-emerald-700 font-semibold mt-0.5">↓ 60% down from initial baseline</p>
      </div>

      <div class="rounded-xl border border-neutral-grey/60 bg-surface p-3.5">
        <div class="flex items-center justify-between">
          <span class="text-[10px] font-bold uppercase tracking-wider text-neutral-muted">Mobility Score</span>
          <Activity class="h-4 w-4 text-blue-600" />
        </div>
        <p class="text-xl font-extrabold text-navy mt-1">8.2 / 10</p>
        <p class="text-[11px] text-emerald-700 font-semibold mt-0.5">↑ Weight bearing improving</p>
      </div>
    </div>

    <!-- Milestones Summary -->
    <div class="portal-card p-4">
      <h4 class="text-xs font-bold uppercase tracking-wider text-navy mb-3">Rehabilitation Milestones</h4>
      <div class="space-y-2.5">
        <div class="flex items-center justify-between text-xs">
          <span class="flex items-center gap-2 font-medium text-navy">
            <CheckCircle class="h-4 w-4 text-emerald-600" />
            Phase 1: Acute Pain & Inflammation Control
          </span>
          <span class="rounded bg-emerald-100 px-2 py-0.5 text-[10px] font-bold text-emerald-800">Achieved</span>
        </div>
        <div class="flex items-center justify-between text-xs">
          <span class="flex items-center gap-2 font-medium text-navy">
            <Activity class="h-4 w-4 text-sage" />
            Phase 2: Progressive Range of Motion & Muscle Loading
          </span>
          <span class="rounded bg-sage-muted px-2 py-0.5 text-[10px] font-bold text-sage">Active Phase</span>
        </div>
        <div class="flex items-center justify-between text-xs text-neutral-muted">
          <span class="flex items-center gap-2 font-medium">
            <span class="h-4 w-4 rounded-full border border-neutral-grey shrink-0" />
            Phase 3: Proprioception & Neuromuscular Retraining
          </span>
          <span class="rounded bg-surface px-2 py-0.5 text-[10px] font-bold">Upcoming</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import {
  Calendar,
  ClipboardList,
  FileText,
  Video,
} from '@lucide/vue'
import type { MessageThread } from '../../types/message'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  thread: MessageThread | null
  patient: Pet | null
}>()

const emit = defineEmits<{
  comingSoon: [message: string]
}>()

const quickActions = computed(() => {
  const petId = props.thread?.petId ?? props.patient?.petId ?? null
  return [
    { label: 'Update Plan', icon: ClipboardList, route: petId ? 'treatment-plan-detail' : 'treatment-plans', routeParam: petId },
    { label: 'Schedule Appointment', icon: Calendar, route: 'appointments' },
    { label: 'Add Clinical Note', icon: FileText, route: petId ? 'patient-detail' : 'patients', routeParam: petId },
    { label: 'Exercise Library Video', icon: Video, route: 'exercises' },
  ]
})
</script>

<template>
  <div class="space-y-4">
    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Patient Info</h3>
      <div v-if="thread && patient" class="mt-4">
        <div class="flex items-center gap-3">
          <div
            class="flex h-12 w-12 items-center justify-center rounded-full bg-sage-muted text-sm font-bold text-sage"
          >
            {{ patient.petName.slice(0, 2).toUpperCase() }}
          </div>
          <div>
            <p class="font-semibold text-navy">{{ patient.petName }}</p>
            <p class="text-xs text-neutral-muted">{{ patient.breed || patient.species }}</p>
            <p class="text-xs text-neutral-muted">Owner: {{ patient.ownerName }}</p>
          </div>
        </div>
        <RouterLink
          :to="{ name: 'patient-detail', params: { petId: patient.petId } }"
          class="portal-card-link mt-4 inline-block"
        >
          View Full Profile →
        </RouterLink>
      </div>
      <p v-else class="mt-4 text-sm text-neutral-muted">No patient selected.</p>
    </section>

    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Quick Actions</h3>
      <div class="mt-3 space-y-2">
        <template v-for="action in quickActions" :key="action.label">
          <RouterLink
            v-if="action.route && action.routeParam"
            :to="{ name: action.route, params: { petId: action.routeParam } }"
            class="flex items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
          >
            <component :is="action.icon" class="h-4 w-4 text-sage" :stroke-width="1.75" />
            {{ action.label }}
          </RouterLink>
          <RouterLink
            v-else-if="action.route"
            :to="{ name: action.route }"
            class="flex items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
          >
            <component :is="action.icon" class="h-4 w-4 text-sage" :stroke-width="1.75" />
            {{ action.label }}
          </RouterLink>
          <button
            v-else
            type="button"
            class="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
            @click="emit('comingSoon', `${action.label} coming soon.`)"
          >
            <component :is="action.icon" class="h-4 w-4 text-sage" :stroke-width="1.75" />
            {{ action.label }}
          </button>
        </template>
      </div>
    </section>
  </div>
</template>

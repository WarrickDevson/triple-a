<script setup lang="ts">
import { ref } from 'vue'
import { ArrowLeft } from '@lucide/vue'
import PatientActionBar from './PatientActionBar.vue'
import PatientOverviewTab from './PatientOverviewTab.vue'
import PatientTabStub from './PatientTabStub.vue'
import type { Appointment } from '../../types/appointment'
import type { PatientDemoMeta } from '../../data/patientDemo'
import type { RehabProgram } from '../../types/exercise'
import type { Pet } from '../../types/pet'

defineProps<{
  patient: Pet | null
  demoMeta: PatientDemoMeta | null
  activeProgram: RehabProgram | null
  nextAppointment: Appointment | null
  progressPercent: number
  loading?: boolean
  showBack?: boolean
}>()

const emit = defineEmits<{
  back: []
}>()

const tabs = [
  { id: 'overview', label: 'Overview' },
  { id: 'assessment', label: 'Assessment' },
  { id: 'plan', label: 'Plan' },
  { id: 'progress', label: 'Progress' },
  { id: 'documents', label: 'Documents' },
  { id: 'notes', label: 'Notes' },
] as const

const activeTab = ref<(typeof tabs)[number]['id']>('overview')
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div v-if="showBack" class="border-b border-neutral-grey/80 px-4 py-3 lg:hidden">
      <button
        type="button"
        class="inline-flex items-center gap-2 text-sm font-semibold text-sage"
        @click="emit('back')"
      >
        <ArrowLeft class="h-4 w-4" :stroke-width="1.75" />
        Back to patients
      </button>
    </div>

    <div v-if="!patient" class="flex flex-1 items-center justify-center p-8">
      <p class="text-sm text-neutral-muted">Select a patient to view their profile.</p>
    </div>

    <template v-else>
      <div class="border-b border-neutral-grey/80 px-4 pt-4">
        <div class="flex gap-1 overflow-x-auto">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            type="button"
            class="shrink-0 rounded-t-lg px-3 py-2 text-sm font-semibold transition-colors"
            :class="
              activeTab === tab.id
                ? 'border-b-2 border-sage text-navy'
                : 'text-neutral-muted hover:text-navy'
            "
            @click="activeTab = tab.id"
          >
            {{ tab.label }}
          </button>
        </div>
      </div>

      <div class="flex-1 overflow-y-auto p-4 sm:p-5">
        <div v-if="loading" class="py-12 text-center text-sm text-neutral-muted">
          Loading patient details...
        </div>

        <template v-else>
          <PatientOverviewTab
            v-if="activeTab === 'overview' && demoMeta"
            :patient="patient"
            :demo-meta="demoMeta"
            :active-program="activeProgram"
            :next-appointment="nextAppointment"
            :progress-percent="progressPercent"
          />
          <PatientTabStub
            v-else-if="activeTab === 'assessment'"
            title="Assessment"
            description="Record and review clinical assessments for this patient."
          />
          <PatientTabStub
            v-else-if="activeTab === 'plan'"
            title="Treatment Plan"
            description="Build and manage rehabilitation plans and exercise prescriptions."
          />
          <PatientTabStub
            v-else-if="activeTab === 'progress'"
            title="Progress"
            description="Track outcome measures and rehabilitation milestones over time."
          />
          <PatientTabStub
            v-else-if="activeTab === 'documents'"
            title="Documents"
            description="Access reports, consent forms, and shared files."
          />
          <PatientTabStub
            v-else-if="activeTab === 'notes'"
            title="Notes"
            description="Clinical notes and session observations will appear here."
          />

          <PatientActionBar v-if="activeTab === 'overview'" />
        </template>
      </div>
    </template>
  </section>
</template>

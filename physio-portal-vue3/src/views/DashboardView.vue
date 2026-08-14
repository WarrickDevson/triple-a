<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  LayoutDashboard,
  FileText,
  Calendar
} from '@lucide/vue'
import { useDashboardStore } from '../store/dashboard'
import { usePatientsStore } from '../store/patients'
import TodaysScheduleCard from '../components/dashboard/TodaysScheduleCard.vue'
import PatientsInCareCard from '../components/dashboard/PatientsInCareCard.vue'
import ProgressOverviewCard from '../components/dashboard/ProgressOverviewCard.vue'
import RecentPatientUpdatesCard from '../components/dashboard/RecentPatientUpdatesCard.vue'
import RecentSoapAssessmentsCard from '../components/dashboard/RecentSoapAssessmentsCard.vue'
import TasksRemindersCard from '../components/dashboard/TasksRemindersCard.vue'
import QuickStatsRow from '../components/dashboard/QuickStatsRow.vue'
import DashboardSoapAssessmentsTab from '../components/dashboard/DashboardSoapAssessmentsTab.vue'

const activeTab = ref<'overview' | 'soap-assessments' | 'schedule'>('overview')

const dashboardStore = useDashboardStore()
const patientsStore = usePatientsStore()

onMounted(() => {
  dashboardStore.fetchDashboard().catch(() => undefined)
  patientsStore.fetchClinicPatients().catch(() => undefined)
})

const patientCount = computed(
  () => dashboardStore.dashboard?.patientCount ?? patientsStore.patients.length,
)

const todaysSchedule = computed(() => dashboardStore.dashboard?.todaysSchedule ?? [])

const appointmentsToday = computed(
  () => dashboardStore.dashboard?.todaysAppointmentsCount ?? 0,
)
</script>

<template>
  <div class="space-y-6">
    <!-- Dashboard Top Navigation Tabs -->
    <div class="flex flex-wrap items-center justify-between gap-3 border-b border-neutral-grey/80 pb-3">
      <div class="flex gap-2">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-all shadow-xs"
          :class="
            activeTab === 'overview'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-surface text-navy hover:bg-neutral-grey/40 border border-neutral-grey/80'
          "
          @click="activeTab = 'overview'"
        >
          <LayoutDashboard class="h-4 w-4" />
          Overview
        </button>

        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-all shadow-xs"
          :class="
            activeTab === 'soap-assessments'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-surface text-navy hover:bg-neutral-grey/40 border border-neutral-grey/80'
          "
          @click="activeTab = 'soap-assessments'"
        >
          <FileText class="h-4 w-4" />
          SOAP Assessments
        </button>

        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-all shadow-xs"
          :class="
            activeTab === 'schedule'
              ? 'bg-sage text-white shadow-sm'
              : 'bg-surface text-navy hover:bg-neutral-grey/40 border border-neutral-grey/80'
          "
          @click="activeTab = 'schedule'"
        >
          <Calendar class="h-4 w-4" />
          Today's Schedule
          <span
            v-if="appointmentsToday > 0"
            class="rounded-full bg-surface/30 px-1.5 py-0.2 text-[10px]"
            :class="activeTab === 'schedule' ? 'text-white' : 'text-sage'"
          >
            {{ appointmentsToday }}
          </span>
        </button>
      </div>
    </div>

    <!-- TAB 1: OVERVIEW -->
    <div v-if="activeTab === 'overview'" class="space-y-6">
      <div class="grid gap-6 lg:grid-cols-3">
        <TodaysScheduleCard
          :appointments="todaysSchedule"
          :loading="dashboardStore.loading"
        />
        <PatientsInCareCard :patient-count="patientCount" />
        <ProgressOverviewCard />
      </div>

      <div class="grid gap-6 lg:grid-cols-3">
        <div class="lg:col-span-2 space-y-6">
          <RecentSoapAssessmentsCard />
          <RecentPatientUpdatesCard />
        </div>
        <TasksRemindersCard />
      </div>

      <QuickStatsRow :appointments-today="appointmentsToday" />
    </div>

    <!-- TAB 2: SOAP ASSESSMENTS -->
    <div v-else-if="activeTab === 'soap-assessments'">
      <DashboardSoapAssessmentsTab />
    </div>

    <!-- TAB 3: TODAY'S SCHEDULE -->
    <div v-else-if="activeTab === 'schedule'" class="grid gap-6 lg:grid-cols-3">
      <div class="lg:col-span-2">
        <TodaysScheduleCard
          :appointments="todaysSchedule"
          :loading="dashboardStore.loading"
        />
      </div>
      <TasksRemindersCard />
    </div>
  </div>
</template>

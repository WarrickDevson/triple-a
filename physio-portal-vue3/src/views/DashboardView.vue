<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useDashboardStore } from '../store/dashboard'
import { usePatientsStore } from '../store/patients'
import TodaysScheduleCard from '../components/dashboard/TodaysScheduleCard.vue'
import PatientsInCareCard from '../components/dashboard/PatientsInCareCard.vue'
import ProgressOverviewCard from '../components/dashboard/ProgressOverviewCard.vue'
import RecentPatientUpdatesCard from '../components/dashboard/RecentPatientUpdatesCard.vue'
import RecentSoapAssessmentsCard from '../components/dashboard/RecentSoapAssessmentsCard.vue'
import TasksRemindersCard from '../components/dashboard/TasksRemindersCard.vue'
import QuickStatsRow from '../components/dashboard/QuickStatsRow.vue'

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
</template>

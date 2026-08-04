import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getPhysioDashboard } from '../api/dashboard'
import type { PhysioDashboard } from '../types/dashboard'

export const useDashboardStore = defineStore('dashboard', () => {
  const dashboard = ref<PhysioDashboard | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchDashboard(force = false) {
    if (dashboard.value && !force) return dashboard.value

    loading.value = true
    error.value = null
    try {
      dashboard.value = await getPhysioDashboard()
      return dashboard.value
    } catch {
      error.value = 'Unable to load dashboard.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  return { dashboard, loading, error, fetchDashboard }
})

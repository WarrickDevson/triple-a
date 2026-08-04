import { apiClient } from './client'
import type { PhysioDashboard } from '../types/dashboard'

export async function getPhysioDashboard(): Promise<PhysioDashboard> {
  const { data } = await apiClient.get<PhysioDashboard>('/api/dashboard/physio')
  return data
}

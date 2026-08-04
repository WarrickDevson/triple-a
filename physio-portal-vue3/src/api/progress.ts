import { apiClient } from './client'
import type { PetProgressSummary } from '../types/dashboard'

export async function getPetProgress(petId: number): Promise<PetProgressSummary> {
  const { data } = await apiClient.get<PetProgressSummary>(`/api/pets/${petId}/progress`)
  return data
}

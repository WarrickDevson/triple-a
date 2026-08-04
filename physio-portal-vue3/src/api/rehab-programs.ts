import { apiClient } from './client'
import type { CreateRehabProgramRequest, RehabProgram } from '../types/exercise'

export async function getRehabProgramsByPet(petId: number): Promise<RehabProgram[]> {
  const { data } = await apiClient.get<RehabProgram[]>(`/api/rehab-programs/pet/${petId}`)
  return data
}

export async function createRehabProgram(request: CreateRehabProgramRequest): Promise<RehabProgram> {
  const { data } = await apiClient.post<RehabProgram>('/api/rehab-programs', request)
  return data
}

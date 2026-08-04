import { apiClient } from './client'
import type { Exercise } from '../types/exercise'

export async function getExercises(species?: string, condition?: string): Promise<Exercise[]> {
  const { data } = await apiClient.get<Exercise[]>('/api/exercises', {
    params: { species, condition },
  })
  return data
}

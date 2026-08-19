import { apiClient } from './client'
import type { CreateExerciseRequest, Exercise } from '../types/exercise'

export async function getExercises(species?: string, condition?: string): Promise<Exercise[]> {
  const { data } = await apiClient.get<Exercise[]>('/api/exercises', {
    params: { species, condition },
  })
  return data
}

export async function createExercise(request: CreateExerciseRequest): Promise<Exercise> {
  const { data } = await apiClient.post<Exercise>('/api/exercises', request)
  return data
}

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

export async function updateExercise(id: number, request: CreateExerciseRequest): Promise<Exercise> {
  const { data } = await apiClient.put<Exercise>(`/api/exercises/${id}`, request)
  return data
}

export async function customizeExercise(id: number): Promise<Exercise> {
  const { data } = await apiClient.post<Exercise>(`/api/exercises/${id}/customize`)
  return data
}

export async function toggleActiveExercise(id: number): Promise<{ exerciseId: number; baseExerciseId: number | null; isActiveForOwners: boolean }> {
  const { data } = await apiClient.post<{ exerciseId: number; baseExerciseId: number | null; isActiveForOwners: boolean }>(`/api/exercises/${id}/toggle-active`)
  return data
}

export async function deleteExercise(id: number): Promise<void> {
  await apiClient.delete(`/api/exercises/${id}`)
}

export async function uploadExerciseMedia(file: File): Promise<{ url: string; fileName: string; contentType: string; isVideo: boolean }> {
  const formData = new FormData()
  formData.append('file', file)
  const { data } = await apiClient.post<{ url: string; fileName: string; contentType: string; isVideo: boolean }>(
    '/api/exercises/upload-media',
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
    },
  )
  return data
}


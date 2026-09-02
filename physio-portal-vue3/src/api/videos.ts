import { apiClient } from './client'
import { API_BASE_URL } from './config'
import type { ReviewVideoRequest, VideoSubmission } from '../types/video'

export function resolveMediaUrl(path: string | null | undefined): string | null {
  if (!path) return null
  if (path.startsWith('http://') || path.startsWith('https://')) return path
  return `${API_BASE_URL.replace(/\/$/, '')}${path.startsWith('/') ? path : `/${path}`}`
}

export async function fetchPendingVideos(): Promise<VideoSubmission[]> {
  const { data } = await apiClient.get<VideoSubmission[]>('/api/videos/pending')
  return data
}

export async function getPetVideos(petId: number): Promise<VideoSubmission[]> {
  const { data } = await apiClient.get<VideoSubmission[]>(`/api/pets/${petId}/videos`)
  return data
}

export async function reviewVideo(id: number, payload: ReviewVideoRequest): Promise<VideoSubmission> {
  const { data } = await apiClient.put<VideoSubmission>(`/api/videos/${id}/review`, payload)
  return data
}

export async function updateVideo(
  id: number,
  payload: { title?: string | null; notes?: string | null },
): Promise<VideoSubmission> {
  const { data } = await apiClient.put<VideoSubmission>(`/api/videos/${id}`, payload)
  return data
}

export async function deleteVideo(id: number): Promise<void> {
  await apiClient.delete(`/api/videos/${id}`)
}

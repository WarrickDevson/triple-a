import { apiClient } from './client'
import { API_BASE_URL } from './config'
import type { ReviewVideoRequest, VideoSubmission } from '../types/video'

export function resolveMediaUrl(path: string | null | undefined): string | null {
  if (!path) return null
  const trimmed = path.trim()
  if (!trimmed) return null

  const base = API_BASE_URL.replace(/\/+$/, '')

  // Legacy/stale signed URL handling: strip GCS domain and query string
  if (trimmed.includes('storage.googleapis.com')) {
    try {
      const urlObj = new URL(trimmed)
      let objPath = urlObj.pathname.replace(/^\/+/, '')
      const slashIdx = objPath.indexOf('/')
      if (slashIdx >= 0) {
        objPath = objPath.slice(slashIdx + 1)
      }
      return `${base}/api/media/view?path=${encodeURIComponent(objPath)}`
    } catch {
      // Fall through
    }
  }

  // Already a full external URL (e.g. YouTube, Unsplash)
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    return trimmed
  }

  // Dedicated /api/media route
  if (trimmed.startsWith('/api/media/') || trimmed.startsWith('api/media/')) {
    return `${base}${trimmed.startsWith('/') ? trimmed : `/${trimmed}`}`
  }

  // Relative storage path (e.g. avatars/..., pets/..., exercise-images/..., documents/...)
  const clean = trimmed.replace(/^\/+/, '')
  const firstSlash = clean.indexOf('/')
  const root = firstSlash > 0 ? clean.slice(0, firstSlash).toLowerCase() : clean.toLowerCase()
  const storageFolders = ['avatars', 'pets', 'exercise-images', 'exercise-videos', 'videos', 'documents', 'attachments', 'uploads']

  if (storageFolders.includes(root)) {
    return `${base}/api/media/view?path=${encodeURIComponent(clean)}`
  }

  return `${base}${trimmed.startsWith('/') ? trimmed : `/${trimmed}`}`
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

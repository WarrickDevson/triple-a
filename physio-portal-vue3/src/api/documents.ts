import { apiClient } from './client'
import type { SharedReport } from '../types/soap'

export interface UploadDocumentParams {
  file: File
  petId: number
  title: string
  category?: string
  shareWithOwner?: boolean
}

export async function uploadClinicalDocument(params: UploadDocumentParams): Promise<SharedReport> {
  const formData = new FormData()
  formData.append('file', params.file)
  formData.append('petId', params.petId.toString())
  formData.append('title', params.title)
  if (params.category) formData.append('category', params.category)
  formData.append('shareWithOwner', (params.shareWithOwner !== false).toString())

  const { data } = await apiClient.post<SharedReport>('/api/documents/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

export async function fetchPetClinicalDocuments(petId: number): Promise<SharedReport[]> {
  const { data } = await apiClient.get<SharedReport[]>(`/api/documents/pet/${petId}`)
  return data
}

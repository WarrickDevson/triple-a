import { apiClient } from './client'
import type { SharedReport } from '../types/soap'

export async function downloadPetReport(petId: number): Promise<void> {
  const response = await apiClient.get<Blob>(`/api/reports/pet/${petId}/download`, {
    responseType: 'blob',
  })

  const disposition = response.headers['content-disposition'] as string | undefined
  const fileNameMatch = disposition?.match(/filename="?([^"]+)"?/)
  const fileName = fileNameMatch?.[1] ?? `TripleA-Report-${petId}.pdf`

  const url = window.URL.createObjectURL(response.data)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  document.body.appendChild(link)
  link.click()
  link.remove()
  window.URL.revokeObjectURL(url)
}

export async function fetchSharedReports(petId: number): Promise<SharedReport[]> {
  const res = await apiClient.get<SharedReport[]>(`/api/reports/pet/${petId}/shared`)
  return res.data
}

export async function shareDocument(
  petId: number,
  payload: { title: string; reportType: string; summary?: string; soapNoteId?: number }
): Promise<SharedReport> {
  const res = await apiClient.post<SharedReport>(`/api/reports/pet/${petId}/share-document`, payload)
  return res.data
}

export async function publishProgressReport(petId: number, title?: string): Promise<SharedReport> {
  const res = await apiClient.post<SharedReport>(`/api/reports/pet/${petId}/publish-progress-report`, null, {
    params: title ? { title } : undefined
  })
  return res.data
}

export async function deleteSharedReport(sharedReportId: number): Promise<boolean> {
  try {
    await apiClient.delete(`/api/reports/shared/${sharedReportId}`)
    return true
  } catch {
    return false
  }
}

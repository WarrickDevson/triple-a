import { apiClient } from './client'

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

import { apiClient } from './client'
import type { SoapNote, CreateSoapNoteRequest, UpdateSoapNoteRequest, SharedReport, OwnerSubjectiveNote } from '../types/soap'
import { DEMO_SOAP_NOTES, DEMO_SHARED_REPORTS, DEMO_OWNER_SUBJECTIVE_NOTES } from '../data/soapDemo'

export async function fetchSoapNotesByPet(petId: number): Promise<SoapNote[]> {
  try {
    const res = await apiClient.get<SoapNote[]>(`/soap-notes/pet/${petId}`)
    return res.data
  } catch {
    return DEMO_SOAP_NOTES[petId] ?? []
  }
}

export async function createSoapNote(petId: number, payload: CreateSoapNoteRequest): Promise<SoapNote> {
  try {
    const res = await apiClient.post<SoapNote>(`/soap-notes/pet/${petId}`, payload)
    return res.data
  } catch {
    const mockNote: SoapNote = {
      soapNoteId: Date.now(),
      petId,
      physioId: 1,
      physioName: 'Dr. Sarah Jenkins, PT',
      appointmentId: payload.appointmentId,
      sessionDate: payload.sessionDate ?? new Date().toISOString(),
      subjective: payload.subjective,
      objective: payload.objective,
      action: payload.action,
      plan: payload.plan,
      stiffnessScore: payload.stiffnessScore,
      painScore: payload.painScore,
      lamenessScore: payload.lamenessScore,
      customMetrics: payload.customMetrics ?? [],
      isSharedWithOwner: payload.shareWithOwner ?? false,
      sharedAtUtc: payload.shareWithOwner ? new Date().toISOString() : null,
      createdAtUtc: new Date().toISOString(),
    }
    if (!DEMO_SOAP_NOTES[petId]) DEMO_SOAP_NOTES[petId] = []
    DEMO_SOAP_NOTES[petId].unshift(mockNote)
    return mockNote
  }
}

export async function updateSoapNote(soapNoteId: number, payload: UpdateSoapNoteRequest): Promise<SoapNote> {
  try {
    const res = await apiClient.put<SoapNote>(`/soap-notes/${soapNoteId}`, payload)
    return res.data
  } catch {
    throw new Error('Could not update SOAP note.')
  }
}

export async function deleteSoapNote(soapNoteId: number): Promise<boolean> {
  try {
    await apiClient.delete(`/soap-notes/${soapNoteId}`)
    return true
  } catch {
    return false
  }
}

export async function fetchSharedReportsByPet(petId: number): Promise<SharedReport[]> {
  try {
    const res = await apiClient.get<SharedReport[]>(`/reports/pet/${petId}/shared`)
    return res.data
  } catch {
    return DEMO_SHARED_REPORTS[petId] ?? []
  }
}

export async function downloadSoapPdf(soapNoteId: number): Promise<void> {
  try {
    const response = await apiClient.get<Blob>(`/soap-notes/${soapNoteId}/pdf`, {
      responseType: 'blob',
    })

    const disposition = response.headers['content-disposition'] as string | undefined
    const fileNameMatch = disposition?.match(/filename="?([^"]+)"?/)
    const fileName = fileNameMatch?.[1] ?? `SOAP_Report_${soapNoteId}.pdf`

    const url = window.URL.createObjectURL(response.data)
    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch (err) {
    console.error('Failed to download SOAP PDF', err)
    alert('Could not download PDF report. Ensure the backend API is running.')
  }
}

export async function fetchOwnerSubjectiveNotes(petId: number): Promise<OwnerSubjectiveNote[]> {
  try {
    const res = await apiClient.get<OwnerSubjectiveNote[]>(`/soap-notes/pet/${petId}/owner-notes`)
    return res.data
  } catch {
    return DEMO_OWNER_SUBJECTIVE_NOTES[petId] ?? []
  }
}

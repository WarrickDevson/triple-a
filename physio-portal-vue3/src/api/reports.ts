import { apiClient } from './client'
import type { CreateReportPayload, SharedReport } from '../types/soap'
import type { Pet } from '../types/pet'
import { demoReportHistory } from '../data/reportsDemo'

export interface DownloadReportOptions {
  type?: string
  customTitle?: string
  summary?: string
  dischargeStatus?: string
  maintenancePlan?: string
  veterinarianNotes?: string
  ownerInstructions?: string
  soapNoteId?: number
  periodFrom?: string
  periodTo?: string
  referencedSessions?: import('../types/soap').ReferencedReportSession[]
  patient?: Pet | null
}

export function sanitizePdfFileName(name: string): string {
  const clean = name
    .replace(/&/g, 'and')
    .replace(/[^a-zA-Z0-9_\-\.]/g, '_')
    .replace(/_+/g, '_')
    .replace(/^_|_$/g, '')

  return clean.toLowerCase().endsWith('.pdf') ? clean : `${clean}.pdf`
}

export async function downloadPetReport(petId: number, options?: DownloadReportOptions): Promise<void> {
  const params: Record<string, string | number> = {}
  if (options?.type) params.type = options.type
  if (options?.customTitle) params.customTitle = options.customTitle
  if (options?.summary) params.summary = options.summary
  if (options?.dischargeStatus) params.dischargeStatus = options.dischargeStatus
  if (options?.maintenancePlan) params.maintenancePlan = options.maintenancePlan
  if (options?.veterinarianNotes) params.veterinarianNotes = options.veterinarianNotes
  if (options?.ownerInstructions) params.ownerInstructions = options.ownerInstructions
  if (options?.soapNoteId) params.soapNoteId = options.soapNoteId
  if (options?.periodFrom) params.periodFrom = options.periodFrom
  if (options?.periodTo) params.periodTo = options.periodTo
  if (options?.referencedSessions && options.referencedSessions.length > 0) {
    params.sessionsJson = JSON.stringify(options.referencedSessions)
  }

  const response = await apiClient.get<Blob>(`/api/reports/pet/${petId}/download`, {
    params,
    responseType: 'blob',
  })

  let fileName = ''
  const disposition = response.headers['content-disposition'] as string | undefined
  if (disposition) {
    const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
    if (match && match[1]) {
      fileName = match[1].replace(/['"]/g, '').trim()
    }
  }

  if (!fileName) {
    const safePetName = (options?.patient?.petName || `Pet-${petId}`).replace(/\s+/g, '_')
    const typeLabel = options?.type ? `_${options.type}` : '_ClinicalReport'
    const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '')
    fileName = `TripleA_${safePetName}${typeLabel}_${dateStr}.pdf`
  }

  await triggerBlobDownload(response.data, fileName)
}

export async function downloadSharedReport(
  sharedReportId: number,
  preferredName?: string
): Promise<void> {
  const response = await apiClient.get<Blob>(`/api/reports/shared/${sharedReportId}/download`, {
    responseType: 'blob',
  })

  let fileName = ''
  const disposition = response.headers['content-disposition'] as string | undefined
  if (disposition) {
    const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
    if (match && match[1]) {
      fileName = match[1].replace(/['"]/g, '').trim()
    }
  }

  if (!fileName) {
    fileName = preferredName || `TripleA_Report_${sharedReportId}.pdf`
  }

  await triggerBlobDownload(response.data, fileName)
}

function triggerBlobDownload(data: any, rawFileName: string): Promise<void> {
  return new Promise((resolve) => {
    const cleanFileName = sanitizePdfFileName(rawFileName)
    const blob = data instanceof Blob
      ? (data.type === 'application/pdf' ? data : data.slice(0, data.size, 'application/pdf'))
      : new Blob([data], { type: 'application/pdf' })

    const reader = new FileReader()
    reader.onloadend = () => {
      const dataUrl = reader.result as string
      const link = document.createElement('a')
      link.href = dataUrl
      link.download = cleanFileName
      link.setAttribute('download', cleanFileName)
      link.style.position = 'fixed'
      link.style.left = '-9999px'
      link.style.top = '-9999px'
      document.body.appendChild(link)

      try {
        link.click()
      } catch {
        const evt = new MouseEvent('click', { bubbles: true, cancelable: true, view: window })
        link.dispatchEvent(evt)
      }

      setTimeout(() => {
        try {
          document.body.removeChild(link)
        } catch {}
        resolve()
      }, 500)
    }

    reader.readAsDataURL(blob)
  })
}

export async function fetchRecentReports(petId?: number): Promise<SharedReport[]> {
  try {
    const res = await apiClient.get<SharedReport[]>('/api/reports/recent', {
      params: petId ? { petId } : undefined,
    })
    if (res.data && res.data.length > 0) {
      return res.data
    }
  } catch {
    // API offline or route not ready fallback
  }

  // Fallback from demo dataset
  return demoReportHistory
    .filter((d) => !petId || d.petId === petId)
    .map((d) => ({
      sharedReportId: d.id,
      petId: d.petId,
      soapNoteId: d.soapNoteId ?? null,
      sharedByPhysioId: 1,
      sharedByPhysioName: d.authorName ?? 'Dr. S. Devson',
      title: d.title,
      reportType: d.reportType === 'Progress Report'
        ? 'PROGRESS_REPORT'
        : d.reportType === 'Discharge Summary'
          ? 'DISCHARGE_SUMMARY'
          : d.reportType === 'Owner Home Program'
            ? 'OWNER_HOME_PROGRAM'
            : 'SOAP_SESSION',
      summary: d.summary ?? '',
      sharedAtUtc: d.generatedAt,
      petName: d.petName,
      ownerName: d.ownerName,
      species: d.species,
      breed: d.breed,
      isActive: d.status === 'Sent',
    }))
}

export async function createReport(payload: CreateReportPayload, patient?: Pet | null): Promise<SharedReport> {
  try {
    const res = await apiClient.post<SharedReport>('/api/reports/create', payload)
    if (res.data) return res.data
  } catch {
    // Graceful offline fallback
  }

  const newReport: SharedReport = {
    sharedReportId: Date.now(),
    petId: payload.petId,
    soapNoteId: payload.soapNoteId ?? null,
    sharedByPhysioId: 1,
    sharedByPhysioName: 'Dr. S. Devson',
    title: payload.title,
    reportType: payload.reportType,
    summary: payload.summary || '',
    sharedAtUtc: new Date().toISOString(),
    petName: patient?.petName || 'Patient',
    ownerName: patient?.ownerName || 'Pet Owner',
    species: patient?.species || 'Canine',
    breed: patient?.breed ?? undefined,
    isActive: payload.shareWithOwner !== false,
    periodFrom: payload.periodFrom,
    periodTo: payload.periodTo,
    referencedSessions: payload.referencedSessions,
  }

  // Also push into demo dataset so it survives page re-filtering
  demoReportHistory.unshift({
    id: newReport.sharedReportId,
    petId: newReport.petId,
    petName: newReport.petName || 'Patient',
    ownerName: newReport.ownerName || 'Pet Owner',
    species: newReport.species,
    breed: newReport.breed,
    reportType: (payload.reportType.includes('DISCHARGE')
      ? 'Discharge Summary'
      : payload.reportType.includes('HOME')
        ? 'Owner Home Program'
        : payload.reportType.includes('SOAP')
          ? 'SOAP Session Report'
          : 'Progress Report') as any,
    title: newReport.title,
    summary: newReport.summary || undefined,
    generatedAt: newReport.sharedAtUtc,
    status: newReport.isActive ? 'Sent' : 'Draft',
    authorName: 'Dr. S. Devson',
  })

  return newReport
}

export async function fetchSharedReports(petId: number): Promise<SharedReport[]> {
  try {
    const res = await apiClient.get<SharedReport[]>(`/api/reports/pet/${petId}/shared`)
    return res.data
  } catch {
    return fetchRecentReports(petId)
  }
}

export async function shareDocument(
  petId: number,
  payload: { title: string; reportType: string; summary?: string; soapNoteId?: number }
): Promise<SharedReport> {
  try {
    const res = await apiClient.post<SharedReport>(`/api/reports/pet/${petId}/share-document`, payload)
    return res.data
  } catch {
    return {
      sharedReportId: Date.now(),
      petId,
      soapNoteId: payload.soapNoteId,
      sharedByPhysioId: 1,
      sharedByPhysioName: 'Dr. S. Devson',
      title: payload.title,
      reportType: payload.reportType,
      summary: payload.summary,
      sharedAtUtc: new Date().toISOString(),
      isActive: true,
    }
  }
}

export async function publishProgressReport(petId: number, title?: string): Promise<SharedReport> {
  try {
    const res = await apiClient.post<SharedReport>(`/api/reports/pet/${petId}/publish-progress-report`, null, {
      params: title ? { title } : undefined,
    })
    return res.data
  } catch {
    return {
      sharedReportId: Date.now(),
      petId,
      sharedByPhysioId: 1,
      sharedByPhysioName: 'Dr. S. Devson',
      title: title || 'Clinical Progress Report',
      reportType: 'PROGRESS_REPORT',
      summary: 'Comprehensive rehabilitation progress summary.',
      sharedAtUtc: new Date().toISOString(),
      isActive: true,
    }
  }
}

export async function deleteSharedReport(sharedReportId: number): Promise<boolean> {
  try {
    await apiClient.delete(`/api/reports/shared/${sharedReportId}`)
    return true
  } catch {
    return true
  }
}

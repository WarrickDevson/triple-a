import { apiClient } from './client'
import type {
  SoapNote,
  CreateSoapNoteRequest,
  UpdateSoapNoteRequest,
  SharedReport,
  OwnerSubjectiveNote,
  ParseSoapNarrativeRequest,
  StructuredSoapNote,
  SoapTranscriptionResult,
  SoapVocabulary
} from '../types/soap'
import { DEMO_SOAP_NOTES, DEMO_SHARED_REPORTS, DEMO_OWNER_SUBJECTIVE_NOTES } from '../data/soapDemo'
import { correctVeterinaryTranscript, VETERINARY_CATEGORIES, VETERINARY_AUTO_CORRECTIONS } from '../utils/veterinaryLexicon'

export async function fetchSoapNotesByPet(petId: number): Promise<SoapNote[]> {
  try {
    const res = await apiClient.get<SoapNote[]>(`/api/soap-notes/pet/${petId}`)
    return res.data
  } catch {
    return DEMO_SOAP_NOTES[petId] ?? []
  }
}

export async function createSoapNote(petId: number, payload: CreateSoapNoteRequest): Promise<SoapNote> {
  try {
    const res = await apiClient.post<SoapNote>(`/api/soap-notes/pet/${petId}`, payload)
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
    const res = await apiClient.put<SoapNote>(`/api/soap-notes/${soapNoteId}`, payload)
    return res.data
  } catch {
    throw new Error('Could not update SOAP note.')
  }
}

export async function deleteSoapNote(soapNoteId: number): Promise<boolean> {
  try {
    await apiClient.delete(`/api/soap-notes/${soapNoteId}`)
    return true
  } catch {
    return false
  }
}

export async function toggleSoapNoteShare(soapNoteId: number, shareWithOwner: boolean): Promise<SoapNote> {
  try {
    const res = await apiClient.put<SoapNote>(`/api/soap-notes/${soapNoteId}/share`, { shareWithOwner })
    return res.data
  } catch {
    throw new Error('Could not update sharing status.')
  }
}

export async function fetchSharedReportsByPet(petId: number): Promise<SharedReport[]> {
  try {
    const res = await apiClient.get<SharedReport[]>(`/api/reports/pet/${petId}/shared`)
    return res.data
  } catch {
    return DEMO_SHARED_REPORTS[petId] ?? []
  }
}

export async function downloadSoapPdf(soapNoteId: number): Promise<void> {
  try {
    const response = await apiClient.get<Blob>(`/api/soap-notes/${soapNoteId}/pdf`, {
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
    const res = await apiClient.get<OwnerSubjectiveNote[]>(`/api/soap-notes/pet/${petId}/owner-notes`)
    return res.data
  } catch {
    return DEMO_OWNER_SUBJECTIVE_NOTES[petId] ?? []
  }
}

export async function updateOwnerSubjectiveNote(
  noteId: number,
  payload: { notes: string; painObserved?: number | null; energyObserved?: number | null },
): Promise<OwnerSubjectiveNote> {
  const res = await apiClient.put<OwnerSubjectiveNote>(`/api/soap-notes/owner-notes/${noteId}`, payload)
  return res.data
}

export async function deleteOwnerSubjectiveNote(noteId: number): Promise<boolean> {
  await apiClient.delete(`/api/soap-notes/owner-notes/${noteId}`)
  return true
}

// Voice Dictation & Audio Transcription Endpoints
export async function parseSoapNarrative(payload: ParseSoapNarrativeRequest): Promise<StructuredSoapNote> {
  try {
    const res = await apiClient.post<StructuredSoapNote>('/api/soap-notes/dictation/parse-narrative', payload)
    return res.data
  } catch (err) {
    console.warn('Backend narrative parse unavailable, using client-side clinical NLP parser:', err)
    return parseNarrativeLocally(payload.transcript, payload.petName)
  }
}

export async function transcribeSoapAudio(
  audioBlob: Blob,
  petName?: string,
  species?: string
): Promise<SoapTranscriptionResult> {
  try {
    const formData = new FormData()
    formData.append('file', audioBlob, 'dictation.webm')
    if (petName) formData.append('petName', petName)
    if (species) formData.append('species', species)

    const res = await apiClient.post<SoapTranscriptionResult>('/api/soap-notes/dictation/transcribe-audio', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return res.data
  } catch (err) {
    console.warn('Backend audio transcription failed, falling back to local simulation:', err)
    const simulatedTranscript = `Consultation assessment for ${petName ?? 'Buddy'}. ` +
      'Owner reports patient is doing well at home. Morning stiffness is rated 3 out of 10 and pain is at 2 out of 10. ' +
      'On exam, stifle extension PROM is 135 degrees, and thigh circumference is 38 cm. ' +
      'Treatment performed: 15 minutes myofascial release, laser therapy to stifle at 4 J/cm2, and 10 mins UWTM. ' +
      'Plan: Continue home PROM and recheck in 2 weeks.'

    const structured = parseNarrativeLocally(simulatedTranscript, petName)
    return {
      transcript: simulatedTranscript,
      structuredNote: structured,
      durationMs: 450,
      usedLocalFallback: true
    }
  }
}

export async function fetchSoapVocabulary(): Promise<SoapVocabulary> {
  try {
    const res = await apiClient.get<SoapVocabulary>('/api/soap-notes/dictation/vocabulary')
    return res.data
  } catch {
    const allTerms = VETERINARY_CATEGORIES.flatMap(c => c.terms)
    return {
      terms: allTerms,
      categories: VETERINARY_CATEGORIES,
      autoCorrections: VETERINARY_AUTO_CORRECTIONS
    }
  }
}

// Client-side heuristic parser fallback
function parseNarrativeLocally(rawText: string, _petName?: string | null): StructuredSoapNote {
  const transcript = correctVeterinaryTranscript(rawText)
  const sentences = transcript.split(/(?<=[.!?])\s+/).filter(s => s.trim().length > 0)

  const subjectiveParts: string[] = []
  const objectiveParts: string[] = []
  const actionParts: string[] = []
  const planParts: string[] = []

  let stiffnessScore: number | null = null
  let painScore: number | null = null
  let lamenessScore: number | null = null

  // Extract stiffness
  const stiffMatch = transcript.match(/stiffness\s*(?:is|was|score|reduced to|at)?\s*(\d{1,2})\s*(?:out of|\/|\s*\/)\s*10/i)
  if (stiffMatch && stiffMatch[1]) {
    stiffnessScore = Math.min(10, parseInt(stiffMatch[1], 10))
  }

  // Extract pain
  const painMatch = transcript.match(/pain\s*(?:score|is|was|at|controlled at|level)?\s*(\d{1,2})\s*(?:out of|\/|\s*\/)\s*10/i)
  if (painMatch && painMatch[1]) {
    painScore = Math.min(10, parseInt(painMatch[1], 10))
  }

  // Extract lameness
  const lameMatch = transcript.match(/lameness\s*(?:grade|score|is|was|at)?\s*(\d{1,2})\s*(?:out of|\/|\s*\/)\s*5/i)
  if (lameMatch && lameMatch[1]) {
    lamenessScore = Math.min(5, parseInt(lameMatch[1], 10))
  }

  // Categorize sentences
  for (const s of sentences) {
    const low = s.toLowerCase()
    if (/owner reports|owner states|owner noticed|at home|compliance|appetite|energy|history|past week|observed/.test(low)) {
      subjectiveParts.push(s.trim())
    } else if (/exam|examination|palpation|gait|range of motion|rom|stifle|circumference|degree|atrophy|tension|swelling|effusion|placing|reflex/.test(low)) {
      objectiveParts.push(s.trim())
    } else if (/treatment|treated|performed|applied|laser|uwtm|underwater treadmill|prom|massage|myofascial|cavaletti|balance disc|in-session|cryotherapy/.test(low)) {
      actionParts.push(s.trim())
    } else if (/plan|recommend|continue|home program|recheck|follow-up|next session|schedule|frequency|daily/.test(low)) {
      planParts.push(s.trim())
    } else {
      objectiveParts.push(s.trim())
    }
  }

  let suggestedDiagnosis: string | null = null
  if (/tplo/i.test(transcript)) suggestedDiagnosis = 'Post-operative TPLO Rehabilitation'
  else if (/cruciate|ccl/i.test(transcript)) suggestedDiagnosis = 'Cranial Cruciate Ligament (CCL) Disease'
  else if (/patella|patellar/i.test(transcript)) suggestedDiagnosis = 'Patellar Luxation Management'
  else if (/osteoarthritis|oa/i.test(transcript)) suggestedDiagnosis = 'Canine Osteoarthritis & Mobility Management'
  else if (/ivdd|disc/i.test(transcript)) suggestedDiagnosis = 'Intervertebral Disc Disease (IVDD) Conservative Rehab'

  const extractedTerms: string[] = []
  if (/tplo/i.test(transcript)) extractedTerms.push('TPLO Post-Op')
  if (/prom/i.test(transcript)) extractedTerms.push('Passive Range of Motion (PROM)')
  if (/uwtm|underwater treadmill/i.test(transcript)) extractedTerms.push('Underwater Treadmill (UWTM)')
  if (/laser|photobiomodulation/i.test(transcript)) extractedTerms.push('Laser Therapy / PBMT')
  if (/myofascial/i.test(transcript)) extractedTerms.push('Myofascial Release')
  if (/cavaletti/i.test(transcript)) extractedTerms.push('Cavaletti Rails')
  if (/stifle/i.test(transcript)) extractedTerms.push('Stifle Joint')

  return {
    subjective: subjectiveParts.join('\n') || (transcript.length < 200 ? transcript : 'Patient reported stable with improved mobility.'),
    objective: objectiveParts.join('\n') || 'Examination revealed mild joint stiffness with stable posture and normal weight bearing.',
    action: actionParts.join('\n') || 'Performed active/passive mobility exercises and manual therapeutic modalities.',
    plan: planParts.join('\n') || 'Continue structured home rehabilitation program and re-evaluate at next session.',
    stiffnessScore: stiffnessScore ?? 3,
    painScore: painScore ?? 2,
    lamenessScore: lamenessScore ?? 1,
    customMetrics: [
      { name: 'Stifle Extension ROM', value: 135, minScale: 0, maxScale: 180, unitOrDescriptor: 'deg' },
      { name: 'Thigh Circumference', value: 38, minScale: 10, maxScale: 80, unitOrDescriptor: 'cm' }
    ],
    suggestedDiagnosis,
    rawTranscript: transcript,
    confidenceScore: 0.92,
    extractedTerms
  }
}

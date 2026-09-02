export interface VideoSubmission {
  videoSubmissionId: number
  petId: number
  petName: string
  exerciseId: number | null
  exerciseTitle: string | null
  title?: string | null
  notes?: string | null
  rawVideoStorageUrl: string
  processedVideoStreamingUrl: string | null
  processingStatus: string
  isReviewed: boolean
  physioFeedbackNotes: string | null
  createdDate: string
}

export interface ReviewVideoRequest {
  feedbackNotes: string
}

export function getVideoTitle(v: VideoSubmission | null | undefined): string {
  if (!v) return 'Video Submission'
  if (v.title && v.title.trim()) return v.title.trim()
  if (v.exerciseTitle && v.exerciseTitle.trim()) return v.exerciseTitle.trim()
  if (v.notes && v.notes.trim()) return v.notes.trim()
  return `Video #${v.videoSubmissionId}`
}


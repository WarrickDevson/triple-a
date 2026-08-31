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

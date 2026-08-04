export interface VideoSubmission {
  videoSubmissionId: number
  petId: number
  petName: string
  exerciseId: number
  exerciseTitle: string
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

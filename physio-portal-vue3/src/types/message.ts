export interface Message {
  messageId: number
  messageThreadId: number
  senderUserId: number
  senderName: string
  body: string
  videoSubmissionId: number | null
  readAt: string | null
  createdDate: string
}

export interface MessageThread {
  messageThreadId: number
  petId: number
  petName: string
  ownerId: number
  ownerName: string
  physioId: number
  physioName: string
  lastMessagePreview: string | null
  lastMessageAt: string | null
  unreadCount: number
}

export interface SendMessageRequest {
  body: string
  videoSubmissionId?: number
}

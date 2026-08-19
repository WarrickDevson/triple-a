import { apiClient } from './client'
import type { Message, MessageThread, SendMessageRequest } from '../types/message'

export async function fetchMessageThreads(): Promise<MessageThread[]> {
  const { data } = await apiClient.get<MessageThread[]>('/api/messages/threads')
  return data
}

export async function fetchPetMessages(petId: number): Promise<Message[]> {
  const { data } = await apiClient.get<Message[]>(`/api/pets/${petId}/messages`)
  return data
}

export async function sendPetMessage(petId: number, request: SendMessageRequest): Promise<Message> {
  const { data } = await apiClient.post<Message>(`/api/pets/${petId}/messages`, request)
  return data
}

export async function markMessageRead(messageId: number): Promise<Message> {
  const { data } = await apiClient.put<Message>(`/api/messages/${messageId}/read`)
  return data
}

export async function uploadMessageAttachment(
  file: File,
): Promise<{ attachmentUrl: string; attachmentName: string; attachmentType: string }> {
  const formData = new FormData()
  formData.append('file', file)
  const { data } = await apiClient.post<{
    attachmentUrl: string
    attachmentName: string
    attachmentType: string
  }>('/api/messages/attachments/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

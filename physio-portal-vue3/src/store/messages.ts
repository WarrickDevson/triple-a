import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import {
  fetchMessageThreads,
  fetchPetMessages,
  markMessageRead,
  sendPetMessage,
} from '../api/messages'
import type { Message, MessageThread, SendMessageRequest } from '../types/message'

export const useMessagesStore = defineStore('messages', () => {
  const threads = ref<MessageThread[]>([])
  const activeMessages = ref<Message[]>([])
  const activePetId = ref<number | null>(null)
  const loading = ref(false)
  const sending = ref(false)
  const error = ref<string | null>(null)

  async function loadThreads() {
    loading.value = true
    error.value = null
    try {
      threads.value = await fetchMessageThreads()
    } catch {
      error.value = 'Unable to load message threads.'
    } finally {
      loading.value = false
    }
  }

  async function openThread(petId: number) {
    activePetId.value = petId
    loading.value = true
    error.value = null
    try {
      activeMessages.value = await fetchPetMessages(petId)
      await loadThreads()
    } catch {
      error.value = 'Unable to load messages.'
    } finally {
      loading.value = false
    }
  }

  async function sendMessage(request: SendMessageRequest) {
    if (activePetId.value === null) return
    sending.value = true
    error.value = null
    try {
      const message = await sendPetMessage(activePetId.value, request)
      activeMessages.value = [...activeMessages.value, message]
      await loadThreads()
      return message
    } catch {
      error.value = 'Unable to send message.'
      throw new Error(error.value)
    } finally {
      sending.value = false
    }
  }

  async function markAsRead(messageId: number) {
    await markMessageRead(messageId)
    activeMessages.value = activeMessages.value.map((message) =>
      message.messageId === messageId
        ? { ...message, readAt: new Date().toISOString() }
        : message,
    )
    await loadThreads()
  }

  return {
    threads,
    activeMessages,
    activePetId,
    loading,
    sending,
    error,
    totalUnreadCount: computed(() =>
      threads.value.reduce((sum, thread) => sum + thread.unreadCount, 0),
    ),
    loadThreads,
    openThread,
    sendMessage,
    markAsRead,
  }
})

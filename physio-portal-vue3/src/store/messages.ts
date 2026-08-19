import * as signalR from '@microsoft/signalr'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import {
  fetchMessageThreads,
  fetchPetMessages,
  markMessageRead,
  sendPetMessage,
} from '../api/messages'
import { API_BASE_URL } from '../api/config'
import { useAuthStore } from './auth'
import type { Message, MessageThread, SendMessageRequest } from '../types/message'

export interface MessageToastNotification {
  id: string
  petId: number
  petName: string
  ownerName: string
  message: string
}

export const useMessagesStore = defineStore('messages', () => {
  const threads = ref<MessageThread[]>([])
  const activeMessages = ref<Message[]>([])
  const activePetId = ref<number | null>(null)
  const loading = ref(false)
  const sending = ref(false)
  const error = ref<string | null>(null)
  const activeNotification = ref<MessageToastNotification | null>(null)

  let pollInterval: ReturnType<typeof setInterval> | null = null
  let hubConnection: signalR.HubConnection | null = null

  function triggerNotification(notif: Omit<MessageToastNotification, 'id'>) {
    const id = Date.now().toString()
    activeNotification.value = { ...notif, id }
    setTimeout(() => {
      if (activeNotification.value?.id === id) {
        activeNotification.value = null
      }
    }, 6000)
  }

  function dismissNotification() {
    activeNotification.value = null
  }

  async function loadThreads(silent = false) {
    if (!silent) loading.value = true
    error.value = null
    try {
      const updated = await fetchMessageThreads()
      if (silent && threads.value.length > 0) {
        for (const newThread of updated) {
          const oldThread = threads.value.find((t) => t.petId === newThread.petId)
          if (
            oldThread &&
            newThread.unreadCount > oldThread.unreadCount &&
            newThread.petId !== activePetId.value
          ) {
            triggerNotification({
              petId: newThread.petId,
              petName: newThread.petName,
              ownerName: newThread.ownerName,
              message: newThread.lastMessagePreview || 'New message received',
            })
          }
        }
      }
      threads.value = updated
    } catch {
      error.value = 'Unable to load message threads.'
    } finally {
      if (!silent) loading.value = false
    }
  }

  async function initSignalR() {
    const auth = useAuthStore()
    if (!auth.accessToken || hubConnection) return

    const hubUrl = `${API_BASE_URL.replace(/\/+$/, '')}/hubs/chat`

    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => auth.accessToken || '',
      })
      .withAutomaticReconnect()
      .build()

    hubConnection.on('ReceiveMessage', (message: Message) => {
      if (activePetId.value !== null) {
        const exists = activeMessages.value.some((m) => m.messageId === message.messageId)
        if (!exists) {
          activeMessages.value = [...activeMessages.value, message]
        }
      }
      loadThreads(true)
    })

    hubConnection.on('MessageRead', (data: { messageId: number; readAt: string }) => {
      activeMessages.value = activeMessages.value.map((m) =>
        m.messageId === data.messageId ? { ...m, readAt: data.readAt } : m,
      )
    })

    hubConnection.on('ThreadUpdated', () => {
      loadThreads(true)
    })

    hubConnection.onclose(() => {
      startPolling()
    })

    try {
      await hubConnection.start()
      if (activePetId.value !== null && hubConnection.state === signalR.HubConnectionState.Connected) {
        await hubConnection.invoke('JoinPetThread', activePetId.value)
      }
      stopPolling()
    } catch {
      startPolling()
    }
  }

  async function openThread(petId: number) {
    if (
      activePetId.value &&
      activePetId.value !== petId &&
      hubConnection &&
      hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      await hubConnection.invoke('LeavePetThread', activePetId.value).catch(() => undefined)
    }

    activePetId.value = petId
    loading.value = true
    error.value = null
    try {
      activeMessages.value = await fetchPetMessages(petId)
      threads.value = threads.value.map((t) =>
        t.petId === petId ? { ...t, unreadCount: 0 } : t,
      )
      if (hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
        await hubConnection.invoke('JoinPetThread', petId).catch(() => undefined)
      }
    } catch {
      error.value = 'Unable to load messages.'
    } finally {
      loading.value = false
    }

    if (!hubConnection) {
      await initSignalR().catch(() => undefined)
    }
  }

  function startPolling() {
    if (pollInterval) return
    pollInterval = setInterval(async () => {
      await loadThreads(true)
      if (activePetId.value !== null) {
        try {
          const newMessages = await fetchPetMessages(activePetId.value)
          if (newMessages.length !== activeMessages.value.length) {
            activeMessages.value = newMessages
          }
          threads.value = threads.value.map((t) =>
            t.petId === activePetId.value ? { ...t, unreadCount: 0 } : t,
          )
        } catch {
          // ignore background errors
        }
      }
    }, 4000)
  }

  function stopPolling() {
    if (pollInterval) {
      clearInterval(pollInterval)
      pollInterval = null
    }
  }

  async function sendMessage(request: SendMessageRequest) {
    if (activePetId.value === null) return
    sending.value = true
    error.value = null
    try {
      const message = await sendPetMessage(activePetId.value, request)
      const exists = activeMessages.value.some((m) => m.messageId === message.messageId)
      if (!exists) {
        activeMessages.value = [...activeMessages.value, message]
      }
      await loadThreads(true)
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
    if (activePetId.value !== null) {
      threads.value = threads.value.map((t) =>
        t.petId === activePetId.value ? { ...t, unreadCount: 0 } : t,
      )
    }
    await loadThreads(true)
  }

  return {
    threads,
    activeMessages,
    activePetId,
    loading,
    sending,
    error,
    activeNotification,
    totalUnreadCount: computed(() =>
      threads.value.reduce((sum, thread) => sum + thread.unreadCount, 0),
    ),
    loadThreads,
    openThread,
    sendMessage,
    markAsRead,
    startPolling,
    stopPolling,
    dismissNotification,
    initSignalR,
  }
})

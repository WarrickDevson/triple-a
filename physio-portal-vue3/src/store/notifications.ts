import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { loadNotificationSettings } from '../data/settingsDemo'
import { useDashboardStore } from './dashboard'
import { useMessagesStore } from './messages'

export interface PortalNotification {
  id: string
  title: string
  message: string
  timeAgo: string
  type: 'message' | 'video' | 'appointment' | 'task'
  read: boolean
  linkName?: string
  linkParams?: Record<string, any>
}

export const useNotificationsStore = defineStore('notifications', () => {
  const readIds = ref<Set<string>>(new Set())

  const customNotifications = ref<PortalNotification[]>([
    {
      id: 'notif-demo-1',
      title: 'Video Review Needed',
      message: 'Buster has submitted a new gait assessment video.',
      timeAgo: '10m ago',
      type: 'video',
      read: false,
      linkName: 'patients',
    },
    {
      id: 'notif-demo-2',
      title: 'Upcoming Appointment',
      message: 'Luna - Post-op Knee Checkup at 14:00 today.',
      timeAgo: '1h ago',
      type: 'appointment',
      read: false,
      linkName: 'appointments',
    },
  ])

  const settings = ref(loadNotificationSettings())

  function reloadSettings() {
    settings.value = loadNotificationSettings()
  }

  const allNotifications = computed<PortalNotification[]>(() => {
    const list: PortalNotification[] = []
    const messagesStore = useMessagesStore()
    const dashboardStore = useDashboardStore()

    // 1. Unread Message Threads
    if (settings.value.inAppMessages) {
      for (const thread of messagesStore.threads) {
        if (thread.unreadCount > 0) {
          const id = `msg-thread-${thread.petId}`
          list.push({
            id,
            title: `New message from ${thread.ownerName}`,
            message: `${thread.petName}: "${thread.lastMessagePreview || 'New message received'}"`,
            timeAgo: 'Just now',
            type: 'message',
            read: readIds.value.has(id),
            linkName: 'messages',
          })
        }
      }
    }

    // 2. Pending Video Reviews
    if (settings.value.inAppVideoReviews && (dashboardStore.dashboard?.pendingVideoReviews ?? 0) > 0) {
      const count = dashboardStore.dashboard?.pendingVideoReviews
      const id = 'pending-video-reviews'
      list.push({
        id,
        title: 'Video Reviews Pending',
        message: `${count} patient video submission${count! > 1 ? 's' : ''} waiting for review.`,
        timeAgo: 'Today',
        type: 'video',
        read: readIds.value.has(id),
        linkName: 'patients',
      })
    }

    // 3. Custom / Demo / Actionable notifications
    for (const item of customNotifications.value) {
      if (item.type === 'appointment' && !settings.value.inAppAppointments) continue
      if (item.type === 'message' && !settings.value.inAppMessages) continue
      if (item.type === 'video' && !settings.value.inAppVideoReviews) continue
      
      list.push({
        ...item,
        read: readIds.value.has(item.id) || item.read,
      })
    }

    return list
  })

  const unreadCount = computed(() => {
    return allNotifications.value.filter((n) => !n.read).length
  })

  function markAsRead(id: string) {
    readIds.value.add(id)
    const item = customNotifications.value.find((n) => n.id === id)
    if (item) {
      item.read = true
    }
    // If marking video review as read, also mark all video notifications as read
    if (id === 'pending-video-reviews' || id === 'notif-demo-1') {
      readIds.value.add('pending-video-reviews')
      readIds.value.add('notif-demo-1')
    }
  }

  function markVideoReviewsAsRead() {
    readIds.value.add('pending-video-reviews')
    readIds.value.add('notif-demo-1')
    for (const item of customNotifications.value) {
      if (item.type === 'video') {
        item.read = true
      }
    }
  }

  function markAllAsRead() {
    for (const item of customNotifications.value) {
      item.read = true
    }
    for (const notif of allNotifications.value) {
      readIds.value.add(notif.id)
    }
    const messagesStore = useMessagesStore()
    for (const thread of messagesStore.threads) {
      thread.unreadCount = 0
    }
  }

  function removeNotification(id: string) {
    customNotifications.value = customNotifications.value.filter((n) => n.id !== id)
  }

  return {
    customNotifications,
    allNotifications,
    unreadCount,
    reloadSettings,
    markAsRead,
    markVideoReviewsAsRead,
    markAllAsRead,
    removeNotification,
  }
})

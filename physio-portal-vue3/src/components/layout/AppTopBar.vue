<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Bell,
  Calendar,
  CheckCheck,
  CircleHelp,
  MessageSquare,
  Search,
  Video,
  X,
} from '@lucide/vue'
import { useAuthStore } from '../../store/auth'
import { type PortalNotification, useNotificationsStore } from '../../store/notifications'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const notificationsStore = useNotificationsStore()

const pageTitle = computed(() => (route.meta.title as string) ?? 'Dashboard')
const isPopoverOpen = ref(false)
const popoverRef = ref<HTMLElement | null>(null)

function togglePopover() {
  isPopoverOpen.value = !isPopoverOpen.value
}

function closePopover() {
  isPopoverOpen.value = false
}

function handleNotificationClick(notif: PortalNotification) {
  notificationsStore.markAsRead(notif.id)
  if (notif.type === 'video') {
    notificationsStore.markVideoReviewsAsRead()
  }
  closePopover()
  if (notif.linkName) {
    router.push({ name: notif.linkName, params: notif.linkParams })
  }
}

function iconForType(type: PortalNotification['type']) {
  if (type === 'video') return Video
  if (type === 'appointment') return Calendar
  return MessageSquare
}

function handleClickOutside(event: MouseEvent) {
  if (popoverRef.value && !popoverRef.value.contains(event.target as Node)) {
    closePopover()
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    closePopover()
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <header class="sticky top-0 z-20 border-b border-navy/8 bg-surface/95 backdrop-blur-md">
    <div class="flex flex-wrap items-center gap-4 px-6 py-4 lg:px-8">
      <h1 class="shrink-0 text-xl font-bold text-navy sm:text-2xl">{{ pageTitle }}</h1>

      <div class="relative mx-auto hidden max-w-md flex-1 md:block">
        <Search class="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          type="search"
          placeholder="Search patients, owners, plans..."
          class="w-full rounded-full border border-neutral-grey bg-white py-2.5 pl-11 pr-4 text-sm text-neutral-dark outline-none transition-colors placeholder:text-neutral-muted/70 focus:border-sage focus:ring-2 focus:ring-sage/15"
        />
      </div>

      <div ref="popoverRef" class="relative ml-auto flex items-center gap-2">
        <!-- Notification Bell Button -->
        <button
          type="button"
          class="relative flex h-10 w-10 items-center justify-center rounded-full text-neutral-muted transition-colors hover:bg-navy/5 hover:text-navy"
          :class="{ 'bg-navy/5 text-navy': isPopoverOpen }"
          aria-label="Notifications"
          @click="togglePopover"
        >
          <Bell class="h-5 w-5" :stroke-width="1.75" />
          <span
            v-if="notificationsStore.unreadCount > 0"
            class="absolute top-1 right-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-accent-amber px-1 text-[9px] font-bold text-navy"
          >
            {{ notificationsStore.unreadCount }}
          </span>
        </button>

        <!-- Floating Notification Dropdown Popover -->
        <Transition
          enter-active-class="transition duration-150 ease-out"
          enter-from-class="opacity-0 scale-95 -translate-y-1"
          enter-to-class="opacity-100 scale-100 translate-y-0"
          leave-active-class="transition duration-100 ease-in"
          leave-from-class="opacity-100 scale-100 translate-y-0"
          leave-to-class="opacity-0 scale-95 -translate-y-1"
        >
          <div
            v-if="isPopoverOpen"
            class="absolute right-0 top-12 z-50 w-80 sm:w-96 rounded-2xl border border-navy/10 bg-white p-4 shadow-xl"
          >
            <!-- Popover Header -->
            <div class="flex items-center justify-between border-b border-navy/6 pb-3">
              <div class="flex items-center gap-2">
                <h3 class="text-sm font-bold text-navy">Notifications</h3>
                <span
                  v-if="notificationsStore.unreadCount > 0"
                  class="rounded-full bg-sage/15 px-2 py-0.5 text-[11px] font-semibold text-sage"
                >
                  {{ notificationsStore.unreadCount }} new
                </span>
              </div>

              <div class="flex items-center gap-1">
                <button
                  v-if="notificationsStore.unreadCount > 0"
                  type="button"
                  class="flex items-center gap-1 text-xs font-semibold text-sage hover:text-sage-dark transition-colors"
                  @click="notificationsStore.markAllAsRead"
                >
                  <CheckCheck class="h-3.5 w-3.5" />
                  Mark read
                </button>
                <button
                  type="button"
                  class="rounded-lg p-1 text-neutral-muted hover:bg-surface hover:text-navy"
                  @click="closePopover"
                >
                  <X class="h-4 w-4" />
                </button>
              </div>
            </div>

            <!-- Notifications List -->
            <div class="mt-2 max-h-80 overflow-y-auto space-y-1 pr-1">
              <div
                v-for="notif in notificationsStore.allNotifications"
                :key="notif.id"
                class="group relative flex items-start gap-3 rounded-xl p-3 transition-colors hover:bg-surface cursor-pointer"
                :class="notif.read ? 'opacity-70' : 'bg-surface/50 font-medium'"
                @click="handleNotificationClick(notif)"
              >
                <!-- Notification Icon -->
                <div
                  class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-sage/15 text-sage"
                >
                  <component :is="iconForType(notif.type)" class="h-4 w-4" :stroke-width="2" />
                </div>

                <!-- Text Content -->
                <div class="min-w-0 flex-1">
                  <div class="flex items-center justify-between gap-1">
                    <p class="text-xs font-bold text-navy truncate">{{ notif.title }}</p>
                    <span class="text-[10px] text-neutral-muted shrink-0">{{ notif.timeAgo }}</span>
                  </div>
                  <p class="mt-0.5 text-xs text-neutral-dark/80 line-clamp-2 leading-relaxed">
                    {{ notif.message }}
                  </p>
                </div>

                <!-- Unread Indicator Dot -->
                <span
                  v-if="!notif.read"
                  class="absolute top-3 right-2 h-2 w-2 rounded-full bg-sage"
                ></span>
              </div>

              <!-- Empty State -->
              <div
                v-if="notificationsStore.allNotifications.length === 0"
                class="py-8 text-center text-xs text-neutral-muted"
              >
                <Bell class="mx-auto h-8 w-8 text-neutral-muted/40 mb-2" />
                <p class="font-semibold text-navy">All caught up!</p>
                <p class="mt-0.5">No active notifications at this time.</p>
              </div>
            </div>

            <!-- Popover Footer -->
            <div class="mt-3 border-t border-navy/6 pt-2 text-center">
              <button
                type="button"
                class="text-xs font-semibold text-sage hover:underline"
                @click="router.push({ name: 'messages' }); closePopover()"
              >
                View all messages & alerts →
              </button>
            </div>
          </div>
        </Transition>

        <!-- Help Button -->
        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-full text-neutral-muted transition-colors hover:bg-navy/5 hover:text-navy"
          aria-label="Help"
        >
          <CircleHelp class="h-5 w-5" :stroke-width="1.75" />
        </button>

        <!-- User Initials Avatar -->
        <div
          v-if="auth.user"
          class="flex h-9 w-9 items-center justify-center rounded-full bg-sage/20 text-xs font-bold text-sage"
          :title="`${auth.user.firstName} ${auth.user.lastName}`"
        >
          {{ auth.user.firstName?.[0] }}{{ auth.user.lastName?.[0] }}
        </div>
      </div>
    </div>
  </header>
</template>

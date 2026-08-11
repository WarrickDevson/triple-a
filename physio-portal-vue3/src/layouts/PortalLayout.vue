<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { MessageSquare, Menu, X } from '@lucide/vue'
import UnverifiedAccountBanner from '../components/UnverifiedAccountBanner.vue'
import AppSidebar from '../components/layout/AppSidebar.vue'
import AppTopBar from '../components/layout/AppTopBar.vue'
import { useMessagesStore } from '../store/messages'

const mobileNavOpen = ref(false)
const router = useRouter()
const messagesStore = useMessagesStore()

function closeMobileNav() {
  mobileNavOpen.value = false
}

function openNotifThread(petId: number) {
  messagesStore.dismissNotification()
  router.push({ name: 'message-thread', params: { petId } })
}
</script>

<template>
  <div class="flex min-h-screen bg-surface">
    <AppSidebar class="fixed inset-y-0 left-0 z-30 hidden lg:flex" />

    <!-- Mobile sidebar overlay -->
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      leave-active-class="transition-opacity duration-200"
      leave-to-class="opacity-0"
    >
      <div
        v-if="mobileNavOpen"
        class="fixed inset-0 z-40 bg-navy/50 lg:hidden"
        @click="closeMobileNav"
      />
    </Transition>

    <Transition
      enter-active-class="transition-transform duration-200"
      enter-from-class="-translate-x-full"
      leave-active-class="transition-transform duration-200"
      leave-to-class="-translate-x-full"
    >
      <AppSidebar
        v-if="mobileNavOpen"
        class="fixed inset-y-0 left-0 z-50 lg:hidden"
        @click="closeMobileNav"
      />
    </Transition>

    <div class="flex min-h-screen flex-1 flex-col lg:pl-[240px]">
      <div class="sticky top-0 z-20 flex items-center gap-3 border-b border-navy/8 bg-surface/95 px-4 py-3 backdrop-blur-md lg:hidden">
        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-lg text-navy hover:bg-navy/5"
          aria-label="Open navigation"
          @click="mobileNavOpen = true"
        >
          <Menu class="h-5 w-5" :stroke-width="1.75" />
        </button>
        <span class="text-sm font-semibold text-navy">Triple A</span>
        <button
          v-if="mobileNavOpen"
          type="button"
          class="ml-auto flex h-10 w-10 items-center justify-center rounded-lg text-navy hover:bg-navy/5"
          aria-label="Close navigation"
          @click="closeMobileNav"
        >
          <X class="h-5 w-5" :stroke-width="1.75" />
        </button>
      </div>

      <UnverifiedAccountBanner />
      <AppTopBar />
      <main class="flex-1 px-4 py-6 sm:px-6 lg:px-8">
        <RouterView />
      </main>
    </div>

    <!-- Floating Message Toast Notification -->
    <Transition
      enter-active-class="transition duration-300 ease-out transform"
      enter-from-class="translate-y-4 opacity-0 sm:translate-y-0 sm:translate-x-4"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
      leave-active-class="transition duration-200 ease-in transform"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="messagesStore.activeNotification"
        class="fixed bottom-6 right-6 z-50 flex max-w-sm cursor-pointer items-start gap-3.5 rounded-2xl border border-sage/30 bg-white p-4 shadow-2xl transition-all hover:border-sage hover:shadow-sage/10"
        @click="openNotifThread(messagesStore.activeNotification.petId)"
      >
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-sage/20 text-sage">
          <MessageSquare class="h-5 w-5" :stroke-width="2" />
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between gap-2">
            <span class="text-[10px] font-bold uppercase tracking-wider text-sage">New Message</span>
            <button
              type="button"
              class="text-neutral-muted hover:text-navy text-xs p-0.5"
              @click.stop="messagesStore.dismissNotification()"
            >
              <X class="h-3.5 w-3.5" />
            </button>
          </div>
          <p class="text-xs font-bold text-navy truncate mt-0.5">
            {{ messagesStore.activeNotification.petName }} ({{ messagesStore.activeNotification.ownerName }})
          </p>
          <p class="text-xs text-neutral-dark/80 truncate mt-0.5">
            "{{ messagesStore.activeNotification.message }}"
          </p>
        </div>
      </div>
    </Transition>
  </div>
</template>

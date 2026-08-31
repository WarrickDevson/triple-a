<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { MessageSquare, Menu, X, Sparkles, Loader2, AlertCircle } from '@lucide/vue'
import UnverifiedAccountBanner from '../components/UnverifiedAccountBanner.vue'
import AppSidebar from '../components/layout/AppSidebar.vue'
import AppTopBar from '../components/layout/AppTopBar.vue'
import PwaInstallButton from '../components/PwaInstallButton.vue'
import PwaReloadPrompt from '../components/PwaReloadPrompt.vue'
import { useMessagesStore } from '../store/messages'
import { useVoiceSessionStore, type VoiceSessionNotification } from '../store/voiceSession'

const mobileNavOpen = ref(false)
const router = useRouter()
const messagesStore = useMessagesStore()
const voiceSessionStore = useVoiceSessionStore()

function closeMobileNav() {
  mobileNavOpen.value = false
}

function openNotifThread(petId: number) {
  messagesStore.dismissNotification()
  router.push({ name: 'message-thread', params: { petId } })
}

function openVoiceSessionReview(notif: VoiceSessionNotification) {
  voiceSessionStore.triggerReviewFromNotification(notif)
  router.push({
    name: 'patient-detail',
    params: { petId: notif.petId },
    query: { openSoap: 'true' }
  })
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
        <div class="ml-auto flex items-center gap-2">
          <PwaInstallButton compact />
          <button
            v-if="mobileNavOpen"
            type="button"
            class="flex h-10 w-10 items-center justify-center rounded-lg text-navy hover:bg-navy/5"
            aria-label="Close navigation"
            @click="closeMobileNav"
          >
            <X class="h-5 w-5" :stroke-width="1.75" />
          </button>
        </div>
      </div>

      <UnverifiedAccountBanner />
      <AppTopBar />
      <main class="flex-1 px-4 py-6 sm:px-6 lg:px-8">
        <RouterView />
      </main>
    </div>

    <!-- 1. Floating Voice Session AI PROCESSING Dialog / Toast -->
    <Transition
      enter-active-class="transition duration-300 ease-out transform"
      enter-from-class="translate-y-4 opacity-0 sm:translate-y-0 sm:translate-x-4"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
      leave-active-class="transition duration-200 ease-in transform"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="voiceSessionStore.activeJob?.status === 'processing'"
        class="fixed bottom-6 right-6 z-50 flex max-w-sm items-start gap-3.5 rounded-2xl border-2 border-purple-400 bg-white p-4 shadow-2xl transition-all ring-4 ring-purple-100"
      >
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-purple-100 text-purple-700">
          <Loader2 class="h-5 w-5 animate-spin" :stroke-width="2.5" />
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between gap-2">
            <span class="text-[10px] font-bold uppercase tracking-wider text-purple-700 flex items-center gap-1.5">
              <span class="h-2 w-2 rounded-full bg-purple-600 animate-ping"></span>
              Processing SOAP with AI
            </span>
            <button
              type="button"
              class="text-neutral-muted hover:text-navy text-xs p-0.5"
              title="Dismiss banner"
              @click.stop="voiceSessionStore.clearActiveJob()"
            >
              <X class="h-3.5 w-3.5" />
            </button>
          </div>
          <p class="text-xs font-bold text-navy truncate mt-0.5">
            {{ voiceSessionStore.activeJob.petName }}
          </p>
          <p class="text-xs text-neutral-dark/80 mt-0.5">
            Gemini AI is transcribing audio and structuring into Subjective, Objective, Action, and Plan...
          </p>
          <div class="mt-2.5 w-full bg-purple-100 h-1.5 rounded-full overflow-hidden">
            <div class="bg-purple-600 h-full w-full rounded-full animate-pulse"></div>
          </div>
        </div>
      </div>
    </Transition>

    <!-- 2. Floating Voice Session AI Processing COMPLETED Toast Notification -->
    <Transition
      enter-active-class="transition duration-300 ease-out transform"
      enter-from-class="translate-y-4 opacity-0 sm:translate-y-0 sm:translate-x-4"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
      leave-active-class="transition duration-200 ease-in transform"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="voiceSessionStore.activeNotification && voiceSessionStore.activeJob?.status !== 'processing'"
        class="fixed bottom-6 right-6 z-50 flex max-w-sm cursor-pointer items-start gap-3.5 rounded-2xl border-2 border-purple-400 bg-white p-4 shadow-2xl transition-all hover:border-purple-600 hover:shadow-purple-500/20"
        @click="openVoiceSessionReview(voiceSessionStore.activeNotification)"
      >
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-purple-100 text-purple-700">
          <Sparkles class="h-5 w-5 animate-pulse" :stroke-width="2" />
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between gap-2">
            <span class="text-[10px] font-bold uppercase tracking-wider text-purple-700">✨ SOAP Note Saved</span>
            <button
              type="button"
              class="text-neutral-muted hover:text-navy text-xs p-0.5"
              @click.stop="voiceSessionStore.dismissNotification()"
            >
              <X class="h-3.5 w-3.5" />
            </button>
          </div>
          <p class="text-xs font-bold text-navy truncate mt-0.5">
            {{ voiceSessionStore.activeNotification.petName }}
          </p>
          <p class="text-xs text-neutral-dark/80 line-clamp-2 mt-0.5">
            Voice session transcribed & structured into 4-quadrant SOAP note. Tap to review.
          </p>
          <div class="mt-2 flex items-center gap-2">
            <button
              type="button"
              class="inline-flex items-center gap-1 rounded-lg bg-purple-600 px-2.5 py-1 text-[11px] font-bold text-white hover:bg-purple-700 transition-colors"
            >
              View & Edit Note →
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- 3. Floating Voice Session AI ERROR Toast Notification -->
    <Transition
      enter-active-class="transition duration-300 ease-out transform"
      enter-from-class="translate-y-4 opacity-0 sm:translate-y-0 sm:translate-x-4"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
      leave-active-class="transition duration-200 ease-in transform"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="voiceSessionStore.activeJob?.status === 'error'"
        class="fixed bottom-6 right-6 z-50 flex max-w-sm items-start gap-3.5 rounded-2xl border-2 border-rose-400 bg-white p-4 shadow-2xl transition-all"
      >
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-rose-100 text-rose-700">
          <AlertCircle class="h-5 w-5" :stroke-width="2" />
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between gap-2">
            <span class="text-[10px] font-bold uppercase tracking-wider text-rose-700">⚠️ SOAP Processing Failed</span>
            <button
              type="button"
              class="text-neutral-muted hover:text-navy text-xs p-0.5"
              @click.stop="voiceSessionStore.activeJob = null"
            >
              <X class="h-3.5 w-3.5" />
            </button>
          </div>
          <p class="text-xs text-rose-900/80 mt-1">
            {{ voiceSessionStore.activeJob.errorMessage || 'Could not process audio session with AI.' }}
          </p>
        </div>
      </div>
    </Transition>

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
        class="fixed bottom-24 right-6 z-50 flex max-w-sm cursor-pointer items-start gap-3.5 rounded-2xl border border-sage/30 bg-white p-4 shadow-2xl transition-all hover:border-sage hover:shadow-sage/10"
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

    <!-- PWA Service Worker Update Prompt -->
    <PwaReloadPrompt />
  </div>
</template>

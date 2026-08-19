<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { RouterLink } from 'vue-router'
import MessageComposer from './MessageComposer.vue'
import { API_BASE_URL } from '../../api/config'
import { useAuthStore } from '../../store/auth'
import type { Message, MessageThread } from '../../types/message'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  thread: MessageThread | null
  patient?: Pet | null
  selectedPetId?: number | null
  messages: Message[]
  loading?: boolean
}>()

const auth = useAuthStore()

const activePetId = computed(() => props.thread?.petId ?? props.patient?.petId ?? props.selectedPetId ?? null)

const headerTitle = computed(() => {
  if (props.thread) return `${props.thread.petName} / Owner: ${props.thread.ownerName}`
  if (props.patient) return `${props.patient.petName} / Owner: ${props.patient.ownerName}`
  return 'Select a conversation'
})

function isOutgoing(message: Message) {
  return message.senderUserId === auth.user?.userId
}

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function resolveMediaUrl(path: string | null | undefined): string | null {
  if (!path) return null
  if (path.startsWith('http://') || path.startsWith('https://')) return path
  return `${API_BASE_URL.replace(/\/+$/, '')}${path.startsWith('/') ? path : `/${path}`}`
}

function isImageAttachment(type?: string | null, url?: string | null) {
  if (type?.startsWith('image/')) return true
  if (!url) return false
  const lower = url.toLowerCase()
  return lower.endsWith('.png') || lower.endsWith('.jpg') || lower.endsWith('.jpeg') || lower.endsWith('.webp') || lower.endsWith('.gif')
}

onMounted(() => {
  scrollToBottom()
})

watch(
  () => props.messages.length,
  () => scrollToBottom(),
)

function scrollToBottom() {
  requestAnimationFrame(() => {
    const el = document.getElementById('chat-messages-end')
    el?.scrollIntoView({ behavior: 'smooth' })
  })
}
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="flex items-center justify-between border-b border-neutral-grey/80 px-4 py-3">
      <div>
        <h2 class="text-sm font-bold text-navy">{{ headerTitle }}</h2>
        <p v-if="activePetId" class="text-xs text-success-green">Online</p>
      </div>
      <RouterLink
        v-if="activePetId"
        :to="{ name: 'patient-detail', params: { petId: activePetId } }"
        class="text-xs font-semibold text-sage hover:text-navy"
      >
        Patient Profile
      </RouterLink>
    </div>

    <div v-if="!activePetId" class="flex flex-1 items-center justify-center p-8">
      <p class="text-sm text-neutral-muted">Select a conversation to start messaging.</p>
    </div>

    <template v-else>
      <div class="flex-1 space-y-3 overflow-y-auto p-4">
        <div v-if="loading" class="text-center text-sm text-neutral-muted">Loading messages...</div>
        <div v-else-if="messages.length === 0" class="py-12 text-center text-sm text-neutral-muted">
          No messages in this conversation yet. Type below to send a message.
        </div>
        <div
          v-for="message in messages"
          :key="message.messageId"
          class="flex"
          :class="isOutgoing(message) ? 'justify-end' : 'justify-start'"
        >
          <div
            class="max-w-[80%] rounded-2xl px-4 py-2.5 text-sm"
            :class="
              isOutgoing(message)
                ? 'rounded-br-md bg-sage text-white'
                : 'rounded-bl-md bg-neutral-grey/60 text-navy'
            "
          >
            <!-- Video Attachment Card inside Message Bubble -->
            <div
              v-if="message.videoSubmissionId"
              class="mb-2 flex items-center gap-2 rounded-xl p-2.5 text-xs shadow-xs"
              :class="isOutgoing(message) ? 'bg-white/15 text-white border border-white/20' : 'bg-white text-navy border border-neutral-grey/60'"
            >
              <span class="text-lg">🎥</span>
              <div class="flex-1 min-w-0">
                <p class="font-bold truncate">Attached Video Submission</p>
                <p class="text-[10px]" :class="isOutgoing(message) ? 'text-white/80' : 'text-neutral-muted'">
                  Video Submission #{{ message.videoSubmissionId }}
                </p>
              </div>
              <RouterLink
                v-if="activePetId"
                :to="{ name: 'patient-detail', params: { petId: activePetId }, query: { tab: 'videos', videoId: message.videoSubmissionId } }"
                class="rounded-lg px-2 py-1 text-[11px] font-bold transition hover:underline"
                :class="isOutgoing(message) ? 'bg-white text-sage' : 'bg-sage text-white'"
              >
                View
              </RouterLink>
            </div>

            <!-- Direct File / Image Attachment Card -->
            <div
              v-if="message.attachmentUrl"
              class="mb-2 rounded-xl p-2 text-xs"
              :class="isOutgoing(message) ? 'bg-white/15 text-white border border-white/20' : 'bg-white text-navy border border-neutral-grey/60'"
            >
              <!-- Image Thumbnail Preview -->
              <div v-if="isImageAttachment(message.attachmentType, message.attachmentUrl)" class="overflow-hidden rounded-lg">
                <a :href="resolveMediaUrl(message.attachmentUrl)!" target="_blank" rel="noopener noreferrer">
                  <img
                    :src="resolveMediaUrl(message.attachmentUrl)!"
                    :alt="message.attachmentName || 'Attachment image'"
                    class="max-h-48 w-full object-cover rounded-lg transition hover:opacity-90"
                  />
                </a>
              </div>
              <!-- Generic File Card -->
              <div v-else class="flex items-center gap-2">
                <span class="text-lg">📄</span>
                <div class="flex-1 min-w-0">
                  <p class="font-bold truncate">{{ message.attachmentName || 'Attachment File' }}</p>
                </div>
                <a
                  :href="resolveMediaUrl(message.attachmentUrl)!"
                  target="_blank"
                  rel="noopener noreferrer"
                  download
                  class="rounded-lg px-2 py-1 text-[11px] font-bold transition hover:underline"
                  :class="isOutgoing(message) ? 'bg-white text-sage' : 'bg-sage text-white'"
                >
                  Open ↗
                </a>
              </div>
            </div>

            <p>{{ message.body }}</p>
            <p
              class="mt-1 text-[10px]"
              :class="isOutgoing(message) ? 'text-white/70' : 'text-neutral-muted'"
            >
              {{ formatTime(message.createdDate) }}
              <span v-if="isOutgoing(message) && message.readAt"> · Read</span>
            </p>
          </div>
        </div>
        <div id="chat-messages-end" />
      </div>
      <MessageComposer />
    </template>
  </section>
</template>

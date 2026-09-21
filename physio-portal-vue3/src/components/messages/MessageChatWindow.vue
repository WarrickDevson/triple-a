<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { Loader2, Play, X } from '@lucide/vue'
import MessageComposer from './MessageComposer.vue'
import { getPetVideos, resolveMediaUrl } from '../../api/videos'
import { useAuthStore } from '../../store/auth'
import type { Message, MessageThread } from '../../types/message'
import type { Pet } from '../../types/pet'
import { formatSaTime } from '../../utils/dateTime'

const props = defineProps<{
  thread: MessageThread | null
  patient?: Pet | null
  selectedPetId?: number | null
  messages: Message[]
  loading?: boolean
}>()

const auth = useAuthStore()

const activePetId = computed(() => props.thread?.petId ?? props.patient?.petId ?? props.selectedPetId ?? null)

const petName = computed(() => props.thread?.petName ?? props.patient?.petName ?? 'Patient')
const ownerName = computed(() => props.thread?.ownerName ?? props.patient?.ownerName ?? 'Owner')
const petDp = computed(() => props.thread?.petProfilePictureUrl ?? props.patient?.profilePictureUrl ?? null)
const ownerDp = computed(() => props.thread?.ownerProfilePictureUrl ?? props.patient?.ownerProfilePictureUrl ?? null)

function getSenderDp(message: Message) {
  return message.senderProfilePictureUrl || ownerDp.value
}

function openImage(url: string, title: string, subtitle?: string) {
  const resolved = resolveMediaUrl(url)
  if (resolved) {
    previewImage.value = { url: resolved, name: subtitle ? `${title} (${subtitle})` : title }
  }
}

const headerTitle = computed(() => {
  if (props.thread) return `${props.thread.petName} / Owner: ${props.thread.ownerName}`
  if (props.patient) return `${props.patient.petName} / Owner: ${props.patient.ownerName}`
  return 'Select a conversation'
})

// In-chat preview modal states
const previewVideo = ref<{
  id: number
  title: string
  url: string | null
  loading: boolean
  error: string | null
} | null>(null)

const previewImage = ref<{
  url: string
  name: string
} | null>(null)

async function openVideoPreview(videoId: number, title?: string | null) {
  previewVideo.value = {
    id: videoId,
    title: title || `Video #${videoId}`,
    url: null,
    loading: true,
    error: null,
  }

  try {
    if (activePetId.value) {
      const vids = await getPetVideos(activePetId.value)
      const match = vids.find((v) => v.videoSubmissionId === videoId)
      if (match) {
        const raw = match.processedVideoStreamingUrl || match.rawVideoStorageUrl
        if (raw) {
          previewVideo.value.url = resolveMediaUrl(raw)
          previewVideo.value.loading = false
          return
        }
      }
    }
    previewVideo.value.error = 'Video stream could not be loaded.'
    previewVideo.value.loading = false
  } catch {
    previewVideo.value.error = 'Unable to fetch video details.'
    previewVideo.value.loading = false
  }
}

function isOutgoing(message: Message) {
  return message.senderUserId === auth.user?.userId
}

function formatTime(value: string) {
  return formatSaTime(value)
}

function isImageAttachment(type?: string | null, url?: string | null) {
  if (type?.startsWith('image/')) return true
  if (!url) return false
  const cleanUrl = url.split('?')[0].toLowerCase()
  return cleanUrl.endsWith('.png') || cleanUrl.endsWith('.jpg') || cleanUrl.endsWith('.jpeg') || cleanUrl.endsWith('.webp') || cleanUrl.endsWith('.gif') || url.toLowerCase().includes('.png') || url.toLowerCase().includes('.jpg') || url.toLowerCase().includes('.jpeg') || url.toLowerCase().includes('.webp')
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
  <section class="portal-card flex h-full flex-col overflow-hidden relative">
    <div class="flex items-center justify-between border-b border-neutral-grey/80 px-4 py-2.5">
      <div v-if="activePetId" class="flex items-center gap-3 min-w-0">
        <!-- Overlapping Pet & Owner Avatars -->
        <div class="flex items-center shrink-0">
          <div
            class="relative z-10 flex h-10 w-10 shrink-0 cursor-pointer items-center justify-center overflow-hidden rounded-full bg-sage-muted text-xs font-bold text-sage ring-2 ring-sage/30 shadow-xs hover:ring-sage transition"
            :title="`${petName} (Click to view full photo)`"
            @click="petDp ? openImage(petDp, petName, 'Pet Patient') : null"
          >
            <img
              v-if="petDp"
              :src="resolveMediaUrl(petDp)!"
              :alt="petName"
              class="h-full w-full object-cover"
            />
            <span v-else>{{ petName.slice(0, 2).toUpperCase() }}</span>
          </div>

          <div
            class="relative -ml-3 z-20 flex h-9 w-9 shrink-0 cursor-pointer items-center justify-center overflow-hidden rounded-full bg-surface text-[11px] font-bold text-navy ring-2 ring-white shadow-xs hover:ring-sage transition"
            :title="`${ownerName} (Click to view full photo)`"
            @click="ownerDp ? openImage(ownerDp, ownerName, 'Pet Owner') : null"
          >
            <img
              v-if="ownerDp"
              :src="resolveMediaUrl(ownerDp)!"
              :alt="ownerName"
              class="h-full w-full object-cover"
            />
            <span v-else>{{ ownerName.slice(0, 2).toUpperCase() }}</span>
          </div>
        </div>

        <div class="min-w-0">
          <h2 class="text-sm font-bold text-navy truncate">
            {{ petName }} <span class="text-xs font-medium text-neutral-muted">· {{ ownerName }}</span>
          </h2>
          <p class="text-[11px] text-success-green flex items-center gap-1">
            <span class="h-1.5 w-1.5 rounded-full bg-success-green inline-block"></span>
            Active Conversation
          </p>
        </div>
      </div>
      <div v-else>
        <h2 class="text-sm font-bold text-navy">{{ headerTitle }}</h2>
      </div>

      <RouterLink
        v-if="activePetId"
        :to="{ name: 'patient-detail', params: { petId: String(activePetId) } }"
        class="text-xs font-semibold text-sage hover:text-navy shrink-0 ml-2"
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
          class="flex items-end gap-2"
          :class="isOutgoing(message) ? 'justify-end' : 'justify-start'"
        >
          <!-- Owner Avatar for incoming messages -->
          <div
            v-if="!isOutgoing(message)"
            class="flex h-7 w-7 shrink-0 cursor-pointer items-center justify-center overflow-hidden rounded-full bg-sage-muted text-[10px] font-bold text-sage ring-1 ring-sage/20 mb-1"
            :title="`${message.senderName} (Click to view photo)`"
            @click="getSenderDp(message) ? openImage(getSenderDp(message)!, message.senderName, 'Pet Owner') : null"
          >
            <img
              v-if="getSenderDp(message)"
              :src="resolveMediaUrl(getSenderDp(message))!"
              :alt="message.senderName"
              class="h-full w-full object-cover"
            />
            <span v-else>{{ message.senderName.slice(0, 2).toUpperCase() }}</span>
          </div>

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
              class="mb-2 rounded-xl p-2.5 text-xs shadow-xs transition"
              :class="isOutgoing(message) ? 'bg-white/15 text-white border border-white/20' : 'bg-white text-navy border border-neutral-grey/60'"
            >
              <div class="flex items-center gap-2.5">
                <button
                  type="button"
                  class="flex h-8 w-8 shrink-0 items-center justify-center rounded-full transition cursor-pointer"
                  :class="isOutgoing(message) ? 'bg-white/20 text-white hover:bg-white/30' : 'bg-sage/15 text-sage hover:bg-sage/25'"
                  title="Play video in chat"
                  @click="openVideoPreview(message.videoSubmissionId, message.videoTitle)"
                >
                  <Play class="h-4 w-4 fill-current ml-0.5" />
                </button>
                <div
                  class="flex-1 min-w-0 cursor-pointer"
                  @click="openVideoPreview(message.videoSubmissionId, message.videoTitle)"
                >
                  <p class="font-bold truncate hover:underline">{{ message.videoTitle || 'Attached Video Submission' }}</p>
                  <p class="text-[10px]" :class="isOutgoing(message) ? 'text-white/80' : 'text-neutral-muted'">
                    {{ message.videoTitle ? `Video #${message.videoSubmissionId}` : `Video Submission #${message.videoSubmissionId}` }} · Click to watch
                  </p>
                </div>
                <div class="flex items-center gap-1 shrink-0">
                  <button
                    type="button"
                    class="rounded-lg px-2 py-1 text-[11px] font-bold transition hover:opacity-90 cursor-pointer"
                    :class="isOutgoing(message) ? 'bg-white/25 text-white' : 'bg-neutral-grey/60 text-navy hover:bg-neutral-grey'"
                    @click="openVideoPreview(message.videoSubmissionId, message.videoTitle)"
                  >
                    Play
                  </button>
                  <RouterLink
                    v-if="activePetId"
                    :to="{ name: 'patient-detail', params: { petId: String(activePetId) }, query: { tab: 'videos', videoId: String(message.videoSubmissionId) } }"
                    class="rounded-lg px-2 py-1 text-[11px] font-bold transition hover:underline flex items-center gap-0.5"
                    :class="isOutgoing(message) ? 'bg-white text-sage' : 'bg-sage text-white'"
                    title="Open in Patient Videos"
                  >
                    <span>Patient</span>
                    <span class="text-[10px]">↗</span>
                  </RouterLink>
                </div>
              </div>
            </div>

            <!-- Direct File / Image Attachment Card -->
            <div
              v-if="message.attachmentUrl"
              class="mb-2 rounded-xl p-2 text-xs"
              :class="isOutgoing(message) ? 'bg-white/15 text-white border border-white/20' : 'bg-white text-navy border border-neutral-grey/60'"
            >
              <!-- Image Thumbnail Preview -->
              <div
                v-if="isImageAttachment(message.attachmentType, message.attachmentUrl)"
                class="overflow-hidden rounded-lg cursor-pointer"
                @click="previewImage = { url: resolveMediaUrl(message.attachmentUrl)!, name: message.attachmentName || 'Attachment image' }"
              >
                <img
                  :src="resolveMediaUrl(message.attachmentUrl)!"
                  :alt="message.attachmentName || 'Attachment image'"
                  class="max-h-48 w-full object-cover rounded-lg transition hover:opacity-90"
                />
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
                  class="rounded-lg px-2.5 py-1 text-[11px] font-bold transition hover:underline"
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

      <MessageComposer :pet-id="activePetId" />
    </template>

    <!-- In-Chat Video Player Modal -->
    <div
      v-if="previewVideo"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/70 p-4 backdrop-blur-xs"
      @click.self="previewVideo = null"
    >
      <div class="portal-card w-full max-w-lg overflow-hidden bg-white shadow-2xl rounded-2xl flex flex-col">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 px-4 py-3 bg-neutral-surface">
          <div class="flex items-center gap-2 truncate">
            <span class="text-base">🎥</span>
            <h3 class="font-bold text-sm text-navy truncate">{{ previewVideo.title }}</h3>
          </div>
          <button
            type="button"
            class="rounded-lg p-1 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy cursor-pointer"
            @click="previewVideo = null"
          >
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="p-4 bg-black flex items-center justify-center min-h-[260px]">
          <div v-if="previewVideo.loading" class="text-center text-white/80 py-8">
            <Loader2 class="h-8 w-8 animate-spin mx-auto text-sage mb-2" />
            <p class="text-xs">Loading video stream...</p>
          </div>
          <div v-else-if="previewVideo.error" class="text-center text-alert-red py-8">
            <p class="text-sm font-semibold">{{ previewVideo.error }}</p>
          </div>
          <video
            v-else-if="previewVideo.url"
            :src="previewVideo.url"
            controls
            autoplay
            class="max-h-[60vh] w-full rounded-lg object-contain"
          />
        </div>

        <div class="flex items-center justify-between border-t border-neutral-grey/80 px-4 py-3 bg-white">
          <button
            type="button"
            class="rounded-lg border border-neutral-grey px-3 py-1.5 text-xs font-semibold text-neutral-dark hover:bg-neutral-grey/30 cursor-pointer"
            @click="previewVideo = null"
          >
            Close
          </button>
          <RouterLink
            v-if="activePetId"
            :to="{ name: 'patient-detail', params: { petId: String(activePetId) }, query: { tab: 'videos', videoId: String(previewVideo.id) } }"
            class="rounded-lg bg-sage px-3 py-1.5 text-xs font-bold text-white hover:bg-sage/90 flex items-center gap-1 cursor-pointer"
            @click="previewVideo = null"
          >
            <span>Open in Patient Videos</span>
            <span class="text-xs">↗</span>
          </RouterLink>
        </div>
      </div>
    </div>

    <!-- In-Chat Image Preview Modal -->
    <div
      v-if="previewImage"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/80 p-4 backdrop-blur-xs"
      @click.self="previewImage = null"
    >
      <div class="portal-card w-full max-w-2xl overflow-hidden bg-white shadow-2xl rounded-2xl flex flex-col">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 px-4 py-3 bg-neutral-surface">
          <div class="flex items-center gap-2 truncate">
            <span class="text-base">🖼️</span>
            <h3 class="font-bold text-sm text-navy truncate">{{ previewImage.name }}</h3>
          </div>
          <div class="flex items-center gap-2">
            <a
              :href="previewImage.url"
              target="_blank"
              rel="noopener noreferrer"
              download
              class="rounded-lg px-2.5 py-1 text-xs font-semibold text-sage hover:bg-sage/10 cursor-pointer"
            >
              Download ↗
            </a>
            <button
              type="button"
              class="rounded-lg p-1 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy cursor-pointer"
              @click="previewImage = null"
            >
              <X class="h-5 w-5" />
            </button>
          </div>
        </div>

        <div class="p-2 bg-neutral-dark/95 flex items-center justify-center max-h-[70vh] overflow-auto">
          <img
            :src="previewImage.url"
            :alt="previewImage.name"
            class="max-h-[65vh] w-auto max-w-full rounded-lg object-contain"
          />
        </div>
      </div>
    </div>
  </section>
</template>

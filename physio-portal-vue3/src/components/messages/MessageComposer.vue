<script setup lang="ts">
import { ref, watch } from 'vue'
import BaseButton from '../BaseButton.vue'
import { uploadMessageAttachment } from '../../api/messages'
import { getPetVideos } from '../../api/videos'
import { useMessagesStore } from '../../store/messages'
import type { VideoSubmission } from '../../types/video'
import { getVideoTitle } from '../../types/video'

interface LocalAttachment {
  url: string
  name: string
  type: string
}

const props = defineProps<{
  petId?: number
  physioId?: number
  ownerId?: number
}>()

const messagesStore = useMessagesStore()
const body = ref('')
const selectedVideo = ref<VideoSubmission | null>(null)
const selectedAttachment = ref<LocalAttachment | null>(null)
const uploadingFile = ref(false)
const showVideoPicker = ref(false)
const availableVideos = ref<VideoSubmission[]>([])
const loadingVideos = ref(false)

const fileInput = ref<HTMLInputElement | null>(null)

watch(
  () => props.petId,
  (newPetId) => {
    selectedVideo.value = null
    selectedAttachment.value = null
    showVideoPicker.value = false
    if (newPetId) {
      loadVideos(newPetId)
    }
  },
  { immediate: true },
)

async function loadVideos(petId: number) {
  loadingVideos.value = true
  try {
    availableVideos.value = await getPetVideos(petId)
  } catch {
    availableVideos.value = []
  } finally {
    loadingVideos.value = false
  }
}

function triggerFileSelect() {
  fileInput.value?.click()
}

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  uploadingFile.value = true
  try {
    const res = await uploadMessageAttachment(file)
    selectedAttachment.value = {
      url: res.attachmentUrl,
      name: res.attachmentName,
      type: res.attachmentType,
    }
  } catch {
    alert('Failed to upload file. Make sure file size is under 25MB.')
  } finally {
    uploadingFile.value = false
    if (target) target.value = ''
  }
}

async function send() {
  if (!props.petId) return
  const text = body.value.trim()
  if (!text && !selectedVideo.value && !selectedAttachment.value) return

  const videoId = selectedVideo.value?.videoSubmissionId ?? undefined
  const attachmentUrl = selectedAttachment.value?.url ?? undefined
  const attachmentName = selectedAttachment.value?.name ?? undefined
  const attachmentType = selectedAttachment.value?.type ?? undefined

  await messagesStore.sendMessage({
    body: text || (selectedVideo.value ? `Video Submission: ${getVideoTitle(selectedVideo.value)}` : 'Attached file'),
    videoSubmissionId: videoId,
    attachmentUrl,
    attachmentName,
    attachmentType,
  })

  body.value = ''
  selectedVideo.value = null
  selectedAttachment.value = null
  showVideoPicker.value = false
}

function selectVideo(video: VideoSubmission) {
  selectedVideo.value = video
  showVideoPicker.value = false
}

function clearVideo() {
  selectedVideo.value = null
}

function clearAttachment() {
  selectedAttachment.value = null
}
</script>

<template>
  <div class="border-t border-neutral-grey/80 bg-white p-4">
    <!-- Hidden HTML5 File Input -->
    <input
      ref="fileInput"
      type="file"
      class="hidden"
      accept="image/*,application/pdf,.doc,.docx,.mp4,.mov"
      @change="handleFileUpload"
    />

    <!-- Staged Video Attachment Pill -->
    <div v-if="selectedVideo" class="mb-2 flex items-center justify-between rounded-lg bg-sage/10 px-3 py-1.5 text-xs text-navy border border-sage/30">
      <div class="flex items-center gap-2 truncate">
        <span class="font-bold text-sage">📹 Attached Video:</span>
        <span class="truncate font-medium">{{ getVideoTitle(selectedVideo) }}</span>
      </div>
      <button type="button" class="ml-2 font-bold text-alert-red hover:underline" @click="clearVideo">
        ✕ Remove
      </button>
    </div>

    <!-- Staged File Attachment Pill -->
    <div v-if="selectedAttachment" class="mb-2 flex items-center justify-between rounded-lg bg-navy/10 px-3 py-1.5 text-xs text-navy border border-navy/20">
      <div class="flex items-center gap-2 truncate">
        <span class="font-bold text-navy">📎 Attached File:</span>
        <span class="truncate font-medium">{{ selectedAttachment.name }}</span>
      </div>
      <button type="button" class="ml-2 font-bold text-alert-red hover:underline" @click="clearAttachment">
        ✕ Remove
      </button>
    </div>

    <!-- File Uploading Progress Indicator -->
    <div v-if="uploadingFile" class="mb-2 rounded-lg bg-surface px-3 py-1.5 text-xs text-neutral-muted">
      Uploading file from computer...
    </div>

    <!-- In-App Video Picker Popover -->
    <div v-if="showVideoPicker" class="mb-3 rounded-xl border border-neutral-grey bg-surface p-3 shadow-sm">
      <div class="mb-2 flex items-center justify-between text-xs font-bold text-navy">
        <span>Select Video Submission to Attach</span>
        <button type="button" class="text-neutral-muted hover:text-navy" @click="showVideoPicker = false">✕</button>
      </div>
      <div v-if="loadingVideos" class="py-2 text-center text-xs text-neutral-muted">Loading videos...</div>
      <div v-else-if="availableVideos.length === 0" class="py-2 text-center text-xs text-neutral-muted">
        No video submissions available for this patient.
      </div>
      <div v-else class="max-h-40 space-y-1.5 overflow-y-auto">
        <button
          v-for="v in availableVideos"
          :key="v.videoSubmissionId"
          type="button"
          class="flex w-full items-center justify-between rounded-lg p-2 text-left text-xs transition hover:bg-sage/10"
          :class="selectedVideo?.videoSubmissionId === v.videoSubmissionId ? 'bg-sage/20 font-bold' : 'bg-white'"
          @click="selectVideo(v)"
        >
          <div>
            <p class="font-semibold text-navy">{{ getVideoTitle(v) }}</p>
            <p class="text-[10px] text-neutral-muted">{{ new Date(v.createdDate).toLocaleDateString() }} · {{ v.isReviewed ? 'Reviewed' : 'Pending Review' }}</p>
          </div>
          <span class="text-xs text-sage font-bold">Attach →</span>
        </button>
      </div>
    </div>

    <form class="flex gap-2 items-end" @submit.prevent="send">
      <!-- File Upload Button (Local Machine) -->
      <button
        type="button"
        title="Attach File/Image from Computer"
        class="flex h-10 w-10 items-center justify-center rounded-xl border border-neutral-grey bg-surface text-base hover:bg-neutral-grey/20 transition"
        :class="selectedAttachment ? 'border-navy text-navy bg-navy/10' : 'text-neutral-muted'"
        @click="triggerFileSelect"
      >
        📎
      </button>

      <!-- Video Submission Button -->
      <button
        type="button"
        title="Attach In-App Video Submission"
        class="flex h-10 w-10 items-center justify-center rounded-xl border border-neutral-grey bg-surface text-base hover:bg-neutral-grey/20 transition"
        :class="selectedVideo ? 'border-sage text-sage bg-sage/10' : 'text-neutral-muted'"
        @click="showVideoPicker = !showVideoPicker"
      >
        🎥
      </button>

      <textarea
        v-model="body"
        rows="2"
        placeholder="Type a message..."
        class="flex-1 resize-none rounded-xl border border-neutral-grey bg-surface px-4 py-2.5 text-sm outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
      />
      <BaseButton
        type="submit"
        variant="accent"
        :disabled="messagesStore.sending || uploadingFile || (!body.trim() && !selectedVideo && !selectedAttachment)"
      >
        {{ messagesStore.sending ? '...' : 'Send' }}
      </BaseButton>
    </form>
    <p v-if="messagesStore.error" class="mt-2 text-xs text-alert-red">{{ messagesStore.error }}</p>
  </div>
</template>

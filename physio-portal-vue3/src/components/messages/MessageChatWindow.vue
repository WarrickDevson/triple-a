<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { RouterLink } from 'vue-router'
import MessageComposer from './MessageComposer.vue'
import { useAuthStore } from '../../store/auth'
import type { Message, MessageThread } from '../../types/message'

const props = defineProps<{
  thread: MessageThread | null
  messages: Message[]
  loading?: boolean
}>()

const auth = useAuthStore()

const headerTitle = computed(() =>
  props.thread ? `${props.thread.petName} / Owner: ${props.thread.ownerName}` : 'Select a conversation',
)

function isOutgoing(message: Message) {
  return message.senderUserId === auth.user?.userId
}

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
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
        <p v-if="thread" class="text-xs text-success-green">Online</p>
      </div>
      <RouterLink
        v-if="thread"
        :to="{ name: 'patient-detail', params: { petId: thread.petId } }"
        class="text-xs font-semibold text-sage hover:text-navy"
      >
        Patient Profile
      </RouterLink>
    </div>

    <div v-if="!thread" class="flex flex-1 items-center justify-center p-8">
      <p class="text-sm text-neutral-muted">Select a conversation to start messaging.</p>
    </div>

    <template v-else>
      <div class="flex-1 space-y-3 overflow-y-auto p-4">
        <div v-if="loading" class="text-center text-sm text-neutral-muted">Loading messages...</div>
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

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MessageChatWindow from '../components/messages/MessageChatWindow.vue'
import MessagePatientPanel from '../components/messages/MessagePatientPanel.vue'
import MessageThreadList from '../components/messages/MessageThreadList.vue'
import BaseButton from '../components/BaseButton.vue'
import { useAuthStore } from '../store/auth'
import { useMessagesStore } from '../store/messages'
import { usePatientsStore } from '../store/patients'

const messagesStore = useMessagesStore()
const patientsStore = usePatientsStore()
const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const mobileView = ref<'list' | 'chat'>('list')
const showStubModal = ref(false)
const stubMessage = ref('')

const selectedPetId = computed(() => {
  const param = route.params.petId
  if (param) return Number(param)
  return messagesStore.activePetId ?? messagesStore.threads[0]?.petId ?? null
})

const selectedThread = computed(
  () => messagesStore.threads.find((t) => t.petId === selectedPetId.value) ?? null,
)

const selectedPatient = computed(() => {
  if (!selectedPetId.value) return null
  return patientsStore.getPatientById(selectedPetId.value)
})

onMounted(async () => {
  await Promise.all([
    messagesStore.loadThreads(),
    patientsStore.fetchClinicPatients().catch(() => undefined),
  ])
  syncRouteAndOpenThread()
})

watch(() => messagesStore.threads, syncRouteAndOpenThread, { deep: true })

async function syncRouteAndOpenThread() {
  if (messagesStore.threads.length === 0) return
  const paramId = route.params.petId ? Number(route.params.petId) : null
  const petId = paramId ?? messagesStore.threads[0]!.petId
  if (!paramId) {
    router.replace({ name: 'message-thread', params: { petId } })
  }
  if (petId) {
    await messagesStore.openThread(petId)
    markUnreadAsRead()
  }
}

async function selectThread(petId: number) {
  router.push({ name: 'message-thread', params: { petId } })
  mobileView.value = 'chat'
}

function markUnreadAsRead() {
  const userId = auth.user?.userId
  if (!userId) return
  for (const message of messagesStore.activeMessages) {
    if (!message.readAt && message.senderUserId !== userId) {
      messagesStore.markAsRead(message.messageId).catch(() => undefined)
    }
  }
}

watch(
  () => messagesStore.activeMessages,
  () => markUnreadAsRead(),
  { deep: true },
)

function showStub(message: string) {
  stubMessage.value = message
  showStubModal.value = true
}
</script>

<template>
  <div class="grid gap-4 xl:grid-cols-[300px_minmax(0,1fr)_260px]">
    <div
      class="min-h-[600px]"
      :class="mobileView === 'chat' ? 'hidden xl:block' : 'block'"
    >
      <MessageThreadList
        :threads="messagesStore.threads"
        :selected-pet-id="selectedPetId"
        :loading="messagesStore.loading"
        @select="selectThread"
      />
    </div>

    <div
      class="min-h-[600px]"
      :class="mobileView === 'list' ? 'hidden xl:block' : 'block'"
    >
      <button
        type="button"
        class="mb-2 text-sm font-semibold text-sage xl:hidden"
        @click="mobileView = 'list'"
      >
        ← Back to inbox
      </button>
      <MessageChatWindow
        :thread="selectedThread"
        :messages="messagesStore.activeMessages"
        :loading="messagesStore.loading"
      />
    </div>

    <div class="min-h-0 hidden xl:block">
      <MessagePatientPanel
        :thread="selectedThread"
        :patient="selectedPatient"
        @coming-soon="showStub"
      />
    </div>

    <div
      v-if="showStubModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
      @click.self="showStubModal = false"
    >
      <div class="portal-card max-w-sm p-6 text-center">
        <p class="text-sm text-neutral-muted">{{ stubMessage }}</p>
        <BaseButton class="mt-4" size="sm" @click="showStubModal = false">Close</BaseButton>
      </div>
    </div>
  </div>
</template>

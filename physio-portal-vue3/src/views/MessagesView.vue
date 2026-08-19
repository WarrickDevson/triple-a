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
  if (param && !isNaN(Number(param))) return Number(param)
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

async function syncRouteAndOpenThread() {
  try {
    const rawParam = route.params.petId
    const paramId = rawParam && !isNaN(Number(rawParam)) ? Number(rawParam) : null
    const firstThreadPetId = messagesStore.threads.length > 0 ? messagesStore.threads[0].petId : null
    const targetPetId = paramId ?? firstThreadPetId

    if (!targetPetId) return

    if (paramId) {
      mobileView.value = 'chat'
      if (messagesStore.activePetId !== targetPetId) {
        await messagesStore.openThread(targetPetId)
        markUnreadAsRead()
      }
    } else if (typeof targetPetId === 'number' && !isNaN(targetPetId)) {
      if (String(route.params.petId) !== String(targetPetId)) {
        await router.replace({ name: 'message-thread', params: { petId: String(targetPetId) } }).catch(() => undefined)
      }
      if (messagesStore.activePetId !== targetPetId) {
        await messagesStore.openThread(targetPetId)
        markUnreadAsRead()
      }
    }
  } catch (err) {
    console.error('Error syncing message thread:', err)
  }
}

watch(() => route.params.petId, () => syncRouteAndOpenThread())

async function selectThread(petId: number) {
  if (!petId || isNaN(petId)) return
  router.push({ name: 'message-thread', params: { petId: String(petId) } })
  mobileView.value = 'chat'
}

function backToInbox() {
  mobileView.value = 'list'
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
  <div class="grid gap-4 lg:grid-cols-[260px_minmax(0,1fr)] xl:grid-cols-[280px_minmax(0,1fr)_240px]">
    <div
      class="min-h-[500px]"
      :class="mobileView === 'chat' ? 'hidden lg:block' : 'block'"
    >
      <MessageThreadList
        :threads="messagesStore.threads"
        :selected-pet-id="selectedPetId"
        :loading="messagesStore.loading"
        @select="selectThread"
      />
    </div>

    <div
      class="min-h-[500px]"
      :class="mobileView === 'list' ? 'hidden lg:block' : 'block'"
    >
      <button
        type="button"
        class="mb-2 text-sm font-semibold text-sage lg:hidden"
        @click="backToInbox"
      >
        ← Back to inbox
      </button>
      <MessageChatWindow
        :thread="selectedThread"
        :patient="selectedPatient"
        :selected-pet-id="selectedPetId"
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

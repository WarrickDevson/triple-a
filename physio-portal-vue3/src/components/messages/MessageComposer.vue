<script setup lang="ts">
import { ref } from 'vue'
import BaseButton from '../BaseButton.vue'
import { useMessagesStore } from '../../store/messages'

const messagesStore = useMessagesStore()
const body = ref('')

async function send() {
  const text = body.value.trim()
  if (!text) return
  await messagesStore.sendMessage({ body: text })
  body.value = ''
}
</script>

<template>
  <div class="border-t border-neutral-grey/80 bg-white p-4">
    <form class="flex gap-2" @submit.prevent="send">
      <textarea
        v-model="body"
        rows="2"
        placeholder="Type a message..."
        class="flex-1 resize-none rounded-xl border border-neutral-grey bg-surface px-4 py-2.5 text-sm outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
      />
      <BaseButton type="submit" variant="accent" :disabled="messagesStore.sending || !body.trim()">
        {{ messagesStore.sending ? '...' : 'Send' }}
      </BaseButton>
    </form>
    <p v-if="messagesStore.error" class="mt-2 text-xs text-alert-red">{{ messagesStore.error }}</p>
  </div>
</template>

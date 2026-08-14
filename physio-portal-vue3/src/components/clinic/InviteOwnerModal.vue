<script setup lang="ts">
import { computed, ref } from 'vue'
import BaseButton from '../BaseButton.vue'
import { useAuthStore } from '../../store/auth'

const emit = defineEmits<{
  (e: 'close'): void
}>()

const auth = useAuthStore()

const recipientEmail = ref('')
const ownerName = ref('')
const sending = ref(false)
const successMessage = ref<string | null>(null)
const errorMessage = ref<string | null>(null)
const copiedCode = ref(false)
const copiedLink = ref(false)

const inviteCode = computed(() => auth.user?.clinicInviteCode ?? 'TRIPLEA-DEMO-01')
const clinicName = computed(() => auth.user?.clinicName ?? 'Triple A Clinic')

const registerUrl = computed(() => {
  const base = window.location.origin
  return `${base}/register?inviteCode=${encodeURIComponent(inviteCode.value)}`
})

async function copyCodeToClipboard() {
  try {
    await navigator.clipboard.writeText(inviteCode.value)
    copiedCode.value = true
    setTimeout(() => {
      copiedCode.value = false
    }, 2000)
  } catch {
    // fallback if clipboard API fails
  }
}

async function copyLinkToClipboard() {
  try {
    await navigator.clipboard.writeText(registerUrl.value)
    copiedLink.value = true
    setTimeout(() => {
      copiedLink.value = false
    }, 2000)
  } catch {
    // fallback
  }
}

async function handleSendInvite() {
  if (!recipientEmail.value.trim()) return

  sending.value = true
  successMessage.value = null
  errorMessage.value = null

  try {
    const res = await auth.sendOwnerInvite(recipientEmail.value.trim(), ownerName.value.trim())
    successMessage.value = res || `Invitation email sent to ${recipientEmail.value}!`
    recipientEmail.value = ''
    ownerName.value = ''
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to send invite email. Please try again.'
  } finally {
    sending.value = false
  }
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card w-full max-w-md p-6 shadow-xl animate-in fade-in zoom-in duration-150">
      <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-3">
        <div>
          <h3 class="text-base font-bold text-navy">Invite Pet Owner</h3>
          <p class="text-xs text-neutral-muted">Share your clinic invite code or send an email invitation.</p>
        </div>
        <button
          type="button"
          class="rounded-lg p-1 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
          @click="emit('close')"
        >
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Invite Code Share Box -->
      <div class="mt-4 rounded-xl border border-sage/40 bg-sage-muted/30 p-4">
        <p class="text-xs font-bold uppercase tracking-wider text-neutral-muted">{{ clinicName }} Invite Code</p>
        <div class="mt-2 flex items-center justify-between">
          <span class="font-mono text-xl font-extrabold tracking-widest text-navy">{{ inviteCode }}</span>
          <div class="flex gap-2">
            <button
              type="button"
              class="rounded-md border border-neutral-grey bg-white px-2.5 py-1 text-xs font-semibold text-navy shadow-sm transition-all hover:border-sage hover:text-sage"
              @click="copyCodeToClipboard"
            >
              {{ copiedCode ? 'Copied!' : 'Copy Code' }}
            </button>
            <button
              type="button"
              class="rounded-md border border-neutral-grey bg-white px-2.5 py-1 text-xs font-semibold text-navy shadow-sm transition-all hover:border-sage hover:text-sage"
              @click="copyLinkToClipboard"
            >
              {{ copiedLink ? 'Copied Link!' : 'Copy Link' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Direct Email Invite Form -->
      <form class="mt-5 space-y-4" @submit.prevent="handleSendInvite">
        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-neutral-muted">
            Owner Email Address <span class="text-alert-red">*</span>
          </label>
          <input
            v-model="recipientEmail"
            type="email"
            required
            placeholder="owner@example.com"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none focus:ring-1 focus:ring-sage"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-neutral-muted">
            Owner Name <span class="font-normal text-neutral-muted/80">(Optional)</span>
          </label>
          <input
            v-model="ownerName"
            type="text"
            placeholder="e.g. Sarah Jenkins"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none focus:ring-1 focus:ring-sage"
          />
        </div>

        <div v-if="successMessage" class="rounded-lg bg-emerald-50 p-3 text-xs font-medium text-emerald-800 border border-emerald-200">
          {{ successMessage }}
        </div>

        <div v-if="errorMessage" class="rounded-lg bg-red-50 p-3 text-xs font-medium text-red-800 border border-red-200">
          {{ errorMessage }}
        </div>

        <div class="flex justify-end gap-3 pt-2">
          <BaseButton variant="secondary" size="sm" type="button" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton size="sm" type="submit" :disabled="sending || !recipientEmail">
            {{ sending ? 'Sending...' : 'Send Email Invite' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

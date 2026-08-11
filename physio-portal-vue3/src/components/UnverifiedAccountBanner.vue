<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { Mail, AlertTriangle, CheckCircle2 } from '@lucide/vue'
import { useAuthStore } from '../store/auth'

const auth = useAuthStore()

const STORAGE_KEY = 'kpw_resend_cooldown_physio'
const COOLDOWN_SECONDS = 60

const cooldownRemaining = ref(0)
const resendSending = ref(false)
const resendSuccess = ref(false)
const errorMessage = ref<string | null>(null)
let timer: ReturnType<typeof setInterval> | null = null

const isUnverified = computed(() => {
  return auth.isAuthenticated && auth.user?.isEmailVerified === false
})

function checkCooldown() {
  const stored = localStorage.getItem(STORAGE_KEY)
  if (!stored) {
    cooldownRemaining.value = 0
    return
  }
  const expiry = parseInt(stored, 10)
  const now = Math.floor(Date.now() / 1000)
  if (expiry > now) {
    cooldownRemaining.value = expiry - now
    startTimer()
  } else {
    localStorage.removeItem(STORAGE_KEY)
    cooldownRemaining.value = 0
  }
}

function startTimer() {
  if (timer) clearInterval(timer)
  timer = setInterval(() => {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (!stored) {
      cooldownRemaining.value = 0
      if (timer) clearInterval(timer)
      return
    }
    const expiry = parseInt(stored, 10)
    const now = Math.floor(Date.now() / 1000)
    const remaining = expiry - now
    if (remaining > 0) {
      cooldownRemaining.value = remaining
    } else {
      cooldownRemaining.value = 0
      if (timer) clearInterval(timer)
      localStorage.removeItem(STORAGE_KEY)
    }
  }, 1000)
}

async function handleResend() {
  if (cooldownRemaining.value > 0 || resendSending.value || !auth.user?.email) return
  
  const expiry = Math.floor(Date.now() / 1000) + COOLDOWN_SECONDS
  localStorage.setItem(STORAGE_KEY, expiry.toString())
  cooldownRemaining.value = COOLDOWN_SECONDS
  startTimer()

  resendSending.value = true
  resendSuccess.value = false
  errorMessage.value = null

  try {
    const ok = await auth.resendVerification(auth.user.email)
    if (ok) {
      resendSuccess.value = true
    } else {
      errorMessage.value = auth.error || 'Failed to resend email.'
    }
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Error sending verification email.'
  } finally {
    resendSending.value = false
  }
}

onMounted(() => {
  checkCooldown()
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div
    v-if="isUnverified"
    class="bg-amber-50 border-b border-amber-200 px-4 py-2.5 text-amber-900 font-medium transition-all"
  >
    <div class="mx-auto flex flex-col sm:flex-row items-center justify-between gap-3 max-w-7xl text-xs">
      <div class="flex items-center gap-2 min-w-0">
        <AlertTriangle class="h-4 w-4 shrink-0 text-amber-600" />
        <p class="truncate">
          <span class="font-bold">Unverified Account:</span>
          Please verify your email address (<span class="underline font-semibold">{{ auth.user?.email }}</span>) to complete account verification.
        </p>
      </div>

      <div class="flex items-center gap-3 shrink-0">
        <span v-if="resendSuccess" class="inline-flex items-center gap-1 text-xs font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded border border-emerald-200">
          <CheckCircle2 class="h-3.5 w-3.5" /> Sent!
        </span>

        <button
          type="button"
          class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg text-xs font-bold bg-amber-600 text-white shadow-sm hover:bg-amber-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          :disabled="cooldownRemaining > 0 || resendSending"
          @click="handleResend"
        >
          <Mail class="h-3.5 w-3.5" />
          <template v-if="resendSending">Sending...</template>
          <template v-else-if="cooldownRemaining > 0">Resend in {{ cooldownRemaining }}s</template>
          <template v-else>Resend Verification Link</template>
        </button>
      </div>
    </div>
  </div>
</template>

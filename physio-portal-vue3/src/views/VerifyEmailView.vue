<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { AlertCircle, CheckCircle2, Clock, ExternalLink, Loader2 } from '@lucide/vue'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { brand } from '../config/brand'
import { useAuthStore } from '../store/auth'

const PLAY_STORE_URL = 'https://play.google.com/store/apps/details?id=com.devson.triplea'
const APP_DEEP_LINK = 'triplea://app'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const loading = ref(true)
const verified = ref(false)
const isApproved = ref<boolean | null>(null)
const userRole = ref<string | null>(null)
const isMobile = ref(false)
const redirecting = ref(false)
const errorMessage = ref<string | null>(null)

const emailParam = ref('')
const tokenParam = ref('')

const resendEmail = ref('')
const resendLoading = ref(false)
const resendSuccess = ref(false)

const STORAGE_KEY = 'kpw_resend_cooldown_physio'
const COOLDOWN_SECONDS = 60
const cooldownRemaining = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

function checkCooldown() {
  const stored = localStorage.getItem(STORAGE_KEY)
  if (!stored) return
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

onMounted(async () => {
  checkCooldown()
  isMobile.value = typeof window !== 'undefined' && /Android|iPhone|iPad|iPod|webOS|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
  emailParam.value = (route.query.email as string || '').trim()
  tokenParam.value = (route.query.token as string || '').trim()
  resendEmail.value = emailParam.value

  if (!emailParam.value || !tokenParam.value) {
    loading.value = false
    errorMessage.value = 'Invalid email verification link.'
    return
  }

  try {
    const res = await auth.verifyEmail(emailParam.value, tokenParam.value)
    if (res) {
      verified.value = true
      isApproved.value = res.isApproved
      userRole.value = res.userRole

      // If user is a Pet Owner or accessing from a mobile browser, attempt to redirect back to the app
      if (res.userRole === 'Owner' || isMobile.value) {
        setTimeout(() => {
          try {
            window.location.href = APP_DEEP_LINK
          } catch (_) {}
        }, 1200)
      } else if (auth.isAuthenticated) {
        if (res.isApproved) {
          redirecting.value = true
          setTimeout(() => {
            router.push({ name: 'dashboard' })
          }, 3000)
        }
      }
    } else {
      errorMessage.value = auth.error || 'Verification token has expired or is invalid.'
    }
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Verification failed.'
  } finally {
    loading.value = false
  }
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})

async function handleResend() {
  if (!resendEmail.value.trim() || cooldownRemaining.value > 0) return
  
  const expiry = Math.floor(Date.now() / 1000) + COOLDOWN_SECONDS
  localStorage.setItem(STORAGE_KEY, expiry.toString())
  cooldownRemaining.value = COOLDOWN_SECONDS
  startTimer()

  resendLoading.value = true
  resendSuccess.value = false

  try {
    const ok = await auth.resendVerification(resendEmail.value.trim())
    if (ok) {
      resendSuccess.value = true
    }
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to resend verification email.'
  } finally {
    resendLoading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-surface px-4 py-12">
    <div class="w-full max-w-md rounded-2xl border border-neutral-grey bg-white p-8 shadow-sm">
      <div class="text-center">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-sage">{{ brand.name }}</p>
        <h1 class="mt-2 text-2xl font-bold text-navy">Email Verification</h1>
      </div>

      <!-- Mobile Google Play Download Banner -->
      <div v-if="isMobile" class="mt-6 flex items-center justify-between rounded-xl bg-navy p-3 text-white shadow-sm">
        <div class="flex items-center gap-2.5">
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-white/10 text-emerald-400">
            <svg class="h-5 w-5" viewBox="0 0 24 24" fill="currentColor">
              <path d="M3.609 1.814L13.792 12 3.61 22.186a2.222 2.222 0 0 1-.227-.887V2.701c0-.328.082-.63.226-.887zm11.246 11.247l2.368 2.368-11.24 6.425 8.872-8.793zm0-2.122L5.983 2.146l11.24 6.425-2.368 2.368zm1.061 1.061l3.327-1.901a1.232 1.232 0 0 0 0 2.138l-3.327 1.901z"/>
            </svg>
          </div>
          <div class="text-left">
            <p class="text-xs font-bold leading-tight">Triple A Companion</p>
            <p class="text-[11px] text-white/70">Get the mobile app</p>
          </div>
        </div>
        <a
          :href="PLAY_STORE_URL"
          target="_blank"
          rel="noopener noreferrer"
          class="rounded-lg bg-sage px-3 py-1.5 text-xs font-bold text-white transition hover:bg-sage/90"
        >
          INSTALL
        </a>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="mt-8 flex flex-col items-center py-8 text-center">
        <Loader2 class="h-10 w-10 animate-spin text-sage" />
        <p class="mt-4 text-sm font-medium text-navy">Verifying your email address...</p>
        <p class="mt-1 text-xs text-neutral-muted">Please wait a moment while we validate your token.</p>
      </div>

      <!-- Success State -->
      <div v-else-if="verified" class="mt-8 flex flex-col items-center text-center">
        <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-50 text-emerald-600 border border-emerald-100">
          <CheckCircle2 class="h-8 w-8" />
        </div>

        <h2 class="mt-4 text-xl font-bold text-navy">Email Verified!</h2>
        <p class="mt-2 text-sm text-neutral-muted">
          Your email address has been successfully verified.
        </p>

        <!-- Pet Owner or Mobile App Redirection Section -->
        <div v-if="userRole === 'Owner' || isMobile" class="mt-6 flex flex-col gap-3 w-full">
          <a
            :href="APP_DEEP_LINK"
            class="flex h-11 w-full items-center justify-center gap-2 rounded-xl bg-sage px-4 text-sm font-bold text-white shadow-sm transition hover:bg-sage/90"
          >
            <ExternalLink class="h-4 w-4" />
            Open in Triple A App
          </a>

          <a
            :href="PLAY_STORE_URL"
            target="_blank"
            rel="noopener noreferrer"
            class="flex h-11 w-full items-center justify-center gap-2 rounded-xl border border-neutral-300 bg-white px-4 text-sm font-semibold text-navy transition hover:bg-neutral-50"
          >
            <svg class="h-4 w-4 text-emerald-600" viewBox="0 0 24 24" fill="currentColor">
              <path d="M3.609 1.814L13.792 12 3.61 22.186a2.222 2.222 0 0 1-.227-.887V2.701c0-.328.082-.63.226-.887zm11.246 11.247l2.368 2.368-11.24 6.425 8.872-8.793zm0-2.122L5.983 2.146l11.24 6.425-2.368 2.368zm1.061 1.061l3.327-1.901a1.232 1.232 0 0 0 0 2.138l-3.327 1.901z"/>
            </svg>
            Download on Google Play
          </a>

          <RouterLink to="/login" class="text-xs text-center text-neutral-muted hover:underline mt-1">
            Proceed to Web Sign In
          </RouterLink>
        </div>

        <!-- Physio Approval Status Note -->
        <template v-else>
          <div v-if="isApproved === false" class="mt-5 w-full rounded-xl border border-amber-200 bg-amber-50 p-4 text-left">
            <div class="flex items-start gap-2.5 text-amber-800 text-xs leading-relaxed">
              <Clock class="h-4 w-4 shrink-0 text-amber-600 mt-0.5" />
              <div>
                <p class="font-bold text-sm text-amber-900">Pending Admin Approval</p>
                <p class="mt-1">
                  Note: Your practitioner account is currently undergoing administrator review. Full portal features will be unlocked once approved.
                </p>
              </div>
            </div>
          </div>

          <div v-else-if="isApproved === true" class="mt-5 w-full rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-left">
            <div class="flex items-start gap-2.5 text-emerald-800 text-xs leading-relaxed">
              <CheckCircle2 class="h-4 w-4 shrink-0 text-emerald-600 mt-0.5" />
              <div>
                <p class="font-bold text-sm text-emerald-900">Account Approved & Verified</p>
                <p class="mt-1">
                  {{ redirecting ? 'Auto-redirecting to your dashboard in 3 seconds...' : 'Your account is active with full practitioner access.' }}
                </p>
              </div>
            </div>
          </div>

          <div class="mt-6 w-full">
            <RouterLink :to="auth.isAuthenticated ? '/dashboard' : '/login'">
              <BaseButton variant="accent" class="w-full h-11">
                {{ auth.isAuthenticated ? 'Go to Dashboard' : 'Proceed to Sign In' }}
              </BaseButton>
            </RouterLink>
          </div>
        </template>
      </div>

      <!-- Error / Expired Token State -->
      <div v-else class="mt-8">
        <div class="flex items-start gap-3 rounded-xl border border-red-200 bg-red-50 p-4 text-red-700">
          <AlertCircle class="h-5 w-5 shrink-0 mt-0.5" />
          <div class="text-xs leading-relaxed">
            <p class="font-semibold text-sm">Verification Failed</p>
            <p class="mt-1">{{ errorMessage }}</p>
          </div>
        </div>

        <div class="mt-6 space-y-4">
          <p class="text-xs text-neutral-muted">
            Request a new verification email by entering your work email address below:
          </p>

          <BaseInput
            id="resendEmail"
            v-model="resendEmail"
            label="Email Address"
            type="email"
          />

          <div v-if="resendSuccess" class="rounded-lg bg-emerald-50 p-3 text-xs font-medium text-emerald-700 text-center">
            A new verification link has been sent to your inbox.
          </div>

          <BaseButton
            variant="secondary"
            class="w-full h-11 text-sm"
            :disabled="resendLoading || !resendEmail.trim() || cooldownRemaining > 0"
            @click="handleResend"
          >
            <template v-if="resendLoading">Sending...</template>
            <template v-else-if="cooldownRemaining > 0">Resend in {{ cooldownRemaining }}s</template>
            <template v-else>Resend Verification Email</template>
          </BaseButton>

          <div class="pt-2 text-center">
            <RouterLink to="/login" class="text-xs font-semibold text-sage hover:underline">
              Back to Login
            </RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

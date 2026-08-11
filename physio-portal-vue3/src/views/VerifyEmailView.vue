<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { AlertCircle, CheckCircle2, Loader2 } from '@lucide/vue'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { brand } from '../config/brand'
import { useAuthStore } from '../store/auth'

const auth = useAuthStore()
const route = useRoute()

const loading = ref(true)
const verified = ref(false)
const errorMessage = ref<string | null>(null)

const emailParam = ref('')
const tokenParam = ref('')

const resendEmail = ref('')
const resendLoading = ref(false)
const resendSuccess = ref(false)

onMounted(async () => {
  emailParam.value = (route.query.email as string || '').trim()
  tokenParam.value = (route.query.token as string || '').trim()
  resendEmail.value = emailParam.value

  if (!emailParam.value || !tokenParam.value) {
    loading.value = false
    errorMessage.value = 'Invalid email verification link.'
    return
  }

  try {
    const success = await auth.verifyEmail(emailParam.value, tokenParam.value)
    if (success) {
      verified.value = true
    } else {
      errorMessage.value = auth.error || 'Verification token has expired or is invalid.'
    }
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Verification failed.'
  } finally {
    loading.value = false
  }
})

async function handleResend() {
  if (!resendEmail.value.trim()) return
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

        <div class="mt-6 w-full">
          <RouterLink to="/login">
            <BaseButton variant="accent" class="w-full h-11">
              Proceed to Login
            </BaseButton>
          </RouterLink>
        </div>
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
            :disabled="resendLoading || !resendEmail.trim()"
            @click="handleResend"
          >
            {{ resendLoading ? 'Sending...' : 'Resend Verification Email' }}
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

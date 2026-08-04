<script setup lang="ts">
import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { brand } from '../config/brand'
import { useAuthStore } from '../store/auth'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({ email: '' })

async function onSubmit() {
  await auth.forgotPassword(form.email.trim())
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-white px-6 py-12">
    <div class="w-full max-w-md">
      <p class="text-xs font-semibold uppercase tracking-[0.2em] text-sage">{{ brand.name }}</p>
      <h1 class="mt-2 text-2xl font-bold text-navy">Forgot password</h1>
      <p class="mt-2 text-sm text-neutral-muted">
        Enter your email. In development, check API logs for the reset link.
      </p>

      <div v-if="auth.error" class="mt-4 rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">
        {{ auth.error }}
      </div>
      <div
        v-if="auth.message"
        class="mt-4 rounded-xl border border-green-200 bg-green-50 p-3 text-sm text-success-green"
      >
        {{ auth.message }}
      </div>

      <form class="mt-6 space-y-4" @submit.prevent="onSubmit">
        <BaseInput id="email" v-model="form.email" label="Email" type="email" required />
        <BaseButton type="submit" variant="accent" class="w-full" :disabled="auth.loading">
          {{ auth.loading ? 'Sending...' : 'Send reset link' }}
        </BaseButton>
      </form>

      <button
        type="button"
        class="mt-4 text-sm font-semibold text-sage hover:underline"
        @click="router.push({ name: 'login' })"
      >
        Back to sign in
      </button>
    </div>
  </div>
</template>

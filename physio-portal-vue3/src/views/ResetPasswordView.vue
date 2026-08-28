<script setup lang="ts">
import { computed, onMounted, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { brand } from '../config/brand'
import { useAuthStore } from '../store/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive({
  token: '',
  newPassword: '',
})

onMounted(() => {
  const token = route.query.token as string | undefined
  if (token) form.token = token
})

const isPasswordValid = computed(() => {
  const p = form.newPassword
  return (
    p.length >= 8 &&
    /[a-z]/.test(p) &&
    /[A-Z]/.test(p) &&
    /[0-9]/.test(p) &&
    /[^a-zA-Z0-9]/.test(p)
  )
})

async function onSubmit() {
  if (!isPasswordValid.value) return
  const ok = await auth.resetPassword(form.token.trim(), form.newPassword)
  if (ok) {
    await router.push({ name: 'login' })
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-white px-6 py-12">
    <div class="w-full max-w-md">
      <p class="text-xs font-semibold uppercase tracking-[0.2em] text-sage">{{ brand.name }}</p>
      <h1 class="mt-2 text-2xl font-bold text-navy">Reset password</h1>

      <div v-if="auth.error" class="mt-4 rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">
        {{ auth.error }}
      </div>

      <form class="mt-6 space-y-4" @submit.prevent="onSubmit">
        <BaseInput id="token" v-model="form.token" label="Reset token" required />
        <div>
          <BaseInput
            id="newPassword"
            v-model="form.newPassword"
            type="password"
            label="New password"
            required
          />
          <p class="mt-1 text-[11px] text-neutral-muted">
            Must be at least 8 characters with uppercase, lowercase, numbers, and symbols (e.g. Pass!123).
          </p>
          <p v-if="form.newPassword && !isPasswordValid" class="mt-1 text-xs text-red-600">
            Password must include uppercase, lowercase, numbers, and symbols.
          </p>
        </div>
        <BaseButton
          type="submit"
          variant="accent"
          class="w-full"
          :disabled="auth.loading || !isPasswordValid || !form.token.trim()"
        >
          {{ auth.loading ? 'Updating...' : 'Update password' }}
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

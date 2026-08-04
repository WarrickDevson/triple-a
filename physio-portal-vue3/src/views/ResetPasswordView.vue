<script setup lang="ts">
import { onMounted, reactive } from 'vue'
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

async function onSubmit() {
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
        <BaseInput
          id="newPassword"
          v-model="form.newPassword"
          type="password"
          label="New password"
          required
        />
        <BaseButton type="submit" variant="accent" class="w-full" :disabled="auth.loading">
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

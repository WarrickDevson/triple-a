<script setup lang="ts">
import { computed, reactive } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { LockKeyhole } from '@lucide/vue'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { brand } from '../config/brand'
import { useAuthStore } from '../store/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive({
  email: '',
  password: '',
})

const isSignInValid = computed(() => form.email.trim().length > 0 && form.password.length > 0)

async function onSubmit() {
  try {
    await auth.login(form)
    const redirect = (route.query.redirect as string) || '/dashboard'
    await router.push(redirect)
  } catch {
    // error shown via store
  }
}
</script>

<template>
  <div class="flex min-h-screen flex-col lg:flex-row">
    <!-- Left: brand panel (50%) -->
    <aside
      class="login-brand relative flex min-h-[320px] flex-col items-center justify-center px-10 py-12 text-white sm:px-14 sm:py-14 lg:min-h-screen lg:w-1/2 lg:px-20 lg:py-16 xl:px-24"
    >
      <div class="login-brand__glow pointer-events-none absolute inset-0" />

      <div class="relative flex w-full max-w-md flex-col gap-10">
        <div>
          <p class="text-xs font-semibold uppercase tracking-[0.22em] text-sage-light">
            {{ brand.tagline }}
          </p>
          <h2 class="mt-4 text-3xl font-bold leading-tight sm:text-4xl">
            Manage animal rehabilitation with confidence.
          </h2>
          <p class="mt-4 text-sm leading-relaxed text-white/70">
            Secure account access, streamlined workflows, and patient records your team can trust.
          </p>
        </div>

        <div class="rounded-2xl border border-white/10 bg-white/8 p-5 backdrop-blur-sm">
          <p class="font-semibold text-white">Built for care teams</p>
          <p class="mt-1 text-sm leading-relaxed text-white/65">
            Access control across clinics and workspaces without complexity.
          </p>
        </div>
      </div>
    </aside>

    <!-- Right: auth form (50%) — no card, sits on white background -->
    <main
      class="flex flex-1 flex-col justify-center bg-white px-8 py-10 sm:px-12 lg:w-1/2 lg:px-16 lg:py-14 xl:px-24"
    >
      <div class="mx-auto w-full max-w-md">
        <div class="flex items-start justify-between gap-4">
          <div>
            <p class="text-xs font-semibold uppercase tracking-[0.2em] text-sage">{{ brand.name }}</p>
            <h1 class="mt-2 text-3xl font-bold text-navy">Welcome Back</h1>
            <p class="mt-2 max-w-md text-sm text-neutral-muted">
              Sign in to continue to your {{ brand.name }} workspace.
            </p>
          </div>
          <div
            class="hidden h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-neutral-grey bg-surface text-sage sm:flex"
          >
            <LockKeyhole class="h-5 w-5" :stroke-width="1.75" />
          </div>
        </div>

        <div
          v-if="auth.error"
          class="mt-6 rounded-xl border border-red-200 bg-red-50 p-3.5 text-sm text-red-700 space-y-2"
        >
          <p>{{ auth.error }}</p>
          <div v-if="auth.error.includes('EMAIL_NOT_VERIFIED')" class="pt-1">
            <button
              type="button"
              class="font-semibold underline hover:text-red-900 text-xs"
              @click="auth.resendVerification(form.email)"
            >
              Resend verification email to {{ form.email }}
            </button>
          </div>
        </div>

        <div v-if="auth.message" class="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 p-3.5 text-sm text-emerald-800 font-medium">
          {{ auth.message }}
        </div>

        <form class="mt-8 space-y-4" @submit.prevent="onSubmit">
          <BaseInput
            id="email"
            v-model="form.email"
            label="Email"
            type="email"
            autocomplete="username"
            icon="UserRound"
            required
          />

          <BaseInput
            id="password"
            v-model="form.password"
            type="password"
            label="Password"
            autocomplete="current-password"
            icon="LockKeyhole"
            required
          />

          <BaseButton
            type="submit"
            variant="primary"
            class="w-full"
            :disabled="!isSignInValid || auth.loading"
          >
            <LockKeyhole class="h-4 w-4" :stroke-width="2" />
            {{ auth.loading ? 'Signing in...' : 'Sign In' }}
          </BaseButton>
        </form>

        <div class="mt-6 flex flex-col items-center justify-center gap-2 text-sm text-neutral-muted">
          <RouterLink to="/forgot-password" class="font-semibold text-sage hover:underline">
            Forgot password?
          </RouterLink>
          <p class="text-xs">
            Don't have an account?
            <RouterLink to="/register" class="font-semibold text-sage hover:underline ml-1">
              Sign up as a Physio
            </RouterLink>
          </p>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.login-brand {
  background: linear-gradient(155deg, #0a1a2e 0%, #122a42 45%, #0a1a2e 100%);
}

.login-brand__glow {
  background:
    radial-gradient(ellipse 70% 55% at 0% 0%, rgb(122 138 92 / 0.35), transparent 55%),
    radial-gradient(ellipse 40% 35% at 85% 90%, rgb(107 122 77 / 0.15), transparent 50%);
}
</style>

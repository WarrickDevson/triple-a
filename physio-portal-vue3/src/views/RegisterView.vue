<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { Building2, CheckCircle2, Mail, ShieldCheck, UserPlus } from '@lucide/vue'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { brand } from '../config/brand'
import { useAuthStore } from '../store/auth'

const auth = useAuthStore()
const route = useRoute()

const submitted = ref(false)
const submittedEmail = ref('')
const resendSuccess = ref(false)
const resendLoading = ref(false)
const hasInviteCode = ref(false)

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  clinicName: '',
  inviteCode: '',
  password: '',
  confirmPassword: '',
})

onMounted(() => {
  if (route.query.inviteCode) {
    form.inviteCode = (route.query.inviteCode as string).trim()
    hasInviteCode.value = true
  }
  if (route.query.email) {
    form.email = (route.query.email as string).trim()
  }
})

const passwordMatch = computed(() => {
  if (!form.confirmPassword) return true
  return form.password === form.confirmPassword
})

const isValid = computed(() => {
  return (
    form.firstName.trim().length > 0 &&
    form.lastName.trim().length > 0 &&
    form.email.trim().length > 0 &&
    form.password.length >= 8 &&
    passwordMatch.value
  )
})

async function onSubmit() {
  if (!isValid.value) return
  try {
    submittedEmail.value = form.email.trim()
    await auth.register({
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      phoneNumber: form.phoneNumber.trim() || undefined,
      clinicName: form.clinicName.trim() || undefined,
      inviteCode: form.inviteCode.trim() || undefined,
      password: form.password,
    })
    submitted.value = true
  } catch {
    // error handled via store
  }
}

async function handleResend() {
  if (!submittedEmail.value) return
  resendLoading.value = true
  resendSuccess.value = false
  try {
    const success = await auth.resendVerification(submittedEmail.value)
    if (success) {
      resendSuccess.value = true
    }
  } finally {
    resendLoading.value = false
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
            Empower your clinical animal rehabilitation.
          </h2>
          <p class="mt-4 text-sm leading-relaxed text-white/70">
            Join MoveWell to build custom rehab plans, track patient progress, and communicate directly with pet owners.
          </p>
        </div>

        <div class="rounded-2xl border border-white/10 bg-white/8 p-5 backdrop-blur-sm">
          <div class="flex items-center gap-3 text-white">
            <ShieldCheck class="h-5 w-5 text-emerald-400" />
            <p class="font-semibold">Verified Clinical Workspace</p>
          </div>
          <p class="mt-2 text-sm leading-relaxed text-white/65">
            Instant workspace setup with invitation codes or direct administrator approval.
          </p>
        </div>
      </div>
    </aside>

    <!-- Right: sign-up form / confirmation panel -->
    <main
      class="flex flex-1 flex-col justify-center bg-white px-8 py-10 sm:px-12 lg:w-1/2 lg:px-16 lg:py-14 xl:px-24"
    >
      <div class="mx-auto w-full max-w-md">
        <!-- Post-submit email verification instructions -->
        <div v-if="submitted" class="space-y-6 text-center">
          <div
            class="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-50 text-emerald-600 border border-emerald-100"
          >
            <Mail class="h-8 w-8" />
          </div>

          <div>
            <h1 class="text-2xl font-bold text-navy">Check Your Email</h1>
            <p class="mt-2 text-sm text-neutral-muted">
              We've sent a verification email to
              <strong class="text-navy font-semibold">{{ submittedEmail }}</strong>.
            </p>
          </div>

          <div class="rounded-xl border border-neutral-grey bg-surface p-4 text-left text-xs text-neutral-muted space-y-2">
            <div class="flex items-start gap-2">
              <CheckCircle2 class="h-4 w-4 shrink-0 text-emerald-600 mt-0.5" />
              <span>Click the verification link in your email to activate your account.</span>
            </div>
            <div v-if="!hasInviteCode" class="flex items-start gap-2">
              <Building2 class="h-4 w-4 shrink-0 text-amber-600 mt-0.5" />
              <span>Because you created a new clinic without an invite code, your account will be reviewed by a MoveWell Administrator once email verification is complete.</span>
            </div>
          </div>

          <div v-if="resendSuccess" class="rounded-lg bg-emerald-50 p-3 text-xs font-medium text-emerald-700">
            A new verification link has been sent to your inbox.
          </div>

          <div class="space-y-3 pt-2">
            <BaseButton
              variant="secondary"
              class="w-full h-11 text-sm"
              :disabled="resendLoading"
              @click="handleResend"
            >
              {{ resendLoading ? 'Sending...' : 'Resend Verification Email' }}
            </BaseButton>

            <RouterLink
              to="/login"
              class="block text-center text-sm font-semibold text-sage hover:underline pt-2"
            >
              Back to Login
            </RouterLink>
          </div>
        </div>

        <!-- Registration form -->
        <div v-else>
          <div class="flex items-start justify-between gap-4">
            <div>
              <p class="text-xs font-semibold uppercase tracking-[0.2em] text-sage">{{ brand.name }}</p>
              <h1 class="mt-2 text-3xl font-bold text-navy">Physio Sign Up</h1>
              <p class="mt-2 text-sm text-neutral-muted">
                Create your practitioner account to manage your practice and patients.
              </p>
            </div>
            <div
              class="hidden h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-neutral-grey bg-surface text-sage sm:flex"
            >
              <UserPlus class="h-5 w-5" :stroke-width="1.75" />
            </div>
          </div>

          <div
            v-if="auth.error"
            class="mt-6 rounded-xl border border-red-200 bg-red-50 p-3.5 text-sm text-red-700"
          >
            {{ auth.error }}
          </div>

          <form class="mt-6 space-y-4" @submit.prevent="onSubmit">
            <div class="grid grid-cols-2 gap-3">
              <BaseInput
                id="firstName"
                v-model="form.firstName"
                label="First Name"
                required
              />
              <BaseInput
                id="lastName"
                v-model="form.lastName"
                label="Last Name"
                required
              />
            </div>

            <BaseInput
              id="email"
              v-model="form.email"
              label="Work Email"
              type="email"
              autocomplete="email"
              required
            />

            <BaseInput
              id="phoneNumber"
              v-model="form.phoneNumber"
              label="Phone Number (Optional)"
              type="tel"
            />

            <BaseInput
              id="clinicName"
              v-model="form.clinicName"
              label="Clinic Name (Optional)"
              placeholder="e.g. MoveWell Rehab Centre"
            />

            <div>
              <BaseInput
                id="inviteCode"
                v-model="form.inviteCode"
                label="Clinic / Admin Invite Code (Optional)"
                placeholder="e.g. MW-123456"
              />
              <p class="mt-1 text-[11px] text-neutral-muted">
                Entering a valid invite code provides instant account approval upon email verification.
              </p>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <BaseInput
                id="password"
                v-model="form.password"
                type="password"
                label="Password"
                autocomplete="new-password"
                icon="LockKeyhole"
                required
              />
              <BaseInput
                id="confirmPassword"
                v-model="form.confirmPassword"
                type="password"
                label="Confirm Password"
                autocomplete="new-password"
                icon="LockKeyhole"
                required
              />
            </div>

            <p v-if="!passwordMatch" class="text-xs text-red-600">
              Passwords do not match.
            </p>

            <BaseButton
              type="submit"
              variant="accent"
              class="mt-4 h-12 w-full gap-2"
              :disabled="auth.loading || !isValid"
            >
              <UserPlus class="h-4 w-4" :stroke-width="2" />
              {{ auth.loading ? 'Creating Account...' : 'Create Account' }}
            </BaseButton>
          </form>

          <p class="mt-6 text-center text-sm text-neutral-muted">
            Already have an account?
            <RouterLink to="/login" class="font-semibold text-sage hover:underline ml-1">
              Sign In
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

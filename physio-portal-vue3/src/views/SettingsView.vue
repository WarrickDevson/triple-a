<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import BaseButton from '../components/BaseButton.vue'
import {
  APPOINTMENT_DURATIONS,
  displayRole,
  loadClinicSettings,
  loadNotificationSettings,
  saveClinicSettings,
  saveNotificationSettings,
  TIMEZONE_OPTIONS,
  type ClinicSettings,
  type NotificationSettings,
} from '../data/settingsDemo'
import { useAuthStore } from '../store/auth'
import { useNotificationsStore } from '../store/notifications'
import InviteOwnerModal from '../components/clinic/InviteOwnerModal.vue'
import EditProfileModal from '../components/profile/EditProfileModal.vue'
import { Camera, Trash2, Upload } from '@lucide/vue'
import AiPromptEditor from '../components/admin/AiPromptEditor.vue'
import { resolveMediaUrl } from '../api/videos'

const auth = useAuthStore()
const router = useRouter()
const imageError = ref(false)
const profileFileInput = ref<HTMLInputElement | null>(null)
const uploadingProfilePhoto = ref(false)
const photoMessage = ref<{ type: 'success' | 'error'; text: string } | null>(null)

watch(() => auth.user?.profilePictureUrl, () => {
  imageError.value = false
})

async function onProfilePhotoSelected(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  uploadingProfilePhoto.value = true
  photoMessage.value = null
  imageError.value = false

  const ok = await auth.uploadProfilePicture(file)
  uploadingProfilePhoto.value = false

  if (ok) {
    photoMessage.value = { type: 'success', text: 'Profile picture updated successfully.' }
    setTimeout(() => {
      photoMessage.value = null
    }, 3500)
  } else {
    photoMessage.value = { type: 'error', text: auth.error || 'Failed to upload photo.' }
  }

  target.value = ''
}

async function removeProfilePhoto() {
  uploadingProfilePhoto.value = true
  photoMessage.value = null

  const ok = await auth.removeProfilePicture()
  uploadingProfilePhoto.value = false

  if (ok) {
    photoMessage.value = { type: 'success', text: 'Profile picture removed.' }
    setTimeout(() => {
      photoMessage.value = null
    }, 3500)
  } else {
    photoMessage.value = { type: 'error', text: auth.error || 'Failed to remove photo.' }
  }
}

const activeTab = ref<'profile' | 'clinic' | 'ai-prompt' | 'notifications' | 'security' | 'privacy'>('profile')
const showStubModal = ref(false)
const showInviteModal = ref(false)
const showEditProfileModal = ref(false)
const stubMessage = ref('')

const clinic = ref<ClinicSettings>(loadClinicSettings())
const notifications = ref<NotificationSettings>(loadNotificationSettings())
const clinicSaveSuccess = ref<string | null>(null)

const changeForm = reactive({
  currentPassword: '',
  newPassword: '',
})
const changeMessage = ref<string | null>(null)

function syncClinicName() {
  if (auth.user?.clinicName) {
    clinic.value.clinicName = auth.user.clinicName
  }
}

onMounted(() => {
  auth.fetchCurrentUser()
    .then(() => syncClinicName())
    .catch(() => undefined)
})

watch(() => auth.user?.clinicName, (newClinicName) => {
  if (newClinicName) {
    clinic.value.clinicName = newClinicName
  }
})

async function persistClinic() {
  clinicSaveSuccess.value = null
  saveClinicSettings(clinic.value)
  if (auth.user) {
    await auth.updateProfile({
      firstName: auth.user.firstName,
      lastName: auth.user.lastName,
      clinicName: clinic.value.clinicName,
    })
  }
  clinicSaveSuccess.value = 'Clinic settings saved successfully.'
  setTimeout(() => {
    clinicSaveSuccess.value = null
  }, 3000)
}

function onProfileUpdated() {
  syncClinicName()
}

function persistNotifications() {
  saveNotificationSettings(notifications.value)
  const notificationsStore = useNotificationsStore()
  notificationsStore.reloadSettings()
}

const isChangePasswordValid = computed(() => {
  const p = changeForm.newPassword
  return (
    p.length >= 8 &&
    /[a-z]/.test(p) &&
    /[A-Z]/.test(p) &&
    /[0-9]/.test(p) &&
    /[^a-zA-Z0-9]/.test(p)
  )
})

async function submitChangePassword() {
  if (!isChangePasswordValid.value) return
  changeMessage.value = null
  const ok = await auth.changePassword(changeForm.currentPassword, changeForm.newPassword)
  if (ok) {
    changeMessage.value = auth.message
    changeForm.currentPassword = ''
    changeForm.newPassword = ''
  }
}

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-4">
    <div class="flex gap-1 overflow-x-auto border-b border-neutral-grey/80">
      <button
        v-for="tab in [
          { id: 'profile', label: 'Profile' },
          { id: 'clinic', label: 'Clinic' },
          { id: 'ai-prompt', label: 'AI Assistant' },
          { id: 'notifications', label: 'Notifications' },
          { id: 'security', label: 'Security' },
          { id: 'privacy', label: 'Privacy & Legal' },
        ] as const"
        :key="tab.id"
        type="button"
        class="shrink-0 px-4 py-2.5 text-sm font-semibold transition-colors"
        :class="activeTab === tab.id ? 'border-b-2 border-sage text-navy' : 'text-neutral-muted hover:text-navy'"
        @click="activeTab = tab.id"
      >
        {{ tab.label }}
      </button>
    </div>

    <section v-if="activeTab === 'ai-prompt'">
      <AiPromptEditor />
    </section>

    <section v-else-if="activeTab === 'profile'" class="portal-card p-6">
      <div class="flex items-center justify-between">
        <h2 class="text-sm font-bold text-navy">Profile</h2>
        <button type="button" class="text-xs font-semibold text-sage hover:underline" @click="showEditProfileModal = true">
          Edit Details
        </button>
      </div>

      <div v-if="auth.user" class="mt-6 space-y-6">
        <!-- Direct Profile Picture Management on Settings Page -->
        <div class="flex flex-col sm:flex-row sm:items-center gap-5 rounded-2xl border border-neutral-grey/70 bg-surface/80 p-5">
          <div class="relative group h-20 w-20 shrink-0">
            <div class="flex h-20 w-20 items-center justify-center overflow-hidden rounded-full bg-sage-muted text-2xl font-bold text-sage ring-2 ring-sage/30 shadow-sm">
              <img
                v-if="auth.user.profilePictureUrl && !imageError"
                :src="resolveMediaUrl(auth.user.profilePictureUrl)!"
                :alt="`${auth.user.firstName} ${auth.user.lastName}`"
                class="h-full w-full object-cover"
                @error="imageError = true"
              />
              <span v-else>
                {{ auth.user.firstName?.[0] }}{{ auth.user.lastName?.[0] }}
              </span>
            </div>
            <button
              type="button"
              class="absolute inset-0 flex items-center justify-center rounded-full bg-navy/60 text-white opacity-0 transition-opacity group-hover:opacity-100"
              title="Click to change photo"
              :disabled="uploadingProfilePhoto"
              @click="profileFileInput?.click()"
            >
              <Camera class="h-6 w-6" />
            </button>
          </div>

          <div class="flex-1 space-y-2">
            <div>
              <p class="text-lg font-bold text-navy">{{ auth.user.firstName }} {{ auth.user.lastName }}</p>
              <p class="text-xs text-neutral-muted">{{ displayRole(auth.user.userRole) }}</p>
            </div>

            <div class="flex flex-wrap items-center gap-2">
              <input
                ref="profileFileInput"
                type="file"
                accept="image/jpeg,image/png,image/webp"
                class="hidden"
                @change="onProfilePhotoSelected"
              />
              <BaseButton
                size="sm"
                variant="secondary"
                :disabled="uploadingProfilePhoto"
                @click="profileFileInput?.click()"
              >
                <Upload class="mr-1.5 h-3.5 w-3.5 inline" />
                {{ uploadingProfilePhoto ? 'Uploading...' : (auth.user.profilePictureUrl ? 'Change Photo' : 'Upload Photo') }}
              </BaseButton>

              <button
                v-if="auth.user.profilePictureUrl"
                type="button"
                class="inline-flex items-center gap-1 text-xs font-semibold text-alert-red hover:underline px-2 py-1"
                :disabled="uploadingProfilePhoto"
                @click="removeProfilePhoto"
              >
                <Trash2 class="h-3.5 w-3.5" />
                Remove
              </button>
            </div>

            <p class="text-[11px] text-neutral-muted">
              JPG, PNG or WebP, up to 5 MB.
            </p>

            <p
              v-if="photoMessage"
              class="text-xs font-semibold"
              :class="photoMessage.type === 'success' ? 'text-success-green' : 'text-alert-red'"
            >
              {{ photoMessage.text }}
            </p>
          </div>
        </div>

        <dl class="grid gap-3 text-sm sm:grid-cols-2">
          <div>
            <dt class="text-neutral-muted">Email</dt>
            <dd class="font-medium text-navy">{{ auth.user.email }}</dd>
          </div>
          <div>
            <dt class="text-neutral-muted">Subscription</dt>
            <dd class="font-medium text-navy">{{ auth.user.subscriptionTier }}</dd>
          </div>
          <div>
            <dt class="text-neutral-muted">Clinic</dt>
            <dd class="font-medium text-navy">{{ auth.user.clinicName ?? clinic.clinicName ?? '—' }}</dd>
          </div>
          <div v-if="auth.user.clinicInviteCode">
            <dt class="text-neutral-muted">Owner invite code</dt>
            <dd class="font-mono font-semibold text-navy">{{ auth.user.clinicInviteCode }}</dd>
          </div>
        </dl>
        <p v-if="auth.user.clinicInviteCode" class="text-sm text-neutral-muted">
          Share this code with pet owners so they can create an account linked to your clinic.
        </p>
        <div class="pt-2">
          <BaseButton size="sm" @click="showInviteModal = true">Send Owner Invite Email</BaseButton>
        </div>
      </div>
    </section>

    <section v-else-if="activeTab === 'clinic'" class="portal-card p-6">
      <h2 class="text-sm font-bold text-navy">Clinic Settings</h2>
      <div v-if="auth.user?.clinicInviteCode" class="mt-4 rounded-xl border border-sage/30 bg-sage-muted/40 p-4 flex items-center justify-between">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Owner invite code</p>
          <p class="mt-1 font-mono text-lg font-bold text-navy">{{ auth.user.clinicInviteCode }}</p>
          <p class="mt-1 text-xs text-neutral-muted">Owners enter this when signing up in the mobile app.</p>
        </div>
        <BaseButton size="sm" variant="secondary" @click="showInviteModal = true">
          Send Email Invite
        </BaseButton>
      </div>
      <form class="mt-4 space-y-4" @submit.prevent="persistClinic">
        <label class="block">
          <span class="text-sm font-medium text-navy">Clinic name</span>
          <input v-model="clinic.clinicName" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>
        <label class="block">
          <span class="text-sm font-medium text-navy">Timezone</span>
          <select v-model="clinic.timezone" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm">
            <option v-for="tz in TIMEZONE_OPTIONS" :key="tz" :value="tz">{{ tz }}</option>
          </select>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-navy">Default appointment duration</span>
          <select
            v-model.number="clinic.defaultAppointmentMinutes"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
          >
            <option v-for="mins in APPOINTMENT_DURATIONS" :key="mins" :value="mins">{{ mins }} minutes</option>
          </select>
        </label>
        <div v-if="clinicSaveSuccess" class="rounded-lg bg-emerald-50 p-2.5 text-xs font-medium text-emerald-800 border border-emerald-200">
          {{ clinicSaveSuccess }}
        </div>
        <BaseButton type="submit" size="sm">Save clinic settings</BaseButton>
      </form>
    </section>

    <section v-else-if="activeTab === 'notifications'" class="portal-card p-6">
      <h2 class="text-sm font-bold text-navy">Notification Preferences</h2>
      <form class="mt-4 space-y-6" @submit.prevent="persistNotifications">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Email</p>
          <label class="mt-2 flex items-center justify-between py-2">
            <span class="text-sm text-navy">Appointments</span>
            <input v-model="notifications.emailAppointments" type="checkbox" class="h-4 w-4 rounded text-sage" />
          </label>
          <label class="flex items-center justify-between py-2">
            <span class="text-sm text-navy">Messages</span>
            <input v-model="notifications.emailMessages" type="checkbox" class="h-4 w-4 rounded text-sage" />
          </label>
          <label class="flex items-center justify-between py-2">
            <span class="text-sm text-navy">Video reviews</span>
            <input v-model="notifications.emailVideoReviews" type="checkbox" class="h-4 w-4 rounded text-sage" />
          </label>
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">In-app</p>
          <label class="mt-2 flex items-center justify-between py-2">
            <span class="text-sm text-navy">Appointments</span>
            <input v-model="notifications.inAppAppointments" type="checkbox" class="h-4 w-4 rounded text-sage" />
          </label>
          <label class="flex items-center justify-between py-2">
            <span class="text-sm text-navy">Messages</span>
            <input v-model="notifications.inAppMessages" type="checkbox" class="h-4 w-4 rounded text-sage" />
          </label>
          <label class="flex items-center justify-between py-2">
            <span class="text-sm text-navy">Video reviews</span>
            <input v-model="notifications.inAppVideoReviews" type="checkbox" class="h-4 w-4 rounded text-sage" />
          </label>
        </div>
        <BaseButton type="submit" size="sm">Save preferences</BaseButton>
      </form>
    </section>

    <section v-else-if="activeTab === 'security'" class="portal-card p-6">
      <h2 class="text-sm font-bold text-navy">Security</h2>
      <p class="mt-2 text-sm text-neutral-muted">Update your password or sign out.</p>
      <form class="mt-4 space-y-3 max-w-md" @submit.prevent="submitChangePassword">
        <label class="block text-sm">
          <span class="font-medium text-navy">Current password</span>
          <input
            v-model="changeForm.currentPassword"
            type="password"
            required
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
          />
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">New password</span>
          <input
            v-model="changeForm.newPassword"
            type="password"
            required
            minlength="8"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
          />
        </label>
        <p class="text-[11px] text-neutral-muted">
          Must be at least 8 characters with uppercase, lowercase, numbers, and symbols (e.g. Pass!123).
        </p>
        <p v-if="changeForm.newPassword && !isChangePasswordValid" class="text-xs text-red-600">
          Password must include uppercase, lowercase, numbers, and symbols.
        </p>
        <p v-if="auth.error" class="text-sm text-alert-red">{{ auth.error }}</p>
        <p v-if="changeMessage" class="text-sm text-success-green">{{ changeMessage }}</p>
        <BaseButton
          type="submit"
          variant="secondary"
          size="sm"
          :disabled="auth.loading || !isChangePasswordValid || !changeForm.currentPassword"
        >
          {{ auth.loading ? 'Updating...' : 'Change password' }}
        </BaseButton>
      </form>
      <div class="mt-6">
        <BaseButton variant="danger" size="sm" @click="logout">Sign out</BaseButton>
      </div>
    </section>

    <section v-else-if="activeTab === 'privacy'" class="portal-card p-6">
      <div class="flex items-center justify-between">
        <h2 class="text-sm font-bold text-navy">Privacy & POPIA Compliance</h2>
        <span class="rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-800 border border-emerald-200">
          POPIA Compliant
        </span>
      </div>
      <p class="mt-2 text-sm text-neutral-muted">
        Triple A processes personal information and patient rehabilitation records in accordance with South Africa's Protection of Personal Information Act (POPIA Act No. 4 of 2013).
      </p>

      <div class="mt-6 grid gap-4 sm:grid-cols-2">
        <div class="rounded-xl border border-navy/10 bg-neutral-light/50 p-4">
          <h3 class="text-xs font-bold uppercase tracking-wider text-navy">Privacy Policy</h3>
          <p class="mt-1 text-xs text-neutral-muted">
            Read our full POPIA data handling terms, the 8 conditions of lawful processing, and clinical record retention policies.
          </p>
          <a
            href="https://mytriplea.co.za/privacy.html"
            target="_blank"
            rel="noopener noreferrer"
            class="mt-3 inline-flex items-center gap-1.5 text-xs font-bold text-sage hover:underline"
          >
            Open Privacy Policy &rarr;
          </a>
        </div>

        <div class="rounded-xl border border-navy/10 bg-neutral-light/50 p-4">
          <h3 class="text-xs font-bold uppercase tracking-wider text-navy">Account & Data Deletion</h3>
          <p class="mt-1 text-xs text-neutral-muted">
            Submit a data deletion request under POPIA Section 24 or view statutory veterinary record retention requirements.
          </p>
          <a
            href="https://mytriplea.co.za/delete-data.html"
            target="_blank"
            rel="noopener noreferrer"
            class="mt-3 inline-flex items-center gap-1.5 text-xs font-bold text-sage hover:underline"
          >
            Data Deletion Portal &rarr;
          </a>
        </div>
      </div>

      <div class="mt-6 rounded-xl border border-navy/10 p-4">
        <h3 class="text-xs font-bold uppercase tracking-wider text-navy">Information Officer</h3>
        <p class="mt-1 text-xs text-neutral-muted">
          For clinical data queries, subject access requests, or regulatory questions:
        </p>
        <p class="mt-2 text-xs font-medium text-navy">
          Email: <a href="mailto:privacy@mytriplea.co.za" class="text-sage font-bold hover:underline">privacy@mytriplea.co.za</a>
        </p>
      </div>
    </section>

  </div>

  <InviteOwnerModal v-if="showInviteModal" @close="showInviteModal = false" />
  <EditProfileModal v-if="showEditProfileModal" @close="showEditProfileModal = false" @updated="onProfileUpdated" />

  <div
    v-if="showStubModal"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
    @click.self="showStubModal = false"
  >
    <div class="portal-card max-w-sm p-6 text-center">
      <p class="text-sm text-neutral-muted">{{ stubMessage }}</p>
      <BaseButton class="mt-4" size="sm" @click="showStubModal = false">Close</BaseButton>
    </div>
  </div>
</template>

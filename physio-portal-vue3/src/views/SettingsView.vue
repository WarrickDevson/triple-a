<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
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

const auth = useAuthStore()
const router = useRouter()

const activeTab = ref<'profile' | 'clinic' | 'notifications' | 'security'>('profile')
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

async function submitChangePassword() {
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
          { id: 'notifications', label: 'Notifications' },
          { id: 'security', label: 'Security' },
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

    <section v-if="activeTab === 'profile'" class="portal-card p-6">
      <div class="flex items-center justify-between">
        <h2 class="text-sm font-bold text-navy">Profile</h2>
        <button type="button" class="text-xs font-semibold text-sage hover:underline" @click="showEditProfileModal = true">
          Edit
        </button>
      </div>
      <div v-if="auth.user" class="mt-6 space-y-4">
        <div class="flex items-center gap-4">
          <div class="flex h-16 w-16 items-center justify-center rounded-full bg-sage-muted text-xl font-bold text-sage">
            {{ auth.user.firstName?.[0] }}{{ auth.user.lastName?.[0] }}
          </div>
          <div>
            <p class="text-lg font-bold text-navy">{{ auth.user.firstName }} {{ auth.user.lastName }}</p>
            <p class="text-sm text-neutral-muted">{{ displayRole(auth.user.userRole) }}</p>
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

    <section v-else class="portal-card p-6">
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
        <p v-if="auth.error" class="text-sm text-alert-red">{{ auth.error }}</p>
        <p v-if="changeMessage" class="text-sm text-success-green">{{ changeMessage }}</p>
        <BaseButton type="submit" variant="secondary" size="sm" :disabled="auth.loading">
          {{ auth.loading ? 'Updating...' : 'Change password' }}
        </BaseButton>
      </form>
      <div class="mt-6">
        <BaseButton variant="danger" size="sm" @click="logout">Sign out</BaseButton>
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

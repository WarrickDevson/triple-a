<script setup lang="ts">
import { ref } from 'vue'
import { Camera, Trash2 } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { useAuthStore } from '../../store/auth'
import { resolveMediaUrl } from '../../api/videos'

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'updated'): void
}>()

const auth = useAuthStore()

const firstName = ref(auth.user?.firstName ?? '')
const lastName = ref(auth.user?.lastName ?? '')
const clinicName = ref(auth.user?.clinicName ?? '')
const phoneNumber = ref(auth.user?.phoneNumber ?? '')
const fileInput = ref<HTMLInputElement | null>(null)
const uploadingPhoto = ref(false)
const imageError = ref(false)

const saving = ref(false)
const errorMessage = ref<string | null>(null)
const successMessage = ref<string | null>(null)

async function onFileSelected(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  uploadingPhoto.value = true
  errorMessage.value = null
  imageError.value = false

  const ok = await auth.uploadProfilePicture(file)
  uploadingPhoto.value = false

  if (ok) {
    successMessage.value = 'Profile picture updated!'
    emit('updated')
  } else {
    errorMessage.value = auth.error || 'Failed to upload photo.'
  }

  target.value = ''
}

async function handleRemovePhoto() {
  uploadingPhoto.value = true
  errorMessage.value = null
  const ok = await auth.removeProfilePicture()
  uploadingPhoto.value = false

  if (ok) {
    successMessage.value = 'Profile picture removed.'
    emit('updated')
  } else {
    errorMessage.value = auth.error || 'Failed to remove photo.'
  }
}

async function handleSubmit() {
  if (!firstName.value.trim() || !lastName.value.trim()) {
    errorMessage.value = 'First and last name are required.'
    return
  }

  saving.value = true
  errorMessage.value = null
  successMessage.value = null

  const ok = await auth.updateProfile({
    firstName: firstName.value.trim(),
    lastName: lastName.value.trim(),
    clinicName: clinicName.value.trim() || undefined,
    phoneNumber: phoneNumber.value.trim() || undefined,
  })

  saving.value = false

  if (ok) {
    successMessage.value = 'Profile updated successfully!'
    setTimeout(() => {
      emit('updated')
      emit('close')
    }, 600)
  } else {
    errorMessage.value = auth.error || 'Failed to update profile.'
  }
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card w-full max-w-md p-6 shadow-xl animate-in fade-in zoom-in duration-150">
      <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-3">
        <div>
          <h3 class="text-base font-bold text-navy">Edit Profile</h3>
          <p class="text-xs text-neutral-muted">Update your personal and clinic information.</p>
        </div>
        <button
          type="button"
          class="rounded-lg p-1 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
          @click="emit('close')"
        >
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <form class="mt-4 space-y-4" @submit.prevent="handleSubmit">
        <!-- Avatar Photo Section -->
        <div class="flex items-center gap-4 border-b border-neutral-grey/40 pb-4">
          <div class="relative flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-full bg-sage/20 text-lg font-bold text-sage ring-2 ring-sage/30">
            <img
              v-if="auth.user?.profilePictureUrl && !imageError"
              :src="resolveMediaUrl(auth.user.profilePictureUrl)!"
              :alt="`${auth.user.firstName} ${auth.user.lastName}`"
              class="h-full w-full object-cover"
              @error="imageError = true"
            />
            <span v-else>
              {{ auth.user?.firstName?.[0] }}{{ auth.user?.lastName?.[0] }}
            </span>
            <div
              v-if="uploadingPhoto"
              class="absolute inset-0 flex items-center justify-center bg-navy/60 text-xs text-white"
            >
              ...
            </div>
          </div>
          <div>
            <div class="flex items-center gap-2">
              <input
                ref="fileInput"
                type="file"
                accept="image/png,image/jpeg,image/webp"
                class="hidden"
                @change="onFileSelected"
              />
              <button
                type="button"
                :disabled="uploadingPhoto"
                class="inline-flex items-center gap-1.5 rounded-lg border border-sage/40 bg-sage-muted/50 px-3 py-1.5 text-xs font-semibold text-navy transition-colors hover:bg-sage-muted"
                @click="fileInput?.click()"
              >
                <Camera class="h-3.5 w-3.5 text-sage" />
                {{ auth.user?.profilePictureUrl ? 'Change photo' : 'Upload photo' }}
              </button>
              <button
                v-if="auth.user?.profilePictureUrl"
                type="button"
                :disabled="uploadingPhoto"
                class="inline-flex items-center gap-1 rounded-lg border border-alert-red/30 px-2 py-1.5 text-xs font-semibold text-alert-red transition-colors hover:bg-red-50"
                @click="handleRemovePhoto"
              >
                <Trash2 class="h-3.5 w-3.5" />
                Remove
              </button>
            </div>
            <p class="mt-1 text-[11px] text-neutral-muted">JPG, PNG, or WebP. Max 5 MB.</p>
          </div>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wider text-neutral-muted">
              First Name <span class="text-alert-red">*</span>
            </label>
            <input
              v-model="firstName"
              type="text"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none focus:ring-1 focus:ring-sage"
            />
          </div>
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wider text-neutral-muted">
              Last Name <span class="text-alert-red">*</span>
            </label>
            <input
              v-model="lastName"
              type="text"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none focus:ring-1 focus:ring-sage"
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-neutral-muted">
            Clinic Name
          </label>
          <input
            v-model="clinicName"
            type="text"
            placeholder="e.g. Apex Veterinary Rehab Clinic"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none focus:ring-1 focus:ring-sage"
          />
          <p class="mt-1 text-[11px] text-neutral-muted">
            Updating your clinic name here will update it across all settings and client invites.
          </p>
        </div>

        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-neutral-muted">
            Phone Number <span class="font-normal text-neutral-muted/80">(Optional)</span>
          </label>
          <input
            v-model="phoneNumber"
            type="tel"
            placeholder="e.g. +27 82 123 4567"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none focus:ring-1 focus:ring-sage"
          />
        </div>

        <div v-if="successMessage" class="rounded-lg bg-emerald-50 p-3 text-xs font-medium text-emerald-800 border border-emerald-200">
          {{ successMessage }}
        </div>

        <div v-if="errorMessage" class="rounded-lg bg-red-50 p-3 text-xs font-medium text-red-800 border border-red-200">
          {{ errorMessage }}
        </div>

        <div class="flex justify-end gap-3 pt-2">
          <BaseButton variant="secondary" size="sm" type="button" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton size="sm" type="submit" :disabled="saving">
            {{ saving ? 'Saving...' : 'Save Changes' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

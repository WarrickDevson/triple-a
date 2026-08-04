<script setup lang="ts">
import { reactive, ref } from 'vue'
import BaseButton from '../BaseButton.vue'
import { PET_SPECIES } from '../../types/pet'
import type { CreatePetRequest } from '../../types/pet'
import { usePatientsStore } from '../../store/patients'

const emit = defineEmits<{
  close: []
  created: [petId: number]
}>()

const patientsStore = usePatientsStore()
const saving = ref(false)
const error = ref<string | null>(null)

const form = reactive({
  ownerFirstName: '',
  ownerLastName: '',
  ownerEmail: '',
  ownerPhone: '',
  temporaryPassword: '',
  petName: '',
  species: 'Canine',
  breed: '',
  diagnosis: '',
})

async function submit() {
  saving.value = true
  error.value = null
  try {
    const request: CreatePetRequest = {
      petName: form.petName.trim(),
      species: form.species,
      breed: form.breed.trim() || undefined,
      newOwner: {
        email: form.ownerEmail.trim(),
        firstName: form.ownerFirstName.trim(),
        lastName: form.ownerLastName.trim(),
        phoneNumber: form.ownerPhone.trim() || undefined,
        temporaryPassword: form.temporaryPassword,
      },
      initialMedicalHistory: form.diagnosis.trim()
        ? { diagnosis: form.diagnosis.trim() }
        : undefined,
    }
    const pet = await patientsStore.createPatient(request)
    emit('created', pet.petId)
    emit('close')
  } catch {
    error.value = 'Unable to create patient. Check the form and try again.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4" @click.self="emit('close')">
    <div class="portal-card w-full max-w-lg p-6">
      <h2 class="text-lg font-bold text-navy">Add patient</h2>
      <p class="mt-1 text-sm text-neutral-muted">
        Creates a new owner account and pet. Share the temporary password with the owner.
      </p>

      <form class="mt-4 space-y-4" @submit.prevent="submit">
        <div class="grid gap-3 sm:grid-cols-2">
          <label class="block text-sm">
            <span class="font-medium text-navy">Owner first name</span>
            <input v-model="form.ownerFirstName" required class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
          </label>
          <label class="block text-sm">
            <span class="font-medium text-navy">Owner last name</span>
            <input v-model="form.ownerLastName" required class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
          </label>
        </div>
        <label class="block text-sm">
          <span class="font-medium text-navy">Owner email</span>
          <input v-model="form.ownerEmail" type="email" required class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">Owner phone (optional)</span>
          <input v-model="form.ownerPhone" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">Temporary password</span>
          <input v-model="form.temporaryPassword" type="password" required minlength="8" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">Pet name</span>
          <input v-model="form.petName" required class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">Species</span>
          <select v-model="form.species" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm">
            <option v-for="s in PET_SPECIES" :key="s" :value="s">{{ s }}</option>
          </select>
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">Breed (optional)</span>
          <input v-model="form.breed" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>
        <label class="block text-sm">
          <span class="font-medium text-navy">Diagnosis (optional)</span>
          <input v-model="form.diagnosis" class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm" />
        </label>

        <p v-if="error" class="text-sm text-alert-red">{{ error }}</p>

        <div class="flex justify-end gap-2 pt-2">
          <BaseButton variant="secondary" size="sm" @click="emit('close')">Cancel</BaseButton>
          <BaseButton type="submit" size="sm" :disabled="saving">
            {{ saving ? 'Creating...' : 'Create patient' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

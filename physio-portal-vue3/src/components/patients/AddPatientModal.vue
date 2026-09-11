<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import BaseButton from '../BaseButton.vue'
import type { CreatePetRequest } from '../../types/pet'
import { usePatientsStore } from '../../store/patients'
import { useSpeciesBreedStore } from '../../store/speciesBreed'

const emit = defineEmits<{
  close: []
  created: [petId: number]
}>()

const patientsStore = usePatientsStore()
const speciesStore = useSpeciesBreedStore()
const saving = ref(false)
const error = ref<string | null>(null)

onMounted(() => {
  speciesStore.loadConfig()
})

const availableBreeds = computed(() => speciesStore.breedsForSpecies(form.species))

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
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Failed to create patient.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
    <div class="portal-card max-h-[90vh] w-full max-w-lg overflow-y-auto p-6">
      <h2 class="text-lg font-bold text-navy">Add Patient</h2>
      <p class="mt-1 text-sm text-neutral-muted">
        Create a new patient and owner account. An invite email will be sent to the owner with their temporary password.
      </p>

      <form class="mt-6 space-y-4" @submit.prevent="submit">
        <div class="grid grid-cols-2 gap-3">
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
            <option v-for="s in speciesStore.speciesOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
          </select>
        </label>
        <label class="block text-sm">
          <div class="flex items-center justify-between">
            <span class="font-medium text-navy">Breed (optional)</span>
            <span v-if="availableBreeds.length > 0" class="text-xs text-neutral-muted">
              {{ availableBreeds.length }} breeds available
            </span>
          </div>
          <input
            v-model="form.breed"
            list="available-breeds-datalist"
            placeholder="Select from list or type custom..."
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm"
          />
          <datalist id="available-breeds-datalist">
            <option
              v-for="b in availableBreeds"
              :key="b.name"
              :value="b.name"
            >
              {{ b.sizeCategory ? `${b.name} (${b.sizeCategory})` : b.name }}
            </option>
          </datalist>
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

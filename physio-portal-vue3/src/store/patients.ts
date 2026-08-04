import { defineStore } from 'pinia'
import { ref } from 'vue'
import { createPet, getClinicPatients } from '../api/pets'
import type { CreatePetRequest, Pet } from '../types/pet'

export const usePatientsStore = defineStore('patients', () => {
  const patients = ref<Pet[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchClinicPatients(force = false) {
    if (patients.value.length > 0 && !force) {
      return patients.value
    }

    loading.value = true
    error.value = null
    try {
      patients.value = await getClinicPatients()
      return patients.value
    } catch {
      error.value = 'Unable to load clinic patients.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  async function createPatient(request: CreatePetRequest) {
    loading.value = true
    error.value = null
    try {
      const pet = await createPet(request)
      patients.value = [pet, ...patients.value]
      return pet
    } catch {
      error.value = 'Unable to create patient profile.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  function getPatientById(petId: number) {
    return patients.value.find((p) => p.petId === petId) ?? null
  }

  return { patients, loading, error, fetchClinicPatients, createPatient, getPatientById }
})

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { createPet, deletePetPhoto, getClinicPatients, uploadPetPhoto } from '../api/pets'
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

  async function uploadPhoto(petId: number, file: File) {
    loading.value = true
    error.value = null
    try {
      const updatedPet = await uploadPetPhoto(petId, file)
      const index = patients.value.findIndex((p) => p.petId === petId)
      if (index !== -1) {
        patients.value[index] = updatedPet
      }
      return updatedPet
    } catch {
      error.value = 'Unable to upload pet photo.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  async function removePhoto(petId: number) {
    loading.value = true
    error.value = null
    try {
      const updatedPet = await deletePetPhoto(petId)
      const index = patients.value.findIndex((p) => p.petId === petId)
      if (index !== -1) {
        patients.value[index] = updatedPet
      }
      return updatedPet
    } catch {
      error.value = 'Unable to remove pet photo.'
      throw new Error(error.value)
    } finally {
      loading.value = false
    }
  }

  function getPatientById(petId: number) {
    return patients.value.find((p) => p.petId === petId) ?? null
  }

  return {
    patients,
    loading,
    error,
    fetchClinicPatients,
    createPatient,
    uploadPhoto,
    removePhoto,
    getPatientById,
  }
})

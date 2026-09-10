import { apiClient } from './client'
import type { CreatePetRequest, Pet, UpdatePetRequest } from '../types/pet'

export async function getClinicPatients(): Promise<Pet[]> {
  const { data } = await apiClient.get<Pet[]>('/api/pets/clinic')
  return data
}

export async function getPetsByOwner(ownerId: number): Promise<Pet[]> {
  const { data } = await apiClient.get<Pet[]>(`/api/pets/owner/${ownerId}`)
  return data
}

export async function createPet(request: CreatePetRequest): Promise<Pet> {
  const { data } = await apiClient.post<Pet>('/api/pets', request)
  return data
}

export async function updatePet(petId: number, request: UpdatePetRequest): Promise<Pet> {
  const { data } = await apiClient.put<Pet>(`/api/pets/${petId}`, request)
  return data
}

export async function uploadPetPhoto(petId: number, file: File): Promise<Pet> {
  const formData = new FormData()
  formData.append('file', file)
  const { data } = await apiClient.post<Pet>(`/api/pets/${petId}/photo`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

export async function deletePetPhoto(petId: number): Promise<Pet> {
  const { data } = await apiClient.delete<Pet>(`/api/pets/${petId}/photo`)
  return data
}

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

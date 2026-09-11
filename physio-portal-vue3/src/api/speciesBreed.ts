import { apiClient } from './client'

export interface BreedConfig {
  name: string
  sizeCategory?: string | null
  conformation?: string | null
  notes?: string | null
}

export interface SpeciesConfig {
  name: string
  displayName: string
  icon?: string | null
  breeds?: BreedConfig[]
}

export interface SpeciesBreedConfig {
  species: SpeciesConfig[]
  lastModifiedAt?: string | null
  lastModifiedBy?: string | null
}

export async function fetchSpeciesBreedConfig(): Promise<SpeciesBreedConfig> {
  const res = await apiClient.get<SpeciesBreedConfig>('/api/species-breeds')
  return res.data
}

export async function updateSpeciesBreedConfig(species: SpeciesConfig[]): Promise<SpeciesBreedConfig> {
  const res = await apiClient.put<SpeciesBreedConfig>('/api/species-breeds', { species })
  return res.data
}

export async function resetSpeciesBreedConfig(): Promise<SpeciesBreedConfig> {
  const res = await apiClient.post<SpeciesBreedConfig>('/api/species-breeds/reset')
  return res.data
}

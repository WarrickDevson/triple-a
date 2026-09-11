import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import {
  fetchSpeciesBreedConfig,
  resetSpeciesBreedConfig as apiResetSpeciesBreedConfig,
  type BreedConfig,
  type SpeciesBreedConfig,
  type SpeciesConfig,
  updateSpeciesBreedConfig,
} from '../api/speciesBreed'

// Standard fallback if API has not responded yet
const DEFAULT_SPECIES_FALLBACK: SpeciesConfig[] = [
  {
    name: 'Canine',
    displayName: 'Canine (Dog)',
    icon: 'dog',
    breeds: [
      { name: 'Dachshund', sizeCategory: 'Small', conformation: 'Chondrodystrophic' },
      { name: 'Corgi (Pembroke / Cardigan)', sizeCategory: 'Small', conformation: 'Chondrodystrophic' },
      { name: 'French Bulldog', sizeCategory: 'Small', conformation: 'Brachycephalic' },
      { name: 'Pug', sizeCategory: 'Small', conformation: 'Brachycephalic' },
      { name: 'Cavalier King Charles Spaniel', sizeCategory: 'Small', conformation: 'Standard' },
      { name: 'Beagle', sizeCategory: 'Medium', conformation: 'Standard' },
      { name: 'Cocker Spaniel', sizeCategory: 'Medium', conformation: 'Standard' },
      { name: 'Border Collie', sizeCategory: 'Medium', conformation: 'Athletic' },
      { name: 'Australian Shepherd', sizeCategory: 'Medium', conformation: 'Athletic' },
      { name: 'Bulldog (English)', sizeCategory: 'Medium', conformation: 'Brachycephalic' },
      { name: 'Golden Retriever', sizeCategory: 'Large', conformation: 'Athletic' },
      { name: 'Labrador Retriever', sizeCategory: 'Large', conformation: 'Athletic' },
      { name: 'German Shepherd', sizeCategory: 'Large', conformation: 'Athletic' },
      { name: 'Rottweiler', sizeCategory: 'Large', conformation: 'Heavy / Muscular' },
      { name: 'Great Dane', sizeCategory: 'Giant', conformation: 'Giant' },
      { name: 'Saint Bernard', sizeCategory: 'Giant', conformation: 'Giant' },
      { name: 'Mixed Breed / Crossbreed', sizeCategory: 'Medium', conformation: 'Variable' },
    ],
  },
  {
    name: 'Feline',
    displayName: 'Feline (Cat)',
    icon: 'cat',
    breeds: [
      { name: 'Domestic Shorthair', sizeCategory: 'Medium', conformation: 'Standard' },
      { name: 'Domestic Longhair', sizeCategory: 'Medium', conformation: 'Standard' },
      { name: 'Maine Coon', sizeCategory: 'Large', conformation: 'Large Frame' },
      { name: 'British Shorthair', sizeCategory: 'Medium', conformation: 'Stocky' },
      { name: 'Ragdoll', sizeCategory: 'Medium', conformation: 'Large Frame' },
      { name: 'Siamese', sizeCategory: 'Medium', conformation: 'Slender / Athletic' },
      { name: 'Persian', sizeCategory: 'Medium', conformation: 'Brachycephalic' },
      { name: 'Bengal', sizeCategory: 'Medium', conformation: 'Athletic' },
      { name: 'Mixed Breed / Domestic', sizeCategory: 'Medium', conformation: 'Standard' },
    ],
  },
  {
    name: 'Equine',
    displayName: 'Equine (Horse)',
    icon: 'horse',
    breeds: [
      { name: 'Thoroughbred', sizeCategory: 'Large', conformation: 'Sport / Racing' },
      { name: 'Warmblood', sizeCategory: 'Large', conformation: 'Dressage / Jumping' },
      { name: 'Quarter Horse', sizeCategory: 'Large', conformation: 'Muscular / Stock' },
      { name: 'Arabian', sizeCategory: 'Medium', conformation: 'Endurance' },
      { name: 'Pony (Welsh / Shetland)', sizeCategory: 'Small', conformation: 'Pony Frame' },
    ],
  },
  {
    name: 'Avian',
    displayName: 'Avian (Bird)',
    icon: 'bird',
    breeds: [
      { name: 'Parrot (African Grey / Amazon / Macaw)', sizeCategory: 'Medium' },
      { name: 'Cockatiel / Parakeet', sizeCategory: 'Small' },
    ],
  },
  {
    name: 'Other',
    displayName: 'Other / Exotic',
    icon: 'paw',
    breeds: [
      { name: 'Rabbit (Domestic / Lop)', sizeCategory: 'Small', conformation: 'Lagomorph' },
      { name: 'Guinea Pig', sizeCategory: 'Small', conformation: 'Rodent' },
      { name: 'Ferret', sizeCategory: 'Small', conformation: 'Carnivore' },
    ],
  },
]

export const useSpeciesBreedStore = defineStore('speciesBreed', () => {
  const speciesList = ref<SpeciesConfig[]>(DEFAULT_SPECIES_FALLBACK)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const lastModifiedAt = ref<string | null>(null)
  const lastModifiedBy = ref<string | null>(null)
  const isLoaded = ref(false)

  const speciesOptions = computed(() =>
    speciesList.value.map((s) => ({
      value: s.name,
      label: s.displayName,
      icon: s.icon,
      breedCount: s.breeds?.length ?? 0,
    })),
  )

  function breedsForSpecies(speciesName?: string): BreedConfig[] {
    if (!speciesName) return []
    const match = speciesList.value.find(
      (s) => s.name.toLowerCase() === speciesName.trim().toLowerCase(),
    )
    return match?.breeds ?? []
  }

  async function loadConfig(force = false): Promise<void> {
    if (isLoaded.value && !force) return
    loading.value = true
    error.value = null
    try {
      const data: SpeciesBreedConfig = await fetchSpeciesBreedConfig()
      if (data?.species && data.species.length > 0) {
        speciesList.value = data.species
        lastModifiedAt.value = data.lastModifiedAt ?? null
        lastModifiedBy.value = data.lastModifiedBy ?? null
      }
      isLoaded.value = true
    } catch (err: any) {
      // Keep default fallback if API is temporarily unavailable
      error.value = err?.response?.data?.message || 'Failed to load species and breed configuration.'
    } finally {
      loading.value = false
    }
  }

  async function saveConfig(newList: SpeciesConfig[]): Promise<SpeciesBreedConfig> {
    loading.value = true
    error.value = null
    try {
      const updated = await updateSpeciesBreedConfig(newList)
      speciesList.value = updated.species
      lastModifiedAt.value = updated.lastModifiedAt ?? null
      lastModifiedBy.value = updated.lastModifiedBy ?? null
      return updated
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to update species and breed configuration.'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function resetConfig(): Promise<SpeciesBreedConfig> {
    loading.value = true
    error.value = null
    try {
      const reset = await apiResetSpeciesBreedConfig()
      speciesList.value = reset.species
      lastModifiedAt.value = reset.lastModifiedAt ?? null
      lastModifiedBy.value = reset.lastModifiedBy ?? null
      return reset
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to reset species configuration.'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    speciesList,
    speciesOptions,
    loading,
    error,
    lastModifiedAt,
    lastModifiedBy,
    isLoaded,
    breedsForSpecies,
    loadConfig,
    saveConfig,
    resetConfig,
  }
})

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { Bookmark, ClipboardList, Dumbbell, Plus } from '@lucide/vue'
import { BODY_REGIONS } from '../../data/exerciseDemo'
import { useSpeciesBreedStore } from '../../store/speciesBreed'

const props = withDefaults(
  defineProps<{
    species: string
    breed?: string
    bodyRegion: string
    difficulty: string
  }>(),
  {
    breed: 'All Breeds',
  },
)

const emit = defineEmits<{
  'update:species': [value: string]
  'update:breed': [value: string]
  'update:bodyRegion': [value: string]
  'update:difficulty': [value: string]
  clear: []
}>()

const speciesStore = useSpeciesBreedStore()

onMounted(() => {
  speciesStore.loadConfig().catch(() => undefined)
})

const speciesOptions = computed(() => {
  const configured = speciesStore.speciesList.map((s) => s.name)
  return ['All Species', ...configured]
})

const breedOptions = computed(() => {
  if (props.species && props.species !== 'All Species') {
    const list = speciesStore.breedsForSpecies(props.species)
    return ['All Breeds', ...list.map((b) => b.name)]
  }
  // If "All Species", collect all breeds across all species
  const allBreeds = new Set<string>()
  for (const sp of speciesStore.speciesList) {
    for (const br of sp.breeds || []) {
      allBreeds.add(br.name)
    }
  }
  return ['All Breeds', ...Array.from(allBreeds).sort()]
})

const difficultyOptions = ['All Levels', '1', '2', '3', '4', '5']
</script>

<template>
  <section class="portal-card flex h-full flex-col overflow-hidden">
    <div class="border-b border-neutral-grey/80 p-4">
      <h2 class="text-sm font-bold text-navy">Quick Actions</h2>
      <div class="mt-3 space-y-2">
        <RouterLink
          :to="{ name: 'treatment-plans' }"
          class="flex items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
        >
          <ClipboardList class="h-4 w-4 text-sage" :stroke-width="1.75" />
          Add to Patient Plan
        </RouterLink>
        <button
          type="button"
          class="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
        >
          <Plus class="h-4 w-4 text-sage" :stroke-width="1.75" />
          Create New Exercise
        </button>
        <RouterLink
          :to="{ name: 'exercises' }"
          class="flex items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
        >
          <Dumbbell class="h-4 w-4 text-sage" :stroke-width="1.75" />
          My Custom Exercises
        </RouterLink>
        <button
          type="button"
          class="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-sm text-navy transition-colors hover:bg-surface"
        >
          <Bookmark class="h-4 w-4 text-sage" :stroke-width="1.75" />
          Equipment Library
        </button>
      </div>
    </div>

    <div class="flex-1 p-4">
      <h2 class="text-sm font-bold text-navy">Filter By</h2>
      <div class="mt-3 space-y-3">
        <label class="block">
          <span class="text-xs font-medium text-neutral-muted">Species</span>
          <select
            :value="species"
            class="mt-1 w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            @change="emit('update:species', ($event.target as HTMLSelectElement).value)"
          >
            <option v-for="opt in speciesOptions" :key="opt" :value="opt">{{ opt }}</option>
          </select>
        </label>
        <label class="block">
          <span class="text-xs font-medium text-neutral-muted">Breed / Conformation</span>
          <select
            :value="breed || 'All Breeds'"
            class="mt-1 w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            @change="emit('update:breed', ($event.target as HTMLSelectElement).value)"
          >
            <option v-for="opt in breedOptions" :key="opt" :value="opt">{{ opt }}</option>
          </select>
        </label>
        <label class="block">
          <span class="text-xs font-medium text-neutral-muted">Body Region</span>
          <select
            :value="bodyRegion"
            class="mt-1 w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            @change="emit('update:bodyRegion', ($event.target as HTMLSelectElement).value)"
          >
            <option v-for="opt in BODY_REGIONS" :key="opt" :value="opt">{{ opt }}</option>
          </select>
        </label>
        <label class="block">
          <span class="text-xs font-medium text-neutral-muted">Difficulty</span>
          <select
            :value="difficulty"
            class="mt-1 w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            @change="emit('update:difficulty', ($event.target as HTMLSelectElement).value)"
          >
            <option v-for="opt in difficultyOptions" :key="opt" :value="opt">{{ opt }}</option>
          </select>
        </label>
        <button
          type="button"
          class="text-xs font-semibold text-sage hover:text-navy"
          @click="emit('clear')"
        >
          Clear Filters
        </button>
      </div>

      <p class="mt-6 rounded-xl border border-neutral-grey/80 bg-surface p-3 text-xs leading-relaxed text-neutral-muted">
        Every exercise can be customised to suit your patient's needs.
      </p>
    </div>
  </section>
</template>

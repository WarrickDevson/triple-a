<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  AlertCircle,
  CheckCircle2,
  Filter,
  Layers,
  Loader2,
  Plus,
  RotateCcw,
  Save,
  Search,
  Tag,
  Trash2,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { useSpeciesBreedStore } from '../../store/speciesBreed'
import type { SpeciesConfig } from '../../api/speciesBreed'

const store = useSpeciesBreedStore()

const localSpeciesList = ref<SpeciesConfig[]>([])
const selectedSpeciesName = ref<string>('Canine')
const searchQuery = ref('')
const selectedSizeFilter = ref<string>('All')

const saving = ref(false)
const resetting = ref(false)
const successMessage = ref<string | null>(null)
const errorMessage = ref<string | null>(null)

// Add Breed Form
const newBreedName = ref('')
const newBreedSize = ref('Medium')
const newBreedConformation = ref('Standard')
const newBreedNotes = ref('')

// Add Species Form / Modal
const showAddSpeciesModal = ref(false)
const newSpeciesName = ref('')
const newSpeciesDisplayName = ref('')
const newSpeciesIcon = ref('paw')

onMounted(async () => {
  await store.loadConfig()
  syncFromStore()
})

function syncFromStore() {
  localSpeciesList.value = JSON.parse(JSON.stringify(store.speciesList))
  if (localSpeciesList.value.length > 0 && !selectedSpecies.value) {
    selectedSpeciesName.value = localSpeciesList.value[0].name
  }
}

const selectedSpecies = computed(() => {
  return localSpeciesList.value.find(
    (s) => s.name.toLowerCase() === selectedSpeciesName.value.toLowerCase(),
  )
})

const totalBreedsCount = computed(() => {
  return localSpeciesList.value.reduce((acc, s) => acc + (s.breeds?.length ?? 0), 0)
})

const hasChanges = computed(() => {
  return JSON.stringify(localSpeciesList.value) !== JSON.stringify(store.speciesList)
})

const filteredBreeds = computed(() => {
  if (!selectedSpecies.value?.breeds) return []
  let list = selectedSpecies.value.breeds

  if (selectedSizeFilter.value !== 'All') {
    list = list.filter((b) => b.sizeCategory === selectedSizeFilter.value)
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(
      (b) =>
        b.name.toLowerCase().includes(q) ||
        (b.conformation && b.conformation.toLowerCase().includes(q)) ||
        (b.notes && b.notes.toLowerCase().includes(q)),
    )
  }

  return list
})

function addBreed() {
  if (!newBreedName.value.trim()) return
  if (!selectedSpecies.value) return

  if (!selectedSpecies.value.breeds) {
    selectedSpecies.value.breeds = []
  }

  const exists = selectedSpecies.value.breeds.some(
    (b) => b.name.toLowerCase() === newBreedName.value.trim().toLowerCase(),
  )
  if (exists) {
    errorMessage.value = `Breed "${newBreedName.value.trim()}" already exists in ${selectedSpecies.value.displayName}.`
    setTimeout(() => {
      errorMessage.value = null
    }, 4000)
    return
  }

  selectedSpecies.value.breeds.push({
    name: newBreedName.value.trim(),
    sizeCategory: newBreedSize.value,
    conformation: newBreedConformation.value.trim() || undefined,
    notes: newBreedNotes.value.trim() || undefined,
  })

  // Sort alphabetically
  selectedSpecies.value.breeds.sort((a, b) => a.name.localeCompare(b.name))

  // Reset inputs
  newBreedName.value = ''
  newBreedNotes.value = ''
}

function removeBreed(index: number) {
  if (!selectedSpecies.value?.breeds) return
  selectedSpecies.value.breeds.splice(index, 1)
}

function addSpecies() {
  if (!newSpeciesName.value.trim()) return
  const rawName = newSpeciesName.value.trim()
  const exists = localSpeciesList.value.some(
    (s) => s.name.toLowerCase() === rawName.toLowerCase(),
  )
  if (exists) {
    errorMessage.value = `Species "${rawName}" already exists.`
    return
  }

  const dispName = newSpeciesDisplayName.value.trim() || rawName
  const newSpec: SpeciesConfig = {
    name: rawName,
    displayName: dispName,
    icon: newSpeciesIcon.value || 'paw',
    breeds: [],
  }

  localSpeciesList.value.push(newSpec)
  selectedSpeciesName.value = rawName

  showAddSpeciesModal.value = false
  newSpeciesName.value = ''
  newSpeciesDisplayName.value = ''
}

function removeSelectedSpecies() {
  if (localSpeciesList.value.length <= 1) {
    errorMessage.value = 'You must have at least one configured species.'
    return
  }
  const idx = localSpeciesList.value.findIndex(
    (s) => s.name.toLowerCase() === selectedSpeciesName.value.toLowerCase(),
  )
  if (idx !== -1) {
    localSpeciesList.value.splice(idx, 1)
    selectedSpeciesName.value = localSpeciesList.value[0].name
  }
}

async function handleSave() {
  saving.value = true
  successMessage.value = null
  errorMessage.value = null
  try {
    await store.saveConfig(localSpeciesList.value)
    successMessage.value = 'Species and breed configuration saved successfully!'
    setTimeout(() => {
      successMessage.value = null
    }, 4000)
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to save configuration.'
  } finally {
    saving.value = false
  }
}

async function handleReset() {
  if (!confirm('Reset species and breeds to standard veterinary defaults? Any custom added breeds will be replaced.')) {
    return
  }
  resetting.value = true
  successMessage.value = null
  errorMessage.value = null
  try {
    await store.resetConfig()
    syncFromStore()
    successMessage.value = 'Configuration successfully restored to system defaults.'
    setTimeout(() => {
      successMessage.value = null
    }, 4000)
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to reset configuration.'
  } finally {
    resetting.value = false
  }
}

function getSizeBadgeClass(size?: string | null) {
  switch (size) {
    case 'Toy':
      return 'bg-purple-50 text-purple-700 border-purple-200'
    case 'Small':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200'
    case 'Medium':
      return 'bg-sky-50 text-sky-700 border-sky-200'
    case 'Large':
      return 'bg-amber-50 text-amber-800 border-amber-200'
    case 'Giant':
      return 'bg-rose-50 text-rose-700 border-rose-200'
    default:
      return 'bg-neutral-100 text-neutral-600 border-neutral-200'
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Top Header Banner -->
    <div class="portal-card p-6 bg-gradient-to-r from-surface to-surface-card border border-neutral-grey/80">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-sage/15 text-sage">
              <Layers class="h-3.5 w-3.5" />
              Platform Master Catalog
            </span>
            <span v-if="hasChanges" class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-800">
              Unsaved Changes
            </span>
          </div>
          <h2 class="text-xl font-bold text-navy">Species & Breed Manager</h2>
          <p class="text-xs text-neutral-muted mt-0.5">
            Configure standardized species, breeds, and morphological size classifications. These sync directly with patient intake forms and exercise routine variation selectors.
          </p>
        </div>

        <div class="flex items-center gap-2 shrink-0">
          <button
            type="button"
            :disabled="resetting"
            class="inline-flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-neutral-muted hover:text-navy border border-neutral-grey rounded-lg bg-white hover:bg-neutral-50 transition-colors"
            @click="handleReset"
          >
            <RotateCcw class="h-3.5 w-3.5" :class="{ 'animate-spin': resetting }" />
            Reset Defaults
          </button>

          <BaseButton
            variant="primary"
            size="sm"
            :disabled="saving || !hasChanges"
            class="gap-1.5"
            @click="handleSave"
          >
            <Loader2 v-if="saving" class="h-4 w-4 animate-spin" />
            <Save v-else class="h-4 w-4" />
            Save Changes
          </BaseButton>
        </div>
      </div>

      <!-- Overview Stats -->
      <div class="mt-4 pt-4 border-t border-neutral-grey/60 flex flex-wrap gap-6 text-xs text-neutral-muted">
        <div>
          <strong class="text-navy font-semibold">{{ localSpeciesList.length }}</strong> Configured Species
        </div>
        <div>
          <strong class="text-navy font-semibold">{{ totalBreedsCount }}</strong> Standardized Breeds
        </div>
        <div v-if="store.lastModifiedAt" class="text-neutral-muted/80">
          Last updated: {{ new Date(store.lastModifiedAt).toLocaleDateString() }} by {{ store.lastModifiedBy || 'Admin' }}
        </div>
      </div>
    </div>

    <!-- Alert Notifications -->
    <div
      v-if="successMessage"
      class="flex items-center gap-2.5 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-xs font-medium text-emerald-800 animate-in fade-in"
    >
      <CheckCircle2 class="h-4 w-4 text-emerald-600 shrink-0" />
      <span>{{ successMessage }}</span>
    </div>

    <div
      v-if="errorMessage"
      class="flex items-center gap-2.5 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-xs font-medium text-rose-800 animate-in fade-in"
    >
      <AlertCircle class="h-4 w-4 text-rose-600 shrink-0" />
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Main Manager Body -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      <!-- Species Selector Sidebar / Column -->
      <div class="lg:col-span-4 space-y-3">
        <div class="flex items-center justify-between px-1">
          <h3 class="text-xs font-bold uppercase tracking-wider text-neutral-muted">
            Species Categories
          </h3>
          <button
            type="button"
            class="inline-flex items-center gap-1 text-xs font-semibold text-sage hover:underline"
            @click="showAddSpeciesModal = true"
          >
            <Plus class="h-3.5 w-3.5" />
            Add Species
          </button>
        </div>

        <div class="portal-card p-2 space-y-1.5 divide-y divide-neutral-grey/40">
          <div
            v-for="s in localSpeciesList"
            :key="s.name"
            class="pt-1.5 first:pt-0"
          >
            <button
              type="button"
              class="w-full flex items-center justify-between p-3 rounded-lg text-left transition-all"
              :class="
                selectedSpeciesName.toLowerCase() === s.name.toLowerCase()
                  ? 'bg-sage/10 text-navy font-bold border border-sage/30 shadow-xs'
                  : 'text-neutral-muted hover:text-navy hover:bg-neutral-50 border border-transparent'
              "
              @click="selectedSpeciesName = s.name"
            >
              <div class="flex items-center gap-2.5">
                <span class="flex h-7 w-7 items-center justify-center rounded-md bg-white border border-neutral-grey/60 text-xs font-bold text-navy shadow-2xs">
                  {{ s.name[0] }}
                </span>
                <div>
                  <p class="text-sm leading-snug">{{ s.displayName }}</p>
                  <p class="text-[11px] font-normal text-neutral-muted">
                    {{ s.breeds?.length ?? 0 }} breeds cataloged
                  </p>
                </div>
              </div>

              <span
                class="px-2 py-0.5 rounded-full text-[11px] font-semibold"
                :class="
                  selectedSpeciesName.toLowerCase() === s.name.toLowerCase()
                    ? 'bg-sage text-white'
                    : 'bg-neutral-100 text-neutral-muted'
                "
              >
                {{ s.breeds?.length ?? 0 }}
              </span>
            </button>
          </div>
        </div>

        <div v-if="selectedSpecies && localSpeciesList.length > 1" class="pt-2 px-1 text-right">
          <button
            type="button"
            class="text-xs font-semibold text-rose-600 hover:text-rose-800 hover:underline inline-flex items-center gap-1"
            @click="removeSelectedSpecies"
          >
            <Trash2 class="h-3.5 w-3.5" />
            Delete {{ selectedSpecies.displayName }}
          </button>
        </div>
      </div>

      <!-- Breeds Management Column -->
      <div class="lg:col-span-8 space-y-4">
        <div v-if="selectedSpecies" class="portal-card p-5 space-y-5">
          <!-- Active Species Header & Add Breed Bar -->
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-neutral-grey/60 pb-4">
            <div>
              <h3 class="text-lg font-bold text-navy">
                {{ selectedSpecies.displayName }} Breeds
              </h3>
              <p class="text-xs text-neutral-muted">
                Standard breeds and size variations used for {{ selectedSpecies.name }} patients and rehab routines.
              </p>
            </div>
            <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-navy text-white">
              {{ selectedSpecies.breeds?.length ?? 0 }} Total
            </span>
          </div>

          <!-- Add New Breed Inline Section -->
          <div class="rounded-xl border border-neutral-grey/80 bg-neutral-50/70 p-4 space-y-3">
            <div class="flex items-center gap-2">
              <Plus class="h-4 w-4 text-sage" />
              <h4 class="text-xs font-bold uppercase tracking-wider text-navy">
                Add New {{ selectedSpecies.name }} Breed
              </h4>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <div>
                <label class="block text-[11px] font-semibold text-navy mb-1">Breed Name *</label>
                <input
                  v-model="newBreedName"
                  type="text"
                  placeholder="e.g. Bernedoodle"
                  class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-1.5 text-xs text-navy outline-none focus:border-sage focus:ring-1 focus:ring-sage"
                  @keydown.enter.prevent="addBreed"
                />
              </div>

              <div>
                <label class="block text-[11px] font-semibold text-navy mb-1">Size Classification</label>
                <select
                  v-model="newBreedSize"
                  class="w-full rounded-lg border border-neutral-grey bg-white px-2.5 py-1.5 text-xs text-navy outline-none focus:border-sage"
                >
                  <option value="Toy">Toy (&lt; 5kg)</option>
                  <option value="Small">Small (5 - 12kg)</option>
                  <option value="Medium">Medium (12 - 25kg)</option>
                  <option value="Large">Large (25 - 45kg)</option>
                  <option value="Giant">Giant (&gt; 45kg)</option>
                </select>
              </div>

              <div>
                <label class="block text-[11px] font-semibold text-navy mb-1">Conformation / Frame</label>
                <input
                  v-model="newBreedConformation"
                  type="text"
                  placeholder="e.g. Chondrodystrophic, Athletic"
                  class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-1.5 text-xs text-navy outline-none focus:border-sage focus:ring-1 focus:ring-sage"
                  @keydown.enter.prevent="addBreed"
                />
              </div>
            </div>

            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pt-1">
              <div class="flex-1">
                <input
                  v-model="newBreedNotes"
                  type="text"
                  placeholder="Clinical rehab notes (e.g. 'Prone to hip dysplasia; avoid high impact jumping')"
                  class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-1.5 text-xs text-navy outline-none focus:border-sage focus:ring-1 focus:ring-sage"
                  @keydown.enter.prevent="addBreed"
                />
              </div>
              <button
                type="button"
                :disabled="!newBreedName.trim()"
                class="inline-flex items-center justify-center gap-1.5 rounded-lg bg-sage px-4 py-1.5 text-xs font-bold text-white shadow-2xs hover:bg-sage-dark transition-all disabled:opacity-50 disabled:pointer-events-none"
                @click="addBreed"
              >
                <Plus class="h-3.5 w-3.5" />
                Add to List
              </button>
            </div>
          </div>

          <!-- Search & Filter Controls -->
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pt-2">
            <!-- Search bar -->
            <div class="relative flex-1 max-w-sm">
              <Search class="absolute left-3 top-2.5 h-3.5 w-3.5 text-neutral-muted" />
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Search breeds or notes..."
                class="w-full rounded-lg border border-neutral-grey bg-white pl-8 pr-3 py-1.5 text-xs text-navy outline-none focus:border-sage"
              />
            </div>

            <!-- Size Filter Pills -->
            <div class="flex items-center gap-1 overflow-x-auto pb-1 text-xs">
              <span class="text-neutral-muted text-[11px] font-medium mr-1 flex items-center gap-1">
                <Filter class="h-3 w-3" /> Size:
              </span>
              <button
                v-for="size in ['All', 'Toy', 'Small', 'Medium', 'Large', 'Giant']"
                :key="size"
                type="button"
                class="px-2.5 py-1 rounded-md text-[11px] font-semibold transition-colors"
                :class="
                  selectedSizeFilter === size
                    ? 'bg-navy text-white shadow-2xs'
                    : 'bg-neutral-100 text-neutral-muted hover:bg-neutral-200'
                "
                @click="selectedSizeFilter = size"
              >
                {{ size }}
              </button>
            </div>
          </div>

          <!-- Breeds Table / List -->
          <div class="rounded-xl border border-neutral-grey/70 overflow-hidden shadow-2xs">
            <div v-if="filteredBreeds.length === 0" class="p-8 text-center text-xs text-neutral-muted">
              No breeds found matching "{{ searchQuery }}".
            </div>

            <div v-else class="divide-y divide-neutral-grey/40 max-h-[500px] overflow-y-auto">
              <div
                v-for="(breed, idx) in filteredBreeds"
                :key="breed.name"
                class="flex items-center justify-between p-3.5 hover:bg-neutral-50/80 transition-colors group"
              >
                <div class="space-y-1 min-w-0 pr-4">
                  <div class="flex items-center gap-2 flex-wrap">
                    <span class="font-bold text-sm text-navy">{{ breed.name }}</span>
                    <span
                      v-if="breed.sizeCategory"
                      class="inline-block px-2 py-0.5 rounded-full text-[10px] font-bold border"
                      :class="getSizeBadgeClass(breed.sizeCategory)"
                    >
                      {{ breed.sizeCategory }}
                    </span>
                    <span
                      v-if="breed.conformation"
                      class="inline-flex items-center gap-1 text-[11px] text-neutral-muted bg-neutral-100 px-2 py-0.5 rounded-md"
                    >
                      <Tag class="h-3 w-3 text-neutral-muted" />
                      {{ breed.conformation }}
                    </span>
                  </div>
                  <p v-if="breed.notes" class="text-xs text-neutral-muted/90 truncate">
                    {{ breed.notes }}
                  </p>
                </div>

                <button
                  type="button"
                  title="Remove breed"
                  class="shrink-0 p-1.5 rounded-md text-neutral-muted hover:text-rose-600 hover:bg-rose-50 transition-colors opacity-80 group-hover:opacity-100"
                  @click="removeBreed(idx)"
                >
                  <Trash2 class="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Species Modal -->
    <div
      v-if="showAddSpeciesModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-xs p-4 animate-in fade-in"
    >
      <div class="portal-card max-w-md w-full p-6 space-y-4 shadow-xl">
        <h3 class="text-lg font-bold text-navy">Add New Species Category</h3>
        <p class="text-xs text-neutral-muted">
          Add an animal category to configure breeds and target rehabilitation programs.
        </p>

        <div class="space-y-3">
          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Species Key / Code *</label>
            <input
              v-model="newSpeciesName"
              type="text"
              placeholder="e.g. Caprine, Reptile"
              class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Display Label</label>
            <input
              v-model="newSpeciesDisplayName"
              type="text"
              placeholder="e.g. Caprine (Goat)"
              class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            />
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 pt-2">
          <button
            type="button"
            class="px-4 py-2 text-xs font-semibold text-neutral-muted hover:text-navy"
            @click="showAddSpeciesModal = false"
          >
            Cancel
          </button>
          <BaseButton
            variant="primary"
            size="sm"
            :disabled="!newSpeciesName.trim()"
            @click="addSpecies"
          >
            Add Species
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

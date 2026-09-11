<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { Plus, Search, ShieldCheck, SlidersHorizontal } from '@lucide/vue'
import ExerciseCategorySidebar from '../components/exercises/ExerciseCategorySidebar.vue'
import ExerciseEditorModal from '../components/exercises/ExerciseEditorModal.vue'
import ExerciseFilterPanel from '../components/exercises/ExerciseFilterPanel.vue'
import ExerciseGrid from '../components/exercises/ExerciseGrid.vue'
import ExerciseTabs from '../components/exercises/ExerciseTabs.vue'
import ExerciseTemplatesTab from '../components/exercises/ExerciseTemplatesTab.vue'
import BaseButton from '../components/BaseButton.vue'
import { getCategoryLabel } from '../data/exerciseDemo'
import { useAuthStore } from '../store/auth'
import { useExercisesStore } from '../store/exercises'
import type { Exercise } from '../types/exercise'

const authStore = useAuthStore()
const exercisesStore = useExercisesStore()

const isSysAdmin = computed(() => authStore.user?.userRole === 'SysAdmin')

const search = ref('')
const selectedCategory = ref('All Categories')
const activeTab = ref<'all' | 'favourites' | 'region' | 'templates'>('all')
const speciesFilter = ref('All Species')
const bodyRegionFilter = ref('All Regions')
const difficultyFilter = ref('All Levels')
const libraryFilter = ref<'all' | 'defaults' | 'custom' | 'overrides'>('all')

const showModal = ref(false)
const modalMode = ref<'create' | 'edit' | 'customize'>('create')
const modalTab = ref<'editor' | 'preview'>('editor')
const selectedExercise = ref<Exercise | null>(null)
const showFiltersMobile = ref(false)

onMounted(() => {
  exercisesStore.fetchExercises().catch(() => undefined)
})

watch(speciesFilter, async (value) => {
  const species = value === 'All Species' ? undefined : value
  await exercisesStore.fetchExercises(species, undefined, true).catch(() => undefined)
})

const filteredExercises = computed(() => {
  let list = exercisesStore.exercises
  const query = search.value.trim().toLowerCase()

  if (activeTab.value === 'favourites') {
    list = list.filter((e) => exercisesStore.isFavourite(e.exerciseId))
  } else if (activeTab.value === 'templates') {
    return []
  }

  // Tier library filter (for physios)
  if (!isSysAdmin.value) {
    if (libraryFilter.value === 'defaults') {
      list = list.filter((e) => e.isSystemDefault || !e.clinicId)
    } else if (libraryFilter.value === 'custom') {
      list = list.filter((e) => !e.isSystemDefault && !e.baseExerciseId)
    } else if (libraryFilter.value === 'overrides') {
      list = list.filter((e) => Boolean(e.baseExerciseId) || e.hasCustomOverride)
    }
  }

  if (selectedCategory.value !== 'All Categories') {
    list = list.filter((e) => getCategoryLabel(e) === selectedCategory.value)
  }

  if (query) {
    list = list.filter(
      (e) =>
        e.title.toLowerCase().includes(query) ||
        (e.shortDescription?.toLowerCase().includes(query) ?? false) ||
        (e.targetedMuscles?.toLowerCase().includes(query) ?? false) ||
        (e.clinicalPurpose?.toLowerCase().includes(query) ?? false),
    )
  }

  if (bodyRegionFilter.value !== 'All Regions') {
    list = list.filter((e) =>
      e.targetedMuscles?.toLowerCase().includes(bodyRegionFilter.value.toLowerCase()),
    )
  }

  if (difficultyFilter.value !== 'All Levels') {
    list = list.filter((e) => e.difficultyLevel === Number(difficultyFilter.value))
  }

  if (activeTab.value === 'region' && bodyRegionFilter.value === 'All Regions') {
    list = list.filter((e) => Boolean(e.targetedMuscles))
  }

  return list
})

const categoryList = computed(() => {
  const set = new Set<string>()
  for (const ex of exercisesStore.exercises) {
    set.add(getCategoryLabel(ex))
  }
  return ['All Categories', ...Array.from(set).sort()]
})

function openCreateModal() {
  selectedExercise.value = null
  modalMode.value = 'create'
  modalTab.value = 'editor'
  showModal.value = true
}

function handleEdit(exercise: Exercise) {
  selectedExercise.value = exercise
  modalMode.value = 'edit'
  modalTab.value = 'editor'
  showModal.value = true
}

function handleCustomize(exercise: Exercise) {
  selectedExercise.value = exercise
  modalMode.value = 'customize'
  modalTab.value = 'editor'
  showModal.value = true
}

function handlePreview(exercise: Exercise) {
  selectedExercise.value = exercise
  modalMode.value = 'edit'
  modalTab.value = 'preview'
  showModal.value = true
}

async function handleToggleActive(exerciseId: number) {
  try {
    await exercisesStore.toggleActive(exerciseId)
  } catch {
    // handled in store
  }
}

function clearFilters() {
  speciesFilter.value = 'All Species'
  bodyRegionFilter.value = 'All Regions'
  difficultyFilter.value = 'All Levels'
  selectedCategory.value = 'All Categories'
  libraryFilter.value = 'all'
  search.value = ''
  exercisesStore.fetchExercises(undefined, undefined, true).catch(() => undefined)
}
</script>

<template>
  <div class="space-y-4">
    <!-- Role Banner -->
    <div
      v-if="isSysAdmin"
      class="rounded-xl border border-navy/20 bg-gradient-to-r from-navy to-slate-800 p-4 text-white shadow-sm flex flex-wrap items-center justify-between gap-3"
    >
      <div class="flex items-center gap-3">
        <div class="flex h-10 w-10 items-center justify-center rounded-lg bg-white/10 text-white">
          <ShieldCheck class="h-6 w-6 text-sage-light" />
        </div>
        <div>
          <h2 class="font-bold text-sm sm:text-base">Admin Exercise Portal (Global Defaults)</h2>
          <p class="text-xs text-white/70">
            Upload default exercise videos & images, customize instruction steps, and manage system defaults across all physio accounts.
          </p>
        </div>
      </div>
      <BaseButton size="sm" variant="accent" @click="openCreateModal">
        <Plus class="h-4 w-4" />
        New Default Exercise
      </BaseButton>
    </div>

    <div
      v-else
      class="rounded-xl border border-sage/30 bg-gradient-to-r from-sage/10 via-white to-amber-50/50 p-4 shadow-xs flex flex-wrap items-center justify-between gap-3"
    >
      <div>
        <h2 class="font-bold text-sm sm:text-base text-navy">Exercise Library & Clinic Customizer</h2>
        <p class="text-xs text-neutral-muted">
          Use admin defaults or customize exercises with your clinic videos, instructions, and images. Both versions are kept so you can choose which stays active for your owners.
        </p>
      </div>

      <div class="flex items-center gap-2">
        <!-- Sub-filter for physios -->
        <select
          v-model="libraryFilter"
          class="rounded-lg border border-neutral-grey bg-white px-3 py-2 text-xs font-semibold text-navy outline-none focus:border-sage"
        >
          <option value="all">All Exercises</option>
          <option value="defaults">System Defaults</option>
          <option value="custom">My Custom Exercises</option>
          <option value="overrides">Customized by Me</option>
        </select>

        <BaseButton size="sm" variant="accent" @click="openCreateModal">
          <Plus class="h-4 w-4" />
          Add Custom Exercise
        </BaseButton>
      </div>
    </div>

    <!-- Search & Filters Header -->
    <div class="flex flex-wrap items-center gap-3">
      <div class="relative min-w-[200px] flex-1">
        <Search class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          v-model="search"
          type="search"
          placeholder="Search exercises, muscles, condition..."
          class="w-full rounded-lg border border-neutral-grey bg-white py-2.5 pl-10 pr-4 text-sm outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
        />
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-lg border border-neutral-grey bg-white px-4 py-2.5 text-sm font-semibold text-navy xl:hidden"
        @click="showFiltersMobile = !showFiltersMobile"
      >
        <SlidersHorizontal class="h-4 w-4" :stroke-width="1.75" />
        Filters
      </button>
    </div>

    <!-- Mobile Category Horizontal Pills (< xl screens) -->
    <div class="xl:hidden flex items-center gap-2 overflow-x-auto pb-1 -mt-1 no-scrollbar">
      <button
        v-for="cat in categoryList"
        :key="cat"
        type="button"
        class="shrink-0 rounded-full px-3 py-1 text-xs font-semibold transition-colors"
        :class="
          selectedCategory === cat
            ? 'bg-sage text-white shadow-2xs'
            : 'bg-white border border-neutral-grey/80 text-neutral-muted hover:text-navy'
        "
        @click="selectedCategory = cat"
      >
        {{ cat }}
      </button>
    </div>

    <!-- Main Grid Layout -->
    <div class="grid gap-6 xl:grid-cols-[220px_minmax(0,1fr)_240px]">
      <div class="hidden min-h-[600px] xl:block">
        <ExerciseCategorySidebar
          :exercises="exercisesStore.exercises"
          :selected-category="selectedCategory"
          @update:selected-category="selectedCategory = $event"
          @new-exercise="openCreateModal"
        />
      </div>

      <section class="portal-card min-h-[600px] overflow-hidden">
        <ExerciseTabs v-model:active-tab="activeTab" />
        <div class="p-5 sm:p-6">
          <ExerciseTemplatesTab v-if="activeTab === 'templates'" />
          <ExerciseGrid
            v-else
            :exercises="filteredExercises"
            :favourite-ids="exercisesStore.favourites"
            :loading="exercisesStore.loading"
            @toggle-favourite="exercisesStore.toggleFavourite"
            @edit="handleEdit"
            @customize="handleCustomize"
            @preview="handlePreview"
            @toggle-active="handleToggleActive"
          />
        </div>
      </section>

      <div class="hidden min-h-[600px] xl:block" :class="{ '!block': showFiltersMobile }">
        <ExerciseFilterPanel
          v-model:species="speciesFilter"
          v-model:body-region="bodyRegionFilter"
          v-model:difficulty="difficultyFilter"
          @clear="clearFilters"
        />
      </div>
    </div>

    <!-- Unified Customizer / Editor Modal -->
    <ExerciseEditorModal
      :open="showModal"
      :exercise="selectedExercise"
      :mode="modalMode"
      :initial-tab="modalTab"
      @close="showModal = false"
      @saved="exercisesStore.fetchExercises(undefined, undefined, true)"
    />
  </div>
</template>


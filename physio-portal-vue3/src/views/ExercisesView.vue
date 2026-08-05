<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { Plus, Search, SlidersHorizontal } from '@lucide/vue'
import CreateExerciseModal from '../components/exercises/CreateExerciseModal.vue'
import ExerciseCategorySidebar from '../components/exercises/ExerciseCategorySidebar.vue'
import ExerciseFilterPanel from '../components/exercises/ExerciseFilterPanel.vue'
import ExerciseGrid from '../components/exercises/ExerciseGrid.vue'
import ExerciseTabs from '../components/exercises/ExerciseTabs.vue'
import BaseButton from '../components/BaseButton.vue'
import { getCategoryLabel } from '../data/exerciseDemo'
import { useExercisesStore } from '../store/exercises'

const exercisesStore = useExercisesStore()

const search = ref('')
const selectedCategory = ref('All Categories')
const activeTab = ref<'all' | 'favourites' | 'region' | 'templates'>('all')
const speciesFilter = ref('All Species')
const bodyRegionFilter = ref('All Regions')
const difficultyFilter = ref('All Levels')
const showNewExerciseModal = ref(false)
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

function clearFilters() {
  speciesFilter.value = 'All Species'
  bodyRegionFilter.value = 'All Regions'
  difficultyFilter.value = 'All Levels'
  selectedCategory.value = 'All Categories'
  search.value = ''
  exercisesStore.fetchExercises(undefined, undefined, true).catch(() => undefined)
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center gap-3">
      <div class="relative min-w-[200px] flex-1">
        <Search class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          v-model="search"
          type="search"
          placeholder="Search exercises, equipment, body part..."
          class="w-full rounded-lg border border-neutral-grey bg-white py-2.5 pl-10 pr-4 text-sm outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
        />
      </div>
      <BaseButton size="sm" variant="accent" @click="showNewExerciseModal = true">
        <Plus class="h-4 w-4" />
        New Exercise
      </BaseButton>
      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-lg border border-neutral-grey bg-white px-4 py-2.5 text-sm font-semibold text-navy lg:hidden"
        @click="showFiltersMobile = !showFiltersMobile"
      >
        <SlidersHorizontal class="h-4 w-4" :stroke-width="1.75" />
        Filters
      </button>
    </div>

    <div class="grid gap-4 xl:grid-cols-[220px_minmax(0,1fr)_240px]">
      <div class="hidden min-h-[600px] xl:block">
        <ExerciseCategorySidebar
          :exercises="exercisesStore.exercises"
          :selected-category="selectedCategory"
          @update:selected-category="selectedCategory = $event"
          @new-exercise="showNewExerciseModal = true"
        />
      </div>

      <section class="portal-card min-h-[600px] overflow-hidden">
        <ExerciseTabs v-model:active-tab="activeTab" />
        <div class="p-4 sm:p-5">
          <div
            v-if="activeTab === 'templates'"
            class="empty-state py-16"
          >
            <p class="text-sm text-neutral-muted">Exercise templates coming soon.</p>
          </div>
          <ExerciseGrid
            v-else
            :exercises="filteredExercises"
            :favourite-ids="exercisesStore.favourites"
            :loading="exercisesStore.loading"
            @toggle-favourite="exercisesStore.toggleFavourite"
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

    <CreateExerciseModal
      :open="showNewExerciseModal"
      @close="showNewExerciseModal = false"
    />
  </div>
</template>

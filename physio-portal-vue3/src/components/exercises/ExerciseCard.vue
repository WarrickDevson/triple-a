<script setup lang="ts">
import { Star } from '@lucide/vue'
import { getCategoryLabel, getExerciseEquipment, getExerciseImage } from '../../data/exerciseDemo'
import type { Exercise } from '../../types/exercise'

defineProps<{
  exercise: Exercise
  isFavourite: boolean
}>()

const emit = defineEmits<{
  toggleFavourite: [exerciseId: number]
}>()
</script>

<template>
  <article class="portal-card overflow-hidden transition-shadow hover:shadow-md">
    <div class="relative aspect-[4/3] bg-sage-muted/40">
      <img
        v-if="getExerciseImage(exercise)"
        :src="getExerciseImage(exercise)!"
        :alt="exercise.title"
        class="h-full w-full object-cover"
      />
      <div
        v-else
        class="flex h-full items-center justify-center text-sm font-medium text-sage/60"
      >
        Exercise
      </div>
      <span
        class="absolute left-3 top-3 rounded-full bg-sage px-2.5 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-white"
      >
        {{ getCategoryLabel(exercise) }}
      </span>
      <button
        type="button"
        class="absolute right-3 top-3 flex h-8 w-8 items-center justify-center rounded-full bg-white/90 shadow-sm transition-colors hover:bg-white"
        :aria-label="isFavourite ? 'Remove from favourites' : 'Add to favourites'"
        @click="emit('toggleFavourite', exercise.exerciseId)"
      >
        <Star
          class="h-4 w-4"
          :class="isFavourite ? 'fill-accent-amber text-accent-amber' : 'text-neutral-muted'"
          :stroke-width="1.75"
        />
      </button>
    </div>
    <div class="p-4">
      <h3 class="font-semibold text-navy">{{ exercise.title }}</h3>
      <p class="mt-1 line-clamp-2 text-xs leading-relaxed text-neutral-muted">
        {{ exercise.shortDescription || exercise.clinicalPurpose || 'Rehabilitation exercise.' }}
      </p>
      <p class="mt-3 text-[11px] text-neutral-muted">
        Equipment: {{ getExerciseEquipment(exercise) }}
      </p>
    </div>
  </article>
</template>

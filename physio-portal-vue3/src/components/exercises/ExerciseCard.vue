<script setup lang="ts">
import { computed } from 'vue'
import { Activity, Edit3, Eye, FileVideo, Sparkles, Star } from '@lucide/vue'
import { getCategoryLabel } from '../../data/exerciseDemo'
import { useAuthStore } from '../../store/auth'
import type { Exercise } from '../../types/exercise'

const props = defineProps<{
  exercise: Exercise
  isFavourite: boolean
}>()

const emit = defineEmits<{
  toggleFavourite: [exerciseId: number]
  edit: [exercise: Exercise]
  customize: [exercise: Exercise]
  preview: [exercise: Exercise]
  toggleActive: [exerciseId: number]
}>()

const authStore = useAuthStore()
const isSysAdmin = computed(() => authStore.user?.userRole === 'SysAdmin')

const displayImage = computed(() => {
  if (props.exercise.coverImageUrl) return props.exercise.coverImageUrl
  const stepWithImg = props.exercise.steps.find((s) => s.imageUrl)
  return stepWithImg ? stepWithImg.imageUrl : null
})

const isCustomOverride = computed(() => Boolean(props.exercise.baseExerciseId))
const isStandaloneCustom = computed(() => !props.exercise.isSystemDefault && !props.exercise.baseExerciseId)
</script>

<template>
  <article class="portal-card overflow-hidden transition-all duration-200 hover:shadow-lg border border-neutral-grey/70 flex flex-col justify-between bg-white rounded-xl">
    <div>
      <!-- Card Header Strip: Category & Status Badges (Never Overlap) -->
      <div class="px-4 py-2.5 flex items-center justify-between gap-2 border-b border-slate-100 bg-slate-50/60">
        <span
          class="rounded-md bg-sage px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white shadow-2xs shrink-0"
        >
          {{ getCategoryLabel(exercise) }}
        </span>

        <div class="flex items-center gap-2 shrink-0">
          <span
            v-if="isSysAdmin"
            class="rounded-md bg-navy px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white shadow-2xs"
          >
            System Default
          </span>
          <span
            v-else-if="isCustomOverride"
            class="rounded-md bg-amber-500 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white shadow-2xs"
          >
            Custom Override
          </span>
          <span
            v-else-if="isStandaloneCustom"
            class="rounded-md bg-emerald-600 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white shadow-2xs"
          >
            Clinic Custom
          </span>
          <span
            v-else-if="exercise.hasCustomOverride"
            class="rounded-md bg-indigo-600 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white shadow-2xs"
          >
            Customized
          </span>

          <button
            type="button"
            class="flex h-6 w-6 items-center justify-center rounded-full text-slate-400 hover:text-amber-500 hover:bg-slate-200/60 transition-colors"
            :aria-label="isFavourite ? 'Remove from favourites' : 'Add to favourites'"
            @click="emit('toggleFavourite', exercise.exerciseId)"
          >
            <Star
              class="h-3.5 w-3.5"
              :class="isFavourite ? 'fill-accent-amber text-accent-amber' : 'text-slate-400'"
              :stroke-width="1.75"
            />
          </button>
        </div>
      </div>

      <!-- Thumbnail Area -->
      <div class="relative aspect-[16/9] bg-gradient-to-br from-slate-50 via-sage-muted/20 to-slate-100 overflow-hidden border-b border-slate-100">
        <img
          v-if="displayImage"
          :src="displayImage"
          :alt="exercise.title"
          class="h-full w-full object-cover transition-transform duration-300 hover:scale-105"
        />
        <div
          v-else
          class="flex h-full flex-col items-center justify-center text-slate-400 select-none p-4"
        >
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-white shadow-xs text-sage transition-transform duration-200 hover:scale-110 mb-1">
            <Activity class="h-5 w-5" />
          </div>
          <span class="text-[11px] font-medium text-slate-400">Exercise Demonstration</span>
        </div>

        <!-- Video Badge Overlay (Cleanly Bottom-Right) -->
        <div v-if="exercise.videoUrl" class="absolute bottom-2.5 right-2.5">
          <span
            class="flex h-6 items-center gap-1 rounded-md bg-black/75 px-2 text-[10px] font-semibold text-white shadow-xs backdrop-blur-xs"
            title="Video guide included"
          >
            <FileVideo class="h-3 w-3 text-sage-light" />
            <span>Video Guide</span>
          </span>
        </div>
      </div>

      <!-- Card Content Body -->
      <div class="p-4 sm:p-5">
        <h3 class="font-bold text-navy text-base leading-snug tracking-tight hover:text-sage transition-colors line-clamp-1" :title="exercise.title">
          {{ exercise.title }}
        </h3>
        <p class="mt-1.5 line-clamp-2 text-xs leading-relaxed text-neutral-muted min-h-[2.2rem]">
          {{ exercise.shortDescription || exercise.clinicalPurpose || 'Rehabilitation exercise guidelines and movement protocol.' }}
        </p>

        <!-- Clean Metadata Tags -->
        <div class="mt-3.5 flex flex-wrap items-center gap-2">
          <span class="inline-flex items-center rounded-md bg-slate-100 px-2.5 py-1 text-[11px] font-medium text-slate-600">
            {{ exercise.targetSpecies || 'All Species' }}
          </span>
          <span class="inline-flex items-center rounded-md bg-slate-100 px-2.5 py-1 text-[11px] font-medium text-slate-600">
            Level {{ exercise.difficultyLevel }}
          </span>
          <span v-if="exercise.steps?.length" class="inline-flex items-center rounded-md bg-sage-muted/30 px-2.5 py-1 text-[11px] font-semibold text-sage">
            {{ exercise.steps.length }} steps
          </span>
        </div>

        <!-- Physio: Active version selector -->
        <div
          v-if="!isSysAdmin && (exercise.hasCustomOverride || isCustomOverride)"
          class="mt-4 rounded-xl border border-amber-200 bg-amber-50/70 p-2.5 text-xs flex items-center justify-between gap-2"
        >
          <div>
            <p class="text-[9px] font-bold uppercase tracking-wider text-amber-900">Active for Owners:</p>
            <p class="text-[11px] font-bold text-navy">
              {{ (exercise.isCustomActive ?? exercise.isActiveForOwners) ? 'Your Custom Version' : 'Admin Default' }}
            </p>
          </div>
          <button
            type="button"
            class="inline-flex items-center shrink-0 rounded-lg px-2.5 py-1.5 text-[11px] font-bold shadow-xs transition-colors"
            :class="
              (exercise.isCustomActive ?? exercise.isActiveForOwners)
                ? 'bg-amber-500 text-white hover:bg-amber-600'
                : 'bg-white border border-neutral-grey text-navy hover:bg-slate-50'
            "
            @click="emit('toggleActive', exercise.customExerciseId || exercise.exerciseId)"
          >
            Switch to {{ (exercise.isCustomActive ?? exercise.isActiveForOwners) ? 'Default' : 'Custom' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Card Action Footer -->
    <div class="border-t border-slate-100 bg-slate-50/60 px-4 py-3 flex items-center justify-between gap-2">
      <button
        type="button"
        class="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500 hover:text-navy transition-colors py-1.5 px-2 rounded-md hover:bg-slate-100 shrink-0"
        @click="emit('preview', exercise)"
      >
        <Eye class="h-3.5 w-3.5" />
        <span>Preview</span>
      </button>

      <div class="flex items-center gap-2 shrink-0">
        <!-- If SysAdmin: Edit default -->
        <button
          v-if="isSysAdmin"
          type="button"
          class="inline-flex items-center gap-1.5 rounded-lg bg-navy px-3 py-1.5 text-xs font-bold text-white transition-all shadow-xs hover:bg-navy-light whitespace-nowrap shrink-0"
          @click="emit('edit', exercise)"
        >
          <Edit3 class="h-3 w-3" />
          <span>Edit Default</span>
        </button>

        <!-- If Physio and this is clinic custom: Edit -->
        <button
          v-else-if="isCustomOverride || isStandaloneCustom"
          type="button"
          class="inline-flex items-center gap-1.5 rounded-lg bg-sage px-3 py-1.5 text-xs font-bold text-white transition-all shadow-xs hover:bg-sage/90 whitespace-nowrap shrink-0"
          @click="emit('edit', exercise)"
        >
          <Edit3 class="h-3 w-3" />
          <span>Edit Custom</span>
        </button>

        <!-- If Physio and this is default: Customize -->
        <button
          v-else
          type="button"
          class="inline-flex items-center gap-1.5 rounded-lg bg-sage px-3 py-1.5 text-xs font-bold text-white transition-all shadow-xs hover:bg-sage/90 whitespace-nowrap shrink-0"
          @click="emit('customize', exercise)"
        >
          <Sparkles class="h-3 w-3" />
          <span>{{ exercise.hasCustomOverride ? 'Edit Override' : 'Customize' }}</span>
        </button>
      </div>
    </div>
  </article>
</template>


<script setup lang="ts">
import { computed } from 'vue'
import { getNextReviewDate } from '../../data/planDemo'
import type { RehabProgram } from '../../types/exercise'

const props = defineProps<{
  program: RehabProgram | null
}>()

const emit = defineEmits<{
  addNote: []
}>()

const createdLabel = computed(() =>
  props.program
    ? new Date(props.program.startDate).toLocaleDateString([], {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      })
    : '—',
)

const nextReview = computed(() =>
  props.program ? getNextReviewDate(props.program.startDate) : '—',
)
</script>

<template>
  <div class="space-y-4">
    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Plan Details</h3>
      <dl class="mt-3 space-y-2 text-sm">
        <div class="flex justify-between gap-2">
          <dt class="text-neutral-muted">Created</dt>
          <dd class="font-medium text-navy">{{ createdLabel }}</dd>
        </div>
        <div class="flex justify-between gap-2">
          <dt class="text-neutral-muted">Last Updated</dt>
          <dd class="font-medium text-navy">{{ createdLabel }}</dd>
        </div>
        <div class="flex justify-between gap-2">
          <dt class="text-neutral-muted">Next Review</dt>
          <dd class="font-medium text-navy">{{ nextReview }}</dd>
        </div>
      </dl>
    </section>

    <section class="portal-card p-4">
      <h3 class="text-sm font-bold text-navy">Patient Notes</h3>
      <p v-if="program?.notes" class="mt-3 text-sm leading-relaxed text-neutral-muted">
        {{ program.notes }}
      </p>
      <p v-else class="mt-3 text-sm text-neutral-muted">
        No notes added yet. Add instructions for the owner here.
      </p>
      <button
        type="button"
        class="mt-4 text-sm font-semibold text-sage hover:text-navy"
        @click="emit('addNote')"
      >
        Add Note
      </button>
    </section>
  </div>
</template>

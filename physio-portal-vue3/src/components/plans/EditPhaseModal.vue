<script setup lang="ts">
import { ref, watch } from 'vue'
import { Save, X } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { PlanPhase } from '../../data/planDemo'

const props = defineProps<{
  open: boolean
  phase: PlanPhase | null
}>()

const emit = defineEmits<{
  close: []
  save: [phaseId: number, data: { title: string; goals: string[] }]
}>()

const title = ref('')
const goalsText = ref('')

watch(
  () => props.phase,
  (p) => {
    if (p) {
      title.value = p.title
      goalsText.value = p.goals.join('\n')
    }
  },
  { immediate: true },
)

function handleSubmit() {
  if (!props.phase || !title.value.trim()) return
  const goalsList = goalsText.value
    .split('\n')
    .map((g) => g.trim())
    .filter(Boolean)

  emit('save', props.phase.id, {
    title: title.value.trim(),
    goals: goalsList,
  })
  emit('close')
}
</script>

<template>
  <div
    v-if="open && phase"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card w-full max-w-md p-6 shadow-xl animate-in fade-in zoom-in-95">
      <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-3">
        <h3 class="text-base font-bold text-navy">Edit {{ phase.label }}</h3>
        <button type="button" class="text-neutral-muted hover:text-navy" @click="emit('close')">
          <X class="h-5 w-5" />
        </button>
      </div>

      <form class="mt-4 space-y-4" @submit.prevent="handleSubmit">
        <label class="block">
          <span class="text-xs font-semibold uppercase tracking-wider text-navy">Phase Title</span>
          <input
            v-model="title"
            required
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none"
          />
        </label>

        <label class="block">
          <span class="text-xs font-semibold uppercase tracking-wider text-navy">Phase Goals (One per line)</span>
          <textarea
            v-model="goalsText"
            rows="5"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none"
          ></textarea>
        </label>

        <div class="flex gap-3 pt-2">
          <BaseButton type="button" variant="secondary" class="flex-1" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton type="submit" class="flex-1">
            <Save class="h-4 w-4" />
            Save Changes
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Plus, X } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  close: []
  add: [phase: { title: string; goals: string[] }]
}>()

const title = ref('')
const goalsText = ref('')

function handleSubmit() {
  if (!title.value.trim()) return
  const goalsList = goalsText.value
    .split('\n')
    .map((g) => g.trim())
    .filter(Boolean)

  emit('add', {
    title: title.value.trim(),
    goals: goalsList.length > 0 ? goalsList : ['Maintain steady rehabilitation progression.'],
  })
  title.value = ''
  goalsText.value = ''
  emit('close')
}
</script>

<template>
  <div
    v-if="open"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card w-full max-w-md p-6 shadow-xl animate-in fade-in zoom-in-95">
      <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-3">
        <h3 class="text-base font-bold text-navy">Add Rehabilitation Phase</h3>
        <button type="button" class="text-neutral-muted hover:text-navy" @click="emit('close')">
          <X class="h-5 w-5" />
        </button>
      </div>

      <form class="mt-4 space-y-4" @submit.prevent="handleSubmit">
        <label class="block">
          <span class="text-xs font-semibold uppercase tracking-wider text-navy">Phase Name / Focus</span>
          <input
            v-model="title"
            required
            placeholder="e.g. Advanced Strength & Agility"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none"
          />
        </label>

        <label class="block">
          <span class="text-xs font-semibold uppercase tracking-wider text-navy">Phase Goals (One per line)</span>
          <textarea
            v-model="goalsText"
            rows="4"
            placeholder="e.g. Trotting on uneven surfaces&#10;Full limb extension during gait&#10;Core stability on peanut balance roll"
            class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none"
          ></textarea>
        </label>

        <div class="flex gap-3 pt-2">
          <BaseButton type="button" variant="secondary" class="flex-1" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton type="submit" class="flex-1">
            <Plus class="h-4 w-4" />
            Add Phase
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

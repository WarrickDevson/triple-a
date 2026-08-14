<script setup lang="ts">
import { ref, watch } from 'vue'
import { FileText, Save, Check, Edit2 } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { RehabProgram } from '../../types/exercise'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patient: Pet
  program: RehabProgram | null
}>()

const emit = defineEmits<{
  saveNotes: [notes: string]
}>()

const isEditing = ref(false)
const notesText = ref(props.program?.notes || '')
const savedSuccess = ref(false)

watch(
  () => props.program?.notes,
  (val) => {
    notesText.value = val || ''
  },
  { immediate: true },
)

function handleSave() {
  emit('saveNotes', notesText.value)
  isEditing.value = false
  savedSuccess.value = true
  setTimeout(() => {
    savedSuccess.value = false
  }, 3000)
}
</script>

<template>
  <div class="p-5 space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-base font-bold text-navy">Plan Notes & Instructions</h3>
        <p class="text-xs text-neutral-muted">
          Clinical guidance, precautions, and owner home instructions for {{ patient.petName }}
        </p>
      </div>

      <div class="flex items-center gap-2">
        <span v-if="savedSuccess" class="flex items-center gap-1 text-xs font-semibold text-emerald-700 bg-emerald-50 px-2.5 py-1 rounded-lg border border-emerald-200">
          <Check class="h-4 w-4" />
          Notes Saved
        </span>
        <BaseButton size="sm" :variant="isEditing ? 'accent' : 'secondary'" @click="isEditing ? handleSave() : (isEditing = true)">
          <Save v-if="isEditing" class="h-4 w-4" />
          <Edit2 v-else class="h-4 w-4" />
          {{ isEditing ? 'Save Notes' : 'Edit Notes' }}
        </BaseButton>
      </div>
    </div>

    <!-- Active View / Edit Box -->
    <div class="portal-card p-6 border border-neutral-grey/60 shadow-sm">
      <div v-if="isEditing" class="space-y-4">
        <label class="block">
          <span class="text-xs font-semibold uppercase tracking-wider text-navy">Edit Rehabilitation Instructions</span>
          <textarea
            v-model="notesText"
            rows="8"
            class="mt-2 w-full rounded-xl border border-neutral-grey p-4 text-sm leading-relaxed focus:border-sage focus:outline-none focus:ring-2 focus:ring-sage/20"
            placeholder="Enter home exercise guidelines, hydrotherapy recommendations, icing protocols, precautions, or special owner instructions..."
          ></textarea>
        </label>

        <div class="flex justify-end gap-3">
          <BaseButton variant="secondary" size="sm" @click="isEditing = false">
            Cancel
          </BaseButton>
          <BaseButton size="sm" @click="handleSave">
            <Save class="h-4 w-4" />
            Save Notes
          </BaseButton>
        </div>
      </div>

      <div v-else>
        <div v-if="program?.notes" class="prose prose-sm max-w-none text-navy leading-relaxed whitespace-pre-line">
          {{ program.notes }}
        </div>
        <div v-else class="py-8 text-center text-sm text-neutral-muted">
          <FileText class="mx-auto h-8 w-8 text-neutral-muted/60 mb-2" />
          <p>No notes added to this treatment plan yet.</p>
          <button
            type="button"
            class="mt-2 text-xs font-semibold text-sage hover:underline"
            @click="isEditing = true"
          >
            Click here to add clinical instructions
          </button>
        </div>
      </div>
    </div>

    <!-- Helpful Precautions Template Cards -->
    <div class="grid gap-4 sm:grid-cols-2">
      <div class="portal-card p-4 bg-amber-50/40 border border-amber-200">
        <h4 class="text-xs font-bold uppercase tracking-wider text-amber-900 flex items-center gap-1.5">
          ⚠️ Key Safety Precautions
        </h4>
        <ul class="mt-2 space-y-1.5 text-xs text-amber-900/90 list-disc list-inside">
          <li>Restrict running, jumping, and stairs between exercises</li>
          <li>Stop session immediately if pet exhibits pain score > 4/10</li>
          <li>Apply cold compress for 10-15 minutes after active sessions</li>
        </ul>
      </div>

      <div class="portal-card p-4 bg-sage-muted/30 border border-sage/30">
        <h4 class="text-xs font-bold uppercase tracking-wider text-sage flex items-center gap-1.5">
          📋 Owner Home Routine Tips
        </h4>
        <ul class="mt-2 space-y-1.5 text-xs text-navy/80 list-disc list-inside">
          <li>Perform exercises on non-slip rubber mats or carpeted floors</li>
          <li>Reward steady compliance with high-value low-calorie treats</li>
          <li>Log daily session completion in the MoveWell owner app</li>
        </ul>
      </div>
    </div>
  </div>
</template>

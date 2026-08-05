<script setup lang="ts">
import { ref, watch } from 'vue'
import { Plus, Trash2, X } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { CreateExerciseRequest, CreateExerciseStepRequest } from '../../types/exercise'
import { useExercisesStore } from '../../store/exercises'

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  close: []
}>()

const exercisesStore = useExercisesStore()

const title = ref('')
const targetSpecies = ref('Canine')
const conditionCategory = ref('Range of Motion')
const difficultyLevel = ref(1)
const targetedMuscles = ref('')
const clinicalPurpose = ref('')
const shortDescription = ref('')
const safetyNotes = ref('')
const commonMistakes = ref('')
const videoUrl = ref('')

interface LocalStep {
  stepNumber: number
  stepInstruction: string
  imageUrl: string
}

const steps = ref<LocalStep[]>([
  { stepNumber: 1, stepInstruction: '', imageUrl: '' },
])

const isSubmitting = ref(false)
const errorMessage = ref('')

watch(
  () => props.open,
  (val) => {
    if (val) {
      resetForm()
    }
  },
)

function resetForm() {
  title.value = ''
  targetSpecies.value = 'Canine'
  conditionCategory.value = 'Range of Motion'
  difficultyLevel.value = 1
  targetedMuscles.value = ''
  clinicalPurpose.value = ''
  shortDescription.value = ''
  safetyNotes.value = ''
  commonMistakes.value = ''
  videoUrl.value = ''
  steps.value = [{ stepNumber: 1, stepInstruction: '', imageUrl: '' }]
  errorMessage.value = ''
  isSubmitting.value = false
}

function addStep() {
  steps.value.push({
    stepNumber: steps.value.length + 1,
    stepInstruction: '',
    imageUrl: '',
  })
}

function removeStep(index: number) {
  if (steps.value.length === 1) return
  steps.value.splice(index, 1)
  steps.value.forEach((step, idx) => {
    step.stepNumber = idx + 1
  })
}

async function submitExercise() {
  if (!title.value.trim()) {
    errorMessage.value = 'Please enter an exercise title.'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  const filteredSteps: CreateExerciseStepRequest[] = steps.value
    .filter((s) => s.stepInstruction.trim().length > 0)
    .map((s, idx) => ({
      stepNumber: idx + 1,
      stepInstruction: s.stepInstruction.trim(),
      imageUrl: s.imageUrl.trim() || undefined,
    }))

  const request: CreateExerciseRequest = {
    title: title.value.trim(),
    targetSpecies: targetSpecies.value,
    conditionCategory: conditionCategory.value,
    difficultyLevel: difficultyLevel.value,
    targetedMuscles: targetedMuscles.value.trim() || undefined,
    clinicalPurpose: clinicalPurpose.value.trim() || undefined,
    shortDescription: shortDescription.value.trim() || undefined,
    safetyNotes: safetyNotes.value.trim() || undefined,
    commonMistakes: commonMistakes.value.trim() || undefined,
    videoUrl: videoUrl.value.trim() || undefined,
    steps: filteredSteps.length > 0 ? filteredSteps : undefined,
  }

  try {
    await exercisesStore.addExercise(request)
    emit('close')
  } catch (err: any) {
    errorMessage.value = err.message || 'Unable to save exercise.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div
    v-if="open"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div
      class="portal-card flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden shadow-2xl animate-in fade-in zoom-in-95"
    >
      <!-- Modal Header -->
      <div class="flex items-center justify-between border-b border-neutral-grey/80 px-6 py-4">
        <div>
          <h2 class="text-lg font-bold text-navy">Add Custom Exercise</h2>
          <p class="text-xs text-neutral-muted">
            Create an exercise entry with clinical guidelines, targeted muscles, and step instructions.
          </p>
        </div>
        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted hover:bg-surface hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Modal Body (Scrollable Form) -->
      <form class="flex-1 overflow-y-auto p-6 space-y-5" @submit.prevent="submitExercise">
        <!-- 1. Primary Information -->
        <div class="space-y-3">
          <h3 class="text-xs font-extrabold uppercase tracking-wider text-sage border-b border-neutral-grey/60 pb-1">
            1. Exercise Overview
          </h3>

          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Exercise Title *</label>
            <input
              v-model="title"
              type="text"
              required
              placeholder="e.g. Passive Cavaletti Pole Walking"
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
            />
          </div>

          <div class="grid grid-cols-3 gap-3">
            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Target Species</label>
              <select
                v-model="targetSpecies"
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
              >
                <option value="Canine">Canine (Dog)</option>
                <option value="Feline">Feline (Cat)</option>
                <option value="Equine">Equine (Horse)</option>
                <option value="All">All Species</option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Category / Region</label>
              <select
                v-model="conditionCategory"
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
              >
                <option value="Range of Motion">Range of Motion</option>
                <option value="Strength">Strength</option>
                <option value="Proprioception & Balance">Proprioception & Balance</option>
                <option value="Hydrotherapy">Hydrotherapy</option>
                <option value="Gait & Conditioning">Gait & Conditioning</option>
                <option value="Post-Op Rehab">Post-Op Rehab</option>
                <option value="General">General</option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Difficulty Level</label>
              <div class="flex items-center gap-1 mt-1.5">
                <button
                  v-for="lvl in 5"
                  :key="lvl"
                  type="button"
                  class="flex h-7 w-7 items-center justify-center rounded-lg text-xs font-bold transition-colors"
                  :class="
                    difficultyLevel >= lvl
                      ? 'bg-amber-400 text-amber-950'
                      : 'bg-neutral-grey/60 text-neutral-muted'
                  "
                  @click="difficultyLevel = lvl"
                >
                  {{ lvl }}
                </button>
              </div>
            </div>
          </div>

          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Short Summary</label>
            <textarea
              v-model="shortDescription"
              rows="2"
              placeholder="Brief summary of what this exercise involves..."
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
            ></textarea>
          </div>
        </div>

        <!-- 2. Clinical Details -->
        <div class="space-y-3">
          <h3 class="text-xs font-extrabold uppercase tracking-wider text-sage border-b border-neutral-grey/60 pb-1">
            2. Clinical Guidelines & Precautions
          </h3>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Targeted Muscles / Joints</label>
              <input
                v-model="targetedMuscles"
                type="text"
                placeholder="e.g. Quadriceps, Hamstrings, Stifle ROM"
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
              />
            </div>
            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Video Demo URL (Optional)</label>
              <input
                v-model="videoUrl"
                type="url"
                placeholder="https://youtube.com/watch?v=..."
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
              />
            </div>
          </div>

          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Clinical Purpose & Indications</label>
            <textarea
              v-model="clinicalPurpose"
              rows="2"
              placeholder="Why is this exercise prescribed? e.g., Improves tarsal flexion and stride extension."
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
            ></textarea>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Safety Notes & Contraindications</label>
              <textarea
                v-model="safetyNotes"
                rows="2"
                placeholder="Contraindications, pain thresholds..."
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
              ></textarea>
            </div>
            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Common Compensations / Mistakes</label>
              <textarea
                v-model="commonMistakes"
                rows="2"
                placeholder="e.g., Owner lifting too high, pet swinging leg outward..."
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
              ></textarea>
            </div>
          </div>
        </div>

        <!-- 3. Step-by-Step Instructions -->
        <div class="space-y-3">
          <div class="flex items-center justify-between border-b border-neutral-grey/60 pb-1">
            <h3 class="text-xs font-extrabold uppercase tracking-wider text-sage">
              3. Step-by-Step Patient Instructions
            </h3>
            <button
              type="button"
              class="inline-flex items-center gap-1 text-xs font-bold text-sage hover:underline"
              @click="addStep"
            >
              <Plus class="h-3.5 w-3.5" />
              Add Step
            </button>
          </div>

          <div
            v-for="(step, index) in steps"
            :key="index"
            class="rounded-xl border border-neutral-grey/80 bg-surface/60 p-3 space-y-2"
          >
            <div class="flex items-center justify-between">
              <span class="text-xs font-bold text-navy">Step {{ index + 1 }}</span>
              <button
                v-if="steps.length > 1"
                type="button"
                class="rounded p-1 text-neutral-muted hover:bg-red-50 hover:text-red-600"
                @click="removeStep(index)"
              >
                <Trash2 class="h-3.5 w-3.5" />
              </button>
            </div>

            <textarea
              v-model="step.stepInstruction"
              rows="2"
              placeholder="Describe step instruction clearly for owner..."
              class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-1.5 text-xs outline-none focus:border-sage"
            ></textarea>

            <input
              v-model="step.imageUrl"
              type="text"
              placeholder="Step Image URL (Optional)"
              class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-1 text-xs outline-none focus:border-sage"
            />
          </div>
        </div>

        <div v-if="errorMessage" class="rounded-lg bg-red-50 p-2.5 text-xs font-medium text-red-700">
          {{ errorMessage }}
        </div>
      </form>

      <!-- Modal Footer -->
      <div class="flex items-center justify-end gap-2 border-t border-neutral-grey/80 px-6 py-4 bg-white">
        <BaseButton type="button" variant="secondary" size="sm" @click="emit('close')">
          Cancel
        </BaseButton>
        <BaseButton
          type="button"
          variant="accent"
          size="sm"
          :disabled="isSubmitting"
          @click="submitExercise"
        >
          {{ isSubmitting ? 'Saving...' : 'Save & Create Exercise' }}
        </BaseButton>
      </div>
    </div>
  </div>
</template>

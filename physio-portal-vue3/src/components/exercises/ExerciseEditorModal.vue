<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import {
  AlertTriangle,
  Eye,
  Film,
  FileVideo,
  Image as ImageIcon,
  Loader2,
  Maximize2,
  Plus,
  Sparkles,
  Trash2,
  UploadCloud,
  X,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { useAuthStore } from '../../store/auth'
import { useExercisesStore } from '../../store/exercises'
import { useSpeciesBreedStore } from '../../store/speciesBreed'
import type { CreateExerciseRequest, CreateExerciseStepRequest, Exercise, ExerciseVideoVariation } from '../../types/exercise'

const props = withDefaults(
  defineProps<{
    open: boolean
    exercise?: Exercise | null
    mode?: 'create' | 'edit' | 'customize'
    initialTab?: 'editor' | 'preview'
  }>(),
  {
    initialTab: 'editor',
  },
)

const emit = defineEmits<{
  close: []
  saved: [exercise: Exercise]
}>()

const authStore = useAuthStore()
const exercisesStore = useExercisesStore()
const speciesStore = useSpeciesBreedStore()

onMounted(() => {
  speciesStore.loadConfig()
})

const isSysAdmin = computed(() => authStore.user?.userRole === 'SysAdmin')

// Form fields
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
const coverImageUrl = ref('')
const videoVariations = ref<ExerciseVideoVariation[]>([])

const previewVariationIndex = ref<number>(-1)
const uploadingVariationIndex = ref<number | null>(null)

interface LocalStep {
  stepNumber: number
  stepInstruction: string
  imageUrl: string
}

const steps = ref<LocalStep[]>([
  { stepNumber: 1, stepInstruction: '', imageUrl: '' },
])

const activeTab = ref<'editor' | 'preview'>('editor')
const isSubmitting = ref(false)
const errorMessage = ref('')
const uploadingVideo = ref(false)
const uploadingCover = ref(false)
const uploadingStepIndex = ref<number | null>(null)

function getYouTubeEmbedUrl(url: string | undefined | null): string | null {
  if (!url) return null
  const trimmed = url.trim()
  const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|&v=|shorts\/)([^#&?]*).*/
  const match = trimmed.match(regExp)
  return match && match[2].length === 11
    ? `https://www.youtube-nocookie.com/embed/${match[2]}?rel=0&modestbranding=1`
    : null
}

const youtubeEmbedUrl = computed(() => getYouTubeEmbedUrl(videoUrl.value))
const isDirectVideoUrl = computed(() => {
  if (!videoUrl.value) return false
  const lower = videoUrl.value.toLowerCase()
  return (
    lower.endsWith('.mp4') ||
    lower.endsWith('.webm') ||
    lower.endsWith('.mov') ||
    lower.includes('/uploads/') ||
    lower.includes('storage.googleapis.com') ||
    lower.includes('.mp4?')
  )
})

function addVideoVariation() {
  const currentSpecies = targetSpecies.value && targetSpecies.value !== 'All'
    ? targetSpecies.value
    : (speciesStore.speciesList[0]?.name || 'Canine')
  const availableBreeds = speciesStore.breedsForSpecies(currentSpecies)
  const defaultBreed = availableBreeds[0]?.name || ''

  videoVariations.value.push({
    species: currentSpecies,
    breedCategory: defaultBreed,
    videoUrl: '',
    title: defaultBreed ? `${currentSpecies} - ${defaultBreed}` : '',
    notes: '',
  })
}

function removeVideoVariation(index: number) {
  videoVariations.value.splice(index, 1)
  if (previewVariationIndex.value >= videoVariations.value.length) {
    previewVariationIndex.value = -1
  }
}

async function handleVariationVideoUpload(index: number, e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return

  const file = input.files[0]
  uploadingVariationIndex.value = index
  errorMessage.value = ''
  try {
    const res = await exercisesStore.uploadMedia(file)
    videoVariations.value[index].videoUrl = res.url
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to upload variation video.'
  } finally {
    uploadingVariationIndex.value = null
    input.value = ''
  }
}

const activePreviewVariation = computed<ExerciseVideoVariation | null>(() => {
  if (previewVariationIndex.value >= 0 && previewVariationIndex.value < videoVariations.value.length) {
    return videoVariations.value[previewVariationIndex.value]
  }
  return null
})

const currentPreviewVideoUrl = computed(() => {
  if (activePreviewVariation.value && activePreviewVariation.value.videoUrl) {
    return activePreviewVariation.value.videoUrl
  }
  return videoUrl.value
})

const currentPreviewYoutubeEmbed = computed(() => getYouTubeEmbedUrl(currentPreviewVideoUrl.value))
const isCurrentPreviewDirectVideo = computed(() => {
  if (!currentPreviewVideoUrl.value) return false
  const lower = currentPreviewVideoUrl.value.toLowerCase()
  return (
    lower.endsWith('.mp4') ||
    lower.endsWith('.webm') ||
    lower.endsWith('.mov') ||
    lower.includes('/uploads/') ||
    lower.includes('storage.googleapis.com') ||
    lower.includes('.mp4?')
  )
})

watch(
  () => [props.open, props.exercise, props.mode],
  () => {
    if (props.open) {
      initForm()
    }
  },
  { immediate: true },
)

function initForm() {
  speciesStore.loadConfig(true).catch(() => undefined)
  errorMessage.value = ''
  isSubmitting.value = false
  activeTab.value = props.initialTab || 'editor'
  previewVariationIndex.value = -1

  if (props.exercise && (props.mode === 'edit' || props.mode === 'customize')) {
    title.value = props.exercise.title
    targetSpecies.value = props.exercise.targetSpecies || 'Canine'
    conditionCategory.value = props.exercise.conditionCategory || 'Range of Motion'
    difficultyLevel.value = props.exercise.difficultyLevel || 1
    targetedMuscles.value = props.exercise.targetedMuscles || ''
    clinicalPurpose.value = props.exercise.clinicalPurpose || ''
    shortDescription.value = props.exercise.shortDescription || ''
    safetyNotes.value = props.exercise.safetyNotes || ''
    commonMistakes.value = props.exercise.commonMistakes || ''
    videoUrl.value = props.exercise.videoUrl || ''
    coverImageUrl.value = props.exercise.coverImageUrl || ''

    if (props.exercise.videoVariations && props.exercise.videoVariations.length > 0) {
      videoVariations.value = props.exercise.videoVariations.map((v) => ({ ...v }))
    } else {
      videoVariations.value = []
    }

    if (props.exercise.steps && props.exercise.steps.length > 0) {
      steps.value = props.exercise.steps.map((s) => ({
        stepNumber: s.stepNumber,
        stepInstruction: s.stepInstruction,
        imageUrl: s.imageUrl || '',
      }))
    } else {
      steps.value = [{ stepNumber: 1, stepInstruction: '', imageUrl: '' }]
    }
  } else {
    // Reset to blank for new
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
    coverImageUrl.value = ''
    videoVariations.value = []
    steps.value = [{ stepNumber: 1, stepInstruction: '', imageUrl: '' }]
  }
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

async function handleVideoFileUpload(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return

  const file = input.files[0]
  uploadingVideo.value = true
  errorMessage.value = ''
  try {
    const res = await exercisesStore.uploadMedia(file)
    videoUrl.value = res.url
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to upload video file.'
  } finally {
    uploadingVideo.value = false
    input.value = ''
  }
}

async function handleCoverFileUpload(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return

  const file = input.files[0]
  uploadingCover.value = true
  errorMessage.value = ''
  try {
    const res = await exercisesStore.uploadMedia(file)
    coverImageUrl.value = res.url
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to upload cover image.'
  } finally {
    uploadingCover.value = false
    input.value = ''
  }
}

async function handleStepImageUpload(index: number, e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return

  const file = input.files[0]
  uploadingStepIndex.value = index
  errorMessage.value = ''
  try {
    const res = await exercisesStore.uploadMedia(file)
    steps.value[index].imageUrl = res.url
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to upload step image.'
  } finally {
    uploadingStepIndex.value = null
    input.value = ''
  }
}

async function saveExercise() {
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

  const payload: CreateExerciseRequest = {
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
    coverImageUrl: coverImageUrl.value.trim() || undefined,
    steps: filteredSteps.length > 0 ? filteredSteps : undefined,
    videoVariations: videoVariations.value
      .filter((v) => v.videoUrl.trim().length > 0)
      .map((v) => ({
        species: v.species?.trim() || null,
        breedCategory: v.breedCategory?.trim() || null,
        videoUrl: v.videoUrl.trim(),
        title: v.title?.trim() || null,
        notes: v.notes?.trim() || null,
      })),
  }

  try {
    let saved: Exercise
    if (props.mode === 'customize' && props.exercise) {
      // 1. Clone via customize endpoint
      const cloned = await exercisesStore.customizeExercise(props.exercise.exerciseId)
      // 2. Update with user changes
      saved = await exercisesStore.editExercise(cloned.exerciseId, payload)
    } else if (props.mode === 'edit' && props.exercise) {
      saved = await exercisesStore.editExercise(props.exercise.exerciseId, payload)
    } else {
      saved = await exercisesStore.addExercise(payload)
    }

    emit('saved', saved)
    emit('close')
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || err.message || 'Unable to save exercise.'
  } finally {
    isSubmitting.value = false
  }
}

const modalTitle = computed(() => {
  if (props.mode === 'customize') return 'Customize Default Exercise'
  if (props.mode === 'edit') {
    return isSysAdmin.value ? 'Edit System Default Exercise' : 'Edit Clinic Exercise'
  }
  return isSysAdmin.value ? 'Add System Default Exercise' : 'Add Custom Clinic Exercise'
})

const modalSubtitle = computed(() => {
  if (props.mode === 'customize') {
    return 'Create your own clinic version. The admin default remains intact and you can choose which one stays active for your owners.'
  }
  if (isSysAdmin.value) {
    return 'Changes made here will serve as the global default for all physiotherapists and owners across the platform.'
  }
  return 'Create a personalized exercise for your clinic. It will only be visible to you and your pet owners.'
})
</script>

<template>
  <div
    v-if="open"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/70 p-3 sm:p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div
      class="portal-card flex max-h-[92vh] w-full max-w-4xl flex-col overflow-hidden shadow-2xl animate-in fade-in zoom-in-95"
    >
      <!-- Modal Header -->
      <div class="flex flex-wrap sm:flex-nowrap items-center justify-between gap-3 border-b border-neutral-grey/80 bg-surface px-4 sm:px-6 py-3.5 sm:py-4">
        <div>
          <div class="flex flex-wrap items-center gap-2">
            <h2 class="text-base sm:text-lg font-bold text-navy">{{ modalTitle }}</h2>
            <span
              v-if="isSysAdmin"
              class="rounded-full bg-navy px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white"
            >
              Admin Default
            </span>
            <span
              v-else-if="props.mode === 'customize'"
              class="rounded-full bg-accent-amber/20 px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-amber-900"
            >
              Clinic Override
            </span>
          </div>
          <p class="text-xs text-neutral-muted mt-0.5 max-w-2xl">
            {{ modalSubtitle }}
          </p>
        </div>

        <div class="flex items-center gap-2 shrink-0">
          <!-- Switch between Editor and Owner Mobile Preview -->
          <div class="flex items-center rounded-lg border border-neutral-grey/70 bg-white p-0.5 text-xs font-semibold">
            <button
              type="button"
              class="rounded-md px-3 py-1.5 transition-colors"
              :class="activeTab === 'editor' ? 'bg-sage text-white shadow-sm' : 'text-navy hover:text-sage'"
              @click="activeTab = 'editor'"
            >
              Editor
            </button>
            <button
              type="button"
              class="inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 transition-colors"
              :class="activeTab === 'preview' ? 'bg-sage text-white shadow-sm' : 'text-navy hover:text-sage'"
              @click="activeTab = 'preview'"
            >
              <Eye class="h-3.5 w-3.5" />
              <span class="hidden xs:inline sm:inline">Owner Preview</span>
              <span class="xs:hidden sm:hidden">Preview</span>
            </button>
          </div>

          <button
            type="button"
            class="rounded-lg p-2 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
            @click="emit('close')"
          >
            <X class="h-5 w-5" />
          </button>
        </div>
      </div>

      <!-- Modal Body -->
      <div class="flex-1 overflow-y-auto p-4 sm:p-6 bg-slate-50/50">
        <!-- TAB 1: FORM EDITOR -->
        <form v-if="activeTab === 'editor'" class="space-y-6" @submit.prevent="saveExercise">
          <!-- 1. Overview & Categorization -->
          <div class="rounded-xl border border-neutral-grey/70 bg-white p-5 shadow-xs space-y-4">
            <h3 class="text-xs font-bold uppercase tracking-wider text-sage border-b border-neutral-grey/50 pb-2">
              1. Exercise Overview & Classification
            </h3>

            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Exercise Title *</label>
              <input
                v-model="title"
                type="text"
                required
                placeholder="e.g. Passive Cavaletti Pole Walking"
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3.5 py-2 text-sm text-navy outline-none focus:border-sage focus:ring-2 focus:ring-sage/15"
              />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Target Species</label>
                <select
                  v-model="targetSpecies"
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
                >
                  <option v-for="s in speciesStore.speciesOptions" :key="s.value" :value="s.value">
                    {{ s.label }}
                  </option>
                  <option value="All">All Species</option>
                </select>
              </div>

              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Category / Area</label>
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
                  <option value="Pain Management">Pain Management</option>
                  <option value="General">General</option>
                </select>
              </div>

              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Difficulty Level (1-5)</label>
                <div class="flex items-center gap-1.5 mt-1">
                  <button
                    v-for="lvl in 5"
                    :key="lvl"
                    type="button"
                    class="flex h-8 w-8 items-center justify-center rounded-lg text-xs font-bold transition-all"
                    :class="
                      difficultyLevel >= lvl
                        ? 'bg-amber-400 text-amber-950 shadow-xs'
                        : 'bg-neutral-grey/50 text-neutral-muted hover:bg-neutral-grey/80'
                    "
                    @click="difficultyLevel = lvl"
                  >
                    {{ lvl }}
                  </button>
                </div>
              </div>
            </div>

            <div>
              <label class="block text-xs font-semibold text-navy mb-1">Short Description / Patient Summary</label>
              <textarea
                v-model="shortDescription"
                rows="2"
                placeholder="Clear summary explaining what this exercise achieves for the animal..."
                class="w-full rounded-lg border border-neutral-grey bg-surface px-3.5 py-2 text-sm text-navy outline-none focus:border-sage"
              ></textarea>
            </div>
          </div>

          <!-- 2. Media: Video & Cover Image -->
          <div class="rounded-xl border border-neutral-grey/70 bg-white p-5 shadow-xs space-y-4">
            <div class="flex items-center justify-between border-b border-neutral-grey/50 pb-2">
              <h3 class="text-xs font-bold uppercase tracking-wider text-sage">
                2. Exercise Video & Cover Image
              </h3>
              <span class="text-[11px] text-neutral-muted">Videos & images render directly in the Owner App</span>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
              <!-- Video section -->
              <div class="space-y-2">
                <label class="block text-xs font-semibold text-navy">
                  Exercise Video ({{ isSysAdmin ? 'Default Demo Video' : 'Custom Clinic Video' }})
                </label>

                <div class="flex items-center gap-2">
                  <label
                    class="inline-flex cursor-pointer items-center gap-2 rounded-lg border border-sage bg-sage/10 px-3 py-2 text-xs font-bold text-sage transition-colors hover:bg-sage/20"
                    :class="{ 'pointer-events-none opacity-50': uploadingVideo }"
                  >
                    <Loader2 v-if="uploadingVideo" class="h-4 w-4 animate-spin" />
                    <UploadCloud v-else class="h-4 w-4" />
                    {{ uploadingVideo ? 'Uploading video...' : 'Upload Video File' }}
                    <input
                      type="file"
                      accept="video/mp4,video/quicktime,video/webm"
                      class="hidden"
                      @change="handleVideoFileUpload"
                    />
                  </label>
                  <span class="text-[11px] text-neutral-muted">MP4, MOV, WebM (max 100MB)</span>
                </div>

                <div class="mt-1">
                  <input
                    v-model="videoUrl"
                    type="url"
                    placeholder="Or enter video URL (GCS, YouTube, MP4)..."
                    class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-xs outline-none focus:border-sage"
                  />
                </div>

                <!-- Video Preview -->
                <div v-if="videoUrl" class="mt-2 rounded-lg border border-neutral-grey/80 overflow-hidden bg-black/5 p-2.5">
                  <div class="flex items-center justify-between mb-1.5">
                    <p class="text-[10px] font-bold uppercase text-neutral-muted">Video Attached:</p>
                    <span v-if="youtubeEmbedUrl" class="rounded bg-red-100 px-1.5 py-0.5 text-[9px] font-bold text-red-700">
                      YouTube (Plays Inline)
                    </span>
                    <span v-else-if="isDirectVideoUrl" class="rounded bg-sage/20 px-1.5 py-0.5 text-[9px] font-bold text-sage">
                      Direct Video Stream
                    </span>
                  </div>

                  <!-- Inline YouTube Player -->
                  <div v-if="youtubeEmbedUrl" class="aspect-video w-full rounded overflow-hidden shadow-xs bg-black">
                    <iframe
                      :src="youtubeEmbedUrl"
                      class="h-full w-full border-0"
                      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                      allowfullscreen
                    ></iframe>
                  </div>

                  <!-- Direct MP4 / GCS Video Player -->
                  <video
                    v-else-if="isDirectVideoUrl"
                    :src="videoUrl"
                    controls
                    class="aspect-video w-full rounded object-contain bg-black"
                  ></video>

                  <div v-else class="flex items-center gap-2 text-xs text-navy py-1">
                    <FileVideo class="h-4 w-4 text-sage shrink-0" />
                    <span class="truncate font-mono text-[11px]">{{ videoUrl }}</span>
                  </div>
                </div>
              </div>

              <!-- Cover Image section -->
              <div class="space-y-2">
                <label class="block text-xs font-semibold text-navy">Exercise Cover / Banner Image</label>

                <div class="flex items-center gap-2">
                  <label
                    class="inline-flex cursor-pointer items-center gap-2 rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-xs font-bold text-navy transition-colors hover:bg-neutral-grey/30"
                    :class="{ 'pointer-events-none opacity-50': uploadingCover }"
                  >
                    <Loader2 v-if="uploadingCover" class="h-4 w-4 animate-spin" />
                    <ImageIcon v-else class="h-4 w-4 text-sage" />
                    {{ uploadingCover ? 'Uploading cover...' : 'Upload Cover Image' }}
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/webp"
                      class="hidden"
                      @change="handleCoverFileUpload"
                    />
                  </label>
                  <span class="text-[11px] text-neutral-muted">PNG, JPG, WebP</span>
                </div>

                <div class="mt-1">
                  <input
                    v-model="coverImageUrl"
                    type="url"
                    placeholder="Or enter image URL..."
                    class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-xs outline-none focus:border-sage"
                  />
                </div>

                <!-- Cover Preview -->
                <div v-if="coverImageUrl" class="mt-2 flex items-center gap-3 rounded-lg border border-neutral-grey/80 p-2 bg-surface">
                  <img :src="coverImageUrl" alt="Cover preview" class="h-16 w-24 rounded object-cover border border-neutral-grey/50" />
                  <div class="min-w-0 flex-1">
                    <p class="text-[11px] font-semibold text-navy">Cover Image Preview</p>
                    <p class="truncate text-[10px] text-neutral-muted font-mono">{{ coverImageUrl }}</p>
                  </div>
                  <button type="button" class="text-red-500 p-1 hover:bg-red-50 rounded" @click="coverImageUrl = ''">
                    <Trash2 class="h-4 w-4" />
                  </button>
                </div>
              </div>

              <!-- 2.1 Video Variations by Species & Breed Section -->
              <div class="rounded-xl border border-neutral-grey/90 bg-surface/60 p-4 space-y-3">
                <div class="flex items-center justify-between">
                  <div>
                    <h4 class="text-xs font-bold text-navy flex items-center gap-1.5">
                      <Film class="h-3.5 w-3.5 text-sage" />
                      Species & Breed Video Variations (Optional)
                    </h4>
                    <p class="text-[11px] text-neutral-muted">
                      Attach specific videos for different animal species, conformations (e.g. Dachshund/Corgi), or sizes.
                    </p>
                  </div>
                  <button
                    type="button"
                    class="inline-flex items-center gap-1 rounded-lg border border-sage bg-sage/10 px-2.5 py-1 text-xs font-bold text-sage transition-colors hover:bg-sage/20"
                    @click="addVideoVariation"
                  >
                    <Plus class="h-3 w-3" /> Add Variation
                  </button>
                </div>

                <div v-if="videoVariations.length === 0" class="rounded-lg border border-dashed border-neutral-grey/80 p-3 text-center text-xs text-neutral-muted">
                  No variations configured yet. The default video above will be used for all patients.
                </div>

                <div
                  v-for="(variation, vIdx) in videoVariations"
                  :key="vIdx"
                  class="rounded-lg border border-neutral-grey/80 bg-white p-3 space-y-3 shadow-2xs"
                >
                  <div class="flex items-center justify-between border-b border-neutral-grey/40 pb-2">
                    <div class="flex items-center gap-2">
                      <span class="rounded bg-navy/10 px-1.5 py-0.5 text-[10px] font-bold text-navy">
                        Variation #{{ vIdx + 1 }}
                      </span>
                      <span v-if="variation.species" class="text-[11px] font-semibold text-sage">
                        {{ variation.species }}
                      </span>
                      <span v-if="variation.breedCategory" class="text-[11px] text-neutral-muted">
                        • {{ variation.breedCategory }}
                      </span>
                    </div>
                    <button
                      type="button"
                      class="text-xs text-red-600 hover:text-red-800 flex items-center gap-1 font-semibold"
                      @click="removeVideoVariation(vIdx)"
                    >
                      <Trash2 class="h-3.5 w-3.5" /> Remove
                    </button>
                  </div>

                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                    <div>
                      <label class="block text-[11px] font-semibold text-navy mb-0.5">Target Species</label>
                      <select
                        v-model="variation.species"
                        class="w-full rounded-md border border-neutral-grey bg-surface px-2.5 py-1.5 text-xs outline-none focus:border-sage"
                      >
                        <option v-for="sp in speciesStore.speciesList" :key="sp.name" :value="sp.name">
                          {{ sp.displayName || sp.name }}
                        </option>
                      </select>
                    </div>
                    <div>
                      <label class="block text-[11px] font-semibold text-navy mb-0.5">Breed / Conformation Tag</label>
                      <input
                        v-model="variation.breedCategory"
                        :list="`breed-datalist-${vIdx}`"
                        type="text"
                        placeholder="Select or type breed (e.g. Dachshund, German Shepherd)"
                        class="w-full rounded-md border border-neutral-grey bg-surface px-2.5 py-1.5 text-xs outline-none focus:border-sage"
                      />
                      <datalist :id="`breed-datalist-${vIdx}`">
                        <option
                          v-for="br in speciesStore.breedsForSpecies(variation.species)"
                          :key="br.name"
                          :value="br.name"
                        >
                          {{ br.name }} ({{ br.sizeCategory }} - {{ br.conformation }})
                        </option>
                      </datalist>
                    </div>
                  </div>

                  <!-- Quick Breed Selection Chips from Species & Breed Config -->
                  <div
                    v-if="speciesStore.breedsForSpecies(variation.species).length > 0"
                    class="flex flex-wrap items-center gap-1.5 rounded-md bg-slate-50 p-2 border border-neutral-grey/50"
                  >
                    <span class="text-[10px] font-semibold text-neutral-muted">Configured Breeds:</span>
                    <button
                      v-for="br in speciesStore.breedsForSpecies(variation.species).slice(0, 8)"
                      :key="br.name"
                      type="button"
                      class="rounded px-2 py-0.5 text-[10px] font-medium transition-colors"
                      :class="
                        variation.breedCategory === br.name
                          ? 'bg-sage text-white font-bold shadow-2xs'
                          : 'bg-white border border-neutral-grey/70 text-navy hover:bg-sage/10 hover:border-sage/40'
                      "
                      @click="
                        variation.breedCategory = br.name;
                        if (!variation.title) variation.title = `${variation.species || 'Target'} - ${br.name}`;
                      "
                    >
                      {{ br.name }}
                    </button>
                  </div>

                  <div>
                    <label class="block text-[11px] font-semibold text-navy mb-0.5">Variation Title (Optional)</label>
                    <input
                      v-model="variation.title"
                      type="text"
                      placeholder="e.g. Low-Rider / Long-Backed Dogs"
                      class="w-full rounded-md border border-neutral-grey bg-surface px-2.5 py-1.5 text-xs outline-none focus:border-sage"
                    />
                  </div>

                  <div>
                    <label class="block text-[11px] font-semibold text-navy mb-0.5">Video URL (YouTube, MP4, or Upload)</label>
                    <div class="flex gap-2">
                      <input
                        v-model="variation.videoUrl"
                        type="url"
                        placeholder="https://youtube.com/watch?v=... or MP4 URL"
                        class="flex-1 rounded-md border border-neutral-grey bg-surface px-2.5 py-1.5 text-xs outline-none focus:border-sage"
                      />
                      <label
                        class="inline-flex cursor-pointer items-center gap-1 rounded-md border border-neutral-grey bg-surface px-2.5 py-1.5 text-xs font-bold text-navy hover:bg-neutral-grey/30 shrink-0"
                        :class="{ 'pointer-events-none opacity-50': uploadingVariationIndex === vIdx }"
                      >
                        <Loader2 v-if="uploadingVariationIndex === vIdx" class="h-3.5 w-3.5 animate-spin" />
                        <UploadCloud v-else class="h-3.5 w-3.5 text-sage" />
                        Upload
                        <input
                          type="file"
                          accept="video/mp4,video/quicktime,video/webm"
                          class="hidden"
                          @change="handleVariationVideoUpload(vIdx, $event)"
                        />
                      </label>
                    </div>
                  </div>

                  <!-- Inline preview for variation -->
                  <div v-if="variation.videoUrl" class="rounded border border-neutral-grey/60 bg-black/5 p-2">
                    <div v-if="getYouTubeEmbedUrl(variation.videoUrl)" class="aspect-video w-full rounded overflow-hidden shadow-xs bg-black">
                      <iframe
                        :src="getYouTubeEmbedUrl(variation.videoUrl)!"
                        class="h-full w-full border-0"
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                        allowfullscreen
                      ></iframe>
                    </div>
                    <video
                      v-else
                      :src="variation.videoUrl"
                      controls
                      class="aspect-video w-full rounded object-contain bg-black"
                    ></video>
                  </div>

                  <div>
                    <label class="block text-[11px] font-semibold text-navy mb-0.5">Clinical Modification Notes</label>
                    <textarea
                      v-model="variation.notes"
                      rows="1.5"
                      placeholder="e.g. Set poles 2-3 inches max from ground to prevent spine hyperextension."
                      class="w-full rounded-md border border-neutral-grey bg-surface px-2.5 py-1.5 text-xs outline-none focus:border-sage"
                    ></textarea>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 3. Clinical Guidelines & Safety Notes -->
          <div class="rounded-xl border border-neutral-grey/70 bg-white p-5 shadow-xs space-y-4">
            <h3 class="text-xs font-bold uppercase tracking-wider text-sage border-b border-neutral-grey/50 pb-2">
              3. Clinical Guidelines & Safety Notes
            </h3>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Targeted Muscles / Joint Mechanics</label>
                <input
                  v-model="targetedMuscles"
                  type="text"
                  placeholder="e.g. Quadriceps, Hamstrings, Stifle ROM, Core"
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
                />
              </div>

              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Clinical Purpose & Indications</label>
                <input
                  v-model="clinicalPurpose"
                  type="text"
                  placeholder="e.g. Promotes active stifle extension and tarsal flexion"
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
                />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Safety Notes & Contraindications</label>
                <textarea
                  v-model="safetyNotes"
                  rows="2"
                  placeholder="Pain thresholds, acute inflammation cautions..."
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
                ></textarea>
              </div>

              <div>
                <label class="block text-xs font-semibold text-navy mb-1">Common Compensations / Mistakes</label>
                <textarea
                  v-model="commonMistakes"
                  rows="2"
                  placeholder="e.g., Pet twisting spine, rushing through poles..."
                  class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
                ></textarea>
              </div>
            </div>
          </div>

          <!-- 4. Step-by-Step Instructions & Step Images -->
          <div class="rounded-xl border border-neutral-grey/70 bg-white p-5 shadow-xs space-y-4">
            <div class="flex items-center justify-between border-b border-neutral-grey/50 pb-2">
              <div>
                <h3 class="text-xs font-bold uppercase tracking-wider text-sage">
                  4. Step-by-Step Instructions & Step Images
                </h3>
                <p class="text-[11px] text-neutral-muted">Step images appear with tap-to-expand clarity on the owner routine screen.</p>
              </div>
              <button
                type="button"
                class="inline-flex items-center gap-1.5 rounded-lg bg-sage/10 px-3 py-1.5 text-xs font-bold text-sage transition-colors hover:bg-sage/20"
                @click="addStep"
              >
                <Plus class="h-3.5 w-3.5" />
                Add Step
              </button>
            </div>

            <div class="space-y-3">
              <div
                v-for="(step, idx) in steps"
                :key="idx"
                class="rounded-xl border border-neutral-grey/70 bg-surface/50 p-4 transition-all hover:border-sage/50 space-y-3"
              >
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    <span class="flex h-6 w-6 items-center justify-center rounded-full bg-navy text-xs font-extrabold text-white">
                      {{ idx + 1 }}
                    </span>
                    <span class="text-xs font-bold text-navy">Step {{ idx + 1 }} Instruction</span>
                  </div>

                  <button
                    v-if="steps.length > 1"
                    type="button"
                    class="rounded-md p-1 text-neutral-muted hover:bg-red-50 hover:text-red-600"
                    title="Remove step"
                    @click="removeStep(idx)"
                  >
                    <Trash2 class="h-4 w-4" />
                  </button>
                </div>

                <textarea
                  v-model="step.stepInstruction"
                  rows="2"
                  placeholder="Explain step instruction clearly so owner can replicate form safely..."
                  class="w-full rounded-lg border border-neutral-grey bg-white px-3 py-2 text-xs outline-none focus:border-sage focus:ring-1 focus:ring-sage/20"
                ></textarea>

                <!-- Step Image Upload or URL -->
                <div class="flex flex-wrap items-center gap-3 pt-1">
                  <label
                    class="inline-flex cursor-pointer items-center gap-1.5 rounded-md border border-neutral-grey bg-white px-2.5 py-1.5 text-[11px] font-semibold text-navy hover:bg-surface"
                    :class="{ 'pointer-events-none opacity-50': uploadingStepIndex === idx }"
                  >
                    <Loader2 v-if="uploadingStepIndex === idx" class="h-3.5 w-3.5 animate-spin" />
                    <ImageIcon v-else class="h-3.5 w-3.5 text-sage" />
                    {{ uploadingStepIndex === idx ? 'Uploading...' : 'Upload Step Image' }}
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/webp"
                      class="hidden"
                      @change="handleStepImageUpload(idx, $event)"
                    />
                  </label>

                  <input
                    v-model="step.imageUrl"
                    type="url"
                    placeholder="Or paste step image URL..."
                    class="min-w-[200px] flex-1 rounded-md border border-neutral-grey bg-white px-2.5 py-1 text-[11px] outline-none focus:border-sage"
                  />

                  <!-- Image preview thumbnail -->
                  <div v-if="step.imageUrl" class="relative group">
                    <img
                      :src="step.imageUrl"
                      alt="Step preview"
                      class="h-10 w-14 rounded object-cover border border-neutral-grey shadow-xs"
                    />
                    <button
                      type="button"
                      class="absolute -top-1.5 -right-1.5 rounded-full bg-red-600 p-0.5 text-white opacity-0 group-hover:opacity-100 transition-opacity"
                      @click="step.imageUrl = ''"
                    >
                      <X class="h-3 w-3" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-if="errorMessage" class="rounded-lg bg-red-50 p-3 text-xs font-semibold text-red-700 border border-red-200">
            {{ errorMessage }}
          </div>
        </form>

        <!-- TAB 2: LIVE OWNER SCREEN MOBILE PREVIEW -->
        <div v-else class="flex flex-col items-center justify-center py-4">
          <div class="mb-4 text-center max-w-md">
            <span class="rounded-full bg-sage/20 px-3 py-1 text-xs font-bold uppercase tracking-wider text-sage">
              Live Owner Screen Preview
            </span>
            <p class="text-xs text-neutral-muted mt-1">
              This preview shows how your exercise, instructions, video, and step images look to the pet owner on mobile.
            </p>

            <!-- Preview As Variation Selector -->
            <div v-if="videoVariations.length > 0" class="mt-3 flex items-center justify-center gap-2">
              <label class="text-xs font-bold text-navy whitespace-nowrap">Preview As:</label>
              <select
                v-model="previewVariationIndex"
                class="rounded-lg border border-neutral-grey bg-white px-3 py-1.5 text-xs font-semibold text-navy outline-none focus:border-sage shadow-xs"
              >
                <option :value="-1">Default / General ({{ targetSpecies }})</option>
                <option
                  v-for="(v, vIdx) in videoVariations"
                  :key="vIdx"
                  :value="vIdx"
                >
                  {{ v.species }} {{ v.breedCategory ? `(${v.breedCategory})` : '' }} - {{ v.title || 'Variation' }}
                </option>
              </select>
            </div>
          </div>

          <!-- Mobile Phone Mockup Frame -->
          <div class="w-full max-w-[370px] rounded-[36px] border-[10px] border-neutral-800 bg-surface shadow-2xl overflow-hidden">
            <!-- Mobile Top Bar -->
            <div class="bg-navy px-5 py-3 text-white">
              <div class="flex items-center justify-between text-[11px] font-bold opacity-80 mb-2">
                <span>9:41</span>
                <span>5G 100%</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold uppercase tracking-wider text-sage-light">MoveWell Routine</span>
                <span class="text-[10px] bg-white/20 px-2 py-0.5 rounded text-white font-semibold">RESET</span>
              </div>
            </div>

            <!-- Mobile Screen Content (Simulating Exercise Routine Screen) -->
            <div class="p-4 space-y-3 max-h-[520px] overflow-y-auto">
              <!-- Active Variation Badge if previewing variation -->
              <div v-if="activePreviewVariation" class="rounded-xl border border-sage/40 bg-sage/10 p-2.5 flex items-start gap-2">
                <Sparkles class="h-4 w-4 text-sage shrink-0 mt-0.5" />
                <div class="text-[11px]">
                  <span class="font-bold text-navy block">Showing {{ activePreviewVariation.species }} {{ activePreviewVariation.breedCategory ? `(${activePreviewVariation.breedCategory})` : '' }} Form</span>
                  <span v-if="activePreviewVariation.notes" class="text-neutral-muted block text-[10px] mt-0.5">{{ activePreviewVariation.notes }}</span>
                </div>
              </div>

              <!-- Exercise Title Card -->
              <div class="rounded-2xl border border-neutral-grey/80 bg-white p-4 shadow-xs space-y-2">
                <div class="flex items-start justify-between gap-2">
                  <h4 class="font-extrabold text-navy text-sm">{{ title || 'Exercise Title' }}</h4>
                  <span class="rounded-full bg-sage px-2 py-0.5 text-[9px] font-bold uppercase tracking-wider text-white shrink-0">
                    {{ conditionCategory }}
                  </span>
                </div>
                <p class="text-[11px] text-neutral-muted leading-relaxed">
                  {{ shortDescription || 'Follow each step carefully to ensure optimal rehabilitation form.' }}
                </p>
                <div class="flex items-center gap-1.5 pt-1">
                  <span class="rounded-md bg-navy/5 px-2 py-0.5 text-[10px] font-bold text-navy">10 reps</span>
                  <span class="rounded-md bg-navy/5 px-2 py-0.5 text-[10px] font-bold text-navy">3 sets</span>
                  <span class="rounded-md bg-sage/20 px-2 py-0.5 text-[10px] font-bold text-sage">Set 1 of 3</span>
                </div>
              </div>

              <!-- Exercise Video Card (Using currentPreviewVideoUrl) -->
              <div v-if="currentPreviewVideoUrl" class="rounded-2xl border border-neutral-grey/80 bg-black overflow-hidden shadow-xs">
                <div class="relative aspect-video flex items-center justify-center bg-neutral-900">
                  <iframe
                    v-if="currentPreviewYoutubeEmbed"
                    :src="currentPreviewYoutubeEmbed"
                    class="h-full w-full border-0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                    allowfullscreen
                  ></iframe>
                  <video
                    v-else-if="isCurrentPreviewDirectVideo"
                    :src="currentPreviewVideoUrl"
                    controls
                    class="h-full w-full object-contain"
                  ></video>
                  <div v-else class="text-center p-4 text-white">
                    <FileVideo class="h-8 w-8 mx-auto mb-1 text-sage" />
                    <p class="text-[11px] font-semibold">Video Stream Loaded</p>
                  </div>
                </div>
              </div>

              <!-- Step Instruction & Image Card -->
              <div
                v-for="(step, sIdx) in steps.filter(s => s.stepInstruction.trim())"
                :key="sIdx"
                class="rounded-2xl border border-neutral-grey/80 bg-white p-4 shadow-xs space-y-2.5"
              >
                <div class="flex items-center justify-between">
                  <span class="text-[11px] font-bold text-navy">
                    Step {{ sIdx + 1 }} of {{ steps.length }}
                  </span>
                  <span class="text-[9px] text-neutral-muted">Form & Posture</span>
                </div>

                <!-- Step Image with Tap-to-Zoom Callout -->
                <div v-if="step.imageUrl" class="relative rounded-xl overflow-hidden border border-neutral-grey/80 bg-surface group">
                  <img :src="step.imageUrl" alt="Step form" class="h-40 w-full object-cover" />
                  <div class="absolute bottom-2 right-2 flex items-center gap-1 rounded-md bg-black/70 px-2 py-0.5 text-[9px] font-bold text-white">
                    <Maximize2 class="h-2.5 w-2.5" /> Tap to zoom
                  </div>
                </div>

                <p class="text-xs text-navy font-medium leading-relaxed">
                  {{ step.stepInstruction }}
                </p>
              </div>

              <!-- Safety Alert -->
              <div v-if="safetyNotes" class="rounded-xl border border-amber-200 bg-amber-50/70 p-3 space-y-1">
                <div class="flex items-center gap-1.5 text-amber-800 text-[11px] font-bold">
                  <AlertTriangle class="h-3.5 w-3.5" /> Safety Notes
                </div>
                <p class="text-[10px] text-amber-900 leading-relaxed">{{ safetyNotes }}</p>
              </div>

              <!-- Action Button -->
              <div class="pt-2">
                <button
                  type="button"
                  class="w-full rounded-xl bg-sage py-3 text-center text-xs font-extrabold uppercase tracking-wider text-white shadow-md"
                >
                  NEXT STEP
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Footer -->
      <div class="flex flex-wrap sm:flex-nowrap items-center justify-between gap-3 border-t border-neutral-grey/80 bg-white px-4 sm:px-6 py-3.5 sm:py-4">
        <div class="text-xs text-neutral-muted">
          <span v-if="props.mode === 'customize'" class="font-medium text-amber-800">
            * Saving creates a clinic-scoped version active for your owners.
          </span>
          <span v-else-if="isSysAdmin" class="font-medium text-navy">
            * Saving updates system defaults across the platform.
          </span>
        </div>

        <div class="flex items-center gap-2.5 shrink-0 ml-auto">
          <BaseButton type="button" variant="secondary" size="sm" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton
            type="button"
            variant="accent"
            size="sm"
            :disabled="isSubmitting || uploadingVideo || uploadingCover"
            @click="saveExercise"
          >
            <Loader2 v-if="isSubmitting" class="h-4 w-4 animate-spin mr-1.5" />
            {{ isSubmitting ? 'Saving...' : 'Save & Publish Exercise' }}
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

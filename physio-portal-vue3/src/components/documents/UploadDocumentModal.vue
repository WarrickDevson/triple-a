<script setup lang="ts">
import { ref, watch } from 'vue'
import { UploadCloud, X, CheckCircle2 } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { DOCUMENT_CATEGORIES, type DocumentCategory } from '../../data/documentsDemo'
import { useDocumentsStore } from '../../store/documents'
import { usePatientsStore } from '../../store/patients'

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  close: []
}>()

const documentsStore = useDocumentsStore()
const patientsStore = usePatientsStore()

const title = ref('')
const petName = ref('')
const ownerName = ref('')
const category = ref<DocumentCategory>('Clinical Notes')

const selectedFile = ref<File | null>(null)
const fileDataUrl = ref<string>('')
const isDragging = ref(false)
const errorMessage = ref('')
const isSubmitting = ref(false)

watch(
  () => props.open,
  (val) => {
    if (val) {
      resetForm()
      if (patientsStore.patients.length === 0) {
        patientsStore.fetchClinicPatients().catch(() => undefined)
      }
    }
  },
)

function resetForm() {
  title.value = ''
  petName.value = ''
  ownerName.value = ''
  category.value = 'Clinical Notes'
  selectedFile.value = null
  fileDataUrl.value = ''
  errorMessage.value = ''
  isSubmitting.value = false
}

function onFileSelect(event: Event) {
  const input = event.target as HTMLInputElement
  if (input.files && input.files[0]) {
    processFile(input.files[0])
  }
}

function onDrop(event: DragEvent) {
  isDragging.value = false
  if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
    processFile(event.dataTransfer.files[0])
  }
}

function processFile(file: File) {
  // Max size 25MB
  if (file.size > 25 * 1024 * 1024) {
    errorMessage.value = 'File size exceeds 25MB limit.'
    return
  }

  errorMessage.value = ''
  selectedFile.value = file

  if (!title.value) {
    // Autofill title without extension
    const nameWithoutExt = file.name.substring(0, file.name.lastIndexOf('.')) || file.name
    title.value = nameWithoutExt
  }

  const reader = new FileReader()
  reader.onload = (e) => {
    fileDataUrl.value = e.target?.result as string
  }
  reader.readAsDataURL(file)
}

function onPatientChange() {
  const found = patientsStore.patients.find((p) => p.petName === petName.value)
  if (found) {
    ownerName.value = found.ownerName || 'Pet Owner'
  }
}

function submitUpload() {
  if (!title.value.trim()) {
    errorMessage.value = 'Please enter a document title.'
    return
  }

  if (!petName.value.trim()) {
    errorMessage.value = 'Please select or enter a patient name.'
    return
  }

  isSubmitting.value = true

  const sizeKb = selectedFile.value
    ? Math.round(selectedFile.value.size / 1024) || 1
    : 150

  const fileType = selectedFile.value?.type || 'application/pdf'
  const today = new Date().toISOString().slice(0, 10)

  documentsStore.addDocument({
    name: title.value.trim(),
    petName: petName.value.trim(),
    ownerName: ownerName.value.trim() || 'Client',
    category: category.value,
    uploadedAt: today,
    sizeKb,
    fileType,
    fileDataUrl: fileDataUrl.value || undefined,
  })

  isSubmitting.value = false
  emit('close')
}
</script>

<template>
  <div
    v-if="open"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card w-full max-w-lg overflow-hidden p-6 shadow-2xl animate-in fade-in zoom-in-95">
      <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-4">
        <div>
          <h2 class="text-lg font-bold text-navy">Upload Clinical Document</h2>
          <p class="text-xs text-neutral-muted">Add consent forms, assessment notes, or imaging files.</p>
        </div>
        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted hover:bg-surface hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <form class="mt-4 space-y-4" @submit.prevent="submitUpload">
        <!-- Dropzone / File Picker -->
        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-neutral-muted mb-1.5">
            Document File
          </label>
          <div
            class="relative flex flex-col items-center justify-center rounded-xl border-2 border-dashed p-6 text-center transition-colors"
            :class="[
              isDragging ? 'border-sage bg-sage-muted/40' : 'border-neutral-grey bg-surface/50 hover:border-sage/70',
              selectedFile ? 'border-sage/80 bg-sage-muted/20' : '',
            ]"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="onDrop"
          >
            <input
              type="file"
              class="absolute inset-0 cursor-pointer opacity-0"
              accept=".pdf,.png,.jpg,.jpeg,.doc,.docx,.txt"
              @change="onFileSelect"
            />

            <div v-if="selectedFile" class="flex flex-col items-center">
              <CheckCircle2 class="h-10 w-10 text-sage mb-2" />
              <p class="text-sm font-bold text-navy">{{ selectedFile.name }}</p>
              <p class="text-xs text-neutral-muted mt-0.5">
                {{ Math.round(selectedFile.size / 1024) }} KB · {{ selectedFile.type || 'Document' }}
              </p>
              <span class="mt-2 text-xs font-semibold text-sage underline">Click to replace file</span>
            </div>

            <div v-else class="flex flex-col items-center">
              <UploadCloud class="h-10 w-10 text-sage mb-2" />
              <p class="text-sm font-semibold text-navy">Drag and drop file here, or browse</p>
              <p class="text-xs text-neutral-muted mt-1">Supports PDF, PNG, JPG, DOCX, TXT (Max 25MB)</p>
            </div>
          </div>
        </div>

        <!-- Document Details -->
        <div>
          <label class="block text-xs font-semibold text-navy mb-1">Document Title *</label>
          <input
            v-model="title"
            type="text"
            required
            placeholder="e.g. Treatment Consent Form"
            class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
          />
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Patient Name *</label>
            <input
              v-model="petName"
              type="text"
              list="patient-options"
              required
              placeholder="e.g. Bella"
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
              @change="onPatientChange"
            />
            <datalist id="patient-options">
              <option v-for="p in patientsStore.patients" :key="p.petId" :value="p.petName">
                {{ p.petName }} ({{ p.ownerName }})
              </option>
            </datalist>
          </div>

          <div>
            <label class="block text-xs font-semibold text-navy mb-1">Category *</label>
            <select
              v-model="category"
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            >
              <option v-for="cat in DOCUMENT_CATEGORIES" :key="cat" :value="cat">{{ cat }}</option>
            </select>
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-navy mb-1">Owner Name</label>
          <input
            v-model="ownerName"
            type="text"
            placeholder="e.g. Sarah Mitchell"
            class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm outline-none focus:border-sage"
          />
        </div>

        <div v-if="errorMessage" class="rounded-lg bg-red-50 p-2.5 text-xs font-medium text-red-700">
          {{ errorMessage }}
        </div>

        <!-- Modal Actions -->
        <div class="flex items-center justify-end gap-2 border-t border-neutral-grey/80 pt-4">
          <BaseButton type="button" variant="secondary" size="sm" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton type="submit" variant="accent" size="sm" :disabled="isSubmitting">
            {{ isSubmitting ? 'Uploading...' : 'Save & Upload Document' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

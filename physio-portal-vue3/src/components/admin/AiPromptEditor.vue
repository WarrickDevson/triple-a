<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  AlertCircle,
  Bot,
  CheckCircle2,
  Copy,
  HelpCircle,
  Loader2,
  RefreshCw,
  RotateCcw,
  Save,
  ShieldAlert,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import {
  fetchAiPromptConfig,
  resetAiPromptConfig,
  updateAiPromptConfig,
  type AiPromptConfig,
} from '../../api/aiPrompt'

const config = ref<AiPromptConfig | null>(null)
const editablePrompt = ref('')
const loading = ref(true)
const saving = ref(false)
const resetting = ref(false)
const successMessage = ref<string | null>(null)
const errorMessage = ref<string | null>(null)
const copiedDefault = ref(false)

async function loadConfig() {
  loading.value = true
  errorMessage.value = null
  try {
    const data = await fetchAiPromptConfig()
    config.value = data
    editablePrompt.value = data.systemPrompt
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to load AI prompt configuration.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadConfig()
})

const hasChanges = computed(() => {
  if (!config.value) return false
  return editablePrompt.value.trim() !== config.value.systemPrompt.trim()
})

const characterCount = computed(() => editablePrompt.value.length)
const lineCount = computed(() => (editablePrompt.value ? editablePrompt.value.split('\n').length : 0))

async function handleSave() {
  if (!editablePrompt.value.trim()) {
    errorMessage.value = 'System prompt cannot be empty.'
    return
  }

  saving.value = true
  errorMessage.value = null
  successMessage.value = null

  try {
    const updated = await updateAiPromptConfig(editablePrompt.value)
    config.value = updated
    editablePrompt.value = updated.systemPrompt
    successMessage.value = 'AI prompt updated successfully! New pet owner chats will immediately use these instructions.'
    setTimeout(() => {
      successMessage.value = null
    }, 5000)
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to update AI prompt.'
  } finally {
    saving.value = false
  }
}

async function handleReset() {
  if (!confirm('Are you sure you want to reset the Wellness Assistant prompt back to Triple A clinic defaults?')) {
    return
  }

  resetting.value = true
  errorMessage.value = null
  successMessage.value = null

  try {
    const res = await resetAiPromptConfig()
    config.value = res
    editablePrompt.value = res.systemPrompt
    successMessage.value = 'AI prompt has been reset to official Triple A clinic defaults.'
    setTimeout(() => {
      successMessage.value = null
    }, 5000)
  } catch (err: any) {
    errorMessage.value = err?.response?.data?.message || 'Failed to reset AI prompt.'
  } finally {
    resetting.value = false
  }
}

function copyDefaultToClipboard() {
  if (!config.value?.defaultSystemPrompt) return
  navigator.clipboard.writeText(config.value.defaultSystemPrompt)
  copiedDefault.value = true
  setTimeout(() => {
    copiedDefault.value = false
  }, 2500)
}

function insertDefaultIntoEditor() {
  if (!config.value?.defaultSystemPrompt) return
  editablePrompt.value = config.value.defaultSystemPrompt
}

function formatDate(dateStr: string | null) {
  if (!dateStr) return 'N/A'
  try {
    const d = new Date(dateStr)
    return d.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return dateStr
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header Card -->
    <div class="rounded-2xl border border-navy/10 bg-white p-6 shadow-sm">
      <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div class="flex items-start gap-4">
          <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-sage/15 text-sage">
            <Bot class="h-6 w-6" />
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h2 class="text-xl font-bold text-navy">Wellness Assistant Prompt & Guidelines</h2>
              <span
                v-if="config?.isCustomized"
                class="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2.5 py-0.5 text-xs font-semibold text-amber-700 border border-amber-200"
              >
                Customized
              </span>
              <span
                v-else-if="config"
                class="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2.5 py-0.5 text-xs font-semibold text-emerald-700 border border-emerald-200"
              >
                Clinic Default
              </span>
            </div>
            <p class="mt-1 text-sm text-navy/70">
              Customize the core instructions and behavioral boundaries that guide the AI Wellness Assistant in the Pet Owner app.
            </p>
          </div>
        </div>

        <div class="flex items-center gap-2 self-start sm:self-auto">
          <BaseButton
            variant="secondary"
            size="sm"
            :disabled="loading || resetting"
            @click="loadConfig"
          >
            <RefreshCw class="mr-1.5 h-4 w-4" :class="{ 'animate-spin': loading }" />
            Refresh
          </BaseButton>
          <BaseButton
            variant="secondary"
            size="sm"
            :disabled="loading || resetting || (!config?.isCustomized && !hasChanges)"
            @click="handleReset"
          >
            <RotateCcw class="mr-1.5 h-4 w-4" />
            Reset to Default
          </BaseButton>
          <BaseButton
            variant="primary"
            size="sm"
            :disabled="loading || saving || !hasChanges"
            @click="handleSave"
          >
            <Loader2 v-if="saving" class="mr-1.5 h-4 w-4 animate-spin" />
            <Save v-else class="mr-1.5 h-4 w-4" />
            Save & Apply
          </BaseButton>
        </div>
      </div>

      <!-- Feedback Alerts -->
      <div
        v-if="successMessage"
        class="mt-4 flex items-center gap-2 rounded-xl bg-emerald-50 p-3.5 text-sm text-emerald-800 border border-emerald-200"
      >
        <CheckCircle2 class="h-4 w-4 shrink-0 text-emerald-600" />
        <span>{{ successMessage }}</span>
      </div>

      <div
        v-if="errorMessage"
        class="mt-4 flex items-center gap-2 rounded-xl bg-red-50 p-3.5 text-sm text-red-800 border border-red-200"
      >
        <AlertCircle class="h-4 w-4 shrink-0 text-red-600" />
        <span>{{ errorMessage }}</span>
      </div>

      <!-- Metadata summary bar -->
      <div
        v-if="config"
        class="mt-5 flex flex-wrap items-center gap-4 rounded-xl bg-surface px-4 py-2.5 text-xs text-navy/70 border border-navy/6"
      >
        <div>
          <span class="font-medium text-navy">Status:</span>
          {{ config.isCustomized ? 'Custom Clinic Prompt Active' : 'Official Triple A Baseline Active' }}
        </div>
        <div v-if="config.lastModifiedAt">
          <span class="font-medium text-navy">Last Updated:</span>
          {{ formatDate(config.lastModifiedAt) }}
        </div>
        <div v-if="config.lastModifiedBy">
          <span class="font-medium text-navy">Updated By:</span>
          {{ config.lastModifiedBy }}
        </div>
        <div class="ml-auto font-mono">
          {{ lineCount }} lines • {{ characterCount }} chars
        </div>
      </div>
    </div>

    <!-- Main Editor Section -->
    <div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
      <!-- Textarea Editor Column (2 cols) -->
      <div class="lg:col-span-2 space-y-4">
        <div class="rounded-2xl border border-navy/10 bg-white p-5 shadow-sm">
          <div class="mb-3 flex items-center justify-between">
            <label for="prompt-editor" class="text-sm font-bold text-navy flex items-center gap-2">
              <span>System Prompt Definition</span>
              <span v-if="hasChanges" class="text-xs font-normal text-amber-600 bg-amber-50 px-2 py-0.5 rounded-full border border-amber-200">
                Unsaved Changes
              </span>
            </label>
            <div class="flex items-center gap-3 text-xs text-navy/60">
              <button
                type="button"
                class="text-sage font-medium hover:underline flex items-center gap-1"
                @click="insertDefaultIntoEditor"
              >
                <Copy class="h-3.5 w-3.5" />
                Fill Default Template
              </button>
            </div>
          </div>

          <div class="relative">
            <textarea
              id="prompt-editor"
              v-model="editablePrompt"
              rows="18"
              class="w-full rounded-xl border border-navy/15 bg-surface/50 p-4 font-mono text-sm leading-relaxed text-navy placeholder-navy/40 focus:border-sage focus:bg-white focus:outline-none focus:ring-2 focus:ring-sage/20 transition-all"
              placeholder="Enter system prompt instructions for the AI assistant..."
              :disabled="loading"
            />
          </div>

          <div class="mt-4 flex items-center justify-between">
            <p class="text-xs text-navy/60">
              Changes take effect immediately across all client sessions upon saving.
            </p>
            <BaseButton
              variant="primary"
              size="sm"
              :disabled="loading || saving || !hasChanges"
              @click="handleSave"
            >
              <Loader2 v-if="saving" class="mr-1.5 h-4 w-4 animate-spin" />
              <Save v-else class="mr-1.5 h-4 w-4" />
              Save & Apply
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Side Guidance & Baseline Card (1 col) -->
      <div class="space-y-4">
        <!-- Guidelines Card -->
        <div class="rounded-2xl border border-navy/10 bg-white p-5 shadow-sm">
          <h3 class="text-sm font-bold text-navy flex items-center gap-2">
            <ShieldAlert class="h-4 w-4 text-sage" />
            Core Clinical Guidelines
          </h3>
          <ul class="mt-3 space-y-2 text-xs text-navy/70 leading-relaxed">
            <li class="flex items-start gap-2">
              <span class="text-sage font-bold">•</span>
              <span><strong>Medical boundaries:</strong> The AI must never prescribe drugs, suggest dosages, or diagnose conditions.</span>
            </li>
            <li class="flex items-start gap-2">
              <span class="text-sage font-bold">•</span>
              <span><strong>Appointments:</strong> Remind users to use the Appointments tab in the app or contact reception.</span>
            </li>
            <li class="flex items-start gap-2">
              <span class="text-sage font-bold">•</span>
              <span><strong>Red flags:</strong> Instruct owners to consult their vet or physiotherapist for worsening pain or post-op issues.</span>
            </li>
            <li class="flex items-start gap-2">
              <span class="text-sage font-bold">•</span>
              <span><strong>Formatting:</strong> Bullet points and concise paragraphs keep recovery guidance actionable.</span>
            </li>
          </ul>
        </div>

        <!-- Approved Default Reference Card -->
        <div class="rounded-2xl border border-navy/10 bg-white p-5 shadow-sm">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-bold text-navy flex items-center gap-2">
              <HelpCircle class="h-4 w-4 text-navy/60" />
              Approved Triple A Baseline
            </h3>
            <button
              type="button"
              class="text-xs text-sage hover:underline flex items-center gap-1"
              @click="copyDefaultToClipboard"
            >
              <CheckCircle2 v-if="copiedDefault" class="h-3.5 w-3.5 text-emerald-600" />
              <Copy v-else class="h-3.5 w-3.5" />
              {{ copiedDefault ? 'Copied' : 'Copy' }}
            </button>
          </div>

          <div class="mt-3 max-h-64 overflow-y-auto rounded-lg bg-surface p-3 font-mono text-xs text-navy/80 border border-navy/6">
            <pre class="whitespace-pre-wrap font-sans leading-relaxed">{{ config?.defaultSystemPrompt || 'Loading default...' }}</pre>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

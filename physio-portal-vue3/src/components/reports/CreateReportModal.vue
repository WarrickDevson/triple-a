<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import {
  X,
  FileText,
  Download,
  Save,
  Share2,
  Sparkles,
  Activity,
  FileCheck2,
} from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { Pet } from '../../types/pet'
import type { CreateReportPayload } from '../../types/soap'
import { REPORT_TYPES } from '../../data/reportsDemo'

const props = defineProps<{
  patients: Pet[]
  initialPetId: number | null
  initialType?: string | null
  saving?: boolean
  downloading?: boolean
}>()

const emit = defineEmits<{
  close: []
  saveAndDownload: [payload: CreateReportPayload]
  saveOnly: [payload: CreateReportPayload]
  quickDownload: [petId: number, options: { type: string; customTitle: string; summary: string; dischargeStatus?: string; maintenancePlan?: string; veterinarianNotes?: string }]
}>()

const selectedPetId = ref<number | null>(props.initialPetId)
const selectedTypeId = ref<'progress' | 'discharge' | 'home-program' | 'soap'>(
  (props.initialType as any) || 'progress'
)

const title = ref('')
const summary = ref('')
const dischargeStatus = ref('Rehabilitation Goals Achieved — Discharged to Home Maintenance')
const maintenancePlan = ref('')
const veterinarianNotes = ref('')
const shareWithOwner = ref(true)

const selectedPatient = computed(() =>
  props.patients.find((p) => p.petId === selectedPetId.value) ?? props.patients[0] ?? null
)

// Auto-fill template drafts when pet or type changes
function applySmartDraft() {
  const pet = selectedPatient.value
  const petName = pet?.petName || 'Patient'
  const ownerName = pet?.ownerName || 'Pet Owner'
  const type = selectedTypeId.value

  const dateStr = new Date().toLocaleDateString('en-US', { month: 'short', year: 'numeric' })

  if (type === 'discharge') {
    title.value = `${petName} - Rehabilitation Discharge Summary (${dateStr})`
    summary.value = `${petName} has successfully completed the prescribed veterinary physical rehabilitation course. Full functional weight-bearing and symmetrical gait have been restored with pain score reduced to 1/10. Patient is formally discharged to the long-term home maintenance protocol.`
    dischargeStatus.value = 'Rehabilitation Goals Achieved — Discharged to Home Maintenance'
    maintenancePlan.value = '1. Continue controlled daily leash walks (20-25 mins twice daily).\n2. Perform prescribed core strengthening and balance exercises 3 times weekly.\n3. Maintain target body weight to avoid excess joint load.'
    veterinarianNotes.value = `${petName} demonstrated remarkable functional recovery with no joint effusion or discomfort on extension. Advise routine annual check-up. Contact physio team if acute lameness occurs.`
  } else if (type === 'home-program') {
    title.value = `${petName} - Home Exercise & Care Protocol`
    summary.value = `Personalized home rehabilitation program for ${petName}. Follow the prescribed daily exercise guidelines to build strength, improve flexibility, and maintain healthy joint mechanics at home.`
    maintenancePlan.value = '• Warm Up: 3-5 minute gentle flat walk before exercises.\n• Cavaletti Rails: 2 sets of 10 repetitions, 2x daily.\n• Sit-to-Stand Squats: 3 sets of 8 repetitions, 2x daily.\n• Ice/Cold Pack: 10 minutes post-exercise if warmth detected.'
    veterinarianNotes.value = 'Owner has been instructed on safe exercise technique and positive reinforcement handling.'
  } else if (type === 'soap') {
    title.value = `${petName} - SOAP Clinical Assessment Summary`
    summary.value = `Clinical examination indicates steady progress. Left stifle extension PROM measured at 135°. Incision and soft tissues are calm with no palpation heat. Prescribed continuing hydrotherapy and laser therapy.`
    maintenancePlan.value = ''
    veterinarianNotes.value = ''
  } else {
    title.value = `${petName} - Clinical Progress & Rehabilitation Report`
    summary.value = `${petName} is progressing well through the current rehabilitation plan. Objective metrics show improved weight-bearing symmetry, reduced morning stiffness, and increased exercise tolerance. Treatment compliance from owner (${ownerName}) has been excellent.`
    maintenancePlan.value = ''
    veterinarianNotes.value = ''
  }
}

watch(
  () => [selectedPetId.value, selectedTypeId.value],
  () => {
    applySmartDraft()
  },
  { immediate: true }
)

function getPayload(): CreateReportPayload {
  const petId = Number(selectedPetId.value || selectedPatient.value?.petId || 1)
  const normalizedType = selectedTypeId.value === 'discharge'
    ? 'DISCHARGE_SUMMARY'
    : selectedTypeId.value === 'home-program'
      ? 'OWNER_HOME_PROGRAM'
      : selectedTypeId.value === 'soap'
        ? 'SOAP_SESSION'
        : 'PROGRESS_REPORT'

  return {
    petId,
    reportType: normalizedType,
    title: title.value.trim() || `${selectedPatient.value?.petName || 'Patient'} - Clinical Report`,
    summary: summary.value.trim(),
    dischargeStatus: selectedTypeId.value === 'discharge' ? dischargeStatus.value : undefined,
    maintenancePlan: maintenancePlan.value.trim() || undefined,
    veterinarianNotes: veterinarianNotes.value.trim() || undefined,
    shareWithOwner: shareWithOwner.value,
  }
}

function handleSaveAndDownload() {
  emit('saveAndDownload', getPayload())
}

function handleSaveOnly() {
  emit('saveOnly', getPayload())
}

function handleQuickDownload() {
  const petId = Number(selectedPetId.value || selectedPatient.value?.petId || 1)
  emit('quickDownload', petId, {
    type: selectedTypeId.value,
    customTitle: title.value.trim() || `${selectedPatient.value?.petName || 'Patient'} - Report`,
    summary: summary.value.trim(),
    dischargeStatus: dischargeStatus.value,
    maintenancePlan: maintenancePlan.value,
    veterinarianNotes: veterinarianNotes.value,
  })
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <div class="portal-card flex max-h-[92vh] w-full max-w-3xl flex-col overflow-hidden shadow-2xl animate-in fade-in zoom-in-95 duration-150">
      <!-- Header -->
      <div class="flex items-start justify-between border-b border-neutral-grey/80 p-5">
        <div class="flex items-center gap-3">
          <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage">
            <Sparkles class="h-6 w-6" :stroke-width="1.75" />
          </div>
          <div>
            <h3 class="text-base font-bold text-navy">Generate New Clinical Report</h3>
            <p class="text-xs text-neutral-muted">
              Create, customize, and generate professional PDF reports for patients and pet owners.
            </p>
          </div>
        </div>
        <button
          type="button"
          class="rounded-lg p-1.5 text-neutral-muted transition-colors hover:bg-surface hover:text-navy"
          @click="emit('close')"
        >
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Form Body (Scrollable) -->
      <div class="flex-1 overflow-y-auto p-6 space-y-5">
        <!-- 1. Patient Selector & Report Type Selector -->
        <div class="grid gap-4 sm:grid-cols-2">
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-navy mb-1.5">
              Select Target Patient
            </label>
            <select
              v-model="selectedPetId"
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage"
            >
              <option v-for="p in patients" :key="p.petId" :value="p.petId">
                {{ p.petName }} ({{ p.species }} · Owner: {{ p.ownerName }})
              </option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-navy mb-1.5">
              Report Category
            </label>
            <div class="grid grid-cols-2 gap-1.5">
              <button
                v-for="rt in REPORT_TYPES"
                :key="rt.id"
                type="button"
                class="rounded-lg border px-2.5 py-1.5 text-left text-xs font-semibold transition-all"
                :class="
                  selectedTypeId === rt.id
                    ? 'border-sage bg-sage text-white shadow-sm'
                    : 'border-neutral-grey/80 bg-surface text-navy hover:border-sage/60'
                "
                @click="selectedTypeId = rt.id"
              >
                <div class="truncate">{{ rt.label }}</div>
                <div class="text-[10px] opacity-80 font-normal truncate">{{ rt.badge }}</div>
              </button>
            </div>
          </div>
        </div>

        <!-- 2. Report Title -->
        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-navy mb-1.5">
            Report Document Title
          </label>
          <input
            v-model="title"
            type="text"
            placeholder="e.g. Champ - Clinical Progress Report"
            class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-2 text-sm text-navy outline-none focus:border-sage font-medium"
          />
        </div>

        <!-- 3. Clinical Summary & Narrative -->
        <div>
          <div class="flex items-center justify-between mb-1.5">
            <label class="text-xs font-bold uppercase tracking-wider text-navy">
              Clinical Findings & Progress Summary
            </label>
            <button
              type="button"
              class="text-[11px] font-semibold text-sage hover:text-navy inline-flex items-center gap-1"
              @click="applySmartDraft"
            >
              <Sparkles class="h-3 w-3" />
              Reset to Smart Draft
            </button>
          </div>
          <textarea
            v-model="summary"
            rows="4"
            placeholder="Enter clinical assessment, outcome trends, and patient response to therapy..."
            class="w-full rounded-lg border border-neutral-grey bg-surface p-3 text-xs leading-relaxed text-navy outline-none focus:border-sage font-normal"
          ></textarea>
        </div>

        <!-- 4. Discharge Summary Specific Fields -->
        <div v-if="selectedTypeId === 'discharge'" class="rounded-xl border border-sage/30 bg-surface p-4 space-y-4">
          <div class="flex items-center gap-2">
            <FileCheck2 class="h-4 w-4 text-sage" />
            <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Discharge & Outcome Specifics</h4>
          </div>

          <div>
            <label class="block text-[11px] font-bold text-neutral-muted mb-1">Discharge Status</label>
            <select
              v-model="dischargeStatus"
              class="w-full rounded-lg border border-neutral-grey bg-surface px-3 py-1.5 text-xs text-navy outline-none focus:border-sage"
            >
              <option value="Rehabilitation Goals Achieved — Discharged to Home Maintenance">
                Rehabilitation Goals Achieved — Discharged to Home Maintenance
              </option>
              <option value="Full Functional Recovery — Routine Annual Checkup">
                Full Functional Recovery — Routine Annual Checkup
              </option>
              <option value="Discharged with Chronic Maintenance Protocol">
                Discharged with Chronic Maintenance Protocol
              </option>
              <option value="Referred to Veterinary Surgeon for Further Evaluation">
                Referred to Veterinary Surgeon for Further Evaluation
              </option>
            </select>
          </div>

          <div>
            <label class="block text-[11px] font-bold text-neutral-muted mb-1">Long-Term Home Maintenance Plan</label>
            <textarea
              v-model="maintenancePlan"
              rows="3"
              class="w-full rounded-lg border border-neutral-grey bg-surface p-2.5 text-xs text-navy outline-none focus:border-sage"
            ></textarea>
          </div>

          <div>
            <label class="block text-[11px] font-bold text-neutral-muted mb-1">Instructions for Referring Veterinarian & Owner</label>
            <textarea
              v-model="veterinarianNotes"
              rows="2"
              class="w-full rounded-lg border border-neutral-grey bg-surface p-2.5 text-xs text-navy outline-none focus:border-sage"
            ></textarea>
          </div>
        </div>

        <!-- 5. Home Program Specific Guidelines -->
        <div v-else-if="selectedTypeId === 'home-program'" class="rounded-xl border border-sage/30 bg-surface p-4 space-y-3">
          <div class="flex items-center gap-2">
            <Activity class="h-4 w-4 text-sage" />
            <h4 class="text-xs font-bold uppercase tracking-wider text-navy">Prescribed Home Routine & Cues</h4>
          </div>
          <div>
            <label class="block text-[11px] font-bold text-neutral-muted mb-1">Prescribed Exercises & Technique Guidelines</label>
            <textarea
              v-model="maintenancePlan"
              rows="4"
              class="w-full rounded-lg border border-neutral-grey bg-surface p-2.5 text-xs text-navy outline-none focus:border-sage"
            ></textarea>
          </div>
        </div>

        <!-- 6. Share With Owner Checkbox -->
        <div class="rounded-xl border border-neutral-grey/80 bg-surface/50 p-3.5 flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600">
              <Share2 class="h-4 w-4" />
            </div>
            <div>
              <p class="text-xs font-bold text-navy">Publish to Pet Owner App & Documents</p>
              <p class="text-[11px] text-neutral-muted">Make this clinical summary immediately visible in the Owner Portal.</p>
            </div>
          </div>
          <label class="relative inline-flex cursor-pointer items-center">
            <input v-model="shareWithOwner" type="checkbox" class="peer sr-only" />
            <div class="peer h-5 w-9 rounded-full bg-neutral-grey/80 after:absolute after:left-[2px] after:top-[2px] after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-all after:content-[''] peer-checked:bg-sage peer-checked:after:translate-x-full peer-focus:outline-none"></div>
          </label>
        </div>
      </div>

      <!-- Footer Actions -->
      <div class="flex flex-wrap items-center justify-between gap-3 border-t border-neutral-grey/80 bg-surface/50 p-4">
        <BaseButton size="sm" variant="secondary" @click="emit('close')">
          Cancel
        </BaseButton>

        <div class="flex flex-wrap items-center gap-2">
          <BaseButton
            size="sm"
            variant="secondary"
            :loading="downloading"
            @click="handleQuickDownload"
          >
            <Download class="h-3.5 w-3.5" />
            Quick Download PDF
          </BaseButton>
          <BaseButton
            size="sm"
            variant="secondary"
            :loading="saving"
            @click="handleSaveOnly"
          >
            <Save class="h-3.5 w-3.5" />
            Save to Reports
          </BaseButton>
          <BaseButton
            size="sm"
            variant="accent"
            :loading="saving || downloading"
            @click="handleSaveAndDownload"
          >
            <FileText class="h-3.5 w-3.5" />
            Save & Download PDF
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

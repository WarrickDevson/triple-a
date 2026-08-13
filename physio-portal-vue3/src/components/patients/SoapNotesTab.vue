<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Plus, Download, ChevronDown, ChevronUp, FileText, CheckCircle2, MessageSquareQuote } from '@lucide/vue'
import type { SoapNote, OwnerSubjectiveNote } from '../../types/soap'
import { fetchSoapNotesByPet, createSoapNote, downloadSoapPdf, fetchOwnerSubjectiveNotes } from '../../api/soapNotes'
import CreateSoapNoteModal from './CreateSoapNoteModal.vue'

const props = defineProps<{
  petId: number
  petName: string
}>()

const notes = ref<SoapNote[]>([])
const ownerNotes = ref<OwnerSubjectiveNote[]>([])
const loading = ref(true)
const showCreateModal = ref(false)

const expandedNoteId = ref<number | null>(null)

function toggleExpand(id: number) {
  expandedNoteId.value = expandedNoteId.value === id ? null : id
}

async function loadNotes() {
  loading.value = true
  try {
    const [soapRes, ownerRes] = await Promise.all([
      fetchSoapNotesByPet(props.petId),
      fetchOwnerSubjectiveNotes(props.petId),
    ])
    notes.value = soapRes
    ownerNotes.value = ownerRes
    if (notes.value.length > 0 && expandedNoteId.value === null) {
      expandedNoteId.value = notes.value[0].soapNoteId
    }
  } finally {
    loading.value = false
  }
}

async function handleNoteCreated(payload: any) {
  try {
    const created = await createSoapNote(props.petId, payload)
    notes.value.unshift(created)
    expandedNoteId.value = created.soapNoteId
    showCreateModal.value = false
  } catch (err) {
    console.error('Failed to create SOAP note', err)
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString([], {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function handleDownloadPdf(soapNoteId: number) {
  downloadSoapPdf(soapNoteId)
}

onMounted(() => {
  loadNotes()
})
</script>

<template>
  <div class="space-y-4">
    <!-- Action Header Bar -->
    <div class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-neutral-grey/80 bg-surface p-4">
      <div>
        <h3 class="text-sm font-bold text-navy">SOAP Clinical Notes & Assessment Records</h3>
        <p class="text-xs text-neutral-muted">Subjective, Objective, Action, and Plan session history for {{ petName }}.</p>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-xl bg-sage px-4 py-2 text-xs font-bold text-white shadow-sm hover:bg-sage/90"
        @click="showCreateModal = true"
      >
        <Plus class="h-4 w-4" />
        New SOAP Note
      </button>
    </div>

    <!-- Submitted Owner Observations Panel -->
    <div class="rounded-xl border border-sage/40 bg-sage-muted/20 p-4">
      <div class="flex items-center justify-between">
        <h4 class="flex items-center gap-2 text-xs font-bold text-navy uppercase tracking-wider">
          <MessageSquareQuote class="h-4 w-4 text-sage" />
          Owner Submitted Home Observations ({{ ownerNotes.length }})
        </h4>
        <span class="text-[11px] font-semibold text-sage bg-sage/10 px-2 py-0.5 rounded-full">Submitted via Owner App</span>
      </div>

      <div v-if="ownerNotes.length === 0" class="mt-2.5 text-xs text-neutral-muted italic">
        No home observations or pre-session updates submitted by the owner for this patient yet.
      </div>

      <div v-else class="mt-3 space-y-2.5">
        <div
          v-for="on in ownerNotes"
          :key="on.ownerSubjectiveNoteId"
          class="rounded-xl border border-neutral-grey/80 bg-surface p-3 text-xs"
        >
          <div class="flex items-center justify-between gap-2">
            <span class="font-bold text-navy">{{ on.ownerName }}</span>
            <span class="text-[10px] text-neutral-muted">{{ new Date(on.noteDate).toLocaleString() }}</span>
          </div>
          <p class="mt-1 text-sm text-navy italic">"{{ on.notes }}"</p>
          <div class="mt-2 flex gap-3 text-[11px] text-neutral-muted">
            <span v-if="on.painObserved != null">Observed Pain: <strong class="text-navy">{{ on.painObserved }}/10</strong></span>
            <span v-if="on.energyObserved != null">Observed Energy: <strong class="text-navy">{{ on.energyObserved }}/10</strong></span>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="py-12 text-center text-sm text-neutral-muted">
      Loading clinical notes...
    </div>

    <!-- Empty State -->
    <div v-else-if="notes.length === 0" class="portal-card py-12 text-center">
      <FileText class="mx-auto h-12 w-12 text-neutral-muted/50" />
      <p class="mt-3 text-sm font-semibold text-navy">No SOAP Notes Recorded Yet</p>
      <p class="mt-1 text-xs text-neutral-muted">Click "New SOAP Note" above to document your first session assessment.</p>
    </div>

    <!-- Notes Timeline List -->
    <div v-else class="space-y-3">
      <div
        v-for="note in notes"
        :key="note.soapNoteId"
        class="portal-card overflow-hidden transition-all border border-neutral-grey/80"
      >
        <!-- Note Summary Header -->
        <div
          class="flex cursor-pointer flex-wrap items-center justify-between gap-3 bg-surface p-4 hover:bg-neutral-grey/20"
          @click="toggleExpand(note.soapNoteId)"
        >
          <div class="flex items-center gap-3">
            <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage font-bold">
              <FileText class="h-5 w-5" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <span class="text-sm font-bold text-navy">{{ formatDate(note.sessionDate) }}</span>
                <span v-if="note.isSharedWithOwner" class="inline-flex items-center gap-1 rounded-full bg-success-green/10 px-2 py-0.5 text-[10px] font-bold text-success-green">
                  <CheckCircle2 class="h-3 w-3" /> Shared with Owner
                </span>
              </div>
              <p class="text-xs text-neutral-muted">Clinician: {{ note.physioName }}</p>
            </div>
          </div>

          <div class="flex items-center gap-4">
            <!-- Score Badges -->
            <div class="flex gap-2">
              <span v-if="note.painScore != null" class="rounded-lg bg-neutral-grey/60 px-2.5 py-1 text-xs font-semibold text-navy">
                Pain: {{ note.painScore }}/10
              </span>
              <span v-if="note.stiffnessScore != null" class="rounded-lg bg-neutral-grey/60 px-2.5 py-1 text-xs font-semibold text-navy">
                Stiffness: {{ note.stiffnessScore }}/10
              </span>
              <span v-if="note.lamenessScore != null" class="rounded-lg bg-neutral-grey/60 px-2.5 py-1 text-xs font-semibold text-navy">
                Lameness: {{ note.lamenessScore }}/5
              </span>
            </div>

            <!-- PDF Download Button -->
            <button
              type="button"
              class="inline-flex items-center gap-1.5 rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 text-xs font-bold text-navy hover:bg-neutral-grey/40"
              title="Download PDF Report"
              @click.stop="handleDownloadPdf(note.soapNoteId)"
            >
              <Download class="h-3.5 w-3.5" />
              PDF Report
            </button>

            <!-- Expand Toggle -->
            <component
              :is="expandedNoteId === note.soapNoteId ? ChevronUp : ChevronDown"
              class="h-5 w-5 text-neutral-muted"
            />
          </div>
        </div>

        <!-- Expanded Note Detail Content -->
        <div v-if="expandedNoteId === note.soapNoteId" class="border-t border-neutral-grey/60 bg-neutral-grey/10 p-5 space-y-4">
          <!-- S - Subjective -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface p-4">
            <h4 class="flex items-center gap-2 text-xs font-bold text-navy uppercase tracking-wider">
              <span class="rounded bg-sage px-1.5 py-0.5 text-[10px] text-white">S</span>
              Subjective (Owner Observations & Feedback)
            </h4>
            <p class="mt-2 text-sm text-navy leading-relaxed">{{ note.subjective || 'No subjective observations noted.' }}</p>
          </div>

          <!-- O - Objective & Metrics -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface p-4">
            <h4 class="flex items-center gap-2 text-xs font-bold text-navy uppercase tracking-wider">
              <span class="rounded bg-sage px-1.5 py-0.5 text-[10px] text-white">O</span>
              Objective Examination & Clinical Ratings
            </h4>
            <p class="mt-2 text-sm text-navy leading-relaxed">{{ note.objective || 'No objective findings noted.' }}</p>

            <!-- Custom Metrics list if available -->
            <div v-if="note.customMetrics && note.customMetrics.length > 0" class="mt-3 grid gap-2 sm:grid-cols-3">
              <div
                v-for="m in note.customMetrics"
                :key="m.name"
                class="rounded-lg border border-neutral-grey/80 bg-neutral-grey/20 p-2 text-xs"
              >
                <p class="font-semibold text-navy">{{ m.name }}</p>
                <p class="text-sm font-bold text-sage">
                  {{ m.value }} <span class="text-xs font-normal text-neutral-muted">{{ m.unitOrDescriptor ?? '' }}</span>
                </p>
              </div>
            </div>
          </div>

          <!-- A - Action -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface p-4">
            <h4 class="flex items-center gap-2 text-xs font-bold text-navy uppercase tracking-wider">
              <span class="rounded bg-sage px-1.5 py-0.5 text-[10px] text-white">A</span>
              Action (Treatment Performed & In-Session Exercises)
            </h4>
            <p class="mt-2 text-sm text-navy leading-relaxed">{{ note.action || 'No treatment details recorded.' }}</p>
          </div>

          <!-- P - Plan -->
          <div class="rounded-xl border border-neutral-grey/80 bg-surface p-4">
            <h4 class="flex items-center gap-2 text-xs font-bold text-navy uppercase tracking-wider">
              <span class="rounded bg-sage px-1.5 py-0.5 text-[10px] text-white">P</span>
              Plan (Future Care & Recommendations)
            </h4>
            <p class="mt-2 text-sm text-navy leading-relaxed">{{ note.plan || 'No future plan recorded.' }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Create SOAP Modal -->
    <CreateSoapNoteModal
      :pet-id="petId"
      :pet-name="petName"
      :is-open="showCreateModal"
      @close="showCreateModal = false"
      @created="handleNoteCreated"
    />
  </div>
</template>

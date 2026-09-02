<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { Plus, Download, ChevronDown, ChevronUp, FileText, CheckCircle2, MessageSquareQuote, Pencil, Trash2, Mic, Share2, X, Loader2 } from '@lucide/vue'
import type { SoapNote, OwnerSubjectiveNote } from '../../types/soap'
import {
  fetchSoapNotesByPet,
  deleteSoapNote,
  downloadSoapPdf,
  fetchOwnerSubjectiveNotes,
  toggleSoapNoteShare,
  deleteOwnerSubjectiveNote,
  updateOwnerSubjectiveNote,
} from '../../api/soapNotes'
import BaseButton from '../BaseButton.vue'
import CreateSoapNoteModal from './CreateSoapNoteModal.vue'
import VoiceSoapDictationModal from '../soap/VoiceSoapDictationModal.vue'

const props = defineProps<{
  petId: number
  petName: string
}>()

const notes = ref<SoapNote[]>([])
const ownerNotes = ref<OwnerSubjectiveNote[]>([])
const loading = ref(true)
const showCreateModal = ref(false)
const showVoiceDictationModal = ref(false)
const editingNote = ref<SoapNote | null>(null)

const expandedNoteId = ref<number | null>(null)

function toggleExpand(id: number) {
  expandedNoteId.value = expandedNoteId.value === id ? null : id
}

function openCreateModal() {
  editingNote.value = null
  showCreateModal.value = true
}

function handleEditNote(note: SoapNote) {
  editingNote.value = note
  showCreateModal.value = true
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

function handleNoteCreated(created: any) {
  notes.value.unshift(created)
  expandedNoteId.value = created.soapNoteId
  showCreateModal.value = false
  editingNote.value = null
}

function handleNoteUpdated(soapNoteId: number, updated: any) {
  const idx = notes.value.findIndex(n => n.soapNoteId === soapNoteId)
  if (idx !== -1) {
    notes.value[idx] = updated
  }
  showCreateModal.value = false
  editingNote.value = null
}

async function handleDeleteNote(soapNoteId: number) {
  if (!confirm('Are you sure you want to delete this SOAP note record?')) return
  try {
    const success = await deleteSoapNote(soapNoteId)
    if (success) {
      notes.value = notes.value.filter(n => n.soapNoteId !== soapNoteId)
    } else {
      alert('Could not delete SOAP note.')
    }
  } catch (err) {
    console.error('Failed to delete SOAP note', err)
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

async function handleToggleShare(note: SoapNote) {
  const newShare = !note.isSharedWithOwner
  try {
    const updated = await toggleSoapNoteShare(note.soapNoteId, newShare)
    note.isSharedWithOwner = updated.isSharedWithOwner
    note.sharedAtUtc = updated.sharedAtUtc
  } catch {
    // Fallback toggle for local state
    note.isSharedWithOwner = newShare
    note.sharedAtUtc = newShare ? new Date().toISOString() : null
  }
}

function handleSoapNoteEvent(e: any) {
  if (!e.detail?.petId || e.detail.petId === props.petId) {
    loadNotes()
  }
}

onMounted(() => {
  loadNotes()
  window.addEventListener('soap-note-created', handleSoapNoteEvent)
})

onUnmounted(() => {
  window.removeEventListener('soap-note-created', handleSoapNoteEvent)
})

// Owner Note Actions
async function handleDeleteOwnerNote(noteId: number) {
  if (!confirm('Are you sure you want to delete this home observation note?')) return
  try {
    const ok = await deleteOwnerSubjectiveNote(noteId)
    if (ok) {
      ownerNotes.value = ownerNotes.value.filter((n) => n.ownerSubjectiveNoteId !== noteId)
    }
  } catch (err) {
    alert('Failed to delete owner note.')
  }
}

const activeEditOwnerNote = ref<OwnerSubjectiveNote | null>(null)
const editOwnerNotesText = ref('')
const editPainObserved = ref<number | null>(null)
const editEnergyObserved = ref<number | null>(null)
const isOwnerNoteSaving = ref(false)

function openEditOwnerNoteModal(note: OwnerSubjectiveNote) {
  activeEditOwnerNote.value = note
  editOwnerNotesText.value = note.notes
  editPainObserved.value = note.painObserved ?? null
  editEnergyObserved.value = note.energyObserved ?? null
}

function closeEditOwnerNoteModal() {
  activeEditOwnerNote.value = null
  editOwnerNotesText.value = ''
  editPainObserved.value = null
  editEnergyObserved.value = null
}

async function handleSaveEditOwnerNote() {
  if (!activeEditOwnerNote.value || !editOwnerNotesText.value.trim()) return
  isOwnerNoteSaving.value = true
  try {
    const updated = await updateOwnerSubjectiveNote(activeEditOwnerNote.value.ownerSubjectiveNoteId, {
      notes: editOwnerNotesText.value.trim(),
      painObserved: editPainObserved.value,
      energyObserved: editEnergyObserved.value,
    })
    const idx = ownerNotes.value.findIndex((n) => n.ownerSubjectiveNoteId === updated.ownerSubjectiveNoteId)
    if (idx !== -1) {
      ownerNotes.value[idx] = updated
    }
    closeEditOwnerNoteModal()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Failed to update owner note.')
  } finally {
    isOwnerNoteSaving.value = false
  }
}
</script>

<template>
  <div class="space-y-4">
    <!-- Action Header Bar -->
    <div class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-neutral-grey/80 bg-surface p-4">
      <div>
        <h3 class="text-sm font-bold text-navy">SOAP Clinical Notes & Assessment Records</h3>
        <p class="text-xs text-neutral-muted">Subjective, Objective, Action, and Plan session history for {{ petName }}.</p>
      </div>

      <div class="flex items-center gap-2">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl border border-purple-300 bg-purple-50 px-3.5 py-2 text-xs font-bold text-purple-700 hover:bg-purple-100 transition-all hover:scale-105 active:scale-95 shadow-xs"
          title="Dictate a full consultation session to auto-fill Subjective, Objective, Action, and Plan with AI"
          @click="showVoiceDictationModal = true"
        >
          <Mic class="h-4 w-4 text-purple-600 animate-pulse" />
          <span>Full SOAP Note</span>
        </button>

        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl bg-sage px-4 py-2 text-xs font-bold text-white shadow-sm hover:bg-sage/90"
          @click="openCreateModal"
        >
          <Plus class="h-4 w-4" />
          New SOAP Note
        </button>
      </div>
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
            <div class="flex items-center gap-2">
              <span class="text-[10px] text-neutral-muted">{{ new Date(on.noteDate).toLocaleString() }}</span>
              <button
                type="button"
                class="inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[11px] font-semibold text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
                title="Edit this note"
                @click="openEditOwnerNoteModal(on)"
              >
                <Pencil class="h-3 w-3 text-sage" />
                Edit
              </button>
              <button
                type="button"
                class="inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[11px] font-semibold text-alert-red/80 hover:bg-rose-50 hover:text-alert-red"
                title="Delete this note"
                @click="handleDeleteOwnerNote(on.ownerSubjectiveNoteId)"
              >
                <Trash2 class="h-3 w-3 text-rose-500" />
                Delete
              </button>
            </div>
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

            <!-- Action Buttons: Share, PDF Report, Edit, Delete -->
            <div class="flex items-center gap-1.5" @click.stop>
              <button
                type="button"
                class="inline-flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-xs font-bold transition-colors"
                :class="
                  note.isSharedWithOwner
                    ? 'border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100'
                    : 'border-neutral-grey/80 bg-surface text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy'
                "
                :title="note.isSharedWithOwner ? 'Shared with Owner (click to unshare)' : 'Click to share with Owner'"
                @click="handleToggleShare(note)"
              >
                <Share2 class="h-3.5 w-3.5" :class="note.isSharedWithOwner ? 'text-emerald-600' : 'text-neutral-muted'" />
                <span>{{ note.isSharedWithOwner ? 'Shared' : 'Share' }}</span>
              </button>

              <button
                type="button"
                class="inline-flex items-center gap-1.5 rounded-lg border border-neutral-grey/80 bg-surface px-2.5 py-1.5 text-xs font-bold text-navy hover:bg-neutral-grey/40"
                title="Download PDF Report"
                @click="handleDownloadPdf(note.soapNoteId)"
              >
                <Download class="h-3.5 w-3.5" />
                PDF
              </button>

              <button
                type="button"
                class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1.5 text-xs font-bold text-navy hover:bg-neutral-grey/40"
                title="Edit SOAP Note"
                @click="handleEditNote(note)"
              >
                <Pencil class="h-3.5 w-3.5 text-sage" />
                Edit
              </button>

              <button
                type="button"
                class="inline-flex items-center gap-1 rounded-lg border border-rose-200 bg-surface px-2 py-1.5 text-xs font-bold text-rose-600 hover:bg-rose-50"
                title="Delete SOAP Note"
                @click="handleDeleteNote(note.soapNoteId)"
              >
                <Trash2 class="h-3.5 w-3.5 text-rose-500" />
              </button>
            </div>

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

          <!-- Preserved Audio Memo & Verbatim Spoken Transcript -->
          <div v-if="note.audioUrl || note.rawTranscript" class="rounded-xl border border-purple-200 bg-purple-50/60 p-4 space-y-3">
            <div class="flex flex-wrap items-center justify-between gap-2">
              <h4 class="flex items-center gap-2 text-xs font-bold text-purple-900 uppercase tracking-wider">
                <Mic class="h-4 w-4 text-purple-600" />
                Preserved Consultation Voice Memo & Transcript
              </h4>
              <a
                v-if="note.audioUrl"
                :href="note.audioUrl"
                download="consultation-voice-memo.webm"
                class="inline-flex items-center gap-1 text-[11px] font-bold text-purple-700 hover:text-purple-900 hover:underline"
              >
                <Download class="h-3 w-3" />
                Download Audio (.webm)
              </a>
            </div>

            <div v-if="note.audioUrl" class="flex items-center gap-3">
              <audio :src="note.audioUrl" controls class="h-8 w-full max-w-md rounded-lg" />
            </div>

            <div v-if="note.rawTranscript">
              <p class="text-[11px] font-semibold text-purple-900/70 mb-1">Verbatim Spoken Transcript:</p>
              <p class="text-xs text-purple-950/80 bg-white/80 rounded-lg p-3 border border-purple-100 italic leading-relaxed whitespace-pre-wrap">
                "{{ note.rawTranscript }}"
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create / Edit SOAP Modal -->
    <CreateSoapNoteModal
      :pet-id="petId"
      :pet-name="petName"
      :is-open="showCreateModal"
      :editing-note="editingNote"
      @close="showCreateModal = false"
      @created="handleNoteCreated"
      @updated="handleNoteUpdated"
    />

    <!-- Standalone Quick Voice Dictation Modal -->
    <VoiceSoapDictationModal
      :is-open="showVoiceDictationModal"
      :pet-id="petId"
      :pet-name="petName"
      species="Canine"
      @close="showVoiceDictationModal = false"
    />

    <!-- Edit Owner Note Modal -->
    <div
      v-if="activeEditOwnerNote"
      class="fixed inset-0 z-60 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-xs"
      @click.self="closeEditOwnerNoteModal"
    >
      <div class="portal-card max-h-[90vh] w-full max-w-md overflow-y-auto bg-white p-5 shadow-xl rounded-2xl space-y-4">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-3">
          <h3 class="text-base font-bold text-navy">Edit Home Observation Note</h3>
          <button
            type="button"
            class="rounded-lg p-1.5 text-neutral-muted hover:bg-neutral-grey/40 hover:text-navy"
            @click="closeEditOwnerNoteModal"
          >
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="space-y-3 text-xs">
          <div>
            <label class="block font-bold text-navy mb-1">Observation Notes</label>
            <textarea
              v-model="editOwnerNotesText"
              rows="4"
              class="portal-input w-full"
              placeholder="Owner observations..."
            />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block font-bold text-navy mb-1">Pain Observed (0-10)</label>
              <input
                v-model.number="editPainObserved"
                type="number"
                min="0"
                max="10"
                class="portal-input w-full"
                placeholder="e.g. 2"
              />
            </div>
            <div>
              <label class="block font-bold text-navy mb-1">Energy Observed (1-10)</label>
              <input
                v-model.number="editEnergyObserved"
                type="number"
                min="1"
                max="10"
                class="portal-input w-full"
                placeholder="e.g. 7"
              />
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 border-t border-neutral-grey/80 pt-3">
          <button
            type="button"
            class="rounded-lg px-3 py-1.5 text-xs font-semibold text-neutral-muted hover:bg-neutral-grey/40 transition"
            @click="closeEditOwnerNoteModal"
          >
            Cancel
          </button>
          <BaseButton
            size="sm"
            variant="accent"
            :disabled="isOwnerNoteSaving || !editOwnerNotesText.trim()"
            @click="handleSaveEditOwnerNote"
          >
            <Loader2 v-if="isOwnerNoteSaving" class="h-3.5 w-3.5 animate-spin" />
            Save Changes
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

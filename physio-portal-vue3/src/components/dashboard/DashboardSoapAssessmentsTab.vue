<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  FileText,
  Plus,
  Download,
  Search,
  ChevronRight,
  PawPrint,
  Share2
} from '@lucide/vue'
import type { SoapNote } from '../../types/soap'
import { fetchSoapNotesByPet, downloadSoapPdf } from '../../api/soapNotes'
import { usePatientsStore } from '../../store/patients'
import CreateSoapNoteModal from '../patients/CreateSoapNoteModal.vue'

const patientsStore = usePatientsStore()

interface EnrichedSoapNote {
  note: SoapNote
  petId: number
  petName: string
  species: string
  breed: string
}

const allNotes = ref<EnrichedSoapNote[]>([])
const loading = ref(true)
const searchQuery = ref('')
const selectedSpeciesFilter = ref<string>('ALL')

// Note creation modal state
const showCreateModal = ref(false)
const selectedPetForNewNote = ref<{ petId: number; petName: string } | null>(null)
const showSelectPetModal = ref(false)

onMounted(async () => {
  await loadAllNotes()
})

async function loadAllNotes() {
  loading.value = true
  try {
    if (patientsStore.patients.length === 0) {
      await patientsStore.fetchClinicPatients()
    }
    const notesAccumulator: EnrichedSoapNote[] = []

    for (const pet of patientsStore.patients) {
      try {
        const notes = await fetchSoapNotesByPet(pet.petId)
        for (const n of notes) {
          notesAccumulator.push({
            note: n,
            petId: pet.petId,
            petName: pet.petName,
            species: pet.species || 'Canine',
            breed: pet.breed || 'Companion'
          })
        }
      } catch (e) {
        console.warn(`Failed to load SOAP notes for pet ${pet.petId}`, e)
      }
    }

    notesAccumulator.sort(
      (a, b) => new Date(b.note.sessionDate).getTime() - new Date(a.note.sessionDate).getTime()
    )
    allNotes.value = notesAccumulator
  } finally {
    loading.value = false
  }
}

const filteredNotes = computed(() => {
  return allNotes.value.filter((item) => {
    const matchesSearch =
      searchQuery.value === '' ||
      item.petName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.note.subjective?.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.note.objective?.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.note.action?.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.note.plan?.toLowerCase().includes(searchQuery.value.toLowerCase())

    const matchesSpecies =
      selectedSpeciesFilter.value === 'ALL' ||
      item.species.toLowerCase() === selectedSpeciesFilter.value.toLowerCase()

    return matchesSearch && matchesSpecies
  })
})

const totalAssessmentsCount = computed(() => allNotes.value.length)
const uniquePatientsCount = computed(() => new Set(allNotes.value.map((n) => n.petId)).size)

const avgPainScore = computed(() => {
  const notesWithPain = allNotes.value.filter((n) => n.note.painScore != null)
  if (notesWithPain.length === 0) return 0
  const sum = notesWithPain.reduce((acc, n) => acc + (n.note.painScore ?? 0), 0)
  return (sum / notesWithPain.length).toFixed(1)
})

function handleDownload(soapNoteId: number) {
  downloadSoapPdf(soapNoteId)
}

function handleStartNewNote(pet: { petId: number; petName: string }) {
  selectedPetForNewNote.value = pet
  showSelectPetModal.value = false
  showCreateModal.value = true
}

function handleNoteSaved() {
  showCreateModal.value = false
  loadAllNotes()
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header & Quick Action Bar -->
    <div class="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-neutral-grey/80 bg-surface p-5 shadow-xs">
      <div>
        <div class="flex items-center gap-2">
          <FileText class="h-6 w-6 text-sage" />
          <h2 class="text-lg font-bold text-navy">Clinical SOAP Assessment Records</h2>
        </div>
        <p class="text-xs text-neutral-muted">
          Access all patient Subjective, Objective, Action, and Plan logs with voice dictation.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl bg-sage px-4 py-2.5 text-xs font-bold text-white shadow-sm hover:bg-sage/90 transition-all hover:scale-105 active:scale-95"
          @click="showSelectPetModal = true"
        >
          <Plus class="h-4 w-4" />
          New SOAP Assessment
        </button>
      </div>
    </div>

    <!-- Quick Metrics Row -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="rounded-2xl border border-neutral-grey/80 bg-surface p-4 shadow-xs">
        <span class="text-xs font-semibold text-neutral-muted">Total Assessments Recorded</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="text-2xl font-bold text-navy">{{ totalAssessmentsCount }}</span>
          <span class="text-xs text-sage font-semibold">Consultations</span>
        </div>
      </div>

      <div class="rounded-2xl border border-neutral-grey/80 bg-surface p-4 shadow-xs">
        <span class="text-xs font-semibold text-neutral-muted">Patients Assessed</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="text-2xl font-bold text-navy">{{ uniquePatientsCount }}</span>
          <span class="text-xs text-neutral-muted">Unique Animals</span>
        </div>
      </div>

      <div class="rounded-2xl border border-neutral-grey/80 bg-surface p-4 shadow-xs">
        <span class="text-xs font-semibold text-neutral-muted">Average In-Clinic Pain Score</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="text-2xl font-bold text-amber-600">{{ avgPainScore }}</span>
          <span class="text-xs text-neutral-muted">/ 10 scale</span>
        </div>
      </div>
    </div>

    <!-- Search & Filter Controls -->
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div class="relative w-full max-w-sm">
        <Search class="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search by patient, symptoms, or plan..."
          class="w-full rounded-xl border border-neutral-grey/80 bg-surface pl-9 pr-4 py-2 text-xs text-navy focus:border-sage focus:outline-none shadow-xs"
        />
      </div>

      <div class="flex items-center gap-2">
        <span class="text-xs font-semibold text-neutral-muted">Species:</span>
        <select
          v-model="selectedSpeciesFilter"
          class="rounded-xl border border-neutral-grey/80 bg-surface px-3 py-1.5 text-xs text-navy focus:border-sage focus:outline-none shadow-xs"
        >
          <option value="ALL">All Species</option>
          <option value="Canine">Canine (Dogs)</option>
          <option value="Feline">Feline (Cats)</option>
          <option value="Equine">Equine (Horses)</option>
        </select>
      </div>
    </div>

    <!-- Assessments Feed -->
    <div v-if="loading" class="rounded-2xl border border-neutral-grey/80 bg-surface p-12 text-center text-xs text-neutral-muted">
      Loading clinic SOAP assessments...
    </div>

    <div
      v-else-if="filteredNotes.length === 0"
      class="rounded-2xl border border-dashed border-neutral-grey/80 bg-surface p-12 text-center"
    >
      <FileText class="mx-auto h-8 w-8 text-neutral-muted/50 mb-2" />
      <h3 class="text-sm font-bold text-navy">No SOAP Assessments Found</h3>
      <p class="text-xs text-neutral-muted mt-1">
        {{ searchQuery ? 'Try adjusting your search terms or filter.' : 'Record your first clinical assessment using voice dictation.' }}
      </p>
      <button
        type="button"
        class="mt-4 inline-flex items-center gap-1.5 rounded-xl bg-sage px-3.5 py-2 text-xs font-bold text-white hover:bg-sage/90"
        @click="showSelectPetModal = true"
      >
        <Plus class="h-4 w-4" />
        Start New Assessment
      </button>
    </div>

    <div v-else class="space-y-4">
      <div
        v-for="item in filteredNotes"
        :key="item.note.soapNoteId"
        class="rounded-2xl border border-neutral-grey/80 bg-surface p-5 shadow-xs transition-all hover:border-sage/50"
      >
        <!-- Card Header -->
        <div class="flex flex-wrap items-center justify-between gap-3 border-b border-neutral-grey/60 pb-3">
          <div class="flex items-center gap-3">
            <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-sage-muted text-sage font-bold">
              <PawPrint class="h-4 w-4" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <router-link
                  :to="{ name: 'patient-detail', params: { petId: item.petId } }"
                  class="text-sm font-bold text-navy hover:text-sage transition-colors"
                >
                  {{ item.petName }}
                </router-link>
                <span class="rounded-full bg-neutral-grey/60 px-2 py-0.5 text-[10px] font-semibold text-neutral-muted">
                  {{ item.species }} · {{ item.breed }}
                </span>
                <span
                  v-if="item.note.isSharedWithOwner"
                  class="inline-flex items-center gap-1 rounded-full bg-sage-muted/60 px-2 py-0.5 text-[10px] font-bold text-sage"
                >
                  <Share2 class="h-2.5 w-2.5" />
                  Shared with Owner
                </span>
              </div>
              <p class="text-[11px] text-neutral-muted">
                Session Date: {{ new Date(item.note.sessionDate).toLocaleDateString() }}
              </p>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <button
              type="button"
              class="inline-flex items-center gap-1.5 rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 px-3 py-1.5 text-xs font-bold text-navy hover:bg-neutral-grey/50 transition-colors"
              @click="handleDownload(item.note.soapNoteId)"
            >
              <Download class="h-3.5 w-3.5 text-sage" />
              Download PDF Report
            </button>
          </div>
        </div>

        <!-- Scores & Clinical Outcome Measures -->
        <div class="mt-3 flex flex-wrap gap-2 text-xs">
          <span
            v-if="item.note.painScore != null"
            class="rounded-lg bg-rose-50 border border-rose-200 px-2.5 py-1 font-bold text-rose-700"
          >
            Pain: {{ item.note.painScore }}/10
          </span>
          <span
            v-if="item.note.stiffnessScore != null"
            class="rounded-lg bg-amber-50 border border-amber-200 px-2.5 py-1 font-bold text-amber-700"
          >
            Stiffness: {{ item.note.stiffnessScore }}/10
          </span>
          <span
            v-if="item.note.lamenessScore != null"
            class="rounded-lg bg-sky-50 border border-sky-200 px-2.5 py-1 font-bold text-sky-700"
          >
            Lameness: {{ item.note.lamenessScore }}/5
          </span>
          <span
            v-for="(metric, mIdx) in item.note.customMetrics || []"
            :key="mIdx"
            class="rounded-lg bg-neutral-grey/40 px-2.5 py-1 font-semibold text-navy"
          >
            {{ metric.name }}: {{ metric.value }} {{ metric.unitOrDescriptor }}
          </span>
        </div>

        <!-- SOAP 4-Quadrant Preview Grid -->
        <div class="mt-4 grid gap-3 sm:grid-cols-2 text-xs">
          <div class="rounded-xl border border-neutral-grey/60 bg-neutral-grey/10 p-3">
            <div class="flex items-center gap-1.5 font-bold text-navy mb-1">
              <span class="rounded bg-sage-muted px-1.5 py-0.5 text-[10px] text-sage">S</span>
              Subjective
            </div>
            <p class="text-neutral-muted line-clamp-3">
              {{ item.note.subjective || 'No subjective observations noted.' }}
            </p>
          </div>

          <div class="rounded-xl border border-neutral-grey/60 bg-neutral-grey/10 p-3">
            <div class="flex items-center gap-1.5 font-bold text-navy mb-1">
              <span class="rounded bg-sage-muted px-1.5 py-0.5 text-[10px] text-sage">O</span>
              Objective
            </div>
            <p class="text-neutral-muted line-clamp-3">
              {{ item.note.objective || 'No objective findings recorded.' }}
            </p>
          </div>

          <div class="rounded-xl border border-neutral-grey/60 bg-neutral-grey/10 p-3">
            <div class="flex items-center gap-1.5 font-bold text-navy mb-1">
              <span class="rounded bg-sage-muted px-1.5 py-0.5 text-[10px] text-sage">A</span>
              Action & Treatment
            </div>
            <p class="text-neutral-muted line-clamp-3">
              {{ item.note.action || 'No session treatments recorded.' }}
            </p>
          </div>

          <div class="rounded-xl border border-neutral-grey/60 bg-neutral-grey/10 p-3">
            <div class="flex items-center gap-1.5 font-bold text-navy mb-1">
              <span class="rounded bg-sage-muted px-1.5 py-0.5 text-[10px] text-sage">P</span>
              Plan & Follow-up
            </div>
            <p class="text-neutral-muted line-clamp-3">
              {{ item.note.plan || 'No follow-up plan scheduled.' }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Select Patient Modal to Start SOAP Note -->
    <div
      v-if="showSelectPetModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
    >
      <div class="w-full max-w-md rounded-2xl bg-surface p-6 shadow-2xl space-y-4">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-3">
          <h3 class="text-base font-bold text-navy">Select Patient for SOAP Note</h3>
          <button
            type="button"
            class="rounded-lg p-1 text-neutral-muted hover:bg-neutral-grey/50"
            @click="showSelectPetModal = false"
          >
            ✕
          </button>
        </div>

        <p class="text-xs text-neutral-muted">
          Choose which patient to document clinical assessment notes for:
        </p>

        <ul class="max-h-64 divide-y divide-neutral-grey/60 overflow-y-auto">
          <li
            v-for="pet in patientsStore.patients"
            :key="pet.petId"
            class="flex items-center justify-between py-2.5 px-2 hover:bg-neutral-grey/30 rounded-xl cursor-pointer transition-colors"
            @click="handleStartNewNote({ petId: pet.petId, petName: pet.petName })"
          >
            <div class="flex items-center gap-2.5">
              <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-sage-muted text-sage font-bold text-xs">
                <PawPrint class="h-4 w-4" />
              </div>
              <div>
                <p class="text-xs font-bold text-navy">{{ pet.petName }}</p>
                <p class="text-[11px] text-neutral-muted">{{ pet.species }} · {{ pet.breed || 'Companion' }}</p>
              </div>
            </div>
            <ChevronRight class="h-4 w-4 text-neutral-muted" />
          </li>
        </ul>
      </div>
    </div>

    <!-- Create SOAP Note Modal -->
    <CreateSoapNoteModal
      v-if="selectedPetForNewNote"
      :is-open="showCreateModal"
      :pet-id="selectedPetForNewNote.petId"
      :pet-name="selectedPetForNewNote.petName"
      @close="showCreateModal = false"
      @created="handleNoteSaved"
    />
  </div>
</template>

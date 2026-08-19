<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { FileText, Download } from '@lucide/vue'
import type { SoapNote } from '../../types/soap'
import { fetchSoapNotesByPet, downloadSoapPdf } from '../../api/soapNotes'
import { usePatientsStore } from '../../store/patients'

const patientsStore = usePatientsStore()
const recentSoapNotes = ref<{ note: SoapNote; petName: string }[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    if (patientsStore.patients.length === 0) {
      await patientsStore.fetchClinicPatients()
    }
    const allNotes: { note: SoapNote; petName: string }[] = []
    
    for (const pet of patientsStore.patients.slice(0, 5)) {
      const notes = await fetchSoapNotesByPet(pet.petId)
      for (const n of notes) {
        allNotes.push({ note: n, petName: pet.petName })
      }
    }
    
    allNotes.sort((a, b) => new Date(b.note.sessionDate).getTime() - new Date(a.note.sessionDate).getTime())
    recentSoapNotes.value = allNotes.slice(0, 4)
  } catch (err) {
    console.error('Failed to load dashboard SOAP notes', err)
  } finally {
    loading.value = false
  }
})

function handleDownload(soapNoteId: number) {
  downloadSoapPdf(soapNoteId)
}
</script>

<template>
  <section class="portal-card p-5">
    <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-3">
      <div class="flex items-center gap-2">
        <FileText class="h-5 w-5 text-sage" />
        <h2 class="portal-card-title">Recent SOAP Assessments</h2>
      </div>
      <span class="rounded-full bg-sage-muted px-2.5 py-0.5 text-xs font-bold text-sage">
        Clinical Log
      </span>
    </div>

    <div v-if="loading" class="py-8 text-center text-xs text-neutral-muted">
      Loading recent SOAP assessments...
    </div>

    <div v-else-if="recentSoapNotes.length === 0" class="py-8 text-center text-xs text-neutral-muted">
      No recent SOAP assessments recorded yet.
    </div>

    <ul v-else class="mt-4 space-y-3">
      <li
        v-for="item in recentSoapNotes"
        :key="item.note.soapNoteId"
        class="rounded-xl border border-neutral-grey/80 bg-neutral-grey/20 p-3 text-xs space-y-2"
      >
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="font-bold text-navy text-sm">{{ item.petName }}</span>
            <span class="text-neutral-muted">· {{ new Date(item.note.sessionDate).toLocaleDateString() }}</span>
          </div>
          <button
            type="button"
            class="inline-flex items-center gap-1 rounded-lg border border-neutral-grey/80 bg-surface px-2 py-1 text-[11px] font-bold text-navy hover:bg-neutral-grey/40"
            @click="handleDownload(item.note.soapNoteId)"
          >
            <Download class="h-3 w-3" />
            PDF
          </button>
        </div>

        <div class="flex gap-2">
          <span v-if="item.note.painScore != null" class="rounded bg-surface px-2 py-0.5 font-semibold text-navy">
            Pain: {{ item.note.painScore }}/10
          </span>
          <span v-if="item.note.stiffnessScore != null" class="rounded bg-surface px-2 py-0.5 font-semibold text-navy">
            Stiffness: {{ item.note.stiffnessScore }}/10
          </span>
          <span v-if="item.note.lamenessScore != null" class="rounded bg-surface px-2 py-0.5 font-semibold text-navy">
            Lameness: {{ item.note.lamenessScore }}/5
          </span>
        </div>

        <p class="text-neutral-muted line-clamp-2 italic">
          "{{ item.note.action || item.note.subjective || 'Session completed.' }}"
        </p>
      </li>
    </ul>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Clock, Calendar, Plus, Trash2 } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patients: Pet[]
}>()

const emit = defineEmits<{
  scheduleWaitlistPatient: [petId: number, preferredNotes?: string]
}>()

interface WaitlistEntry {
  id: number
  petId: number
  petName: string
  ownerName: string
  ownerPhone: string
  requestedDate: string
  urgency: 'Urgent' | 'Standard' | 'Flexible'
  preferredSlot: string
  notes: string
}

const initialWaitlist: WaitlistEntry[] = [
  {
    id: 101,
    petId: 2,
    petName: 'Maverick',
    ownerName: 'Emma van der Berg',
    ownerPhone: '+27 82 345 6789',
    requestedDate: '2026-08-12',
    urgency: 'Urgent',
    preferredSlot: 'Weekday Mornings (8am - 11am)',
    notes: 'Post-op hydrotherapy follow-up required ASAP after stitches removal.',
  },
  {
    id: 102,
    petId: 3,
    petName: 'Rocky',
    ownerName: 'James Cooper',
    ownerPhone: '+27 83 987 6543',
    requestedDate: '2026-08-11',
    urgency: 'Standard',
    preferredSlot: 'Any Afternoon (2pm - 5pm)',
    notes: 'Needs gait reassessment and laser therapy session.',
  },
  {
    id: 103,
    petId: 1,
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    ownerPhone: '+27 84 123 4567',
    requestedDate: '2026-08-10',
    urgency: 'Flexible',
    preferredSlot: 'Saturdays preferred',
    notes: 'Maintenance massage & passive stretching routine check.',
  },
]

const waitlist = ref<WaitlistEntry[]>(initialWaitlist)
const showAddModal = ref(false)

const newEntry = ref({
  petId: props.patients[0]?.petId || 1,
  urgency: 'Standard' as WaitlistEntry['urgency'],
  preferredSlot: 'Mornings (8am - 12pm)',
  notes: '',
})

function removeFromWaitlist(id: number) {
  waitlist.value = waitlist.value.filter((item) => item.id !== id)
}

function handleAddWaitlist() {
  const selectedPet = props.patients.find((p) => p.petId === Number(newEntry.value.petId))
  if (!selectedPet) return

  waitlist.value.unshift({
    id: Date.now(),
    petId: selectedPet.petId,
    petName: selectedPet.petName,
    ownerName: selectedPet.ownerName,
    ownerPhone: (selectedPet as any).ownerPhone || '+27 80 000 0000',
    requestedDate: new Date().toISOString().slice(0, 10),
    urgency: newEntry.value.urgency,
    preferredSlot: newEntry.value.preferredSlot,
    notes: newEntry.value.notes.trim() || 'Waitlist entry recorded by clinician.',
  })

  newEntry.value.notes = ''
  showAddModal.value = false
}

function getUrgencyBadge(urgency: WaitlistEntry['urgency']) {
  switch (urgency) {
    case 'Urgent':
      return 'bg-red-50 text-red-700 border-red-200'
    case 'Standard':
      return 'bg-blue-50 text-blue-700 border-blue-200'
    case 'Flexible':
      return 'bg-gray-50 text-gray-700 border-gray-200'
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-sm font-bold text-navy">Priority Waitlist Queue</h3>
        <p class="text-xs text-neutral-muted">
          {{ waitlist.length }} patients waiting for cancellation / open appointment slots
        </p>
      </div>

      <BaseButton size="sm" @click="showAddModal = true">
        <Plus class="h-4 w-4" />
        Add to Waitlist
      </BaseButton>
    </div>

    <div v-if="waitlist.length === 0" class="empty-state py-12">
      <p class="text-sm text-neutral-muted">No patients currently on the waitlist.</p>
    </div>

    <div v-else class="space-y-3">
      <div
        v-for="item in waitlist"
        :key="item.id"
        class="portal-card p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4 border border-neutral-grey/60 hover:border-sage/40 transition-all shadow-sm"
      >
        <div class="space-y-1.5 min-w-0 flex-1">
          <div class="flex items-center gap-2">
            <span
              class="rounded-md border px-2 py-0.5 text-[10px] font-extrabold uppercase tracking-wider"
              :class="getUrgencyBadge(item.urgency)"
            >
              {{ item.urgency }}
            </span>
            <h4 class="text-sm font-bold text-navy">{{ item.petName }}</h4>
            <span class="text-xs text-neutral-muted">({{ item.ownerName }})</span>
          </div>

          <div class="flex items-center gap-4 text-xs text-neutral-muted">
            <span class="flex items-center gap-1">
              <Clock class="h-3.5 w-3.5 text-sage" />
              {{ item.preferredSlot }}
            </span>
            <span class="flex items-center gap-1">
              <Calendar class="h-3.5 w-3.5 text-sage" />
              Requested {{ item.requestedDate }}
            </span>
          </div>

          <p class="text-xs text-navy/90 italic">"{{ item.notes }}"</p>
        </div>

        <div class="flex items-center gap-2 shrink-0">
          <BaseButton size="sm" @click="emit('scheduleWaitlistPatient', item.petId, item.notes)">
            <Calendar class="h-3.5 w-3.5" />
            Schedule Now
          </BaseButton>
          <button
            type="button"
            class="p-2 text-neutral-muted hover:text-alert-red rounded-lg hover:bg-surface transition-colors"
            title="Remove from waitlist"
            @click="removeFromWaitlist(item.id)"
          >
            <Trash2 class="h-4 w-4" />
          </button>
        </div>
      </div>
    </div>

    <!-- Add Waitlist Modal -->
    <div
      v-if="showAddModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
      @click.self="showAddModal = false"
    >
      <div class="portal-card w-full max-w-md p-6 shadow-xl">
        <h3 class="text-lg font-bold text-navy">Add Patient to Waitlist</h3>
        <p class="text-xs text-neutral-muted mt-0.5">Queue a patient for preferred scheduling openings</p>

        <form class="mt-4 space-y-4" @submit.prevent="handleAddWaitlist">
          <label class="block">
            <span class="text-xs font-semibold uppercase tracking-wider text-navy">Select Patient</span>
            <select
              v-model="newEntry.petId"
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm bg-white focus:border-sage"
            >
              <option v-for="p in patients" :key="p.petId" :value="p.petId">
                {{ p.petName }} ({{ p.ownerName }})
              </option>
            </select>
          </label>

          <div class="grid grid-cols-2 gap-3">
            <label class="block">
              <span class="text-xs font-semibold uppercase tracking-wider text-navy">Urgency</span>
              <select
                v-model="newEntry.urgency"
                class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm bg-white focus:border-sage"
              >
                <option value="Urgent">Urgent</option>
                <option value="Standard">Standard</option>
                <option value="Flexible">Flexible</option>
              </select>
            </label>

            <label class="block">
              <span class="text-xs font-semibold uppercase tracking-wider text-navy">Preferred Time</span>
              <input
                v-model="newEntry.preferredSlot"
                required
                class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage"
                placeholder="e.g. Mornings (8am - 12pm)"
              />
            </label>
          </div>

          <label class="block">
            <span class="text-xs font-semibold uppercase tracking-wider text-navy">Clinical Notes / Priority Reason</span>
            <textarea
              v-model="newEntry.notes"
              rows="3"
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage"
              placeholder="e.g. Needs immediate post-surgery review if slot opens up..."
            ></textarea>
          </label>

          <div class="flex gap-3 pt-2">
            <BaseButton type="button" variant="secondary" class="flex-1" @click="showAddModal = false">
              Cancel
            </BaseButton>
            <BaseButton type="submit" class="flex-1">Add to Waitlist</BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

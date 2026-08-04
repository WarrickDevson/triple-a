<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import BaseButton from '../BaseButton.vue'
import BaseInput from '../BaseInput.vue'
import { useAppointmentsStore } from '../../store/appointments'
import { usePatientsStore } from '../../store/patients'

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  close: []
  created: []
}>()

const appointmentsStore = useAppointmentsStore()
const patientsStore = usePatientsStore()

const saving = ref(false)
const form = reactive({
  petId: '',
  date: '',
  time: '09:00',
  clientNotes: '',
  clinicianNotes: '',
})

const patientOptions = computed(() => patientsStore.patients)

async function onSubmit() {
  if (!form.petId || !form.date) return
  saving.value = true
  try {
    const scheduledDateTime = new Date(`${form.date}T${form.time}`).toISOString()
    await appointmentsStore.scheduleAppointment({
      petId: Number(form.petId),
      scheduledDateTime,
      clientNotes: form.clientNotes || undefined,
      clinicianNotes: form.clinicianNotes || undefined,
    })
    emit('created')
    emit('close')
    form.petId = ''
    form.date = ''
    form.time = '09:00'
    form.clientNotes = ''
    form.clinicianNotes = ''
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div
    v-if="open"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
    @click.self="emit('close')"
  >
    <div class="portal-card w-full max-w-md p-6">
      <h3 class="text-lg font-bold text-navy">New Appointment</h3>
      <form class="mt-5 space-y-4" @submit.prevent="onSubmit">
        <label class="flex flex-col gap-1.5">
          <span class="text-sm font-medium text-navy">Patient</span>
          <select
            v-model="form.petId"
            required
            class="min-h-11 rounded-lg border border-neutral-grey bg-surface px-3 text-sm text-navy outline-none focus:border-sage"
          >
            <option value="" disabled>Select patient</option>
            <option v-for="pet in patientOptions" :key="pet.petId" :value="pet.petId">
              {{ pet.petName }} ({{ pet.ownerName }})
            </option>
          </select>
        </label>
        <div class="grid grid-cols-2 gap-3">
          <label class="flex flex-col gap-1.5">
            <span class="text-sm font-medium text-navy">Date</span>
            <input
              v-model="form.date"
              type="date"
              required
              class="min-h-11 rounded-lg border border-neutral-grey bg-surface px-3 text-sm outline-none focus:border-sage"
            />
          </label>
          <label class="flex flex-col gap-1.5">
            <span class="text-sm font-medium text-navy">Time</span>
            <input
              v-model="form.time"
              type="time"
              required
              class="min-h-11 rounded-lg border border-neutral-grey bg-surface px-3 text-sm outline-none focus:border-sage"
            />
          </label>
        </div>
        <BaseInput v-model="form.clinicianNotes" label="Clinician notes" multiline />
        <BaseInput v-model="form.clientNotes" label="Client notes" multiline />

        <div class="flex gap-3 pt-2">
          <BaseButton type="button" variant="secondary" class="flex-1" @click="emit('close')">
            Cancel
          </BaseButton>
          <BaseButton type="submit" class="flex-1" :disabled="saving">
            {{ saving ? 'Saving...' : 'Create' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

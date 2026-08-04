<script setup lang="ts">
import { computed } from 'vue'
import { ChevronLeft, ChevronRight } from '@lucide/vue'
import { APPOINTMENT_TYPES } from '../../data/appointmentDemo'

const props = defineProps<{
  selectedDate: Date
  showCancelled: boolean
  showCompleted: boolean
}>()

const emit = defineEmits<{
  'update:selectedDate': [value: Date]
  'update:showCancelled': [value: boolean]
  'update:showCompleted': [value: boolean]
}>()

const monthLabel = computed(() =>
  props.selectedDate.toLocaleDateString([], { month: 'long', year: 'numeric' }),
)

const calendarDays = computed(() => {
  const year = props.selectedDate.getFullYear()
  const month = props.selectedDate.getMonth()
  const firstDay = new Date(year, month, 1)
  const startOffset = (firstDay.getDay() + 6) % 7
  const daysInMonth = new Date(year, month + 1, 0).getDate()

  const days: Array<{ date: Date | null; label: string }> = []
  for (let i = 0; i < startOffset; i++) days.push({ date: null, label: '' })
  for (let d = 1; d <= daysInMonth; d++) {
    days.push({ date: new Date(year, month, d), label: String(d) })
  }
  return days
})

function isSameDay(a: Date, b: Date) {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  )
}

function shiftMonth(delta: number) {
  const next = new Date(props.selectedDate)
  next.setMonth(next.getMonth() + delta)
  emit('update:selectedDate', next)
}

function selectDay(date: Date) {
  emit('update:selectedDate', date)
}
</script>

<template>
  <section class="portal-card p-4">
    <div class="flex items-center justify-between">
      <button type="button" class="rounded-lg p-1 text-neutral-muted hover:bg-surface" @click="shiftMonth(-1)">
        <ChevronLeft class="h-4 w-4" />
      </button>
      <p class="text-sm font-bold text-navy">{{ monthLabel }}</p>
      <button type="button" class="rounded-lg p-1 text-neutral-muted hover:bg-surface" @click="shiftMonth(1)">
        <ChevronRight class="h-4 w-4" />
      </button>
    </div>

    <div class="mt-3 grid grid-cols-7 gap-1 text-center text-[10px] font-semibold text-neutral-muted">
      <span v-for="d in ['M', 'T', 'W', 'T', 'F', 'S', 'S']" :key="d">{{ d }}</span>
    </div>
    <div class="mt-1 grid grid-cols-7 gap-1">
      <button
        v-for="(day, index) in calendarDays"
        :key="index"
        type="button"
        class="flex h-8 items-center justify-center rounded-lg text-xs"
        :class="
          day.date && isSameDay(day.date, selectedDate)
            ? 'bg-sage font-semibold text-white'
            : day.date
              ? 'text-navy hover:bg-surface'
              : ''
        "
        :disabled="!day.date"
        @click="day.date && selectDay(day.date)"
      >
        {{ day.label }}
      </button>
    </div>

    <div class="mt-4 space-y-2 border-t border-neutral-grey/80 pt-4">
      <label class="flex items-center gap-2 text-xs text-neutral-muted">
        <input
          type="checkbox"
          class="rounded border-neutral-grey text-sage"
          :checked="showCancelled"
          @change="emit('update:showCancelled', ($event.target as HTMLInputElement).checked)"
        />
        Show Cancellations
      </label>
      <label class="flex items-center gap-2 text-xs text-neutral-muted">
        <input
          type="checkbox"
          class="rounded border-neutral-grey text-sage"
          :checked="showCompleted"
          @change="emit('update:showCompleted', ($event.target as HTMLInputElement).checked)"
        />
        Show Completed
      </label>
      <label class="flex items-center gap-2 text-xs text-neutral-muted">
        <input type="checkbox" class="rounded border-neutral-grey text-sage" />
        Show Waitlist
      </label>
    </div>

    <div class="mt-4 border-t border-neutral-grey/80 pt-4">
      <p class="text-xs font-bold text-navy">Appointment Types</p>
      <ul class="mt-2 space-y-1.5">
        <li
          v-for="type in APPOINTMENT_TYPES"
          :key="type.label"
          class="flex items-center gap-2 text-[11px] text-neutral-muted"
        >
          <span class="h-2.5 w-2.5 rounded-full" :style="{ backgroundColor: type.color }" />
          {{ type.label }}
        </li>
      </ul>
    </div>
  </section>
</template>

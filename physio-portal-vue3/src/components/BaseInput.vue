<script setup lang="ts">
import { computed, ref } from 'vue'
import { LockKeyhole, UserRound, type LucideIcon } from '@lucide/vue'

const props = defineProps<{
  modelValue: string
  label: string
  id?: string
  type?: string
  placeholder?: string
  required?: boolean
  multiline?: boolean
  hint?: string
  icon?: 'UserRound' | 'LockKeyhole'
  autocomplete?: string
  inputmode?: 'url' | 'email' | 'search' | 'text' | 'none' | 'tel' | 'numeric' | 'decimal'
  maxlength?: number | string
}>()

defineEmits<{
  'update:modelValue': [value: string]
}>()

const iconMap: Record<string, LucideIcon> = {
  UserRound,
  LockKeyhole,
}

const inputIcon = computed(() => (props.icon ? iconMap[props.icon] : null))

const passwordVisible = ref(false)
const isPassword = computed(() => props.type === 'password')
const inputType = computed(() => {
  if (!isPassword.value) return props.type ?? 'text'
  return passwordVisible.value ? 'text' : 'password'
})
</script>

<template>
  <label class="flex flex-col gap-1.5">
    <span class="text-sm font-medium text-navy">{{ label }}</span>
    <textarea
      v-if="multiline"
      :id="id"
      :value="modelValue"
      :placeholder="placeholder"
      :required="required"
      rows="4"
      class="rounded-xl border border-neutral-grey bg-surface px-4 py-3 text-base text-neutral-dark outline-none transition-colors placeholder:text-neutral-muted/60 focus:border-sage focus:ring-2 focus:ring-sage/20"
      @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />
    <div v-else class="relative">
      <component
        :is="inputIcon"
        v-if="inputIcon"
        class="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted"
        :stroke-width="1.75"
      />
      <input
        :id="id"
        :type="inputType"
        :value="modelValue"
        :placeholder="placeholder"
        :required="required"
        :autocomplete="autocomplete"
        :inputmode="inputmode"
        :maxlength="maxlength"
        class="min-h-11 w-full rounded-xl border border-neutral-grey bg-surface text-base text-neutral-dark outline-none transition-colors placeholder:text-neutral-muted/60 focus:border-sage focus:ring-2 focus:ring-sage/20"
        :class="[inputIcon ? 'pl-10 pr-4' : 'px-4', isPassword ? 'pr-12' : '']"
        @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      />
      <button
        v-if="isPassword"
        type="button"
        class="absolute inset-y-0 right-0 flex cursor-pointer items-center px-3 text-neutral-muted transition-colors hover:text-sage"
        :aria-label="passwordVisible ? 'Hide password' : 'Show password'"
        @click="passwordVisible = !passwordVisible"
      >
        <svg
          v-if="!passwordVisible"
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
          class="h-5 w-5"
          aria-hidden="true"
        >
          <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z" />
          <circle cx="12" cy="12" r="3" />
        </svg>
        <svg
          v-else
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
          class="h-5 w-5"
          aria-hidden="true"
        >
          <path d="M9.88 9.88a3 3 0 1 0 4.24 4.24" />
          <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c6.5 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68" />
          <path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3.5 7 10 7a9.74 9.74 0 0 0 5.39-1.61" />
          <line x1="2" x2="22" y1="2" y2="22" />
        </svg>
      </button>
    </div>
    <p v-if="hint" class="text-xs text-neutral-muted">{{ hint }}</p>
  </label>
</template>

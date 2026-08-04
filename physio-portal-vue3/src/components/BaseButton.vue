<script setup lang="ts">
defineProps<{
  variant?: 'primary' | 'secondary' | 'accent' | 'ghost' | 'danger' | 'onDark'
  size?: 'md' | 'sm'
  type?: 'button' | 'submit'
  disabled?: boolean
}>()
</script>

<template>
  <button
    :type="type ?? 'button'"
    :disabled="disabled"
    class="inline-flex cursor-pointer items-center justify-center gap-2 rounded-xl text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-60"
    :class="[
      size === 'sm' ? 'min-h-9 px-4 text-xs' : 'min-h-11 px-6',
      variant === 'secondary'
        ? 'border border-navy/15 bg-white text-navy hover:border-navy/30 hover:bg-navy/5'
        : variant === 'accent'
          ? 'bg-sage text-white hover:bg-sage-light'
          : variant === 'ghost'
            ? 'border border-transparent bg-transparent text-neutral-muted hover:bg-navy/5 hover:text-navy'
            : variant === 'danger'
              ? 'bg-alert-red text-white hover:bg-alert-red/90'
              : variant === 'onDark'
                ? 'border border-white/25 bg-white/10 text-white hover:bg-white/18'
                : 'bg-navy text-white hover:bg-navy-light',
    ]"
  >
    <slot />
  </button>
</template>

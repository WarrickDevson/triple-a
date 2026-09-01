<script setup lang="ts">
import { Download } from '@lucide/vue'
import { usePwaInstall } from '../composables/usePwaInstall'

withDefaults(
  defineProps<{
    compact?: boolean
  }>(),
  {
    compact: false,
  }
)

const { isInstallable, isInstalled, promptInstall } = usePwaInstall()
</script>

<template>
  <div v-if="isInstallable && !isInstalled" :class="compact ? 'inline-flex' : 'w-full'">
    <button
      v-if="compact"
      type="button"
      class="flex items-center gap-1.5 rounded-full bg-sage/20 px-2.5 py-1 text-xs font-semibold text-sage-light hover:bg-sage/30 hover:text-white transition-colors"
      title="Install App"
      @click="promptInstall"
    >
      <Download class="h-3.5 w-3.5" />
      <span>Install</span>
    </button>
    <button
      v-else
      type="button"
      class="flex w-full items-center justify-center gap-2 rounded-lg bg-sage/25 px-3 py-2 text-xs font-semibold text-sage-light hover:bg-sage/35 hover:text-white transition-colors"
      @click="promptInstall"
    >
      <Download class="h-3.5 w-3.5" />
      <span>Install App</span>
    </button>
  </div>
</template>

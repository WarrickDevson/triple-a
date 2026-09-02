<script setup lang="ts">
import { Download, X } from '@lucide/vue'
import { usePwaInstall } from '../composables/usePwaInstall'
import logoUrl from '../assets/brand/triple-a-logo.png'

const { isInstalled, isDismissed, dismissBanner, promptInstall } = usePwaInstall()

function dismiss() {
  dismissBanner()
}

async function handleInstallClick() {
  await promptInstall()
}
</script>

<template>
  <div>
    <!-- Prominent Fixed Bottom Install Banner -->
    <div
      v-if="!isInstalled && !isDismissed"
      id="pwa-install-banner"
      style="position: fixed; bottom: 0; left: 0; right: 0; z-index: 99999; display: flex;"
      class="p-3 sm:p-4 bg-navy text-white shadow-2xl border-t border-white/20 items-center justify-between gap-3 sm:max-w-md sm:bottom-4 sm:left-4 sm:right-auto sm:rounded-2xl sm:border"
    >
      <div class="flex items-center gap-3 min-w-0">
        <img
          :src="logoUrl"
          alt="Triple A"
          class="h-11 w-11 shrink-0 rounded-xl bg-white/10 object-contain p-1 border border-white/10 shadow-sm"
        />
        <div class="min-w-0">
          <p class="text-xs sm:text-sm font-bold text-white truncate">Install Triple A Portal</p>
          <p class="text-[11px] text-white/70 truncate">Add to home screen for 1-tap access</p>
        </div>
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <button
          type="button"
          class="flex items-center gap-1.5 rounded-xl bg-sage px-3.5 py-2 text-xs font-bold text-white shadow hover:bg-sage-light active:scale-95 transition-all"
          @click="handleInstallClick"
        >
          <Download class="h-4 w-4" :stroke-width="2.2" />
          <span>Install</span>
        </button>
        <button
          type="button"
          class="rounded-lg p-1.5 text-white/60 hover:text-white hover:bg-white/10 transition-colors"
          title="Dismiss"
          aria-label="Dismiss banner"
          @click="dismiss"
        >
          <X class="h-4 w-4" />
        </button>
      </div>
    </div>
  </div>
</template>

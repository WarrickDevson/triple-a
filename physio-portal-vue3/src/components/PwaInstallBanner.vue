<script setup lang="ts">
import { ref } from 'vue'
import { Download, X, Share, PlusSquare } from '@lucide/vue'
import { usePwaInstall } from '../composables/usePwaInstall'
import logoUrl from '../assets/brand/triple-a-logo.png'

const { isInstalled, isIOS, showInstructionsModal, promptInstall } = usePwaInstall()
const dismissed = ref(false)

function dismiss() {
  dismissed.value = true
}

async function handleInstallClick() {
  await promptInstall()
}
</script>

<template>
  <div>
    <!-- Prominent Fixed Bottom Install Banner -->
    <div
      v-if="!isInstalled && !dismissed"
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

    <!-- Visual Install Guide (for iOS / fallback) -->
    <Teleport to="body">
      <div
        v-if="showInstructionsModal"
        class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-sm"
        @click.self="showInstructionsModal = false"
      >
        <div class="relative w-full max-w-sm rounded-2xl border border-navy/10 bg-white p-6 shadow-2xl">
          <button
            type="button"
            class="absolute top-4 right-4 rounded-lg p-1 text-neutral-muted hover:bg-surface hover:text-navy"
            @click="showInstructionsModal = false"
          >
            <X class="h-5 w-5" />
          </button>

          <div class="flex items-center gap-3 mb-4">
            <img
              :src="logoUrl"
              alt="Triple A"
              class="h-10 w-10 shrink-0 rounded-xl bg-surface object-contain p-1 border border-navy/10"
            />
            <div>
              <h3 class="text-sm font-bold text-navy">Install Triple A</h3>
              <p class="text-xs text-neutral-muted">Add to your Home Screen</p>
            </div>
          </div>

          <!-- iOS Instructions -->
          <ol v-if="isIOS" class="space-y-3 text-xs text-neutral-dark">
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">1</span>
              <span>Tap the <strong class="text-navy">Share</strong> button <Share class="inline h-3.5 w-3.5 text-navy" /> at the bottom of Safari.</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">2</span>
              <span>Scroll down and tap <strong class="text-navy">Add to Home Screen</strong> <PlusSquare class="inline h-3.5 w-3.5 text-navy" />.</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">3</span>
              <span>Tap <strong class="text-navy">Add</strong> in the top-right corner.</span>
            </li>
          </ol>

          <!-- Android / Chrome / Other Instructions -->
          <ol v-else class="space-y-3 text-xs text-neutral-dark">
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">1</span>
              <span>Tap your browser menu (<strong class="text-navy">⋮</strong> three dots).</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">2</span>
              <span>Tap <strong class="text-navy">Install app</strong> or <strong class="text-navy">Add to Home screen</strong>.</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">3</span>
              <span>Confirm <strong class="text-navy">Install</strong>.</span>
            </li>
          </ol>

          <button
            type="button"
            class="mt-6 w-full rounded-xl bg-navy py-2.5 text-xs font-semibold text-white hover:bg-navy-light"
            @click="showInstructionsModal = false"
          >
            Got it
          </button>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { Download, Share, PlusSquare, X } from '@lucide/vue'
import { usePwaInstall } from '../composables/usePwaInstall'

withDefaults(
  defineProps<{
    compact?: boolean
  }>(),
  {
    compact: false,
  }
)

const { isInstallable, isInstalled, isIOS, showInstructionsModal, promptInstall } = usePwaInstall()
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

    <!-- Install Instructions Modal -->
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
            <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-sage/20 text-sage">
              <Download class="h-5 w-5" />
            </div>
            <div>
              <h3 class="text-sm font-bold text-navy">Install Triple A</h3>
              <p class="text-xs text-neutral-muted">Add to your Home Screen</p>
            </div>
          </div>

          <!-- iOS Instructions -->
          <ol v-if="isIOS" class="space-y-3 text-xs text-neutral-dark">
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">1</span>
              <span>Tap the <strong class="text-navy">Share</strong> button <Share class="inline h-3.5 w-3.5 text-navy" /> in Safari.</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">2</span>
              <span>Scroll down and tap <strong class="text-navy">Add to Home Screen</strong> <PlusSquare class="inline h-3.5 w-3.5 text-navy" />.</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">3</span>
              <span>Tap <strong class="text-navy">Add</strong> in the top right corner.</span>
            </li>
          </ol>

          <!-- Android / Chrome / Edge Instructions -->
          <ol v-else class="space-y-3 text-xs text-neutral-dark">
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">1</span>
              <span>Tap your browser menu (<strong class="text-navy">⋮</strong> or three dots).</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">2</span>
              <span>Select <strong class="text-navy">Install app</strong> or <strong class="text-navy">Add to Home screen</strong>.</span>
            </li>
            <li class="flex items-start gap-2.5">
              <span class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface font-bold text-navy">3</span>
              <span>Follow the prompt to confirm.</span>
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

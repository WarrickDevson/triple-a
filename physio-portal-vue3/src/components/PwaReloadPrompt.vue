<script setup lang="ts">
import { ref } from 'vue'
import { RefreshCw, X } from '@lucide/vue'
import { useRegisterSW } from 'virtual:pwa-register/vue'

const isDismissed = ref(false)

const {
  offlineReady,
  needRefresh,
  updateServiceWorker,
} = useRegisterSW({
  immediate: true,
  onRegistered(r) {
    console.debug('PWA Service Worker registered:', r)
  },
  onRegisterError(error) {
    console.warn('PWA Service Worker registration error:', error)
  },
})

function close() {
  offlineReady.value = false
  needRefresh.value = false
  isDismissed.value = true
}

async function handleUpdate() {
  await updateServiceWorker(true)
}
</script>

<template>
  <Transition
    enter-active-class="transition duration-300 ease-out transform"
    enter-from-class="translate-y-4 opacity-0 sm:translate-y-0 sm:translate-x-4"
    enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
    leave-active-class="transition duration-200 ease-in transform"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="(needRefresh || offlineReady) && !isDismissed"
      class="fixed bottom-6 left-6 z-50 flex max-w-sm items-center gap-3 rounded-2xl border border-navy/10 bg-white p-4 shadow-2xl ring-1 ring-black/5"
      role="alert"
    >
      <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-sage/15 text-sage">
        <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': needRefresh }" />
      </div>

      <div class="min-w-0 flex-1">
        <p class="text-xs font-bold text-navy">
          {{ needRefresh ? 'New version available!' : 'App ready for offline use' }}
        </p>
        <p class="text-[11px] text-neutral-muted mt-0.5">
          {{ needRefresh ? 'Click reload to update to the latest features.' : 'Cached content is available offline.' }}
        </p>
      </div>

      <div class="flex items-center gap-1.5 shrink-0">
        <button
          v-if="needRefresh"
          type="button"
          class="rounded-lg bg-navy px-2.5 py-1.5 text-xs font-semibold text-white hover:bg-navy-light transition-colors"
          @click="handleUpdate"
        >
          Reload
        </button>
        <button
          type="button"
          class="rounded-lg p-1 text-neutral-muted hover:bg-surface hover:text-navy transition-colors"
          title="Dismiss"
          @click="close"
        >
          <X class="h-4 w-4" />
        </button>
      </div>
    </div>
  </Transition>
</template>

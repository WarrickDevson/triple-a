<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { Download, RotateCcw, X, ZoomIn, ZoomOut } from '@lucide/vue'

const props = defineProps<{
  imageUrl: string
  title: string
  subtitle?: string | null
}>()

const emit = defineEmits<{
  close: []
}>()

const zoom = ref(1)

function zoomIn() {
  if (zoom.value < 4) zoom.value = Math.min(4, +(zoom.value + 0.25).toFixed(2))
}

function zoomOut() {
  if (zoom.value > 0.5) zoom.value = Math.max(0.5, +(zoom.value - 0.25).toFixed(2))
}

function resetZoom() {
  zoom.value = 1
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    emit('close')
  }
}

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeydown)
})
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/90 p-4 backdrop-blur-md transition-opacity"
    @click.self="emit('close')"
  >
    <div class="relative flex h-full max-h-[92vh] w-full max-w-4xl flex-col overflow-hidden rounded-2xl bg-neutral-dark/95 shadow-2xl ring-1 ring-white/10">
      <!-- Top Bar -->
      <div class="flex items-center justify-between border-b border-white/10 bg-black/40 px-5 py-3 text-white">
        <div class="min-w-0 flex-1 pr-4">
          <h3 class="truncate text-base font-bold">{{ title }}</h3>
          <p v-if="subtitle" class="truncate text-xs text-white/70">{{ subtitle }}</p>
        </div>

        <!-- Controls -->
        <div class="flex items-center gap-2">
          <div class="hidden items-center gap-1 rounded-lg bg-white/10 p-1 sm:flex">
            <button
              type="button"
              class="rounded p-1 text-white/80 hover:bg-white/10 hover:text-white transition"
              title="Zoom out"
              @click="zoomOut"
            >
              <ZoomOut class="h-4 w-4" />
            </button>
            <span class="px-1 text-xs font-mono text-white/70">{{ Math.round(zoom * 100) }}%</span>
            <button
              type="button"
              class="rounded p-1 text-white/80 hover:bg-white/10 hover:text-white transition"
              title="Zoom in"
              @click="zoomIn"
            >
              <ZoomIn class="h-4 w-4" />
            </button>
            <button
              v-if="zoom !== 1"
              type="button"
              class="rounded p-1 text-white/80 hover:bg-white/10 hover:text-white transition"
              title="Reset zoom"
              @click="resetZoom"
            >
              <RotateCcw class="h-3.5 w-3.5" />
            </button>
          </div>

          <a
            :href="imageUrl"
            target="_blank"
            rel="noopener noreferrer"
            download
            class="rounded-lg bg-white/10 p-2 text-white/80 transition hover:bg-white/20 hover:text-white"
            title="Download full size"
          >
            <Download class="h-4 w-4" />
          </a>

          <button
            type="button"
            class="rounded-lg bg-white/10 p-2 text-white/80 transition hover:bg-white/20 hover:text-white"
            title="Close (Esc)"
            @click="emit('close')"
          >
            <X class="h-5 w-5" />
          </button>
        </div>
      </div>

      <!-- Image Area -->
      <div
        class="relative flex flex-1 items-center justify-center overflow-auto p-4 select-none"
        @click.self="emit('close')"
      >
        <img
          :src="imageUrl"
          :alt="title"
          class="max-h-[75vh] w-auto max-w-full rounded-lg object-contain transition-transform duration-200 shadow-lg"
          :style="{ transform: `scale(${zoom})` }"
        />
      </div>

      <!-- Bottom Status -->
      <div class="border-t border-white/10 bg-black/40 px-4 py-2 text-center text-[11px] text-white/50">
        Click outside or press Esc to close · Use zoom controls or wheel to inspect
      </div>
    </div>
  </div>
</template>

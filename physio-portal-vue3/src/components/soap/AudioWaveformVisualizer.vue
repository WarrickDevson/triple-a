<script setup lang="ts">
import { computed } from 'vue'
import type { RecordingState } from '../../types/soap'
import { Mic, Pause, Play, Square, AlertCircle, Sparkles } from '@lucide/vue'

const props = defineProps<{
  recordingState: RecordingState
  formattedTime: string
  audioLevel: number
  frequencies: number[]
  errorMessage?: string
}>()

const emit = defineEmits<{
  pause: []
  resume: []
  stop: []
  start: []
}>()

const isRecording = computed(() => props.recordingState === 'recording')
const isPaused = computed(() => props.recordingState === 'paused')
</script>

<template>
  <div class="rounded-2xl border border-neutral-grey/80 bg-surface p-4 shadow-sm transition-all">
    <!-- Header: Status & Timer -->
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-2.5">
        <div
          class="relative flex h-8 w-8 items-center justify-center rounded-xl transition-all"
          :class="
            isRecording
              ? 'bg-rose-500 text-white shadow-lg shadow-rose-500/20'
              : isPaused
                ? 'bg-amber-500 text-white'
                : 'bg-sage-muted text-sage'
          "
        >
          <Mic class="h-4 w-4" :class="{ 'animate-pulse': isRecording }" />
          <span
            v-if="isRecording"
            class="absolute -top-0.5 -right-0.5 h-2.5 w-2.5 rounded-full bg-rose-500 ring-2 ring-white animate-ping"
          />
        </div>

        <div>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-navy">
              {{
                isRecording
                  ? 'Listening & Recording...'
                  : isPaused
                    ? 'Recording Paused'
                    : recordingState === 'processing'
                      ? 'Processing Audio...'
                      : recordingState === 'completed'
                        ? 'Audio Recorded'
                        : 'Ready to Dictate'
              }}
            </span>
            <span
              v-if="isRecording"
              class="rounded-full bg-rose-100 px-2 py-0.5 text-[10px] font-bold text-rose-700 uppercase tracking-wider"
            >
              LIVE MIC
            </span>
          </div>
          <p class="text-[11px] text-neutral-muted">Hands-free clinical speech recognition & AI structuring</p>
        </div>
      </div>

      <!-- Timer & Audio Level Indicator -->
      <div class="flex items-center gap-3">
        <div class="text-right">
          <span class="font-mono text-base font-bold text-navy">{{ formattedTime }}</span>
          <p class="text-[10px] text-neutral-muted">Duration</p>
        </div>
      </div>
    </div>

    <!-- Error Alert if any -->
    <div
      v-if="errorMessage"
      class="mt-3 flex items-center gap-2 rounded-xl bg-amber-50 border border-amber-200 p-2.5 text-xs text-amber-800"
    >
      <AlertCircle class="h-4 w-4 shrink-0 text-amber-600" />
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Animated Waveform Bars Container -->
    <div class="relative mt-4 flex h-20 items-center justify-center gap-1 overflow-hidden rounded-xl bg-neutral-grey/25 px-4 py-2">
      <!-- Background Grid lines for visual depth -->
      <div class="absolute inset-0 flex flex-col justify-between opacity-15 pointer-events-none p-2">
        <div class="border-b border-navy/20 w-full" />
        <div class="border-b border-navy/20 w-full" />
        <div class="border-b border-navy/20 w-full" />
      </div>

      <!-- Frequency Equalizer Bars -->
      <div
        v-for="(freq, idx) in frequencies"
        :key="idx"
        class="w-1.5 rounded-full transition-all duration-75"
        :class="
          isRecording
            ? freq > 60
              ? 'bg-rose-500'
              : freq > 35
                ? 'bg-sage'
                : 'bg-sage/50'
            : isPaused
              ? 'bg-amber-400'
              : 'bg-neutral-grey/70'
        "
        :style="{
          height: isRecording || isPaused ? `${Math.max(12, freq)}%` : '15%',
          transform: isRecording ? `scaleY(${Math.max(0.2, freq / 100)})` : 'scaleY(0.2)'
        }"
      />
    </div>

    <!-- Live Volume Level Bar -->
    <div v-if="isRecording" class="mt-2 flex items-center gap-2">
      <span class="text-[10px] font-semibold text-neutral-muted">Mic Volume:</span>
      <div class="h-1.5 flex-1 rounded-full bg-neutral-grey/60 overflow-hidden">
        <div
          class="h-full rounded-full transition-all duration-75"
          :class="audioLevel > 70 ? 'bg-rose-500' : 'bg-sage'"
          :style="{ width: `${audioLevel}%` }"
        />
      </div>
      <span class="text-[10px] font-mono text-neutral-muted w-7 text-right">{{ audioLevel }}%</span>
    </div>

    <!-- Quick Audio Action Controls -->
    <div class="mt-3 flex items-center justify-between border-t border-neutral-grey/60 pt-3">
      <div class="flex items-center gap-2">
        <button
          v-if="isRecording"
          type="button"
          class="inline-flex items-center gap-1.5 rounded-xl border border-neutral-grey/80 bg-surface px-3 py-1.5 text-xs font-semibold text-navy hover:bg-neutral-grey/40"
          @click="emit('pause')"
        >
          <Pause class="h-3.5 w-3.5" />
          Pause
        </button>

        <button
          v-if="isPaused"
          type="button"
          class="inline-flex items-center gap-1.5 rounded-xl bg-amber-500 px-3 py-1.5 text-xs font-bold text-white shadow-sm hover:bg-amber-600"
          @click="emit('resume')"
        >
          <Play class="h-3.5 w-3.5 fill-white" />
          Resume
        </button>

        <button
          v-if="isRecording || isPaused"
          type="button"
          class="inline-flex items-center gap-1.5 rounded-xl bg-rose-600 px-3.5 py-1.5 text-xs font-bold text-white shadow-sm hover:bg-rose-700"
          @click="emit('stop')"
        >
          <Square class="h-3.5 w-3.5 fill-white" />
          Finish Dictation
        </button>
      </div>

      <div class="text-[11px] text-neutral-muted flex items-center gap-1">
        <Sparkles class="h-3 w-3 text-sage" />
        <span>Veterinary STT lexicon active</span>
      </div>
    </div>
  </div>
</template>

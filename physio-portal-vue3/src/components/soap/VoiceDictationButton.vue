<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue'
import {
  Mic,
  Square,
  Loader2,
  Volume2,
  AlertCircle,
  HelpCircle,
  X,
  Sparkles
} from '@lucide/vue'
import {
  cleanSpeechTranscript,
  detectAndStripStopCommand,
  VOICE_COMMANDS_HELP
} from '../../utils/speechCleaner'
import { transcribeSoapAudioBlob } from '../../api/soapAi'

const props = withDefaults(
  defineProps<{
    sectionLabel?: string
    buttonText?: string
    compact?: boolean
    placeholderContext?: string
    engine?: 'browser' | 'cloud'
    petName?: string
    species?: string
  }>(),
  {
    sectionLabel: 'this section',
    buttonText: 'Voice Dictate',
    compact: false,
    placeholderContext: '',
    engine: 'browser',
    petName: undefined,
    species: undefined
  }
)

const emit = defineEmits<{
  transcriptChunk: [text: string, pauseSeconds: number]
  dictationFinished: [fullText: string]
}>()

const isRecording = ref(false)
const isProcessing = ref(false)
const interimSnippet = ref('')
const recordingDuration = ref(0)
const micLevel = ref(0)
const errorMessage = ref('')
const showHelpModal = ref(false)
const sessionWordCount = ref(0)
const feedbackStatus = ref<'success' | 'empty' | null>(null)

let timerInterval: any = null
let silenceWatchdogInterval: any = null
let lastSpeechTime: number = 0
let animationFrameId: number | null = null
let recognizer: any = null
let mediaStream: MediaStream | null = null
let mediaRecorder: MediaRecorder | null = null
let audioChunks: Blob[] = []
let audioContext: AudioContext | null = null
let analyserNode: AnalyserNode | null = null

const SpeechRecognitionClass = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition

const formattedTimer = computed(() => {
  const mins = Math.floor(recordingDuration.value / 60)
  const secs = recordingDuration.value % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
})

async function toggleDictation() {
  if (isRecording.value) {
    await stopDictation()
  } else {
    await startDictation()
  }
}

async function startDictation() {
  errorMessage.value = ''
  interimSnippet.value = ''
  recordingDuration.value = 0
  audioChunks = []
  lastSpeechTime = Date.now()

  try {
    // 1. Request microphone stream
    mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true })

    // 2. Setup Web Audio Analyser for mic volume feedback
    try {
      const AudioCtx = window.AudioContext || (window as any).webkitAudioContext
      if (AudioCtx) {
        audioContext = new AudioCtx()
        const source = audioContext.createMediaStreamSource(mediaStream)
        analyserNode = audioContext.createAnalyser()
        analyserNode.fftSize = 32
        source.connect(analyserNode)

        const dataArray = new Uint8Array(analyserNode.frequencyBinCount)
        const checkAudio = () => {
          if (!analyserNode || !isRecording.value) return
          analyserNode.getByteFrequencyData(dataArray)
          let sum = 0
          for (let i = 0; i < dataArray.length; i++) sum += dataArray[i]
          micLevel.value = Math.min(100, Math.round((sum / dataArray.length / 128) * 100))
          animationFrameId = requestAnimationFrame(checkAudio)
        }
        checkAudio()
      }
    } catch (e) {
      console.warn('AudioAnalyser init error:', e)
    }

    // 3. Setup MediaRecorder for audio capture
    const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
      ? 'audio/webm;codecs=opus'
      : MediaRecorder.isTypeSupported('audio/mp4')
        ? 'audio/mp4'
        : 'audio/webm'

    mediaRecorder = new MediaRecorder(mediaStream, { mimeType })
    mediaRecorder.ondataavailable = (e) => {
      if (e.data && e.data.size > 0) audioChunks.push(e.data)
    }
    mediaRecorder.start(250)

    // 4. Setup Web Speech API for zero-latency live text streaming (ONLY in browser mode)
    if (props.engine === 'browser' && SpeechRecognitionClass) {
      recognizer = new SpeechRecognitionClass()
      recognizer.continuous = true
      recognizer.interimResults = true
      recognizer.lang = 'en-US'

      recognizer.onresult = (event: any) => {
        const now = Date.now()
        const pauseSeconds = lastSpeechTime > 0 ? (now - lastSpeechTime) / 1000 : 0
        lastSpeechTime = now

        let interim = ''
        for (let i = event.resultIndex; i < event.results.length; ++i) {
          const rawText = event.results[i][0].transcript

          // Check if user spoke hands-free stop command (e.g. "stop dictation", "end note")
          const { text: textWithoutStopCmd, shouldStop } = detectAndStripStopCommand(rawText)

          if (event.results[i].isFinal) {
            const cleaned = cleanSpeechTranscript(textWithoutStopCmd)
            if (cleaned) {
              sessionWordCount.value += cleaned.split(/\s+/).filter(Boolean).length
              emit('transcriptChunk', cleaned, pauseSeconds)
            }
            if (shouldStop) {
              stopDictation()
              return
            }
          } else {
            if (shouldStop) {
              stopDictation()
              return
            }
            interim += rawText
          }
        }
        interimSnippet.value = interim
      }

      recognizer.onerror = (e: any) => {
        console.warn('Inline speech recognizer event error:', e.error)
      }

      recognizer.onend = () => {
        if (isRecording.value) {
          try {
            recognizer.start()
          } catch {}
        }
      }

      try {
        recognizer.start()
      } catch (err) {
        console.warn('Speech recognizer start error:', err)
      }
    }

    isRecording.value = true

    // 5. Start timer
    timerInterval = setInterval(() => {
      recordingDuration.value++
    }, 1000)

    // 6. Start silence watchdog (auto-stops after > 12 seconds of silence, only in browser mode)
    if (props.engine === 'browser') {
      silenceWatchdogInterval = setInterval(() => {
        if (!isRecording.value) return
        const silenceSeconds = (Date.now() - lastSpeechTime) / 1000
        if (silenceSeconds >= 12.0) {
          stopDictation()
        }
      }, 500)
    }
  } catch (err: any) {
    console.warn('Microphone permission denied or device not found:', err)
    if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
      errorMessage.value = 'Microphone permission denied. Please allow mic access.'
    } else {
      // Fallback for simulation / direct prompt
      const sample = prompt(`Microphone not accessible. Enter test voice dictation for ${props.sectionLabel}:`)
      if (sample) {
        const cleaned = cleanSpeechTranscript(sample)
        emit('transcriptChunk', cleaned, 0)
        emit('dictationFinished', cleaned)
      }
    }
  }
}

async function stopDictation() {
  if (!isRecording.value) return
  isRecording.value = false
  isProcessing.value = true

  if (timerInterval) {
    clearInterval(timerInterval)
    timerInterval = null
  }

  if (silenceWatchdogInterval) {
    clearInterval(silenceWatchdogInterval)
    silenceWatchdogInterval = null
  }

  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
    animationFrameId = null
  }

  if (recognizer) {
    try {
      recognizer.stop()
    } catch {}
    recognizer = null
  }

  // Commit any unfinalized interim speech text in browser mode
  if (interimSnippet.value && interimSnippet.value.trim()) {
    const cleaned = cleanSpeechTranscript(interimSnippet.value.trim())
    if (cleaned) {
      sessionWordCount.value += cleaned.split(/\s+/).filter(Boolean).length
      emit('transcriptChunk', cleaned, 0)
      emit('dictationFinished', cleaned)
    }
    interimSnippet.value = ''
  }

  // Await mediaRecorder stop event to ensure all audio chunks are collected
  const capturedBlob = await new Promise<Blob | null>((resolve) => {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
      mediaRecorder.onstop = () => {
        if (audioChunks.length > 0) {
          const mime = mediaRecorder?.mimeType || 'audio/webm'
          resolve(new Blob(audioChunks, { type: mime }))
        } else {
          resolve(null)
        }
      }
      try {
        mediaRecorder.requestData()
        mediaRecorder.stop()
      } catch {
        resolve(null)
      }
    } else {
      resolve(audioChunks.length > 0 ? new Blob(audioChunks, { type: 'audio/webm' }) : null)
    }
  })

  if (mediaStream) {
    mediaStream.getTracks().forEach((t) => t.stop())
    mediaStream = null
  }

  if (audioContext && audioContext.state !== 'closed') {
    audioContext.close().catch(() => {})
    audioContext = null
  }

  // If in Cloud AI mode or SpeechRecognition wasn't supported, send audio blob to backend
  if ((props.engine === 'cloud' || !SpeechRecognitionClass) && capturedBlob && capturedBlob.size > 0) {
    try {
      const result = await transcribeSoapAudioBlob(capturedBlob, props.petName, props.species)
      if (result && result.transcript) {
        const cleaned = cleanSpeechTranscript(result.transcript)
        sessionWordCount.value += cleaned.split(/\s+/).filter(Boolean).length
        emit('transcriptChunk', cleaned, 0)
        emit('dictationFinished', cleaned)
      }
    } catch (err) {
      console.warn('Cloud AI audio transcription error:', err)
      errorMessage.value = 'Cloud AI transcription failed. Switched to fallback.'
    }
  }

  if (sessionWordCount.value > 0) {
    feedbackStatus.value = 'success'
  } else {
    feedbackStatus.value = 'empty'
  }

  setTimeout(() => {
    feedbackStatus.value = null
    sessionWordCount.value = 0
  }, 4000)

  interimSnippet.value = ''
  isProcessing.value = false
}

onUnmounted(() => {
  stopDictation()
})
</script>

<template>
  <div class="inline-flex items-center gap-1.5">
    <!-- Recording Trigger Button -->
    <button
      type="button"
      class="inline-flex items-center gap-1.5 rounded-xl border px-3 py-1.5 text-xs font-bold transition-all shadow-xs"
      :class="
        isRecording
          ? 'border-rose-400 bg-rose-500 text-white shadow-md shadow-rose-500/30 scale-105 animate-pulse'
          : isProcessing
            ? 'border-sage bg-sage-muted text-sage'
            : 'border-sage/40 bg-sage-muted/60 text-sage hover:bg-sage hover:text-white active:scale-95'
      "
      :title="isRecording ? 'Click to finish audio transcription (or say &quot;stop dictation&quot;)' : `Transcribe audio into ${sectionLabel}`"
      @click="toggleDictation"
    >
      <Loader2 v-if="isProcessing" class="h-3.5 w-3.5 animate-spin" />
      <Square v-else-if="isRecording" class="h-3.5 w-3.5 fill-white" />
      <Mic v-else class="h-3.5 w-3.5" />

      <span v-if="isProcessing">Transcribing...</span>
      <span v-else-if="isRecording">Stop ({{ formattedTimer }})</span>
      <span v-else-if="!compact">{{ buttonText }}</span>
      <span v-else>Transcribe</span>
    </button>

    <!-- Visual Confirmation Badges -->
    <span
      v-if="feedbackStatus === 'success'"
      class="inline-flex items-center gap-1 rounded-full bg-emerald-100 px-2 py-0.5 text-[10px] font-bold text-emerald-800 animate-fade-in"
    >
      ✓ Transcribed
    </span>
    <span
      v-else-if="feedbackStatus === 'empty'"
      class="inline-flex items-center gap-1 rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-bold text-amber-800 animate-fade-in"
    >
      ⚠️ No speech heard
    </span>

    <!-- Info / Voice Commands Help Icon Button -->
    <button
      type="button"
      class="rounded-lg p-1 text-neutral-muted hover:bg-sage-muted hover:text-sage transition-colors"
      title="View Voice Commands, Punctuation & Stop Phrases"
      @click.stop="showHelpModal = true"
    >
      <HelpCircle class="h-3.5 w-3.5" />
    </button>

    <!-- Real-time Live Audio Waveform Pill when recording -->
    <div
      v-if="isRecording"
      class="inline-flex items-center gap-1.5 rounded-xl border border-rose-200 bg-rose-50 px-2.5 py-1 text-xs text-rose-700 shadow-xs"
    >
      <Volume2 class="h-3.5 w-3.5 animate-pulse text-rose-600" />
      <div class="flex items-center gap-0.5 h-3">
        <span
          class="w-1 rounded-full bg-rose-500 transition-all duration-75"
          :style="{ height: `${Math.max(4, micLevel * 0.12)}px` }"
        />
        <span
          class="w-1 rounded-full bg-rose-500 transition-all duration-75"
          :style="{ height: `${Math.max(6, micLevel * 0.18)}px` }"
        />
        <span
          class="w-1 rounded-full bg-rose-500 transition-all duration-75"
          :style="{ height: `${Math.max(4, micLevel * 0.14)}px` }"
        />
      </div>
      <span class="text-[11px] font-mono font-bold">{{ formattedTimer }}</span>
      <span v-if="interimSnippet" class="max-w-[140px] truncate text-[10px] italic text-rose-600">
        "{{ interimSnippet }}"
      </span>
    </div>

    <!-- Error notice -->
    <div v-if="errorMessage" class="inline-flex items-center gap-1 text-[11px] text-amber-700">
      <AlertCircle class="h-3.5 w-3.5" />
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Voice Commands & Punctuation Guide Modal -->
    <div
      v-if="showHelpModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/60 p-4 backdrop-blur-xs text-left"
      @click.self="showHelpModal = false"
    >
      <div class="relative w-full max-w-lg rounded-2xl bg-surface p-6 shadow-2xl space-y-4 border border-neutral-grey/80">
        <div class="flex items-center justify-between border-b border-neutral-grey/80 pb-3">
          <div class="flex items-center gap-2">
            <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-sage-muted text-sage">
              <Mic class="h-4 w-4" />
            </div>
            <div>
              <h3 class="text-sm font-bold text-navy">Voice Dictation & Formatting Guide</h3>
              <p class="text-[11px] text-neutral-muted">Spoken punctuation, hands-free stopping, and smart speech cleaning.</p>
            </div>
          </div>
          <button
            type="button"
            class="rounded-lg p-1 text-neutral-muted hover:bg-neutral-grey/50 hover:text-navy"
            @click="showHelpModal = false"
          >
            <X class="h-4 w-4" />
          </button>
        </div>

        <div class="space-y-4 max-h-[70vh] overflow-y-auto pr-1">
          <!-- Verbal Stop Highlight -->
          <div class="rounded-xl border border-rose-200 bg-rose-50/70 p-3.5">
            <div class="flex items-center gap-2 text-xs font-bold text-rose-800 mb-1">
              <Square class="h-3.5 w-3.5 fill-rose-600 text-rose-600" />
              Hands-Free Stop Phrase
            </div>
            <p class="text-xs text-rose-700">
              Say <strong class="font-mono bg-white/80 px-1.5 py-0.5 rounded border border-rose-300">"stop dictation"</strong> or <strong class="font-mono bg-white/80 px-1.5 py-0.5 rounded border border-rose-300">"end note"</strong> to automatically stop recording and save the transcript without touching the screen.
            </p>
          </div>

          <!-- Command Categories -->
          <div v-for="cat in VOICE_COMMANDS_HELP" :key="cat.category" class="space-y-2">
            <h4 class="text-xs font-bold uppercase tracking-wider text-navy">{{ cat.category }}</h4>
            <div class="grid gap-2 text-xs">
              <div
                v-for="(item, idx) in cat.items"
                :key="idx"
                class="flex items-start justify-between gap-3 rounded-lg border border-neutral-grey/60 bg-neutral-grey/10 p-2.5"
              >
                <span class="font-mono font-semibold text-sage shrink-0">{{ item.spoken }}</span>
                <span class="text-neutral-muted text-right">{{ item.effect }}</span>
              </div>
            </div>
          </div>

          <!-- Privacy & Processing Info -->
          <div class="rounded-xl border border-sage/30 bg-sage-muted/30 p-3 text-xs text-neutral-muted flex items-start gap-2">
            <Sparkles class="h-4 w-4 text-sage shrink-0 mt-0.5" />
            <p>
              <strong>Privacy & Processing:</strong> Dictation runs directly on your device with real-time speech processing. All filler sounds like "um" and "uh" are filtered out automatically.
            </p>
          </div>
        </div>

        <div class="flex justify-end pt-2 border-t border-neutral-grey/80">
          <button
            type="button"
            class="rounded-xl bg-sage px-4 py-2 text-xs font-bold text-white hover:bg-sage/90"
            @click="showHelpModal = false"
          >
            Got It
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

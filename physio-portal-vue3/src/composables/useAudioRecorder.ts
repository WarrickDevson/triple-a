import { ref, computed, onUnmounted } from 'vue'
import type { RecordingState, OfflineSoapRecording } from '../types/soap'
import { correctVeterinaryTranscript, type ClinicalAudioSample } from '../utils/veterinaryLexicon'

const OFFLINE_STORAGE_KEY = 'triple_a_offline_soap_recordings'

export function useAudioRecorder() {
  const recordingState = ref<RecordingState>('idle')
  const isSpeechRecognitionSupported = ref(false)
  const isMicrophoneAvailable = ref(true)
  const permissionDenied = ref(false)
  const errorMessage = ref('')

  const durationSeconds = ref(0)
  let timerInterval: any = null

  // Real-time audio waveform data
  const audioLevel = ref(0) // 0 to 100
  const waveformFrequencies = ref<number[]>(new Array(32).fill(10))

  // Transcripts
  const liveTranscript = ref('')
  const finalTranscript = ref('')
  const interimTranscript = ref('')

  // Recorded Audio Blob
  const audioBlob = ref<Blob | null>(null)
  const audioUrl = ref<string | null>(null)

  // Web Audio Context & Analyser
  let mediaStream: MediaStream | null = null
  let mediaRecorder: MediaRecorder | null = null
  let audioContext: AudioContext | null = null
  let analyserNode: AnalyserNode | null = null
  let animationFrameId: number | null = null

  // Speech Recognition
  let speechRecognizer: any = null

  // Offline queue
  const offlineQueue = ref<OfflineSoapRecording[]>(loadOfflineQueue())

  // Check Web Speech API support
  const SpeechRecognitionClass = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition
  if (SpeechRecognitionClass) {
    isSpeechRecognitionSupported.value = true
  }

  const formattedTime = computed(() => {
    const mins = Math.floor(durationSeconds.value / 60)
    const secs = durationSeconds.value % 60
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  })

  const fullTranscript = computed(() => {
    const combined = (finalTranscript.value + ' ' + interimTranscript.value).trim()
    return correctVeterinaryTranscript(combined)
  })

  function setupSpeechRecognition() {
    if (!SpeechRecognitionClass) return

    try {
      speechRecognizer = new SpeechRecognitionClass()
      speechRecognizer.continuous = true
      speechRecognizer.interimResults = true
      speechRecognizer.lang = 'en-US'

      speechRecognizer.onresult = (event: any) => {
        let interim = ''
        for (let i = event.resultIndex; i < event.results.length; ++i) {
          const transcriptChunk = event.results[i][0].transcript
          if (event.results[i].isFinal) {
            finalTranscript.value += (finalTranscript.value ? ' ' : '') + transcriptChunk.trim()
          } else {
            interim += transcriptChunk
          }
        }
        interimTranscript.value = interim
        liveTranscript.value = fullTranscript.value
      }

      speechRecognizer.onerror = (event: any) => {
        console.warn('Speech recognition event error:', event.error)
        if (event.error === 'not-allowed') {
          permissionDenied.value = true
        }
      }

      speechRecognizer.onend = () => {
        if (recordingState.value === 'recording') {
          try {
            speechRecognizer.start()
          } catch {
            // Ignore restart failure
          }
        }
      }
    } catch (err) {
      console.warn('Could not initialize SpeechRecognition:', err)
    }
  }

  function startWaveformAnalyser(stream: MediaStream) {
    try {
      const AudioCtx = window.AudioContext || (window as any).webkitAudioContext
      if (!AudioCtx) return

      audioContext = new AudioCtx()
      const source = audioContext.createMediaStreamSource(stream)
      analyserNode = audioContext.createAnalyser()
      analyserNode.fftSize = 64
      analyserNode.smoothingTimeConstant = 0.8
      source.connect(analyserNode)

      const bufferLength = analyserNode.frequencyBinCount
      const dataArray = new Uint8Array(bufferLength)

      const updateFrequency = () => {
        if (!analyserNode || recordingState.value !== 'recording') return

        analyserNode.getByteFrequencyData(dataArray)

        let sum = 0
        const bars: number[] = []
        for (let i = 0; i < bufferLength; i++) {
          sum += dataArray[i]
          // Map to 10 - 100 height
          bars.push(Math.max(10, Math.round((dataArray[i] / 255) * 100)))
        }

        const avg = sum / bufferLength
        audioLevel.value = Math.min(100, Math.round((avg / 128) * 100))
        waveformFrequencies.value = bars

        animationFrameId = requestAnimationFrame(updateFrequency)
      }

      updateFrequency()
    } catch (err) {
      console.warn('Web Audio Analyser error:', err)
      startSimulatedWaveform()
    }
  }

  function startSimulatedWaveform() {
    const updateSimulated = () => {
      if (recordingState.value !== 'recording') return

      const bars: number[] = []
      for (let i = 0; i < 32; i++) {
        // Dynamic undulating wave pattern
        const noise = Math.random() * 40 + 20
        const wave = Math.sin(Date.now() / 200 + i) * 30 + 40
        bars.push(Math.min(100, Math.max(15, Math.round((noise + wave) / 2))))
      }
      waveformFrequencies.value = bars
      audioLevel.value = Math.round(bars.reduce((a, b) => a + b, 0) / bars.length)

      animationFrameId = requestAnimationFrame(() => {
        setTimeout(updateSimulated, 50)
      })
    }
    updateSimulated()
  }

  async function startRecording() {
    errorMessage.value = ''
    permissionDenied.value = false
    durationSeconds.value = 0
    finalTranscript.value = ''
    interimTranscript.value = ''
    liveTranscript.value = ''
    audioBlob.value = null
    if (audioUrl.value) {
      URL.revokeObjectURL(audioUrl.value)
      audioUrl.value = null
    }

    try {
      // 1. Get User Media
      mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true })
      isMicrophoneAvailable.value = true

      // 2. MediaRecorder setup
      const audioChunks: Blob[] = []
      const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
        ? 'audio/webm;codecs=opus'
        : MediaRecorder.isTypeSupported('audio/mp4')
          ? 'audio/mp4'
          : 'audio/webm'

      mediaRecorder = new MediaRecorder(mediaStream, { mimeType })
      mediaRecorder.ondataavailable = (e) => {
        if (e.data && e.data.size > 0) {
          audioChunks.push(e.data)
        }
      }
      mediaRecorder.onstop = () => {
        audioBlob.value = new Blob(audioChunks, { type: mimeType })
        audioUrl.value = URL.createObjectURL(audioBlob.value)
      }

      mediaRecorder.start(250)
      recordingState.value = 'recording'

      // 3. Audio Visualizer
      startWaveformAnalyser(mediaStream)

      // 4. Speech Recognition
      setupSpeechRecognition()
      if (speechRecognizer) {
        try {
          speechRecognizer.start()
        } catch {
          // Ignore
        }
      }

      // 5. Timer
      timerInterval = setInterval(() => {
        durationSeconds.value++
      }, 1000)
    } catch (err: any) {
      console.warn('Microphone permission not granted or hardware not found:', err)
      isMicrophoneAvailable.value = false
      if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
        permissionDenied.value = true
        errorMessage.value = 'Microphone permission was denied. Please allow microphone access or use sample voice consultations.'
      } else {
        errorMessage.value = 'No microphone device detected. You can test dictation using the clinical samples below.'
      }
      // Fallback to simulated audio mode for smooth demonstration
      startSimulatedRecording()
    }
  }

  function startSimulatedRecording() {
    recordingState.value = 'recording'
    startSimulatedWaveform()
    timerInterval = setInterval(() => {
      durationSeconds.value++
    }, 1000)
  }

  async function stopRecording(): Promise<{ transcript: string; blob: Blob | null; url: string | null }> {
    if (recordingState.value !== 'recording' && recordingState.value !== 'paused') {
      return { transcript: fullTranscript.value, blob: audioBlob.value, url: audioUrl.value }
    }

    recordingState.value = 'processing'

    if (timerInterval) {
      clearInterval(timerInterval)
      timerInterval = null
    }

    if (animationFrameId) {
      cancelAnimationFrame(animationFrameId)
      animationFrameId = null
    }

    if (speechRecognizer) {
      try {
        speechRecognizer.stop()
      } catch {
        // Ignore
      }
    }

    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
      mediaRecorder.stop()
    }

    if (mediaStream) {
      mediaStream.getTracks().forEach((t) => t.stop())
      mediaStream = null
    }

    if (audioContext && audioContext.state !== 'closed') {
      audioContext.close().catch(() => {})
      audioContext = null
    }

    recordingState.value = 'completed'
    audioLevel.value = 0
    waveformFrequencies.value = new Array(32).fill(10)

    const finalResult = fullTranscript.value
    return {
      transcript: finalResult,
      blob: audioBlob.value,
      url: audioUrl.value
    }
  }

  function pauseRecording() {
    if (recordingState.value === 'recording') {
      recordingState.value = 'paused'
      if (mediaRecorder && mediaRecorder.state === 'recording') {
        mediaRecorder.pause()
      }
      if (speechRecognizer) {
        try {
          speechRecognizer.stop()
        } catch {}
      }
    }
  }

  function resumeRecording() {
    if (recordingState.value === 'paused') {
      recordingState.value = 'recording'
      if (mediaRecorder && mediaRecorder.state === 'paused') {
        mediaRecorder.resume()
      }
      if (speechRecognizer) {
        try {
          speechRecognizer.start()
        } catch {}
      }
    }
  }

  function resetRecording() {
    if (timerInterval) clearInterval(timerInterval)
    if (animationFrameId) cancelAnimationFrame(animationFrameId)
    if (mediaStream) mediaStream.getTracks().forEach((t) => t.stop())
    if (speechRecognizer) {
      try {
        speechRecognizer.stop()
      } catch {}
    }

    recordingState.value = 'idle'
    durationSeconds.value = 0
    finalTranscript.value = ''
    interimTranscript.value = ''
    liveTranscript.value = ''
    audioLevel.value = 0
    waveformFrequencies.value = new Array(32).fill(10)
    errorMessage.value = ''
    if (audioUrl.value) {
      URL.revokeObjectURL(audioUrl.value)
      audioUrl.value = null
    }
    audioBlob.value = null
  }

  // Load a clinical sample consultation
  function loadClinicalSample(sample: ClinicalAudioSample) {
    resetRecording()
    finalTranscript.value = sample.transcript
    liveTranscript.value = sample.transcript
    durationSeconds.value = 45
    recordingState.value = 'completed'
  }

  // Offline queue operations
  function loadOfflineQueue(): OfflineSoapRecording[] {
    try {
      const stored = localStorage.getItem(OFFLINE_STORAGE_KEY)
      return stored ? JSON.parse(stored) : []
    } catch {
      return []
    }
  }

  function saveToOfflineQueue(item: Omit<OfflineSoapRecording, 'id' | 'timestamp' | 'isSynced'>) {
    const newItem: OfflineSoapRecording = {
      ...item,
      id: 'rec-' + Date.now(),
      timestamp: new Date().toISOString(),
      isSynced: false
    }
    offlineQueue.value.unshift(newItem)
    try {
      localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(offlineQueue.value))
    } catch (e) {
      console.warn('Could not persist offline recording to localStorage:', e)
    }
    return newItem
  }

  function removeOfflineRecording(id: string) {
    offlineQueue.value = offlineQueue.value.filter((i) => i.id !== id)
    localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(offlineQueue.value))
  }

  onUnmounted(() => {
    resetRecording()
  })

  return {
    recordingState,
    isSpeechRecognitionSupported,
    isMicrophoneAvailable,
    permissionDenied,
    errorMessage,
    durationSeconds,
    formattedTime,
    audioLevel,
    waveformFrequencies,
    liveTranscript,
    fullTranscript,
    audioBlob,
    audioUrl,
    startRecording,
    stopRecording,
    pauseRecording,
    resumeRecording,
    resetRecording,
    loadClinicalSample,
    offlineQueue,
    saveToOfflineQueue,
    removeOfflineRecording
  }
}

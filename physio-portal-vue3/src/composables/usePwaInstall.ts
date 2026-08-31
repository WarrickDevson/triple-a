import { ref } from 'vue'

interface BeforeInstallPromptEvent extends Event {
  readonly platforms: string[]
  readonly userChoice: Promise<{
    outcome: 'accepted' | 'dismissed'
    platform: string
  }>
  prompt(): Promise<void>
}

const deferredPrompt = ref<BeforeInstallPromptEvent | null>(null)
const isInstalled = ref(false)
// Show install option unless app is already running as installed standalone PWA
const isInstallable = ref(true)
const isIOS = ref(false)
const showInstructionsModal = ref(false)

if (typeof window !== 'undefined') {
  // Check standalone
  if (
    window.matchMedia('(display-mode: standalone)').matches ||
    (window.navigator as unknown as { standalone?: boolean }).standalone === true
  ) {
    isInstalled.value = true
    isInstallable.value = false
  }

  // Detect iOS Safari
  const ua = window.navigator.userAgent.toLowerCase()
  const isApple = /iphone|ipad|ipod/.test(ua)
  if (isApple) {
    isIOS.value = true
  }

  // Check if index.html already captured the prompt before Vue mounted
  const win = window as unknown as { __deferredPwaPrompt?: BeforeInstallPromptEvent }
  if (win.__deferredPwaPrompt) {
    deferredPrompt.value = win.__deferredPwaPrompt
  }

  window.addEventListener('pwa-prompt-ready', () => {
    if (win.__deferredPwaPrompt) {
      deferredPrompt.value = win.__deferredPwaPrompt
    }
  })

  window.addEventListener('beforeinstallprompt', (e: Event) => {
    e.preventDefault()
    deferredPrompt.value = e as BeforeInstallPromptEvent
    if (!isInstalled.value) {
      isInstallable.value = true
    }
    console.debug('PWA: beforeinstallprompt captured')
  })

  window.addEventListener('appinstalled', () => {
    isInstalled.value = true
    isInstallable.value = false
    deferredPrompt.value = null
    console.debug('PWA: app installed')
  })
}

export function usePwaInstall() {
  async function promptInstall(): Promise<boolean> {
    if (deferredPrompt.value) {
      try {
        await deferredPrompt.value.prompt()
        const choiceResult = await deferredPrompt.value.userChoice
        if (choiceResult.outcome === 'accepted') {
          isInstalled.value = true
          isInstallable.value = false
          deferredPrompt.value = null
          showInstructionsModal.value = false
          return true
        }
      } catch (err) {
        console.warn('PWA install prompt error:', err)
      }
    } else {
      // If browser doesn't support or hasn't fired beforeinstallprompt yet
      showInstructionsModal.value = true
    }
    return false
  }

  return {
    isInstallable,
    isInstalled,
    isIOS,
    showInstructionsModal,
    promptInstall,
  }
}

import { ref } from 'vue'

interface BeforeInstallPromptEvent extends Event {
  readonly platforms: string[]
  readonly userChoice: Promise<{
    outcome: 'accepted' | 'dismissed'
    platform: string
  }>
  prompt(): Promise<void>
}

const DISMISSED_KEY = 'pwa_install_banner_dismissed'
const INSTALLED_KEY = 'pwa_installed'

const deferredPrompt = ref<BeforeInstallPromptEvent | null>(null)
const isInstalled = ref(false)
const isInstallable = ref(true)
const isIOS = ref(false)
const isDismissed = ref(false)

if (typeof window !== 'undefined') {
  // Check localStorage for dismissal state
  isDismissed.value = localStorage.getItem(DISMISSED_KEY) === 'true'

  // Check if previously marked as installed
  if (localStorage.getItem(INSTALLED_KEY) === 'true') {
    isInstalled.value = true
    isInstallable.value = false
  }

  // Check standalone display mode
  if (
    window.matchMedia('(display-mode: standalone)').matches ||
    (window.navigator as unknown as { standalone?: boolean }).standalone === true
  ) {
    isInstalled.value = true
    isInstallable.value = false
    localStorage.setItem(INSTALLED_KEY, 'true')
  }

  // Check getInstalledRelatedApps if supported
  if ('getInstalledRelatedApps' in window.navigator) {
    ;(window.navigator as any)
      .getInstalledRelatedApps()
      .then((relatedApps: any[]) => {
        if (relatedApps && relatedApps.length > 0) {
          isInstalled.value = true
          isInstallable.value = false
          localStorage.setItem(INSTALLED_KEY, 'true')
        }
      })
      .catch(() => {
        // ignore detection failure
      })
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
  })

  window.addEventListener('appinstalled', () => {
    isInstalled.value = true
    isInstallable.value = false
    deferredPrompt.value = null
    localStorage.setItem(INSTALLED_KEY, 'true')
  })
}

export function usePwaInstall() {
  function dismissBanner() {
    isDismissed.value = true
    if (typeof window !== 'undefined') {
      localStorage.setItem(DISMISSED_KEY, 'true')
    }
  }

  async function promptInstall(): Promise<boolean> {
    if (deferredPrompt.value) {
      try {
        await deferredPrompt.value.prompt()
        const choiceResult = await deferredPrompt.value.userChoice
        if (choiceResult.outcome === 'accepted') {
          isInstalled.value = true
          isInstallable.value = false
          deferredPrompt.value = null
          if (typeof window !== 'undefined') {
            localStorage.setItem(INSTALLED_KEY, 'true')
          }
          return true
        }
      } catch (err) {
        console.warn('PWA install prompt error:', err)
      }
    }
    return false
  }

  return {
    isInstallable,
    isInstalled,
    isIOS,
    isDismissed,
    dismissBanner,
    promptInstall,
  }
}

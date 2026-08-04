import { computed } from 'vue'

const logoModules = import.meta.glob<{ default: string }>(
  '../assets/brand/*.{png,svg,jpg,webp}',
  { eager: true },
)

const logoUrl = Object.values(logoModules)[0]?.default ?? null

export function useBrandLogo() {
  const hasLogo = computed(() => logoUrl !== null)

  return {
    logoUrl,
    hasLogo,
  }
}

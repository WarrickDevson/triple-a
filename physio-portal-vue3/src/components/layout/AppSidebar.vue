<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Calendar,
  CheckSquare,
  ClipboardList,
  CreditCard,
  Dumbbell,
  FileBarChart,
  FolderOpen,
  LayoutDashboard,
  MessageSquare,
  PawPrint,
  Settings,
  ShieldCheck,
  TrendingUp,
} from '@lucide/vue'
import { brand } from '../../config/brand'
import { portalNavItems } from '../../config/navigation'
import { useAuthStore } from '../../store/auth'
import { useMessagesStore } from '../../store/messages'
import { useBrandLogo } from '../../composables/useBrandLogo'

const iconMap = {
  LayoutDashboard,
  PawPrint,
  Calendar,
  ClipboardList,
  Dumbbell,
  TrendingUp,
  MessageSquare,
  FileBarChart,
  FolderOpen,
  CheckSquare,
  CreditCard,
  Settings,
  ShieldCheck,
} as const

const auth = useAuthStore()
const messagesStore = useMessagesStore()
const route = useRoute()
const router = useRouter()
const { logoUrl, hasLogo } = useBrandLogo()

const navItems = computed(() => {
  if (auth.user?.userRole === 'SysAdmin') {
    return [
      { name: 'admin-physios', label: 'Admin Management', to: { name: 'admin-physios' }, icon: 'ShieldCheck' },
      { name: 'exercises', label: 'Exercise Library', to: { name: 'exercises' }, icon: 'Dumbbell' },
      { name: 'settings', label: 'Settings', to: { name: 'settings' }, icon: 'Settings' },
    ]
  }
  return portalNavItems
})

onMounted(() => {
  messagesStore.loadThreads().catch(() => undefined)
})

const activeRoute = computed(() => String(route.name ?? ''))

function navBadge(name: string) {
  if (name === 'messages' && messagesStore.totalUnreadCount > 0) {
    return messagesStore.totalUnreadCount
  }
  const item = portalNavItems.find((entry) => entry.name === name)
  return item?.badge
}

function isActive(name: string) {
  if (name === 'patients' && activeRoute.value === 'patient-detail') return true
  if (name === 'treatment-plans' && activeRoute.value === 'treatment-plan-detail') return true
  if (name === 'messages' && activeRoute.value === 'message-thread') return true
  if (name === 'progress' && activeRoute.value === 'progress-detail') return true
  return activeRoute.value === name
}

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}

function displayRole(role?: string) {
  if (!role) return 'Veterinary Physiotherapist'
  return role.replace(/([A-Z])/g, ' $1').trim()
}
</script>

<template>
  <aside class="flex h-full w-[240px] shrink-0 flex-col bg-navy text-white">
    <div class="border-b border-white/10 px-4 py-4">
      <img
        v-if="hasLogo"
        :src="logoUrl!"
        :alt="brand.alt"
        class="h-9 w-auto max-w-[120px] object-contain object-left"
      />
      <div v-else>
        <p class="text-lg font-bold tracking-tight">
          Triple <span class="text-sage-light">A</span>
        </p>
        <p class="mt-1 text-[9px] font-semibold uppercase tracking-[0.16em] text-white/55">
          {{ brand.tagline }}
        </p>
      </div>
    </div>

    <nav class="flex-1 overflow-y-auto py-4" aria-label="Portal navigation">
      <RouterLink
        v-for="item in navItems"
        :key="item.name"
        :to="item.to"
        class="nav-item"
        :class="{ 'nav-item--active': isActive(item.name) }"
      >
        <component :is="iconMap[item.icon as keyof typeof iconMap]" class="h-[18px] w-[18px] shrink-0" :stroke-width="1.75" />
        <span class="flex-1">{{ item.label }}</span>
        <span
          v-if="navBadge(item.name)"
          class="flex h-5 min-w-5 items-center justify-center rounded-full bg-accent-amber px-1.5 text-[10px] font-bold text-navy"
        >
          {{ navBadge(item.name) }}
        </span>
      </RouterLink>
    </nav>

    <div class="border-t border-white/10 p-4">
      <div v-if="auth.user" class="mb-3 flex items-center gap-3">
        <div
          class="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-sage/30 text-sm font-bold text-white"
        >
          {{ auth.user.firstName?.[0] }}{{ auth.user.lastName?.[0] }}
        </div>
        <div class="min-w-0">
          <p class="truncate text-sm font-semibold text-white">
            {{ auth.user.firstName }} {{ auth.user.lastName }}
          </p>
          <p class="truncate text-xs text-white/55">{{ displayRole(auth.user.userRole) }}</p>
        </div>
      </div>
      <button
        type="button"
        class="w-full rounded-lg border border-white/20 px-3 py-2 text-xs font-semibold text-white/80 transition-colors hover:bg-white/10 hover:text-white"
        @click="logout"
      >
        Sign Out
      </button>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { Bell, CircleHelp, Search } from '@lucide/vue'
import { useAuthStore } from '../../store/auth'

const route = useRoute()
const auth = useAuthStore()

const pageTitle = computed(() => (route.meta.title as string) ?? 'Dashboard')
</script>

<template>
  <header class="sticky top-0 z-20 border-b border-navy/8 bg-surface/95 backdrop-blur-md">
    <div class="flex flex-wrap items-center gap-4 px-6 py-4 lg:px-8">
      <h1 class="shrink-0 text-xl font-bold text-navy sm:text-2xl">{{ pageTitle }}</h1>

      <div class="relative mx-auto hidden max-w-md flex-1 md:block">
        <Search class="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-neutral-muted" />
        <input
          type="search"
          placeholder="Search patients, owners, plans..."
          class="w-full rounded-full border border-neutral-grey bg-white py-2.5 pl-11 pr-4 text-sm text-neutral-dark outline-none transition-colors placeholder:text-neutral-muted/70 focus:border-sage focus:ring-2 focus:ring-sage/15"
        />
      </div>

      <div class="ml-auto flex items-center gap-2">
        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-full text-neutral-muted transition-colors hover:bg-navy/5 hover:text-navy"
          aria-label="Notifications"
        >
          <Bell class="h-5 w-5" :stroke-width="1.75" />
        </button>
        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-full text-neutral-muted transition-colors hover:bg-navy/5 hover:text-navy"
          aria-label="Help"
        >
          <CircleHelp class="h-5 w-5" :stroke-width="1.75" />
        </button>
        <div
          v-if="auth.user"
          class="flex h-9 w-9 items-center justify-center rounded-full bg-sage/20 text-xs font-bold text-sage"
          :title="`${auth.user.firstName} ${auth.user.lastName}`"
        >
          {{ auth.user.firstName?.[0] }}{{ auth.user.lastName?.[0] }}
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Menu, X } from '@lucide/vue'
import AppSidebar from '../components/layout/AppSidebar.vue'
import AppTopBar from '../components/layout/AppTopBar.vue'

const mobileNavOpen = ref(false)

function closeMobileNav() {
  mobileNavOpen.value = false
}
</script>

<template>
  <div class="flex min-h-screen bg-surface">
    <AppSidebar class="fixed inset-y-0 left-0 z-30 hidden lg:flex" />

    <!-- Mobile sidebar overlay -->
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      leave-active-class="transition-opacity duration-200"
      leave-to-class="opacity-0"
    >
      <div
        v-if="mobileNavOpen"
        class="fixed inset-0 z-40 bg-navy/50 lg:hidden"
        @click="closeMobileNav"
      />
    </Transition>

    <Transition
      enter-active-class="transition-transform duration-200"
      enter-from-class="-translate-x-full"
      leave-active-class="transition-transform duration-200"
      leave-to-class="-translate-x-full"
    >
      <AppSidebar
        v-if="mobileNavOpen"
        class="fixed inset-y-0 left-0 z-50 lg:hidden"
        @click="closeMobileNav"
      />
    </Transition>

    <div class="flex min-h-screen flex-1 flex-col lg:pl-[240px]">
      <div class="sticky top-0 z-20 flex items-center gap-3 border-b border-navy/8 bg-surface/95 px-4 py-3 backdrop-blur-md lg:hidden">
        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-lg text-navy hover:bg-navy/5"
          aria-label="Open navigation"
          @click="mobileNavOpen = true"
        >
          <Menu class="h-5 w-5" :stroke-width="1.75" />
        </button>
        <span class="text-sm font-semibold text-navy">Triple A</span>
        <button
          v-if="mobileNavOpen"
          type="button"
          class="ml-auto flex h-10 w-10 items-center justify-center rounded-lg text-navy hover:bg-navy/5"
          aria-label="Close navigation"
          @click="closeMobileNav"
        >
          <X class="h-5 w-5" :stroke-width="1.75" />
        </button>
      </div>

      <AppTopBar />
      <main class="flex-1 px-4 py-6 sm:px-6 lg:px-8">
        <RouterView />
      </main>
    </div>
  </div>
</template>

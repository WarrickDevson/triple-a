<script setup lang="ts">
import { onMounted } from 'vue'
import { setUnauthorizedHandler } from './api/client'
import { useAuthStore } from './store/auth'
import PwaInstallBanner from './components/PwaInstallBanner.vue'

const auth = useAuthStore()

onMounted(() => {
  auth.initialize()
  setUnauthorizedHandler(() => auth.logout())
})
</script>

<template>
  <RouterView />
  <PwaInstallBanner />
</template>

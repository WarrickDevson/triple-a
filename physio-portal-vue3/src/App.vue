<script setup lang="ts">
import { onMounted } from 'vue'
import { setUnauthorizedHandler } from './api/client'
import { useAuthStore } from './store/auth'

const auth = useAuthStore()

onMounted(() => {
  auth.initialize()
  setUnauthorizedHandler(() => auth.logout())
})
</script>

<template>
  <RouterView />
</template>

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  base: '/portal/',
  plugins: [vue(), tailwindcss()],
  server: {
    port: 5287,
    strictPort: true,
    open: true,
  },
})

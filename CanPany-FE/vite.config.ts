import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  // Đảm bảo dev + preview dùng fallback SPA (deep link / F5).
  appType: 'spa',
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes('node_modules')) return undefined

          if (
            id.includes('/react/') ||
            id.includes('/react-dom/') ||
            id.includes('/react-router-dom/')
          ) {
            return 'react-vendor'
          }

          if (
            id.includes('/@tanstack/react-query/') ||
            id.includes('/react-hook-form/') ||
            id.includes('/@hookform/resolvers/') ||
            id.includes('/zod/') ||
            id.includes('/zustand/')
          ) {
            return 'data-vendor'
          }

          if (
            id.includes('/axios/') ||
            id.includes('/date-fns/') ||
            id.includes('/lucide-react/') ||
            id.includes('/clsx/') ||
            id.includes('/tailwind-merge/')
          ) {
            return 'ui-vendor'
          }

          return undefined
        },
      },
    },
  },
})

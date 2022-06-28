import { defineConfig } from 'vite'
import preact from '@preact/preset-vite'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [preact()],
  server: {
    port: 5000
  },
  // https://github.com/vitejs/vite/issues/8644
  esbuild: {
    logOverride: { 'this-is-undefined-in-esm': 'silent' }
  }
})

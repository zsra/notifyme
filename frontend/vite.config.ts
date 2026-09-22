import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    globals: true,
    // The default "threads" pool crashes on this Windows/Node combination (worker threads never
    // finish initializing before the first test file runs); forks are slower to spin up but
    // reliable here.
    pool: 'forks',
  },
})

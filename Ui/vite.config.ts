import { fileURLToPath, URL } from "node:url"
import react from "@vitejs/plugin-react"
import { defineConfig } from "vite"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    // the API has no CORS policy; the dev server forwards /api so the browser stays on one origin
    proxy: {
      "/api": "http://localhost:5180",
    },
  },
})

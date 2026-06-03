import path from "node:path"
import tailwindcss from "@tailwindcss/vite"
import react from "@vitejs/plugin-react"
import { defineConfig } from "vite"

const localBackendTarget = process.env.VITE_API_PROXY_TARGET ?? "http://127.0.0.1:5020"

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    hmr: {
      host: "127.0.0.1",
      protocol: "ws",
    },
    proxy: {
      "/api": {
        target: localBackendTarget,
        changeOrigin: true,
        secure: false,
      },
      "/openapi": {
        target: localBackendTarget,
        changeOrigin: true,
        secure: false,
      },
    },
  },
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
    dedupe: ["react", "react-dom", "react/jsx-runtime"],
  },
  build: {
    rolldownOptions: {
      output: {
        codeSplitting: {
          groups: [
            {
              test: /node_modules\/(?:@radix-ui|radix-ui|class-variance-authority|tailwind-merge)\//,
              name: "ui-vendor",
            },
            {
              test: /node_modules\/(?:react|react-dom|react-router-dom|@tanstack\/react-query|zustand|sonner|lucide-react|next-themes|openapi-fetch|openapi-react-query)\//,
              name: "react-vendor",
            },
          ],
        },
      },
    },
  },
})

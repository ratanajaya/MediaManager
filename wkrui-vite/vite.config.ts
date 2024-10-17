import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      src: "/src",
      _assets: "/src/_assets",
      _utils: "/src/_utils",
      _shared: "/src/_shared",
      pages: "/src/pages",
    },
  },
})

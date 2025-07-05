import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ command }) => ({
    plugins: [react()],
    publicDir: command === 'build' ? false : 'public' ,
    define: {
        'VITE_NODE_ENV': `${command === 'build' ? `process.env.VITE_NODE_ENV === 'development'` : 'import.meta.env'}`,
    },
    server: {
        port: 5173,
        host: 'localhost',
        open: true,
        proxy: {
            '/api': {
                target: 'https://localhost:7250',
                changeOrigin: true,
                secure: false,
            }
        }
    },
    optimizeDeps: {
        exclude: ['chrome-extension://*']
    },
}));

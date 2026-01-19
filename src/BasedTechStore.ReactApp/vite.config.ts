import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import type { ProxyOptions } from 'vite';
import type Server from 'http-proxy';
import type { IncomingMessage } from 'http';
// https://vite.dev/config/
export default defineConfig(({ command, mode }) => ({
    plugins: [react()],
    //publicDir: 'public',
    define: {
        'VITE_NODE_ENV': JSON.stringify(command === 'build' ? 'production' : 'development'),
    },
    server: {
        port: 3000,
        host: true,
        //open: true,
        proxy: {
            '/api': {
                target: mode === 'development' ? 'http://localhost:7250' : 'http://webapi:80',
                changeOrigin: true,
                secure: false,
                configure: (proxy: Server) => {
                    proxy.on('error', (err: Error) => {
                        console.log('Proxy error:', err);
                    });
                    proxy.on('proxyReq', (_proxyReq, req: IncomingMessage) => {
                        console.log('Proxying request:', req.method, req.url);
                    });
                }
            } as ProxyOptions
        },
    },
    preview: {
        port: 3000,
        host: '0.0.0.0',
        allowedHosts: ['basedtech-store.com'],
    },
    build: {
        outDir: 'dist',
        sourcemap: mode === 'development',
        minify: mode === 'production',
        rollupOptions: {
            output: {
                manualChunks: {
                    vendor: ['react', 'react-dom'],
                    router: ['react-router-dom'],
                    bootstrap: ['bootstrap', 'react-bootstrap']
                }
            }
        }
    },
    optimizeDeps: {
        exclude: ['chrome-extension://*']
    },
}));

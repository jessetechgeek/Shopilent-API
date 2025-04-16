import path from "path"
import tailwindcss from "@tailwindcss/vite"
import {defineConfig} from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
    plugins: [react(), tailwindcss()],
    resolve: {
        alias: {
            "@": path.resolve(__dirname, "./src"),
        },
    },
    server: {
        port: 5173,
        host: '0.0.0.0', // Required for Docker container access
        watch: {
            usePolling: true, // Enable polling for Docker file system
            interval: 1000,   // Polling interval in ms
        },
        hmr: {
            // Explicitly set the HMR client to connect through the exposed port
            clientPort: 9802, // This is your externally mapped port
            // You may need to uncomment and adjust this if using a custom network setup
            // host: 'localhost', 
        }
    }
})

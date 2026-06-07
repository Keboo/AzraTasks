import { defineConfig } from 'vite'
import { heyApiPlugin } from '@hey-api/vite-plugin';
import vue from '@vitejs/plugin-vue'
import vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'
import path from 'path'

// Get backend URL from Aspire service discovery or fallback
// Aspire sets environment variables in the format: services__{service-name}__{protocol}__{index}
// For a service named "AzraTasks-backend", it would be services__AzraTasks-backend__https__0 or services__AzraTasks-backend__http__0
// On Linux, Aspire may use VITE_BACKEND_HTTP format instead
// Prefer HTTP to avoid dev-certificate trust issues (especially in CI environments)
// BACKEND_URL is set in production builds via CI/CD (from Terraform outputs)
const backendUrl = process.env.BACKEND_URL
  || process.env['services__AzraTasks-backend__http__0'] 
  || process.env['services__AzraTasks-backend__https__0'] 
  || process.env.VITE_BACKEND_HTTP
  || process.env.VITE_BACKEND_HTTPS
  || process.env.services__backend__http__0 
  || process.env.services__backend__https__0 
  || 'https://localhost:5001'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue({
      template: { transformAssetUrls },
    }),
    vuetify({
      autoImport: true,
    }),
    heyApiPlugin({
      config: {
        input: '../AzraTasks.Api/AzraTasks.Api.json',
        output: 'src/services/api',
      },
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  define: {
    '__API_BASE_URL__': JSON.stringify(backendUrl),
    '__APPLICATIONINSIGHTS_CONNECTION_STRING__': JSON.stringify(process.env.APPLICATIONINSIGHTS_CONNECTION_STRING || ''),
  },
  server: {
    host: process.env.VITE_HOST_URL || 'localhost',
    port: parseInt(process.env.PORT || '5173'),
    proxy: {
      '/api': {
        target: backendUrl,
        changeOrigin: true,
        secure: false,
        cookieDomainRewrite: 'localhost',
      },
      '/hubs': {
        target: backendUrl,
        changeOrigin: true,
        secure: false,
        ws: true,
        cookieDomainRewrite: 'localhost',
      },
    },
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: true,
  },
})

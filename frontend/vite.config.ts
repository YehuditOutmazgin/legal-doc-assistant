import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  resolve: {
    alias: {
      '@': resolve(__dirname, './src')
    }
  },
  build: {
    rollupOptions: {
      input: {
        main: resolve(__dirname, 'index.html'),
        login: resolve(__dirname, 'src/pages/login/login.html'),
        dashboard: resolve(__dirname, 'src/pages/dashboard/dashboard.html'),
        contracts: resolve(__dirname, 'src/pages/contracts/contracts.html'),
        contractDetail: resolve(__dirname, 'src/pages/contracts/contract-detail.html'),
        clients: resolve(__dirname, 'src/pages/clients/clients.html'),
        clientDetail: resolve(__dirname, 'src/pages/clients/client-detail.html'),
        templates: resolve(__dirname, 'src/pages/templates/templates.html'),
        templateDetail: resolve(__dirname, 'src/pages/templates/template-detail.html')
      }
    }
  },
  server: {
    port: 3000,
    open: true
  }
});

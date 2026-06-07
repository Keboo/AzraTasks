import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import './index.css'

import { createApp } from 'vue'

import App from './App.vue'
import vuetify from './plugins/vuetify'
import router from './router'
import { client as apiClient } from './services/api/client.gen'
import { initTelemetry } from './services/telemetry'

apiClient.setConfig({
  baseUrl: __API_BASE_URL__,
  credentials: 'include',
})

initTelemetry()

createApp(App)
  .use(router)
  .use(vuetify)
  .mount('#app')

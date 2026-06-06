import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import './index.css'

import { createApp } from 'vue'

import App from './App.vue'
import vuetify from './plugins/vuetify'
import router from './router'
import { initTelemetry } from './services/telemetry'

initTelemetry()

createApp(App)
  .use(router)
  .use(vuetify)
  .mount('#app')

import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi'

const vuetify = createVuetify({
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: { mdi },
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: "#B05A36",
          secondary: "#2A2B2F",
          surface: "#F5EEE1",
          background: "#FEF9EF"
        },
      },
    },
  },
})

export default vuetify

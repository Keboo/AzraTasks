<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { useAuth } from '@/composables/useAuth'

const auth = useAuth()
const route = useRoute()
const router = useRouter()

const form = reactive({
  email: '',
  password: '',
  rememberMe: true,
})

const errorMessage = ref('')

async function submit() {
  errorMessage.value = ''

  try {
    await auth.login(form)

    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/lists'
    await router.push(redirect)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to sign in.'
  }
}
</script>

<template>
  <v-row justify="center">
    <v-col
      cols="12"
      sm="10"
      md="6"
      lg="4"
    >
      <v-card
        rounded="xl"
        elevation="2"
      >
        <v-card-title class="text-h5 pt-6 px-6">
          Sign in
        </v-card-title>
        <v-card-text class="px-6">
          <v-alert
            v-if="errorMessage"
            type="error"
            variant="tonal"
            class="mb-4"
          >
            {{ errorMessage }}
          </v-alert>

          <v-form @submit.prevent="submit">
            <v-text-field
              v-model="form.email"
              data-testid="email-input"
              label="Email"
              type="email"
              autocomplete="email"
              variant="outlined"
              class="mb-2"
              required
            />
            <v-text-field
              v-model="form.password"
              data-testid="password-input"
              label="Password"
              type="password"
              autocomplete="current-password"
              variant="outlined"
              class="mb-2"
              required
            />
            <v-checkbox
              v-model="form.rememberMe"
              label="Keep me signed in"
              density="compact"
            />
            <v-btn
              data-testid="login-button"
              type="submit"
              color="primary"
              block
              size="large"
              :loading="auth.loading.value"
            >
              Login
            </v-btn>
          </v-form>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

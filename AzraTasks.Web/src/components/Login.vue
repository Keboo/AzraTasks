<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { useAuth } from '@/composables/useAuth'
import type { ProblemDetails } from '@/services/api'

const auth = useAuth()
const route = useRoute()
const router = useRouter()

const errorMessage = ref('')


const form = reactive({
  email: '',
  password: '',
  rememberMe: true,
})

async function submit() {
  errorMessage.value = ''

  try {
    await auth.login(form)

    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/lists'
    await router.push(redirect)
  } catch (error) {
    const problem = error as ProblemDetails
    errorMessage.value = problem.detail ?? 'Unable to sign in.'
  }
}
</script>

<template>
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
</template>

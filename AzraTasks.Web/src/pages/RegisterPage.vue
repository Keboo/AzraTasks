<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

import { useAuth } from '@/composables/useAuth'

const auth = useAuth()
const router = useRouter()

const form = reactive({
  email: '',
  password: '',
  confirmPassword: '',
})

const errorMessage = ref('')

const passwordMismatch = computed(() =>
  form.confirmPassword.length > 0 && form.password !== form.confirmPassword,
)

async function submit() {
  errorMessage.value = ''

  if (passwordMismatch.value) {
    errorMessage.value = 'Passwords do not match.'
    return
  }

  try {
    await auth.register(form)
    await router.push({ name: 'lists' })
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to create your account.'
  }
}
</script>

<template>
  <v-row justify="center">
    <v-col cols="12" sm="10" md="6" lg="4">
      <v-card rounded="xl" elevation="2">
        <v-card-title class="text-h5 pt-6 px-6">Create account</v-card-title>
        <v-card-text class="px-6">
          <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-4">
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
              autocomplete="new-password"
              variant="outlined"
              class="mb-2"
              required
            />
            <v-text-field
              v-model="form.confirmPassword"
              data-testid="confirm-password-input"
              label="Confirm password"
              type="password"
              autocomplete="new-password"
              variant="outlined"
              class="mb-4"
              required
            />
            <v-btn
              data-testid="register-button"
              type="submit"
              color="primary"
              block
              size="large"
              :loading="auth.loading.value"
            >
              Register
            </v-btn>
          </v-form>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

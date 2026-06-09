<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'

import { useAuth } from '@/composables/useAuth'

const router = useRouter()
const auth = useAuth()

const isAuthenticated = computed(() => auth.isAuthenticated.value)
const isLoading = computed(() => auth.loading.value && !auth.initialized.value)
const userLabel = computed(() => auth.user.value?.email ?? auth.user.value?.userName ?? '')

onMounted(async () => {
  await auth.ensureInitialized()
})

async function handleLogout() {
  await auth.logout()
  await router.push({ name: 'home' })
}
</script>

<template>
  <v-app>
    <v-app-bar
      :elevation="0"
      color="surface"
      density="comfortable"
    >
      <v-app-bar-title
        class="cursor-pointer"
        @click="router.push({ name: 'auth' })"
      >
        Azra Tasks
      </v-app-bar-title>

      <template v-if="isAuthenticated">
        <span class="text-body-2 mr-4">{{ userLabel }}</span>
        <v-btn
          data-testid="nav-logout-button"
          color="primary"
          variant="outlined"
          rounded="xl"
          @click="handleLogout"
        >
          Logout
        </v-btn>
      </template>
    </v-app-bar>

    <v-main>
      <v-container class="py-8">
        <v-progress-linear
          v-if="isLoading"
          color="primary"
          indeterminate
          rounded
          class="mb-6"
        />
        <router-view />
      </v-container>
    </v-main>
  </v-app>
</template>

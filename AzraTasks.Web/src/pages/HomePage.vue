<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'

import { useAuth } from '@/composables/useAuth'

const router = useRouter()
const auth = useAuth()

const primaryActionLabel = computed(() => (auth.isAuthenticated.value ? 'Open my lists' : 'Start organizing'))

function goToPrimaryAction() {
  void router.push({ name: auth.isAuthenticated.value ? 'lists' : 'register' })
}
</script>

<template>
  <v-row
    align="center"
    justify="center"
  >
    <v-col
      cols="12"
      md="10"
      lg="8"
    >
      <v-sheet
        rounded="xl"
        color="surface"
        elevation="2"
        class="pa-8 pa-md-12"
      >
        <div class="text-overline text-primary mb-4">
          Q&amp;A out, TODOs in
        </div>
        <h1 class="text-h3 font-weight-bold mb-4">
          Keep your work in one clean, private list app.
        </h1>
        <p class="text-body-1 text-medium-emphasis mb-8">
          AzraTasks now focuses on personal TODO lists. Create lists, add tasks, and track completion
          without the old room and question workflow getting in the way.
        </p>

        <div class="d-flex flex-wrap ga-4">
          <v-btn
            data-testid="home-primary-action"
            color="primary"
            size="large"
            @click="goToPrimaryAction"
          >
            {{ primaryActionLabel }}
          </v-btn>
          <v-btn
            v-if="!auth.isAuthenticated.value"
            variant="tonal"
            size="large"
            @click="router.push({ name: 'login' })"
          >
            Sign in
          </v-btn>
        </div>
      </v-sheet>
    </v-col>
  </v-row>
</template>

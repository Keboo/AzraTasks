<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goToPrimaryAction() {
  void router.push({ name: 'auth' })
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
          Lists for simplifying your life
        </div>
        <h1 class="text-h3 font-weight-bold mb-4">
          Keep your work in one clean, private list app.
        </h1>
        <p class="text-body-1 text-medium-emphasis mb-8">
          AzraTasks focuses on personal TODO lists. Create lists, add tasks, and track completion.
        </p>

        <div class="d-flex flex-wrap ga-4">
          <v-btn
            data-testid="home-primary-action"
            color="primary"
            size="large"
            @click="goToPrimaryAction"
          >
            Start organizing
          </v-btn>
        </div>
      </v-sheet>
    </v-col>
  </v-row>
</template>

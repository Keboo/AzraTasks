<script setup lang="ts">
import { ref } from 'vue'
import Login from '@/components/Login.vue'
import Register from '@/components/Register.vue'
import router from '@/router'

const tab = ref('login')

function goToHome() {
  void router.push({ name: 'home' })
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
          <v-btn
            variant="text"
            class="text-headline-large"
            @click="goToHome"
          >
            <v-icon icon="mdi-arrow-left" />
            AzraTasks
          </v-btn>
          <v-tabs
            v-model="tab"
            color="primary"
          >
            <v-tab value="login">
              Login
            </v-tab>
            <v-tab value="register">
              Register
            </v-tab>
          </v-tabs>
        </v-card-title>
        <v-card-text class="px-6">
          <v-tabs-window v-model="tab">
            <v-tabs-window-item value="login">
              <v-sheet
                class="pa-5"
              >
                <login />
              </v-sheet>
            </v-tabs-window-item>
            <v-tabs-window-item value="register">
              <v-sheet
                class="pa-5"
              >
                <register />
              </v-sheet>
            </v-tabs-window-item>
          </v-tabs-window>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>
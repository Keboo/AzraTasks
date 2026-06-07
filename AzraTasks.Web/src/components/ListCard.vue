<script setup lang="ts">
import type { TodoListDto } from '@/services/api'

defineProps<{ list: TodoListDto }>()

const emit = defineEmits<{
  remove: [listId: string]
  open: [listId: string]
}>()
</script>

<template>
  <v-card
    data-testid="todo-list-card"
    rounded="xl"
    elevation="1"
  >
    <v-card-title class="d-flex align-center">
      {{ list.name }} - ({{ list.itemCount }})
      <v-spacer />
      <v-btn
        color="error"
        icon="mdi-delete"
        variant="text"
        @click="emit('remove', list.id!)"
      />
    </v-card-title>
    <v-card-subtitle>
      Created {{ new Date(list.createdDate!).toLocaleString() }}
    </v-card-subtitle>
    <v-card-actions>
      <v-btn
        color="primary"
        variant="text"
        @click="emit('open', list.id!)"
      >
        Open
      </v-btn>
      <v-spacer />
    </v-card-actions>
  </v-card>
</template>
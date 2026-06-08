<script setup lang="ts">
import type { TodoListDto } from '@/services/api'

const props = defineProps<{ list: TodoListDto }>()

const emit = defineEmits<{
  remove: [list: TodoListDto]
  open: [list: TodoListDto]
}>()

const handleDeleteClick = (event: MouseEvent) => {
  event.stopPropagation()
  emit('remove', props.list)
}

const percentageFormatter = new Intl.NumberFormat(undefined, {
  style: 'percent',
  maximumFractionDigits: 0
});
</script>

<template>
  <v-card
    data-testid="todo-list-card"
    rounded="xl"
    elevation="1"
    @click="emit('open', list)"
  >
    <v-card-title class="d-flex align-center">
      <span>{{ list.name }}</span>
      <span v-if="list.itemCount != 0">- {{ percentageFormatter.format(list.completedItemCount! / list.itemCount!) }}</span>
      <v-spacer />
      <v-btn
        color="error"
        icon="mdi-delete"
        variant="text"
        @click="handleDeleteClick"
      />
    </v-card-title>
    <v-card-subtitle>
      Last updated {{ new Date(list.lastModified!).toLocaleString() }}
    </v-card-subtitle>
    <v-card-actions>
      <v-btn
        color="primary"
        variant="text"
        @click="emit('open', list)"
      >
        Open
      </v-btn>
      <v-spacer />
    </v-card-actions>
  </v-card>
</template>
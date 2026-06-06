<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { todoApi } from '@/services/todoApi'
import type { TodoItem, TodoList } from '@/types'

const route = useRoute()
const router = useRouter()

const list = ref<TodoList | null>(null)
const items = ref<TodoItem[]>([])
const loading = ref(false)
const saving = ref(false)
const editingItem = ref<TodoItem | null>(null)
const editedTitle = ref('')
const newItemTitle = ref('')
const errorMessage = ref('')

const listId = computed(() => String(route.params.listId ?? ''))
const completedCount = computed(() => items.value.filter((item) => item.isCompleted).length)
const editDialogOpen = computed({
  get: () => editingItem.value !== null,
  set: (isOpen: boolean) => {
    if (!isOpen) {
      editingItem.value = null
      editedTitle.value = ''
    }
  },
})

async function loadList() {
  if (!listId.value) {
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    const [loadedList, loadedItems] = await Promise.all([
      todoApi.getList(listId.value),
      todoApi.getItems(listId.value),
    ])

    list.value = loadedList
    items.value = loadedItems
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to load the list.'
  } finally {
    loading.value = false
  }
}

async function addItem() {
  if (!listId.value) {
    return
  }

  saving.value = true
  errorMessage.value = ''

  try {
    const item = await todoApi.createItem(listId.value, { title: newItemTitle.value })
    items.value = [item, ...items.value]
    newItemTitle.value = ''
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to add the item.'
  } finally {
    saving.value = false
  }
}

async function toggleCompletion(item: TodoItem) {
  if (!listId.value) {
    return
  }

  try {
    const updated = await todoApi.setItemCompletion(listId.value, item.id, {
      isCompleted: !item.isCompleted,
    })

    items.value = items.value
      .map((existingItem) => (existingItem.id === item.id ? updated : existingItem))
      .sort(compareItems)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to update the item.'
  }
}

function openEditDialog(item: TodoItem) {
  editingItem.value = item
  editedTitle.value = item.title
}

async function saveEdit() {
  if (!listId.value || !editingItem.value) {
    return
  }

  saving.value = true
  errorMessage.value = ''

  try {
    const updated = await todoApi.updateItem(listId.value, editingItem.value.id, {
      title: editedTitle.value,
    })

    items.value = items.value
      .map((existingItem) => (existingItem.id === updated.id ? updated : existingItem))
      .sort(compareItems)

    editingItem.value = null
    editedTitle.value = ''
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to save the item.'
  } finally {
    saving.value = false
  }
}

async function deleteItem(itemId: string) {
  if (!listId.value) {
    return
  }

  try {
    await todoApi.deleteItem(listId.value, itemId)
    items.value = items.value.filter((item) => item.id !== itemId)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to delete the item.'
  }
}

function compareItems(left: TodoItem, right: TodoItem) {
  if (left.isCompleted !== right.isCompleted) {
    return Number(left.isCompleted) - Number(right.isCompleted)
  }

  return new Date(right.createdDate).getTime() - new Date(left.createdDate).getTime()
}

watch(
  () => route.params.listId,
  () => {
    void loadList()
  },
)

onMounted(() => {
  void loadList()
})
</script>

<template>
  <div class="d-flex flex-column ga-6">
    <div class="d-flex flex-wrap justify-space-between align-center ga-4">
      <div>
        <v-btn
          variant="text"
          class="mb-2 px-0"
          @click="router.push({ name: 'lists' })"
        >
          &larr; Back to lists
        </v-btn>
        <h1 class="text-h4">
          {{ list?.name ?? 'Loading list...' }}
        </h1>
        <p class="text-body-1 text-medium-emphasis">
          {{ completedCount }} of {{ items.length }} tasks completed
        </p>
      </div>
    </div>

    <v-alert
      v-if="errorMessage"
      type="error"
      variant="tonal"
    >
      {{ errorMessage }}
    </v-alert>

    <v-card
      rounded="xl"
      elevation="1"
    >
      <v-card-text class="d-flex flex-wrap ga-4 align-center">
        <v-text-field
          v-model="newItemTitle"
          data-testid="todo-item-input"
          label="Add a task"
          variant="outlined"
          hide-details
          class="flex-grow-1"
          @keyup.enter="addItem"
        />
        <v-btn
          data-testid="add-todo-item-button"
          color="primary"
          size="large"
          :loading="saving"
          @click="addItem"
        >
          Add task
        </v-btn>
      </v-card-text>
    </v-card>

    <v-progress-linear
      v-if="loading"
      color="primary"
      indeterminate
      rounded
    />

    <v-list
      v-else
      lines="two"
      bg-color="transparent"
      class="d-flex flex-column ga-3 pa-0"
    >
      <v-list-item
        v-for="item in items"
        :key="item.id"
        data-testid="todo-item-row"
        rounded="xl"
        class="bg-surface elevation-1"
      >
        <template #prepend>
          <v-checkbox-btn
            :model-value="item.isCompleted"
            color="primary"
            @update:model-value="toggleCompletion(item)"
          />
        </template>

        <v-list-item-title :class="{ 'text-decoration-line-through': item.isCompleted }">
          {{ item.title }}
        </v-list-item-title>
        <v-list-item-subtitle>
          Updated
          {{ new Date(item.lastModifiedDate ?? item.createdDate).toLocaleString() }}
        </v-list-item-subtitle>

        <template #append>
          <div class="d-flex ga-2">
            <v-btn
              icon="mdi-pencil"
              variant="text"
              @click="openEditDialog(item)"
            />
            <v-btn
              icon="mdi-delete"
              variant="text"
              color="error"
              @click="deleteItem(item.id)"
            />
          </div>
        </template>
      </v-list-item>

      <v-list-item
        v-if="items.length == 0"
        rounded="xl"
        class="bg-surface elevation-1"
      >
        <v-list-item-title>No tasks yet</v-list-item-title>
        <v-list-item-subtitle>Add your first task above.</v-list-item-subtitle>
      </v-list-item>
    </v-list>

    <v-dialog
      v-model="editDialogOpen"
      max-width="520"
    >
      <v-card rounded="xl">
        <v-card-title>Edit task</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="editedTitle"
            data-testid="edit-todo-item-input"
            label="Task title"
            variant="outlined"
            autofocus
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn
            variant="text"
            @click="editDialogOpen = false"
          >
            Cancel
          </v-btn>
          <v-btn
            color="primary"
            :loading="saving"
            @click="saveEdit"
          >
            Save
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

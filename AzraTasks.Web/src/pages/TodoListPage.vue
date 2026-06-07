<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import {
  createItem,
  deleteItem as deleteItemApi,
  getItems,
  getList,
  setItemCompletion,
  updateItem,
} from '@/services/api'
import type { TodoItemDto, TodoListDto } from '@/services/api'

const route = useRoute()
const router = useRouter()

const list = ref<TodoListDto | null>(null)
const items = ref<TodoItemDto[]>([])
const loading = ref(false)
const saving = ref(false)
const editingItem = ref<TodoItemDto | null>(null)
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
    const [listResult, itemsResult] = await Promise.all([
      getList({ path: { listId: listId.value }, throwOnError: true }),
      getItems({ path: { listId: listId.value }, throwOnError: true }),
    ])

    list.value = listResult.data
    items.value = itemsResult.data
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
    const { data: item } = await createItem({
      path: { listId: listId.value },
      body: { title: newItemTitle.value },
      throwOnError: true,
    })
    items.value = [item, ...items.value]
    newItemTitle.value = ''
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to add the item.'
  } finally {
    saving.value = false
  }
}

async function toggleCompletion(item: TodoItemDto) {
  if (!listId.value) {
    return
  }

  try {
    const { data: updated } = await setItemCompletion({
      path: { listId: listId.value, itemId: item.id! },
      body: { isCompleted: !item.isCompleted },
      throwOnError: true,
    })

    items.value = items.value
      .map((existingItem) => (existingItem.id === item.id ? updated : existingItem))
      .sort(compareItems)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to update the item.'
  }
}

function openEditDialog(item: TodoItemDto) {
  editingItem.value = item
  editedTitle.value = item.title ?? ''
}

async function saveEdit() {
  if (!listId.value || !editingItem.value) {
    return
  }

  saving.value = true
  errorMessage.value = ''

  try {
    const { data: updated } = await updateItem({
      path: { listId: listId.value, itemId: editingItem.value.id! },
      body: { title: editedTitle.value },
      throwOnError: true,
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

async function removeItem(itemId: string) {
  if (!listId.value) {
    return
  }

  try {
    await deleteItemApi({
      path: { listId: listId.value, itemId },
      throwOnError: true,
    })
    items.value = items.value.filter((item) => item.id !== itemId)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to delete the item.'
  }
}

function compareItems(left: TodoItemDto, right: TodoItemDto) {
  if (left.isCompleted !== right.isCompleted) {
    return Number(left.isCompleted) - Number(right.isCompleted)
  }

  return new Date(right.createdDate!).getTime() - new Date(left.createdDate!).getTime()
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
          {{ new Date(item.lastModifiedDate ?? item.createdDate!).toLocaleString() }}
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
              @click="removeItem(item.id!)"
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

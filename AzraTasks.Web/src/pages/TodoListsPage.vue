<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

import ListCard from '@/components/ListCard.vue'
import {
  createList as createListApi,
  deleteList as deleteListApi,
  getLists,
} from '@/services/api'
import type { TodoListDto } from '@/services/api'

const router = useRouter()

const lists = ref<TodoListDto[]>([])
const loading = ref(false)
const createDialogOpen = ref(false)

const creating = ref(false)
const deletingList = ref<TodoListDto | null>(null)
const newListName = ref('')
const errorMessage = ref('')

async function loadLists() {
  loading.value = true
  errorMessage.value = ''

  try {
    const { data } = await getLists({ throwOnError: true })
    lists.value = data
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to load your lists.'
  } finally {
    loading.value = false
  }
}

async function createList() {
  creating.value = true
  errorMessage.value = ''

  try {
    const { data: list } = await createListApi({
      body: { name: newListName.value },
      throwOnError: true,
    })
    createDialogOpen.value = false
    newListName.value = ''
    lists.value = [list, ...lists.value]
    await router.push({ name: 'list', params: { listId: list.id } })
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to create the list.'
  } finally {
    creating.value = false
  }
}

async function removeList(listId: string) {
  errorMessage.value = ''

  try {
    await deleteListApi({ path: { listId }, throwOnError: true })
    lists.value = lists.value.filter((list) => list.id !== listId)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to delete the list.'
  } finally {
    deletingList.value = null
  }
}

onMounted(() => {
  void loadLists()
})
</script>

<template>
  <div class="d-flex flex-column ga-6">
    <div class="d-flex flex-wrap justify-space-between align-center ga-4">
      <div>
        <h1 class="text-h4 mb-2">
          My lists
        </h1>
        <p class="text-body-1 text-medium-emphasis">
          Organize tasks into focused lists and keep progress moving.
        </p>
      </div>

      <v-btn
        data-testid="create-list-button"
        color="primary"
        size="large"
        prepend-icon="mdi-plus"
        @click="createDialogOpen = true"
      >
        New list
      </v-btn>
    </div>

    <v-alert
      v-if="errorMessage"
      type="error"
      variant="tonal"
    >
      {{ errorMessage }}
    </v-alert>

    <v-progress-linear
      v-if="loading"
      color="primary"
      indeterminate
      rounded
    />

    <v-row v-else>
      <v-col
        v-for="list in lists"
        :key="list.id"
        cols="12"
        md="6"
        lg="4"
      >
        <ListCard
          :list="list"
          @open="(list) => router.push({ name: 'list', params: { listId: list.id } })"
          @remove="(list) => { deletingList = list; }"
        />
      </v-col>

      <v-col
        v-if="!lists.length"
        cols="12"
      >
        <v-sheet
          rounded="xl"
          color="surface"
          class="pa-8 text-center"
        >
          <div class="text-h6 mb-2">
            No lists yet
          </div>
          <p class="text-body-2 text-medium-emphasis mb-4">
            Create your first list to start tracking work.
          </p>
          <v-btn
            color="primary"
            @click="createDialogOpen = true"
          >
            Create a list
          </v-btn>
        </v-sheet>
      </v-col>
    </v-row>

    <v-dialog
      v-model="createDialogOpen"
      max-width="480"
    >
      <v-card rounded="xl">
        <v-card-title>Create a new list</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newListName"
            data-testid="list-name-dialog-input"
            label="List name"
            variant="outlined"
            autofocus
            @keydown.enter="createList"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn
            variant="text"
            @click="createDialogOpen = false"
          >
            Cancel
          </v-btn>
          <v-btn
            color="primary"
            :loading="creating"
            @click="createList"
          >
            Create
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog
      max-width="480"
      :model-value="deletingList !== null"
      @update:model-value="(val) => { if (!val) deletingList = null }"
    >
      <v-card
        v-if="deletingList"
        rounded="xl"
      >
        <v-card-title>Delete list</v-card-title>
        <v-card-text>
          Are you sure you want to delete the list '{{ deletingList.name }}'?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn
            variant="text"
            @click="deletingList = null"
          >
            Cancel
          </v-btn>
          <v-btn
            color="error"
            @click="removeList(deletingList.id!)"
          >
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

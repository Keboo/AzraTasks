import { apiClient } from '@/services/apiClient'
import type {
  CreateTodoItemRequest,
  CreateTodoListRequest,
  SetTodoItemCompletionRequest,
  TodoItem,
  TodoList,
  UpdateTodoItemRequest,
} from '@/types'

export const todoApi = {
  getLists() {
    return apiClient.get<TodoList[]>('/api/todo-lists')
  },

  getList(listId: string) {
    return apiClient.get<TodoList>(`/api/todo-lists/${listId}`)
  },

  createList(request: CreateTodoListRequest) {
    return apiClient.post<TodoList>('/api/todo-lists', request)
  },

  deleteList(listId: string) {
    return apiClient.delete(`/api/todo-lists/${listId}`)
  },

  getItems(listId: string) {
    return apiClient.get<TodoItem[]>(`/api/todo-lists/${listId}/items`)
  },

  createItem(listId: string, request: CreateTodoItemRequest) {
    return apiClient.post<TodoItem>(`/api/todo-lists/${listId}/items`, request)
  },

  updateItem(listId: string, itemId: string, request: UpdateTodoItemRequest) {
    return apiClient.put<TodoItem>(`/api/todo-lists/${listId}/items/${itemId}`, request)
  },

  setItemCompletion(listId: string, itemId: string, request: SetTodoItemCompletionRequest) {
    return apiClient.put<TodoItem>(`/api/todo-lists/${listId}/items/${itemId}/completion`, request)
  },

  deleteItem(listId: string, itemId: string) {
    return apiClient.delete(`/api/todo-lists/${listId}/items/${itemId}`)
  },
}

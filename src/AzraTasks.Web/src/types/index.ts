export interface TodoList {
  id: string
  name: string
  createdDate: string
}

export interface TodoItem {
  id: string
  listId: string
  title: string
  isCompleted: boolean
  createdDate: string
  lastModifiedDate?: string
}

export interface UserInfo {
  userId: string
  userName: string
  email: string
  isAuthenticated: boolean
}

export interface LoginRequest {
  email: string
  password: string
  rememberMe?: boolean
}

export interface RegisterRequest {
  email: string
  password: string
  confirmPassword: string
}

export interface CreateTodoListRequest {
  name: string
}

export interface CreateTodoItemRequest {
  title: string
}

export interface UpdateTodoItemRequest {
  title: string
}

export interface SetTodoItemCompletionRequest {
  isCompleted: boolean
}

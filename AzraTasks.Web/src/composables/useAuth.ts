import { computed, reactive, readonly } from 'vue'

import { apiClient } from '@/services/apiClient'
import type { LoginRequest, RegisterRequest, UserInfo } from '@/types'

type AuthState = {
  user: UserInfo | null
  loading: boolean
  initialized: boolean
}

const state = reactive<AuthState>({
  user: null,
  loading: false,
  initialized: false,
})

function normalizeUser(user: UserInfo | null | undefined): UserInfo | null {
  return user?.isAuthenticated ? user : null
}

async function refreshUser() {
  state.loading = true

  try {
    const user = await apiClient.get<UserInfo>('/api/auth/user')
    state.user = normalizeUser(user)
  } catch {
    state.user = null
  } finally {
    state.loading = false
    state.initialized = true
  }
}

async function ensureInitialized() {
  if (state.initialized) {
    return
  }

  await refreshUser()
}

async function login(credentials: LoginRequest) {
  state.loading = true

  try {
    const user = await apiClient.post<UserInfo>('/api/auth/login', credentials)
    state.user = normalizeUser(user)
    state.initialized = true
  } finally {
    state.loading = false
  }
}

async function register(input: RegisterRequest) {
  state.loading = true

  try {
    const user = await apiClient.post<UserInfo>('/api/auth/register', input)
    state.user = normalizeUser(user)
    state.initialized = true
  } finally {
    state.loading = false
  }
}

async function logout() {
  state.loading = true

  try {
    await apiClient.post('/api/auth/logout')
    state.user = null
    state.initialized = true
  } finally {
    state.loading = false
  }
}

export function useAuth() {
  return {
    state: readonly(state),
    user: computed(() => state.user),
    isAuthenticated: computed(() => state.user?.isAuthenticated ?? false),
    loading: computed(() => state.loading),
    initialized: computed(() => state.initialized),
    ensureInitialized,
    refreshUser,
    login,
    register,
    logout,
  }
}

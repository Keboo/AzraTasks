import { computed, reactive, readonly } from 'vue'

import {
  getApiAuthUser,
  postApiAuthLogin,
  postApiAuthLogout,
  postApiAuthRegister,
} from '@/services/api'
import type { LoginRequest, RegisterRequest, UserInfo } from '@/services/api'

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
    const { data: user } = await getApiAuthUser({ throwOnError: true })
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
    await postApiAuthLogin({ body: credentials, throwOnError: true })
    await refreshUser()
  } finally {
    state.loading = false
  }
}

async function register(input: RegisterRequest) {
  state.loading = true

  try {
    await postApiAuthRegister({ body: input, throwOnError: true })
    await refreshUser()
  } finally {
    state.loading = false
  }
}

async function logout() {
  state.loading = true

  try {
    await postApiAuthLogout({ throwOnError: true })
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

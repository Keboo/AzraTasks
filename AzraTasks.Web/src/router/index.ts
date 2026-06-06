import { createRouter, createWebHistory } from 'vue-router'

import { useAuth } from '@/composables/useAuth'
import HomePage from '@/pages/HomePage.vue'
import LoginPage from '@/pages/LoginPage.vue'
import RegisterPage from '@/pages/RegisterPage.vue'
import TodoListPage from '@/pages/TodoListPage.vue'
import TodoListsPage from '@/pages/TodoListsPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomePage,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginPage,
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterPage,
      meta: { guestOnly: true },
    },
    {
      path: '/lists',
      name: 'lists',
      component: TodoListsPage,
      meta: { requiresAuth: true },
    },
    {
      path: '/lists/:listId',
      name: 'list',
      component: TodoListPage,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuth()
  await auth.ensureInitialized()

  if (to.meta.requiresAuth && !auth.isAuthenticated.value) {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (to.meta.guestOnly && auth.isAuthenticated.value) {
    return { name: 'lists' }
  }

  return true
})

export default router

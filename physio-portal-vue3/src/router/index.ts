import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../store/auth'
import PortalLayout from '../layouts/PortalLayout.vue'

const router = createRouter({
  history: createWebHistory('/portal/'),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/verify-email',
      name: 'verify-email',
      component: () => import('../views/VerifyEmailView.vue'),
    },
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: () => import('../views/ForgotPasswordView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: () => import('../views/ResetPasswordView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/',
      component: PortalLayout,
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          redirect: '/dashboard',
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('../views/DashboardView.vue'),
          meta: { title: 'Dashboard' },
        },
        {
          path: 'admin/physios',
          name: 'admin-physios',
          component: () => import('../views/AdminPhysiosView.vue'),
          meta: { title: 'Admin Physio Management' },
        },
        {
          path: 'patients',
          name: 'patients',
          component: () => import('../views/PatientsView.vue'),
          meta: { title: 'Patients' },
        },
        {
          path: 'patients/:petId',
          name: 'patient-detail',
          component: () => import('../views/PatientsView.vue'),
          meta: { title: 'Patients' },
        },
        {
          path: 'appointments',
          name: 'appointments',
          component: () => import('../views/AppointmentsView.vue'),
          meta: { title: 'Appointments' },
        },
        {
          path: 'treatment-plans',
          name: 'treatment-plans',
          component: () => import('../views/TreatmentPlansView.vue'),
          meta: { title: 'Treatment Plans' },
        },
        {
          path: 'treatment-plans/:petId',
          name: 'treatment-plan-detail',
          component: () => import('../views/TreatmentPlansView.vue'),
          meta: { title: 'Treatment Plans' },
        },
        {
          path: 'exercises',
          name: 'exercises',
          component: () => import('../views/ExercisesView.vue'),
          meta: { title: 'Exercise Library' },
        },
        {
          path: 'progress',
          name: 'progress',
          component: () => import('../views/ProgressView.vue'),
          meta: { title: 'Progress' },
        },
        {
          path: 'progress/:petId',
          name: 'progress-detail',
          component: () => import('../views/ProgressView.vue'),
          meta: { title: 'Progress' },
        },
        {
          path: 'messages/:petId?',
          name: 'messages',
          component: () => import('../views/MessagesView.vue'),
          meta: { title: 'Messages' },
        },
        {
          path: 'messages/thread/:petId?',
          name: 'message-thread',
          component: () => import('../views/MessagesView.vue'),
          meta: { title: 'Messages' },
        },
        {
          path: 'reports',
          name: 'reports',
          component: () => import('../views/ReportsView.vue'),
          meta: { title: 'Reports' },
        },
        {
          path: 'documents',
          name: 'documents',
          component: () => import('../views/DocumentsView.vue'),
          meta: { title: 'Documents' },
        },
        {
          path: 'tasks',
          name: 'tasks',
          component: () => import('../views/TasksView.vue'),
          meta: { title: 'Tasks' },
        },
        {
          path: 'billing',
          name: 'billing',
          component: () => import('../views/BillingView.vue'),
          meta: { title: 'Billing' },
        },
        {
          path: 'settings',
          name: 'settings',
          component: () => import('../views/SettingsView.vue'),
          meta: { title: 'Settings' },
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)

  if (requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guestOnly && auth.isAuthenticated) {
    if (auth.user?.userRole === 'SysAdmin') {
      return { name: 'admin-physios' }
    }
    return { name: 'dashboard' }
  }

  if (auth.isAuthenticated && auth.user?.userRole === 'SysAdmin') {
    const physioOnlyRoutes = [
      'dashboard',
      'patients',
      'patient-detail',
      'appointments',
      'treatment-plans',
      'treatment-plan-detail',
      'progress',
      'progress-detail',
      'messages',
      'message-thread',
      'reports',
      'documents',
      'tasks',
      'billing',
    ]
    if (physioOnlyRoutes.includes(String(to.name))) {
      return { name: 'admin-physios' }
    }
  }

  return true
})

export default router

import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import * as authApi from '../api/auth'
import { setAuthTokens } from '../api/client'
import type { AuthResponse, AuthUser, LoginRequest, UpdateProfileRequest } from '../types/auth'

const STORAGE_KEY = 'kpw_auth'

interface StoredAuth {
  accessToken: string
  refreshToken: string
  user: AuthUser
}

function loadStoredAuth(): StoredAuth | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as StoredAuth
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const accessToken = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const message = ref<string | null>(null)

  const isAuthenticated = computed(() => !!accessToken.value && !!user.value)

  function persist() {
    if (accessToken.value && refreshToken.value && user.value) {
      localStorage.setItem(
        STORAGE_KEY,
        JSON.stringify({
          accessToken: accessToken.value,
          refreshToken: refreshToken.value,
          user: user.value,
        }),
      )
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  function applyAuth(response: AuthResponse) {
    accessToken.value = response.accessToken
    refreshToken.value = response.refreshToken
    user.value = response.user
    setAuthTokens(response.accessToken, response.refreshToken)
    persist()
  }

  function initialize() {
    const stored = loadStoredAuth()
    if (stored) {
      accessToken.value = stored.accessToken
      refreshToken.value = stored.refreshToken
      user.value = stored.user
      setAuthTokens(stored.accessToken, stored.refreshToken)
    }
  }

  async function login(payload: LoginRequest) {
    loading.value = true
    error.value = null
    try {
      const data = await authApi.login(payload)
      applyAuth(data)
    } catch (err: unknown) {
      error.value = 'Invalid email or password.'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchCurrentUser() {
    const data = await authApi.fetchCurrentUser()
    user.value = data
    persist()
  }

  async function updateProfile(payload: UpdateProfileRequest) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const updatedUser = await authApi.updateProfile(payload)
      user.value = updatedUser
      persist()
      message.value = 'Profile updated successfully.'
      return true
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to update profile.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function forgotPassword(email: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.forgotPassword({ email })
      message.value = data.message
      return data.message
    } catch {
      error.value = 'Unable to send reset instructions.'
      return null
    } finally {
      loading.value = false
    }
  }

  async function resetPassword(token: string, newPassword: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.resetPassword({ token, newPassword })
      message.value = data.message
      return true
    } catch {
      error.value = 'Invalid or expired reset link.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function changePassword(currentPassword: string, newPassword: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.changePassword({ currentPassword, newPassword })
      message.value = data.message
      return true
    } catch {
      error.value = 'Unable to change password. Check your current password.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function sendOwnerInvite(recipientEmail: string, ownerName?: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.sendOwnerInvite({ recipientEmail, ownerName })
      message.value = data.message
      return data.message
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to send invite email.'
      throw err
    } finally {
      loading.value = false
    }
  }

  function logout() {
    accessToken.value = null
    refreshToken.value = null
    user.value = null
    setAuthTokens(null, null)
    persist()
  }

  return {
    user,
    accessToken,
    refreshToken,
    loading,
    error,
    message,
    isAuthenticated,
    initialize,
    login,
    fetchCurrentUser,
    updateProfile,
    forgotPassword,
    resetPassword,
    changePassword,
    sendOwnerInvite,
    logout,
  }
})

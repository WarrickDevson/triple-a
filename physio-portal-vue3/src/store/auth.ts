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
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Invalid email or password.'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function register(payload: import('../types/auth').RegisterRequest) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.register(payload)
      return data
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Registration failed. Please check details.'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function verifyEmail(email: string, token: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.verifyEmail({ email, token })
      message.value = data.message
      if (user.value) {
        user.value = {
          ...user.value,
          isEmailVerified: true,
          isApproved: data.isApproved,
        }
        persist()
      }
      return data
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Verification failed or link expired.'
      return null
    } finally {
      loading.value = false
    }
  }

  async function resendVerification(email: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.resendVerification({ email })
      message.value = data.message
      return true
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to resend verification link.'
      return false
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

  async function fetchPendingPhysios() {
    loading.value = true
    error.value = null
    try {
      return await authApi.fetchPendingPhysios()
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to load physios.'
      return []
    } finally {
      loading.value = false
    }
  }

  async function approvePhysio(userId: number) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.approvePhysio(userId)
      message.value = data.message
      return true
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to approve physio.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function rejectPhysio(userId: number) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.rejectPhysio(userId)
      message.value = data.message
      return true
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to reject physio.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function markEmailVerified(userId: number) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.markEmailVerified(userId)
      message.value = data.message
      return true
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to mark email as verified.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function sendAdminInvite(recipientEmail: string, clinicName?: string) {
    loading.value = true
    error.value = null
    message.value = null
    try {
      const data = await authApi.sendAdminInvite({ recipientEmail, clinicName })
      message.value = data.message
      return true
    } catch (err: any) {
      error.value = err?.response?.data?.message || 'Failed to send invitation.'
      return false
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
    register,
    verifyEmail,
    resendVerification,
    fetchCurrentUser,
    updateProfile,
    forgotPassword,
    resetPassword,
    changePassword,
    sendOwnerInvite,
    fetchPendingPhysios,
    approvePhysio,
    rejectPhysio,
    markEmailVerified,
    sendAdminInvite,
    logout,
  }
})

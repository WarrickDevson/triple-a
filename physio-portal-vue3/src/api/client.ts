import axios from 'axios'
import { API_BASE_URL } from './config'

export { API_BASE_URL }

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

function getStoredTokens(): { access: string | null; refresh: string | null } {
  try {
    const raw = typeof localStorage !== 'undefined' ? localStorage.getItem('kpw_auth') : null
    if (!raw) return { access: null, refresh: null }
    const parsed = JSON.parse(raw)
    return {
      access: parsed.accessToken || null,
      refresh: parsed.refreshToken || null,
    }
  } catch {
    return { access: null, refresh: null }
  }
}

const initialTokens = getStoredTokens()
let accessToken: string | null = initialTokens.access
let refreshToken: string | null = initialTokens.refresh
let onUnauthorized: (() => void) | null = null

export function setAuthTokens(access: string | null, refresh: string | null) {
  accessToken = access
  refreshToken = refresh
}

export function setUnauthorizedHandler(handler: () => void) {
  onUnauthorized = handler
}

apiClient.interceptors.request.use((config) => {
  if (!accessToken) {
    const stored = getStoredTokens()
    if (stored.access) {
      accessToken = stored.access
      refreshToken = stored.refresh
    }
  }

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const url = originalRequest?.url || ''
    const isAuthEndpoint =
      url.includes('/api/auth/login') ||
      url.includes('/api/auth/change-password') ||
      url.includes('/api/auth/refresh') ||
      url.includes('/api/auth/forgot-password') ||
      url.includes('/api/auth/reset-password')

    // Avoid multiple simultaneous refresh loops
    if (error.response?.status === 401 && refreshToken && !originalRequest._retry && !isAuthEndpoint) {
      originalRequest._retry = true
      try {
        const { data } = await axios.post(`${API_BASE_URL}/api/auth/refresh`, {
          refreshToken,
        })
        setAuthTokens(data.accessToken, data.refreshToken)
        if (typeof localStorage !== 'undefined') {
          const raw = localStorage.getItem('kpw_auth')
          if (raw) {
            const parsed = JSON.parse(raw)
            parsed.accessToken = data.accessToken
            parsed.refreshToken = data.refreshToken
            localStorage.setItem('kpw_auth', JSON.stringify(parsed))
          }
        }
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
        return apiClient(originalRequest)
      } catch {
        onUnauthorized?.()
      }
    }

    if (error.response?.status === 401 && !isAuthEndpoint) {
      onUnauthorized?.()
    }

    return Promise.reject(error)
  },
)

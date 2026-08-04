import axios from 'axios'
import { API_BASE_URL } from './config'

export { API_BASE_URL }

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

let accessToken: string | null = null
let refreshToken: string | null = null
let onUnauthorized: (() => void) | null = null

export function setAuthTokens(access: string | null, refresh: string | null) {
  accessToken = access
  refreshToken = refresh
}

export function setUnauthorizedHandler(handler: () => void) {
  onUnauthorized = handler
}

apiClient.interceptors.request.use((config) => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && refreshToken && !originalRequest._retry) {
      originalRequest._retry = true
      try {
        const { data } = await axios.post(`${API_BASE_URL}/api/auth/refresh`, {
          refreshToken,
        })
        setAuthTokens(data.accessToken, data.refreshToken)
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
        return apiClient(originalRequest)
      } catch {
        onUnauthorized?.()
      }
    }

    if (error.response?.status === 401) {
      onUnauthorized?.()
    }

    return Promise.reject(error)
  },
)

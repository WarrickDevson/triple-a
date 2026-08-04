import { apiClient } from './client'
import type {
  AuthResponse,
  AuthUser,
  ChangePasswordRequest,
  ForgotPasswordRequest,
  LoginRequest,
  MessageResponse,
  RegisterRequest,
  ResetPasswordRequest,
} from '../types/auth'

export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/auth/login', payload)
  return data
}

export async function register(payload: RegisterRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/auth/register', payload)
  return data
}

export async function fetchCurrentUser(): Promise<AuthUser> {
  const { data } = await apiClient.get<AuthUser>('/api/auth/me')
  return data
}

export async function forgotPassword(payload: ForgotPasswordRequest): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>('/api/auth/forgot-password', payload)
  return data
}

export async function resetPassword(payload: ResetPasswordRequest): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>('/api/auth/reset-password', payload)
  return data
}

export async function changePassword(payload: ChangePasswordRequest): Promise<MessageResponse> {
  const { data } = await apiClient.put<MessageResponse>('/api/auth/change-password', payload)
  return data
}

import { apiClient } from './client'
import type {
  AdminPurgeUserRequest,
  AdminUserSummary,
  AuthResponse,
  AuthUser,
  ChangePasswordRequest,
  ForgotPasswordRequest,
  LoginRequest,
  MessageResponse,
  PhysioApproval,
  RegisterRequest,
  ResendVerificationEmailRequest,
  ResetPasswordRequest,
  SendAdminInviteRequest,
  SendOwnerInviteRequest,
  UpdateProfileRequest,
  VerifyEmailRequest,
  VerifyEmailResponse,
} from '../types/auth'

export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/auth/login', payload)
  return data
}

export async function register(payload: RegisterRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/auth/register', {
    role: 'Physio',
    ...payload,
  })
  return data
}

export async function checkEmail(email: string): Promise<{ exists: boolean; message?: string }> {
  const { data } = await apiClient.get<{ exists: boolean; message?: string }>('/api/auth/check-email', {
    params: { email: email.trim() },
  })
  return data
}

export async function verifyEmail(payload: VerifyEmailRequest): Promise<VerifyEmailResponse> {
  const { data } = await apiClient.post<VerifyEmailResponse>('/api/auth/verify-email', payload)
  return data
}

export async function resendVerification(payload: ResendVerificationEmailRequest): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>('/api/auth/resend-verification', payload)
  return data
}

export async function fetchCurrentUser(): Promise<AuthUser> {
  const { data } = await apiClient.get<AuthUser>('/api/auth/me')
  return data
}

export async function updateProfile(payload: UpdateProfileRequest): Promise<AuthUser> {
  const { data } = await apiClient.put<AuthUser>('/api/auth/profile', payload)
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

export async function sendOwnerInvite(payload: SendOwnerInviteRequest): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>('/api/auth/send-owner-invite', payload)
  return data
}

export async function fetchPendingPhysios(): Promise<PhysioApproval[]> {
  const { data } = await apiClient.get<PhysioApproval[]>('/api/admin/physios')
  return data
}

export async function approvePhysio(userId: number): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>(`/api/admin/physios/${userId}/approve`)
  return data
}

export async function rejectPhysio(userId: number): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>(`/api/admin/physios/${userId}/reject`)
  return data
}

export async function markEmailVerified(userId: number): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>(`/api/admin/physios/${userId}/verify-email`)
  return data
}

export async function sendAdminInvite(payload: SendAdminInviteRequest): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>('/api/admin/send-physio-invite', payload)
  return data
}

export async function fetchAdminUsers(query?: string, role?: string): Promise<AdminUserSummary[]> {
  const params = new URLSearchParams()
  if (query) params.append('query', query)
  if (role) params.append('role', role)
  const qs = params.toString() ? `?${params.toString()}` : ''
  const { data } = await apiClient.get<AdminUserSummary[]>(`/api/admin/users${qs}`)
  return data
}

export async function purgeUserData(userId: number, payload?: AdminPurgeUserRequest): Promise<MessageResponse> {
  const { data } = await apiClient.post<MessageResponse>(`/api/admin/users/${userId}/purge`, payload || {})
  return data
}


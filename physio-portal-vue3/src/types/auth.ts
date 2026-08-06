export interface AuthUser {
  userId: number
  email: string
  firstName: string
  lastName: string
  userRole: string
  subscriptionTier: string
  clinicId: number | null
  clinicName?: string | null
  clinicInviteCode?: string | null
  isEmailVerified?: boolean
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthUser
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  phoneNumber?: string
  inviteCode: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface SendOwnerInviteRequest {
  recipientEmail: string
  ownerName?: string
}

export interface MessageResponse {
  message: string
}

export interface UpdateProfileRequest {
  firstName: string
  lastName: string
  phoneNumber?: string
  clinicName?: string
}

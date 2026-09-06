import { apiClient, apiFetchJson } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import { clearSession, saveAccessToken } from "@/shared/auth/session.store"
import type { components } from "@/shared/api/generated/schema"

type LoginUserRequest = components["schemas"]["LoginUserRequest"]
type RegisterUserRequest = components["schemas"]["RegisterUserRequest"]
type TokenResponse = components["schemas"]["TokenResponse"]
export type UpdateUserPreferencesRequest = components["schemas"]["UpdateUserPreferencesRequest"]
export type PrivacyPolicy = components["schemas"]["PrivacyPolicyResponse"]
export type UserProfile = components["schemas"]["UserProfileResponse"]

export interface RegisterUserResponse {
  email: string
  codeExpiresAt: string
  resendAvailableAt: string
}

export interface ConfirmEmailVerificationRequest {
  email: string
  code: string
}

export interface RequestEmailChangeRequest {
  newEmail: string
  currentPassword: string
}

export async function login(request: LoginUserRequest) {
  const tokens = await unwrapApiResponse<TokenResponse>(
    apiClient.POST("/api/auth/login", {
      body: request,
    }),
  )

  saveAccessToken(tokens)
  return tokens
}

export async function register(request: RegisterUserRequest) {
  return apiFetchJson<RegisterUserResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify(request),
  })
}

export async function refreshSession() {
  const tokens = await unwrapApiResponse<TokenResponse>(apiClient.POST("/api/auth/refresh"))

  saveAccessToken(tokens)
  return tokens
}

export async function getCurrentUser() {
  return await unwrapApiResponse(apiClient.GET("/api/auth/me"))
}

export async function updateUserPreferences(request: UpdateUserPreferencesRequest) {
  return await unwrapApiResponse(
    apiClient.PUT("/api/auth/me/preferences", {
      body: request,
    }),
  )
}

export async function logout() {
  await unwrapEmptyApiResponse(apiClient.POST("/api/auth/logout"))
  clearSession()
}

export function confirmEmailVerification(request: ConfirmEmailVerificationRequest) {
  return apiFetchJson<undefined>("/api/auth/email-verification/confirm", {
    method: "POST",
    body: JSON.stringify(request),
  })
}

export function resendEmailVerification(email: string) {
  return apiFetchJson<undefined>("/api/auth/email-verification/resend", {
    method: "POST",
    body: JSON.stringify({ email }),
  })
}

export function requestPasswordReset(email: string) {
  return apiFetchJson<undefined>("/api/auth/password-reset/request", {
    method: "POST",
    body: JSON.stringify({ email }),
  })
}

export function completePasswordReset(token: string, newPassword: string) {
  return apiFetchJson<undefined>("/api/auth/password-reset/complete", {
    method: "POST",
    body: JSON.stringify({ token, newPassword }),
  })
}

export function updateDisplayName(displayName: string) {
  return apiFetchJson<UserProfile>("/api/auth/me/display-name", {
    method: "PATCH",
    body: JSON.stringify({ displayName }),
  })
}

export function requestEmailChange(request: RequestEmailChangeRequest) {
  return apiFetchJson<undefined>("/api/auth/me/email-change/request", {
    method: "POST",
    body: JSON.stringify(request),
  })
}

export async function confirmEmailChange(code: string) {
  await apiFetchJson<undefined>("/api/auth/me/email-change/confirm", {
    method: "POST",
    body: JSON.stringify({ code }),
  })
  clearSession()
}

export async function changePassword(currentPassword: string, newPassword: string) {
  await apiFetchJson<undefined>("/api/auth/me/password/change", {
    method: "POST",
    body: JSON.stringify({ currentPassword, newPassword }),
  })
  clearSession()
}

export function getPrivacyPolicy() {
  return apiFetchJson<PrivacyPolicy>("/api/legal/privacy-policy")
}

export function acceptPrivacyPolicy(privacyPolicyVersion: string) {
  return apiFetchJson<UserProfile>("/api/auth/me/privacy-policy/accept", {
    method: "POST",
    body: JSON.stringify({ privacyPolicyVersion }),
  })
}

export async function deleteAccount(currentPassword: string) {
  await apiFetchJson<undefined>("/api/auth/me/delete", {
    method: "POST",
    body: JSON.stringify({ currentPassword }),
  })
  clearSession()
}

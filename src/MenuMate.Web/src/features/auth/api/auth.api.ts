import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import { clearSession, saveAccessToken } from "@/shared/auth/session.store"
import type { components } from "@/shared/api/generated/schema"

type LoginUserRequest = components["schemas"]["LoginUserRequest"]
type RegisterUserRequest = components["schemas"]["RegisterUserRequest"]
type RegisterUserResponse = components["schemas"]["RegisterUserResponse"]
type TokenResponse = components["schemas"]["TokenResponse"]
export type UserProfile = components["schemas"]["UserProfileResponse"]

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
  const response = await unwrapApiResponse<RegisterUserResponse>(
    apiClient.POST("/api/auth/register", {
      body: request,
    }),
  )

  saveAccessToken(response.tokens)
  return response
}

export async function refreshSession() {
  const tokens = await unwrapApiResponse<TokenResponse>(apiClient.POST("/api/auth/refresh"))

  saveAccessToken(tokens)
  return tokens
}

export async function getCurrentUser() {
  return unwrapApiResponse<UserProfile>(apiClient.GET("/api/auth/me"))
}

export async function logout() {
  await unwrapEmptyApiResponse(apiClient.POST("/api/auth/logout"))
  clearSession()
}

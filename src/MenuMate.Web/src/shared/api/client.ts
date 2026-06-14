import createFetchClient from "openapi-fetch"
import createQueryClient from "openapi-react-query"

import { apiUrl } from "@/shared/config/api"
import { clearSession, getAccessToken, saveAccessToken } from "@/shared/auth/session.store"
import { toApiException } from "@/shared/api/errors"
import type { components, paths } from "@/shared/api/generated/schema"

type TokenResponse = components["schemas"]["TokenResponse"]

let refreshPromise: Promise<string | null> | null = null

export const apiClient = createFetchClient<paths>({
  baseUrl: apiUrl(""),
  credentials: "include",
  fetch: authFetch,
})

export const apiQueryClient = createQueryClient(apiClient)

export async function apiFetchJson<TData>(path: string, init?: RequestInit): Promise<TData> {
  const headers = new Headers(init?.headers)
  if (init?.body && !(init.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json")
  }

  const response = await authFetch(apiUrl(path), {
    ...init,
    headers,
  })

  if (!response.ok) {
    let error: unknown = response.statusText
    try {
      error = await response.json()
    } catch {
      // Keep the status text when the response is not JSON.
    }

    throw toApiException(error, response.status)
  }

  if (response.status === 204) {
    return undefined as TData
  }

  return (await response.json()) as TData
}

export async function apiFetchBlob(path: string, init?: RequestInit): Promise<Blob> {
  const headers = new Headers(init?.headers)
  if (init?.body && !(init.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json")
  }

  const response = await authFetch(apiUrl(path), {
    ...init,
    headers,
  })

  if (!response.ok) {
    let error: unknown = response.statusText
    try {
      error = await response.json()
    } catch {
      // Keep the status text when the response is not JSON.
    }

    throw toApiException(error, response.status)
  }

  return response.blob()
}

async function authFetch(input: RequestInfo | URL, init?: RequestInit) {
  const response = await fetchWithAccessToken(input, init, getAccessToken())

  if (response.status !== 401 || isAutoRefreshDisabledRequest(input)) {
    return response
  }

  const refreshedAccessToken = await refreshAccessToken()
  if (!refreshedAccessToken) {
    return response
  }

  return fetchWithAccessToken(input, init, refreshedAccessToken)
}

async function fetchWithAccessToken(
  input: RequestInfo | URL,
  init: RequestInit | undefined,
  accessToken: string | null,
) {
  const headers = createHeaders(input, init)

  if (accessToken) {
    headers.set("Authorization", `Bearer ${accessToken}`)
  }

  return fetch(input, {
    ...init,
    credentials: "include",
    headers,
  })
}

async function refreshAccessToken() {
  refreshPromise ??= refreshAccessTokenCore().finally(() => {
    refreshPromise = null
  })

  return refreshPromise
}

async function refreshAccessTokenCore() {
  const response = await fetch(apiUrl("/api/auth/refresh"), {
    method: "POST",
    credentials: "include",
  })

  if (!response.ok) {
    clearSession()
    return null
  }

  const tokens = (await response.json()) as unknown
  if (!isTokenResponse(tokens)) {
    clearSession()
    return null
  }

  saveAccessToken(tokens)

  return tokens.accessToken
}

function isTokenResponse(value: unknown): value is TokenResponse {
  return (
    typeof value === "object" &&
    value !== null &&
    "accessToken" in value &&
    typeof value.accessToken === "string" &&
    "expiresAt" in value &&
    typeof value.expiresAt === "string"
  )
}

function isAutoRefreshDisabledRequest(input: RequestInfo | URL) {
  const requestUrl = getRequestUrl(input)

  return requestUrl.includes("/api/auth/refresh") || requestUrl.includes("/api/auth/logout")
}

function getRequestUrl(input: RequestInfo | URL) {
  if (typeof input === "string") {
    return input
  }

  if (input instanceof URL) {
    return input.href
  }

  return input.url
}

function createHeaders(input: RequestInfo | URL, init: RequestInit | undefined) {
  const headers = new Headers(input instanceof Request ? input.headers : undefined)

  new Headers(init?.headers).forEach((value, key) => {
    headers.set(key, value)
  })

  return headers
}

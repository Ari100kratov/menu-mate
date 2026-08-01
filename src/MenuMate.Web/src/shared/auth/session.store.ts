import { create } from "zustand"

import type { components } from "@/shared/api/generated/schema"

type TokenResponse = components["schemas"]["TokenResponse"]
type UserProfileResponse = components["schemas"]["UserProfileResponse"]

interface SessionState {
  accessToken: string | null
  accessTokenExpiresAt: string | null
  refreshBlocked: boolean
  offlineAccess: boolean
  user: UserProfileResponse | null
  setTokens: (tokens: TokenResponse) => void
  setUser: (user: UserProfileResponse | null) => void
  startOfflineAccess: (user: UserProfileResponse) => void
  endOfflineAccess: () => void
  clear: () => void
}

export const useSessionStore = create<SessionState>((set) => ({
  accessToken: null,
  accessTokenExpiresAt: null,
  refreshBlocked: false,
  offlineAccess: false,
  user: null,
  setTokens: (tokens) => {
    set({
      accessToken: tokens.accessToken,
      accessTokenExpiresAt: tokens.expiresAt,
      refreshBlocked: false,
      offlineAccess: false,
    })
  },
  setUser: (user) => {
    set({ user })
  },
  startOfflineAccess: (user) => {
    set({
      offlineAccess: true,
      user,
    })
  },
  endOfflineAccess: () => {
    set({
      offlineAccess: false,
      user: null,
    })
  },
  clear: () => {
    set({
      accessToken: null,
      accessTokenExpiresAt: null,
      refreshBlocked: true,
      offlineAccess: false,
      user: null,
    })
  },
}))

export function getAccessToken() {
  return useSessionStore.getState().accessToken
}

export function saveAccessToken(tokens: TokenResponse) {
  useSessionStore.getState().setTokens(tokens)
}

export function clearSession() {
  useSessionStore.getState().clear()
}

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"

import {
  getCurrentUser,
  login,
  logout,
  refreshSession,
  register,
} from "@/features/auth/api/auth.api"
import { clearSession, useSessionStore } from "@/shared/auth/session.store"

export const authQueryKeys = {
  currentUser: ["auth", "current-user"] as const,
  refresh: ["auth", "refresh"] as const,
}

export function useCurrentUserQuery() {
  const setUser = useSessionStore((state) => state.setUser)

  return useQuery({
    queryKey: authQueryKeys.currentUser,
    queryFn: async () => {
      const user = await getCurrentUser()
      setUser(user)
      return user
    },
    staleTime: 60_000,
  })
}

export function useRefreshSessionQuery(enabled: boolean) {
  return useQuery({
    queryKey: authQueryKeys.refresh,
    queryFn: refreshSession,
    enabled,
    retry: false,
    staleTime: Number.POSITIVE_INFINITY,
  })
}

export function useLoginMutation() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: login,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.currentUser })
      void navigate("/", { replace: true })
    },
  })
}

export function useRegisterMutation() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: register,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.currentUser })
      void navigate("/recipes", { replace: true })
    },
  })
}

export function useLogoutMutation() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: logout,
    onSettled: () => {
      clearSession()
      queryClient.clear()
      void navigate("/login", { replace: true })
    },
  })
}

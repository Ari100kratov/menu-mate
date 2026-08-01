import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"

import {
  getCurrentUser,
  login,
  logout,
  refreshSession,
  register,
  updateUserPreferences,
  type UserProfile,
  type UpdateUserPreferencesRequest,
} from "@/features/auth/api/auth.api"
import { offlineShoppingRecordQueryKey } from "@/features/shopping-lists/api/shopping-list-offline.queries"
import { clearOfflineShoppingRecord } from "@/features/shopping-lists/model/shopping-list-offline.storage"
import { clearSession, useSessionStore } from "@/shared/auth/session.store"

export const authQueryKeys = {
  currentUser: ["auth", "current-user"] as const,
  refresh: ["auth", "refresh"] as const,
}

export function useCurrentUserQuery(enabled = true) {
  const setUser = useSessionStore((state) => state.setUser)

  return useQuery({
    queryKey: authQueryKeys.currentUser,
    queryFn: async () => {
      const user = await getCurrentUser()
      setUser(user)
      return user
    },
    staleTime: 60_000,
    enabled,
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
    networkMode: "always",
    onSettled: async () => {
      try {
        await clearOfflineShoppingRecord()
      } finally {
        clearSession()
        queryClient.removeQueries({ queryKey: offlineShoppingRecordQueryKey })
        queryClient.clear()
        void navigate("/login", { replace: true })
      }
    },
  })
}

export function useUpdateUserPreferencesMutation() {
  const queryClient = useQueryClient()
  const setUser = useSessionStore((state) => state.setUser)

  return useMutation({
    mutationFn: (request: UpdateUserPreferencesRequest) => updateUserPreferences(request),
    onMutate: async (request) => {
      await queryClient.cancelQueries({ queryKey: authQueryKeys.currentUser })
      const previous = queryClient.getQueryData<UserProfile>(authQueryKeys.currentUser)
      const optimistic = previous
        ? {
            ...previous,
            preferences: request,
          }
        : undefined

      if (optimistic) {
        queryClient.setQueryData(authQueryKeys.currentUser, optimistic)
        setUser(optimistic)
      }

      return { previous }
    },
    onError: (_error, _request, context) => {
      if (context?.previous) {
        queryClient.setQueryData(authQueryKeys.currentUser, context.previous)
        setUser(context.previous)
      }
    },
    onSuccess: (user) => {
      queryClient.setQueryData(authQueryKeys.currentUser, user)
      setUser(user)
    },
  })
}

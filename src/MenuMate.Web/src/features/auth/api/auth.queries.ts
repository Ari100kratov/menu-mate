import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"

import {
  acceptPrivacyPolicy,
  changePassword,
  completePasswordReset,
  confirmEmailChange,
  confirmEmailVerification,
  deleteAccount,
  getCurrentUser,
  getPrivacyPolicy,
  login,
  logout,
  refreshSession,
  register,
  requestEmailChange,
  requestPasswordReset,
  resendEmailVerification,
  updateDisplayName,
  updateUserPreferences,
  type UserProfile,
  type UpdateUserPreferencesRequest,
} from "@/features/auth/api/auth.api"
import { offlineShoppingRecordQueryKey } from "@/features/shopping-lists/api/shopping-list-offline.queries"
import { clearOfflineShoppingRecord } from "@/features/shopping-lists/model/shopping-list-offline.storage"
import { clearSession, useSessionStore } from "@/shared/auth/session.store"

export const authQueryKeys = {
  currentUser: ["auth", "current-user"] as const,
  privacyPolicy: ["legal", "privacy-policy"] as const,
  refresh: ["auth", "refresh"] as const,
}

export function usePrivacyPolicyQuery() {
  return useQuery({
    queryKey: authQueryKeys.privacyPolicy,
    queryFn: getPrivacyPolicy,
    staleTime: 60 * 60 * 1000,
  })
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
  const navigate = useNavigate()

  return useMutation({
    mutationFn: register,
    onSuccess: (response) => {
      void navigate(`/verify-email?email=${encodeURIComponent(response.email)}`, { replace: true })
    },
  })
}

export function useConfirmEmailVerificationMutation() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: confirmEmailVerification,
    onSuccess: (_response, request) => {
      void navigate("/login", {
        replace: true,
        state: { email: request.email, emailVerified: true },
      })
    },
  })
}

export function useResendEmailVerificationMutation() {
  return useMutation({ mutationFn: resendEmailVerification })
}

export function useRequestPasswordResetMutation() {
  return useMutation({ mutationFn: requestPasswordReset })
}

export function useCompletePasswordResetMutation() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: ({ token, newPassword }: { token: string; newPassword: string }) =>
      completePasswordReset(token, newPassword),
    onSuccess: () => {
      void navigate("/login", { replace: true, state: { passwordReset: true } })
    },
  })
}

export function useUpdateDisplayNameMutation() {
  const queryClient = useQueryClient()
  const setUser = useSessionStore((state) => state.setUser)

  return useMutation({
    mutationFn: updateDisplayName,
    onSuccess: (user) => {
      queryClient.setQueryData(authQueryKeys.currentUser, user)
      setUser(user)
    },
  })
}

export function useRequestEmailChangeMutation() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: requestEmailChange,
    onSuccess: (_response, request) => {
      void navigate(`/profile/email-change/confirm?email=${encodeURIComponent(request.newEmail)}`)
    },
  })
}

export function useConfirmEmailChangeMutation() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: confirmEmailChange,
    onSuccess: () => {
      clearSession()
      queryClient.clear()
      void navigate("/login", { replace: true, state: { emailChanged: true } })
    },
  })
}

export function useChangePasswordMutation() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: ({
      currentPassword,
      newPassword,
    }: {
      currentPassword: string
      newPassword: string
    }) => changePassword(currentPassword, newPassword),
    onSuccess: () => {
      clearSession()
      queryClient.clear()
      void navigate("/login", { replace: true, state: { passwordChanged: true } })
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

export function useAcceptPrivacyPolicyMutation() {
  const queryClient = useQueryClient()
  const setUser = useSessionStore((state) => state.setUser)
  const navigate = useNavigate()

  return useMutation({
    mutationFn: acceptPrivacyPolicy,
    onSuccess: (user) => {
      queryClient.setQueryData(authQueryKeys.currentUser, user)
      setUser(user)
      void navigate("/", { replace: true })
    },
  })
}

export function useDeleteAccountMutation() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: deleteAccount,
    onSuccess: async () => {
      try {
        await clearOfflineShoppingRecord()
      } finally {
        clearSession()
        queryClient.removeQueries({ queryKey: offlineShoppingRecordQueryKey })
        queryClient.clear()
        void navigate("/login", { replace: true, state: { accountDeleted: true } })
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

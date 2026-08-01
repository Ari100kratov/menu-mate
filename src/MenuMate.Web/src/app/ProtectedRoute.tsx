import { useEffect } from "react"
import { Navigate, Outlet, useLocation } from "react-router-dom"

import { AppShellSkeleton } from "@/app/AppShellSkeleton"
import type { UserProfile } from "@/features/auth/api/auth.api"
import { useRefreshSessionQuery } from "@/features/auth/api/auth.queries"
import { useOfflineShoppingRecordQuery } from "@/features/shopping-lists/api/shopping-list-offline.queries"
import { useSessionStore } from "@/shared/auth/session.store"
import { useNetworkStatus } from "@/shared/lib/network-status"

export function ProtectedRoute() {
  const location = useLocation()
  const accessToken = useSessionStore((state) => state.accessToken)
  const refreshBlocked = useSessionStore((state) => state.refreshBlocked)
  const offlineAccess = useSessionStore((state) => state.offlineAccess)
  const isBrowserOnline = useNetworkStatus()
  const shouldRefreshSession = !accessToken && !refreshBlocked
  const refreshQuery = useRefreshSessionQuery(shouldRefreshSession)
  const refetchSession = refreshQuery.refetch
  const offlineRecordQuery = useOfflineShoppingRecordQuery()
  const refreshUnavailable =
    !isBrowserOnline ||
    refreshQuery.fetchStatus === "paused" ||
    isNetworkFailure(refreshQuery.error)

  useEffect(() => {
    if (!offlineAccess || !isBrowserOnline) {
      return
    }

    void refetchSession()
  }, [isBrowserOnline, offlineAccess, refetchSession])

  if (accessToken || refreshQuery.isSuccess) {
    return <Outlet />
  }

  if (offlineAccess) {
    if (refreshQuery.error && !refreshUnavailable) {
      return <EndOfflineAccess />
    }

    return isOfflineShoppingPath(location.pathname) ? (
      <Outlet />
    ) : (
      <Navigate to="/shopping" replace />
    )
  }

  if (!refreshBlocked && offlineRecordQuery.data && refreshUnavailable) {
    return <StartOfflineAccess user={offlineRecordQuery.data.user} />
  }

  if (
    offlineRecordQuery.isPending ||
    (shouldRefreshSession && refreshQuery.isPending && refreshQuery.fetchStatus !== "paused")
  ) {
    return <AppShellSkeleton />
  }

  return <Navigate to="/login" replace state={{ from: location }} />
}

function StartOfflineAccess({ user }: { user: UserProfile }) {
  const startOfflineAccess = useSessionStore((state) => state.startOfflineAccess)

  useEffect(() => {
    startOfflineAccess(user)
  }, [startOfflineAccess, user])

  return <AppShellSkeleton />
}

function EndOfflineAccess() {
  const endOfflineAccess = useSessionStore((state) => state.endOfflineAccess)

  useEffect(() => {
    endOfflineAccess()
  }, [endOfflineAccess])

  return <AppShellSkeleton />
}

function isOfflineShoppingPath(pathname: string) {
  return pathname === "/" || pathname === "/shopping"
}

function isNetworkFailure(error: unknown) {
  return error instanceof TypeError
}

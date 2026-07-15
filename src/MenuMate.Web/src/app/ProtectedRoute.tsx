import { Navigate, Outlet, useLocation } from "react-router-dom"

import { AppShellSkeleton } from "@/app/AppShellSkeleton"
import { useRefreshSessionQuery } from "@/features/auth/api/auth.queries"
import { useSessionStore } from "@/shared/auth/session.store"

export function ProtectedRoute() {
  const location = useLocation()
  const accessToken = useSessionStore((state) => state.accessToken)
  const refreshBlocked = useSessionStore((state) => state.refreshBlocked)
  const shouldRefreshSession = !accessToken && !refreshBlocked
  const refreshQuery = useRefreshSessionQuery(shouldRefreshSession)

  if (accessToken || refreshQuery.isSuccess) {
    return <Outlet />
  }

  if (shouldRefreshSession && refreshQuery.isPending) {
    return <AppShellSkeleton />
  }

  return <Navigate to="/login" replace state={{ from: location }} />
}

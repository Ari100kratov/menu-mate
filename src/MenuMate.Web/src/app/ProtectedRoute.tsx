import { Navigate, Outlet, useLocation } from "react-router-dom"

import { useRefreshSessionQuery } from "@/features/auth/api/auth.queries"
import { useSessionStore } from "@/shared/auth/session.store"
import { PageSkeleton } from "@/shared/ui/feedback"

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
    return (
      <main className="mx-auto flex min-h-svh w-full max-w-5xl items-center px-4">
        <PageSkeleton />
      </main>
    )
  }

  return <Navigate to="/login" replace state={{ from: location }} />
}

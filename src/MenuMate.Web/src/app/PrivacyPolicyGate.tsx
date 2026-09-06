import { Navigate, Outlet, useLocation } from "react-router-dom"

import { AppShellSkeleton } from "@/app/AppShellSkeleton"
import { useCurrentUserQuery } from "@/features/auth/api/auth.queries"
import { useSessionStore } from "@/shared/auth/session.store"
import { ErrorAlert } from "@/shared/ui/feedback"

export function PrivacyPolicyGate() {
  const location = useLocation()
  const offlineAccess = useSessionStore((state) => state.offlineAccess)
  const currentUserQuery = useCurrentUserQuery(!offlineAccess)

  if (offlineAccess) {
    return <Outlet />
  }

  if (currentUserQuery.isPending) {
    return <AppShellSkeleton />
  }

  if (currentUserQuery.error) {
    return (
      <main className="mx-auto flex min-h-svh max-w-xl items-center px-4">
        <ErrorAlert error={currentUserQuery.error} />
      </main>
    )
  }

  if (currentUserQuery.data.requiresPrivacyPolicyAcceptance) {
    return <Navigate to="/privacy/accept" replace state={{ from: location }} />
  }

  return <Outlet />
}

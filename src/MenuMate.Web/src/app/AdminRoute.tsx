import { Navigate, Outlet } from "react-router-dom"

import { useCurrentUserQuery } from "@/features/auth/api/auth.queries"
import { isAdministrator } from "@/features/auth/model/roles"
import { Skeleton } from "@/shared/ui/skeleton"

export function AdminRoute() {
  const currentUserQuery = useCurrentUserQuery()

  if (currentUserQuery.isPending) {
    return (
      <div className="space-y-4" role="status" aria-label="Проверяем права доступа">
        <Skeleton className="h-11 w-56" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (!currentUserQuery.data || !isAdministrator(currentUserQuery.data.roles)) {
    return <Navigate to="/profile" replace />
  }

  return <Outlet />
}

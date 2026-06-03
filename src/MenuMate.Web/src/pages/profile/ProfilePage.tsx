import { LogOut } from "lucide-react"

import { useCurrentUserQuery, useLogoutMutation } from "@/features/auth/api/auth.queries"
import { ProfileOverview } from "@/features/profile/ui/ProfileOverview"
import { ProfileSettings } from "@/features/profile/ui/ProfileSettings"
import { useSessionStore } from "@/shared/auth/session.store"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { MobileStickyActions, PageHeader } from "@/shared/ui/page"

export default function ProfilePage() {
  const currentUserQuery = useCurrentUserQuery()
  const logoutMutation = useLogoutMutation()
  const accessTokenExpiresAt = useSessionStore((state) => state.accessTokenExpiresAt)

  if (currentUserQuery.isPending) {
    return <PageSkeleton />
  }

  if (currentUserQuery.error) {
    return <ErrorAlert error={currentUserQuery.error} />
  }

  function logout() {
    logoutMutation.mutate()
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Профиль"
        description="Аккаунт, текущая сессия, роли и настройки интерфейса."
        action={<LogoutButton isPending={logoutMutation.isPending} onClick={logout} />}
      />

      <MobileStickyActions>
        <LogoutButton isPending={logoutMutation.isPending} onClick={logout} />
      </MobileStickyActions>

      {logoutMutation.error ? <ErrorAlert error={logoutMutation.error} /> : null}

      <ProfileOverview user={currentUserQuery.data} accessTokenExpiresAt={accessTokenExpiresAt} />
      <ProfileSettings />
    </div>
  )
}

function LogoutButton({ isPending, onClick }: { isPending: boolean; onClick: () => void }) {
  return (
    <Button type="button" variant="outline" disabled={isPending} onClick={onClick}>
      <LogOut />
      {isPending ? "Выходим..." : "Выйти"}
    </Button>
  )
}

import { LogOut } from "lucide-react"

import { useCurrentUserQuery, useLogoutMutation } from "@/features/auth/api/auth.queries"
import { ProfileOverview } from "@/features/profile/ui/ProfileOverview"
import { ProfileSkeleton } from "@/features/profile/ui/ProfileSkeleton"
import { ProfileSettings } from "@/features/profile/ui/ProfileSettings"
import { useSessionStore } from "@/shared/auth/session.store"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function ProfilePage() {
  const currentUserQuery = useCurrentUserQuery()
  const logoutMutation = useLogoutMutation()
  const accessTokenExpiresAt = useSessionStore((state) => state.accessTokenExpiresAt)

  if (currentUserQuery.isPending) {
    return <ProfileSkeleton />
  }

  if (currentUserQuery.error) {
    return <ErrorAlert error={currentUserQuery.error} />
  }

  function logout() {
    logoutMutation.mutate()
  }

  return (
    <div className="space-y-5">
      <div className="flex justify-end">
        <LogoutButton isPending={logoutMutation.isPending} onClick={logout} />
      </div>

      {logoutMutation.error ? <ErrorAlert error={logoutMutation.error} /> : null}

      <ProfileOverview user={currentUserQuery.data} accessTokenExpiresAt={accessTokenExpiresAt} />
      <ProfileSettings />
    </div>
  )
}

function LogoutButton({ isPending, onClick }: { isPending: boolean; onClick: () => void }) {
  return (
    <Button
      type="button"
      variant="outline"
      className="w-full sm:w-auto"
      disabled={isPending}
      onClick={onClick}
    >
      <LogOut />
      {isPending ? "Выходим..." : "Выйти"}
    </Button>
  )
}

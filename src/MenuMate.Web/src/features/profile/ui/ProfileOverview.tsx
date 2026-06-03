import { Clock3, KeyRound, Mail, UserRound } from "lucide-react"

import type { UserProfile } from "@/features/auth/api/auth.api"
import { formatDateTime, formatRole } from "@/features/profile/ui/profile-format"
import { PageSection } from "@/shared/ui/page"
import { ProfileField } from "./ProfileField"

interface ProfileOverviewProps {
  user: UserProfile
  accessTokenExpiresAt: string | null
}

export function ProfileOverview({ user, accessTokenExpiresAt }: ProfileOverviewProps) {
  return (
    <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_20rem]">
      <PageSection>
        <div className="flex items-start gap-3">
          <div className="bg-primary/10 text-primary flex size-11 shrink-0 items-center justify-center rounded-md">
            <UserRound className="size-5" />
          </div>
          <div className="min-w-0">
            <h2 className="truncate text-xl font-semibold tracking-normal">{user.displayName}</h2>
            <p className="text-muted-foreground text-sm break-all">{user.id}</p>
          </div>
        </div>

        <dl className="grid gap-3 sm:grid-cols-2">
          <ProfileField icon={Mail} label="Email" value={user.email} />
          <ProfileField
            icon={Clock3}
            label="Access token"
            value={formatDateTime(accessTokenExpiresAt)}
          />
        </dl>
      </PageSection>

      <PageSection title="Роли">
        {user.roles.length > 0 ? (
          <div className="flex flex-wrap gap-2">
            {user.roles.map((role) => (
              <span
                key={role}
                className="bg-secondary text-secondary-foreground inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs font-medium"
              >
                <KeyRound className="size-3" />
                {formatRole(role)}
              </span>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground text-sm">Роли не назначены.</p>
        )}
      </PageSection>
    </section>
  )
}

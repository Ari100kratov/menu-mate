import type { UserProfile } from "@/features/auth/api/auth.api"
import { ProfileShoppingSettings } from "./ProfileShoppingSettings"
import { ProfileThemeSettings } from "./ProfileThemeSettings"
import { ProfileAccountSettings } from "./ProfileAccountSettings"
import { ProfileDataAndDeletionSettings } from "./ProfileDataAndDeletionSettings"

export function ProfileSettings({ user }: { user: UserProfile }) {
  return (
    <section className="grid gap-4">
      <ProfileThemeSettings />
      <ProfileShoppingSettings user={user} />
      <ProfileAccountSettings user={user} />
      <ProfileDataAndDeletionSettings user={user} />
    </section>
  )
}

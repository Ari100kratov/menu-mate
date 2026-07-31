import type { UserProfile } from "@/features/auth/api/auth.api"
import { ProfileShoppingSettings } from "./ProfileShoppingSettings"
import { ProfileThemeSettings } from "./ProfileThemeSettings"

export function ProfileSettings({ user }: { user: UserProfile }) {
  return (
    <section className="grid gap-4">
      <ProfileThemeSettings />
      <ProfileShoppingSettings user={user} />
    </section>
  )
}

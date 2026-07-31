import { ShoppingCart } from "lucide-react"

import type { UserProfile } from "@/features/auth/api/auth.api"
import { useUpdateUserPreferencesMutation } from "@/features/auth/api/auth.queries"
import { ErrorAlert } from "@/shared/ui/feedback"
import { PageSection } from "@/shared/ui/page"
import { Switch } from "@/shared/ui/switch"

export function ProfileShoppingSettings({ user }: { user: UserProfile }) {
  const updatePreferencesMutation = useUpdateUserPreferencesMutation()

  return (
    <PageSection title="Покупки">
      {updatePreferencesMutation.error ? (
        <ErrorAlert error={updatePreferencesMutation.error} />
      ) : null}
      <label
        htmlFor="show-shopping-list-preview"
        className="flex cursor-pointer items-center gap-3 rounded-md border p-3"
      >
        <span className="bg-primary/10 text-primary flex size-9 shrink-0 items-center justify-center rounded-md">
          <ShoppingCart className="size-4" />
        </span>
        <span className="min-w-0 flex-1 space-y-1">
          <span className="block font-medium">Предпросмотр списка покупок</span>
          <span className="text-muted-foreground block text-sm">
            Показывать блюда и ингредиенты перед заменой списка покупок.
          </span>
        </span>
        <Switch
          id="show-shopping-list-preview"
          checked={user.preferences.showShoppingListPreview}
          disabled={updatePreferencesMutation.isPending}
          aria-label="Показывать предпросмотр списка покупок"
          onCheckedChange={(checked) => {
            updatePreferencesMutation.mutate({ showShoppingListPreview: checked })
          }}
        />
      </label>
    </PageSection>
  )
}

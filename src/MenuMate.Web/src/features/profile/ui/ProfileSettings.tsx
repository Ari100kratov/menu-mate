import { Settings2, ShieldCheck } from "lucide-react"

import { PageSection } from "@/shared/ui/page"
import { ProfileShoppingPreferences } from "./ProfileShoppingPreferences"
import { ProfileThemeSettings } from "./ProfileThemeSettings"
import { SettingsRow } from "./SettingsRow"

export function ProfileSettings() {
  return (
    <section className="grid gap-4 lg:grid-cols-2">
      <ProfileThemeSettings />
      <ProfileShoppingPreferences />

      <PageSection
        className="lg:col-span-2"
        title="Справочники и доступ"
        description="Теги, единицы и категории не выносятся в основную навигацию MVP."
      >
        <SettingsRow
          icon={Settings2}
          title="Единицы и категории"
          description="Пользовательские дефолты уже доступны в настройках покупок; глобальные справочники должны появляться только при явном продуктовом сценарии."
        />
        <SettingsRow
          icon={ShieldCheck}
          title="Админские сценарии"
          description="Модерация тегов и справочники остаются будущим admin-flow и не занимают место в мобильной навигации."
        />
      </PageSection>
    </section>
  )
}

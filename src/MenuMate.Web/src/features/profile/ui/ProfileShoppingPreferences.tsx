import { RotateCcw, ShoppingBasket } from "lucide-react"

import { Button } from "@/shared/ui/button"
import {
  shoppingCategoryOptions,
  shoppingUnitOptions,
  type ShoppingCategoryValue,
  type ShoppingUnitValue,
} from "@/shared/config/shopping-taxonomy"
import { useUserPreferencesStore } from "@/shared/config/user-preferences.store"
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { PageSection } from "@/shared/ui/page"
import { Select } from "@/shared/ui/select"
import { SettingsRow } from "./SettingsRow"

export function ProfileShoppingPreferences() {
  const defaultShoppingUnit = useUserPreferencesStore((state) => state.defaultShoppingUnit)
  const defaultShoppingCategory = useUserPreferencesStore((state) => state.defaultShoppingCategory)
  const setDefaultShoppingUnit = useUserPreferencesStore((state) => state.setDefaultShoppingUnit)
  const setDefaultShoppingCategory = useUserPreferencesStore(
    (state) => state.setDefaultShoppingCategory,
  )
  const resetShoppingDefaults = useUserPreferencesStore((state) => state.resetShoppingDefaults)

  return (
    <PageSection
      title="Покупки"
      description="Дефолты применяются к новым ручным позициям списка покупок."
      action={
        <Button type="button" variant="ghost" onClick={resetShoppingDefaults}>
          <RotateCcw />
          Сбросить
        </Button>
      }
    >
      <FieldGroup className="grid gap-3 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor="default-shopping-unit">Единица</FieldLabel>
          <Select
            id="default-shopping-unit"
            value={defaultShoppingUnit}
            onChange={(event) => {
              setDefaultShoppingUnit(event.target.value as ShoppingUnitValue)
            }}
          >
            {shoppingUnitOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </Select>
          <FieldDescription>Используется при добавлении продукта вручную.</FieldDescription>
        </Field>

        <Field>
          <FieldLabel htmlFor="default-shopping-category">Категория</FieldLabel>
          <Select
            id="default-shopping-category"
            value={defaultShoppingCategory}
            onChange={(event) => {
              setDefaultShoppingCategory(event.target.value as ShoppingCategoryValue)
            }}
          >
            {shoppingCategoryOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </Select>
          <FieldDescription>Помогает быстрее группировать ручные покупки.</FieldDescription>
        </Field>
      </FieldGroup>

      <SettingsRow
        icon={ShoppingBasket}
        title="Локальные предпочтения"
        description="Эти значения не являются справочниками и не меняют уже созданные позиции."
      />
    </PageSection>
  )
}

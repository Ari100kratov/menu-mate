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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"
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
            value={defaultShoppingUnit}
            onValueChange={(value) => {
              setDefaultShoppingUnit(value as ShoppingUnitValue)
            }}
          >
            <SelectTrigger id="default-shopping-unit" className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {shoppingUnitOptions.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <FieldDescription>Используется при добавлении продукта вручную.</FieldDescription>
        </Field>

        <Field>
          <FieldLabel htmlFor="default-shopping-category">Категория</FieldLabel>
          <Select
            value={defaultShoppingCategory}
            onValueChange={(value) => {
              setDefaultShoppingCategory(value as ShoppingCategoryValue)
            }}
          >
            <SelectTrigger id="default-shopping-category" className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {shoppingCategoryOptions.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
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

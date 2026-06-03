import { useMemo } from "react"

import type { ShoppingItemFormValues } from "@/features/shopping-lists/model/shopping-list-ui"
import { getDefaultShoppingItemFormValues } from "@/features/shopping-lists/model/shopping-list-ui"
import { useUserPreferencesStore } from "@/shared/config/user-preferences.store"
import { ShoppingListItemForm } from "./ShoppingListItemForm"

interface ShoppingListAddItemPanelProps {
  isHidden: boolean
  isSubmitting: boolean
  onSubmit: (values: ShoppingItemFormValues) => void
}

export function ShoppingListAddItemPanel({
  isHidden,
  isSubmitting,
  onSubmit,
}: ShoppingListAddItemPanelProps) {
  const defaultShoppingUnit = useUserPreferencesStore((state) => state.defaultShoppingUnit)
  const defaultShoppingCategory = useUserPreferencesStore((state) => state.defaultShoppingCategory)
  const initialValues = useMemo(
    () =>
      getDefaultShoppingItemFormValues({
        unit: defaultShoppingUnit,
        category: defaultShoppingCategory,
      }),
    [defaultShoppingCategory, defaultShoppingUnit],
  )

  if (isHidden) {
    return null
  }

  return (
    <ShoppingListItemForm
      key={`${defaultShoppingUnit}:${defaultShoppingCategory}`}
      title="Добавить продукт"
      submitLabel="Добавить"
      submitIcon="add"
      isSubmitting={isSubmitting}
      initialValues={initialValues}
      resetAfterSubmit
      onSubmit={onSubmit}
    />
  )
}

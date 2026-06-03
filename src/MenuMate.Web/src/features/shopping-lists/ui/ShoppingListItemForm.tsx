import { ListPlus, Pencil, X } from "lucide-react"

import type { ShoppingItemFormValues } from "@/features/shopping-lists/model/shopping-list-ui"
import { Button } from "@/shared/ui/button"
import { FieldGroup } from "@/shared/ui/field"
import { ShoppingItemCommentField } from "./ShoppingItemCommentField"
import { ShoppingItemSelectFields } from "./ShoppingItemSelectFields"
import { ShoppingItemTextFields } from "./ShoppingItemTextFields"
import { useShoppingItemForm } from "./useShoppingItemForm"

interface ShoppingListItemFormProps {
  title: string
  submitLabel: string
  submitIcon: "add" | "save"
  isSubmitting: boolean
  initialValues: ShoppingItemFormValues
  resetAfterSubmit?: boolean
  onSubmit: (values: ShoppingItemFormValues) => void
  onCancel?: () => void
}

export function ShoppingListItemForm({
  title,
  submitLabel,
  submitIcon,
  isSubmitting,
  initialValues,
  resetAfterSubmit = false,
  onSubmit,
  onCancel,
}: ShoppingListItemFormProps) {
  const form = useShoppingItemForm({
    initialValues,
    resetAfterSubmit,
    onSubmit,
  })

  return (
    <form
      className="space-y-4 rounded-md border p-4"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      <h3 className="font-semibold tracking-normal">{title}</h3>
      <FieldGroup className="grid gap-3 md:grid-cols-6">
        <ShoppingItemTextFields form={form} />
        <ShoppingItemSelectFields form={form} />
        <ShoppingItemCommentField form={form} />
      </FieldGroup>

      <div className="flex flex-wrap gap-2">
        <Button type="submit" disabled={isSubmitting}>
          {submitIcon === "add" ? <ListPlus /> : <Pencil />}
          {isSubmitting ? "Сохраняем..." : submitLabel}
        </Button>
        {onCancel ? (
          <Button type="button" variant="ghost" onClick={onCancel}>
            <X />
            Отмена
          </Button>
        ) : null}
      </div>
    </form>
  )
}

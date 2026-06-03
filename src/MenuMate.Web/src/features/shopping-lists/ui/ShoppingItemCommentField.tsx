import type { ShoppingItemFormApi } from "@/features/shopping-lists/ui/useShoppingItemForm"
import { Field, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

interface ShoppingItemCommentFieldProps {
  form: ShoppingItemFormApi
}

export function ShoppingItemCommentField({ form }: ShoppingItemCommentFieldProps) {
  return (
    <form.Field name="comment">
      {(field) => (
        <Field className="md:col-span-6">
          <FieldLabel htmlFor={field.name}>Комментарий</FieldLabel>
          <Input
            id={field.name}
            name={field.name}
            value={field.state.value}
            onBlur={field.handleBlur}
            onChange={(event) => {
              field.handleChange(event.target.value)
            }}
          />
        </Field>
      )}
    </form.Field>
  )
}

import type { ShoppingItemFormApi } from "@/features/shopping-lists/ui/useShoppingItemForm"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

interface ShoppingItemTextFieldsProps {
  form: ShoppingItemFormApi
}

export function ShoppingItemTextFields({ form }: ShoppingItemTextFieldsProps) {
  return (
    <>
      <form.Field name="name">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field className="md:col-span-3" data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Название</FieldLabel>
              <Input
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
                aria-invalid={isInvalid}
              />
              {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>

      <form.Field name="amount">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field className="md:col-span-3" data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Количество</FieldLabel>
              <Input
                id={field.name}
                name={field.name}
                inputMode="decimal"
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
                aria-invalid={isInvalid}
              />
              {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>
    </>
  )
}

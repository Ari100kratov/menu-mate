import type { ShoppingItemFormApi } from "@/features/shopping-lists/ui/useShoppingItemForm"
import {
  shoppingCategoryOptions,
  shoppingQuantityKindOptions,
  shoppingUnitOptions,
} from "@/shared/config/shopping-taxonomy"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Select } from "@/shared/ui/select"

interface ShoppingItemSelectFieldsProps {
  form: ShoppingItemFormApi
}

export function ShoppingItemSelectFields({ form }: ShoppingItemSelectFieldsProps) {
  return (
    <>
      <form.Field name="unit">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field className="md:col-span-2" data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Единица</FieldLabel>
              <Select
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
                aria-invalid={isInvalid}
              >
                {shoppingUnitOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </Select>
              {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>

      <form.Field name="quantityKind">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field className="md:col-span-2" data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Тип</FieldLabel>
              <Select
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
                aria-invalid={isInvalid}
              >
                {shoppingQuantityKindOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </Select>
              {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>

      <form.Field name="category">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field className="md:col-span-2" data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Категория</FieldLabel>
              <Select
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
                aria-invalid={isInvalid}
              >
                {shoppingCategoryOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </Select>
              {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>
    </>
  )
}

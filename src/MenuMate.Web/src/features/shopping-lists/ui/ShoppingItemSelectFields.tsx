import type { ShoppingItemFormApi } from "@/features/shopping-lists/ui/useShoppingItemForm"
import {
  shoppingCategoryOptions,
  shoppingQuantityKindOptions,
  shoppingUnitOptions,
} from "@/shared/config/shopping-taxonomy"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

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
                name={field.name}
                value={field.state.value}
                onValueChange={field.handleChange}
              >
                <SelectTrigger
                  id={field.name}
                  className="w-full"
                  onBlur={field.handleBlur}
                  aria-invalid={isInvalid}
                >
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
                name={field.name}
                value={field.state.value}
                onValueChange={field.handleChange}
              >
                <SelectTrigger
                  id={field.name}
                  className="w-full"
                  onBlur={field.handleBlur}
                  aria-invalid={isInvalid}
                >
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {shoppingQuantityKindOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>

      <form.Subscribe selector={(state) => state.values.productId}>
        {(productId) =>
          productId ? null : (
            <form.Field name="category">
              {(field) => {
                const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

                return (
                  <Field className="md:col-span-2" data-invalid={isInvalid}>
                    <FieldLabel htmlFor={field.name}>Категория нового продукта</FieldLabel>
                    <Select
                      name={field.name}
                      value={field.state.value}
                      onValueChange={field.handleChange}
                    >
                      <SelectTrigger
                        id={field.name}
                        className="w-full"
                        onBlur={field.handleBlur}
                        aria-invalid={isInvalid}
                      >
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
                    {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
                  </Field>
                )
              }}
            </form.Field>
          )
        }
      </form.Subscribe>
    </>
  )
}

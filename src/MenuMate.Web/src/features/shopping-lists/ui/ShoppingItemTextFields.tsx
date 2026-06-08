import { Search } from "lucide-react"

import { useProductsQuery } from "@/features/products/api/products.queries"
import type { ShoppingItemFormApi } from "@/features/shopping-lists/ui/useShoppingItemForm"
import { shoppingCategoryOptions } from "@/shared/config/shopping-taxonomy"
import { Field, FieldDescription, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

interface ShoppingItemTextFieldsProps {
  form: ShoppingItemFormApi
}

export function ShoppingItemTextFields({ form }: ShoppingItemTextFieldsProps) {
  return (
    <form.Subscribe
      selector={(state) => ({
        name: state.values.name,
        productId: state.values.productId,
      })}
    >
      {({ name, productId }) => (
        <ShoppingItemTextInputs form={form} name={name} productId={productId} />
      )}
    </form.Subscribe>
  )
}

interface ShoppingItemTextInputsProps extends ShoppingItemTextFieldsProps {
  name: string
  productId: string
}

function ShoppingItemTextInputs({ form, name, productId }: ShoppingItemTextInputsProps) {
  const productsQuery = useProductsQuery(name)
  const suggestions = name.trim() && !productId ? (productsQuery.data ?? []).slice(0, 6) : []

  return (
    <>
      <form.Field name="name">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field className="md:col-span-6" data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Название</FieldLabel>
              <div className="relative">
                <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
                <Input
                  id={field.name}
                  name={field.name}
                  className="pl-9"
                  value={field.state.value}
                  autoComplete="off"
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    field.handleChange(event.target.value)
                    form.setFieldValue("productId", "")
                  }}
                  aria-invalid={isInvalid}
                />
              </div>
              {suggestions.length > 0 ? (
                <div className="bg-popover divide-y rounded-md border shadow-sm">
                  {suggestions.map((product) => (
                    <button
                      key={product.id}
                      type="button"
                      className="hover:bg-muted flex w-full items-center justify-between gap-3 px-3 py-2 text-left text-sm"
                      onClick={() => {
                        field.handleChange(product.name)
                        form.setFieldValue("productId", product.id)
                        form.setFieldValue("category", product.category)
                      }}
                    >
                      <span className="font-medium">{product.name}</span>
                      <span className="text-muted-foreground text-xs">
                        {shoppingCategoryOptions.find((option) => option.value === product.category)
                          ?.label ?? product.category}
                      </span>
                    </button>
                  ))}
                </div>
              ) : null}
              {productId ? (
                <FieldDescription>Выбран продукт из общего каталога.</FieldDescription>
              ) : null}
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

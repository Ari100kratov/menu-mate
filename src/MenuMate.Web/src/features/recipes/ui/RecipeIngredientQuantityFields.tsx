import {
  measurementUnitOptions,
  quantityKindOptions,
} from "@/features/recipes/model/recipe-form-options"
import { ingredientFieldName } from "@/features/recipes/ui/recipe-form-fields"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select } from "@/shared/ui/select"

interface RecipeIngredientQuantityFieldsProps {
  form: RecipeFormApi
  index: number
}

export function RecipeIngredientQuantityFields({
  form,
  index,
}: RecipeIngredientQuantityFieldsProps) {
  return (
    <>
      <form.Field name={ingredientFieldName(index, "quantityKind")}>
        {(quantityKindField) => (
          <Field className="md:col-span-3">
            <FieldLabel htmlFor={quantityKindField.name}>Тип количества</FieldLabel>
            <Select
              id={quantityKindField.name}
              name={quantityKindField.name}
              value={quantityKindField.state.value}
              onBlur={quantityKindField.handleBlur}
              onChange={(event) => {
                quantityKindField.handleChange(event.target.value)
              }}
            >
              {quantityKindOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </Select>
          </Field>
        )}
      </form.Field>

      <form.Subscribe selector={(state) => state.values.ingredients[index]?.quantityKind}>
        {(quantityKind) => (
          <form.Field name={ingredientFieldName(index, "amount")}>
            {(amountField) => {
              const isInvalid = amountField.state.meta.isTouched && !amountField.state.meta.isValid
              const isToTaste = quantityKind === "ToTaste"

              return (
                <Field className="md:col-span-2" data-invalid={isInvalid}>
                  <FieldLabel htmlFor={amountField.name}>Количество</FieldLabel>
                  <Input
                    id={amountField.name}
                    name={amountField.name}
                    inputMode="decimal"
                    value={amountField.state.value}
                    disabled={isToTaste}
                    onBlur={amountField.handleBlur}
                    onChange={(event) => {
                      amountField.handleChange(event.target.value)
                    }}
                    aria-invalid={isInvalid}
                  />
                  {isInvalid ? <FieldError errors={amountField.state.meta.errors} /> : null}
                </Field>
              )
            }}
          </form.Field>
        )}
      </form.Subscribe>

      <form.Field name={ingredientFieldName(index, "unit")}>
        {(unitField) => (
          <Field className="md:col-span-2">
            <FieldLabel htmlFor={unitField.name}>Единица</FieldLabel>
            <Select
              id={unitField.name}
              name={unitField.name}
              value={unitField.state.value}
              onBlur={unitField.handleBlur}
              onChange={(event) => {
                unitField.handleChange(event.target.value)
              }}
            >
              {measurementUnitOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </Select>
          </Field>
        )}
      </form.Field>
    </>
  )
}

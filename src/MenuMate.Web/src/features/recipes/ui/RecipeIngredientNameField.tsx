import { ingredientFieldName } from "@/features/recipes/ui/recipe-form-fields"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

interface RecipeIngredientNameFieldProps {
  form: RecipeFormApi
  index: number
}

export function RecipeIngredientNameField({ form, index }: RecipeIngredientNameFieldProps) {
  return (
    <form.Field name={ingredientFieldName(index, "productName")}>
      {(productNameField) => {
        const isInvalid =
          productNameField.state.meta.isTouched && !productNameField.state.meta.isValid

        return (
          <Field className="md:col-span-3" data-invalid={isInvalid}>
            <FieldLabel htmlFor={productNameField.name}>Продукт</FieldLabel>
            <Input
              id={productNameField.name}
              name={productNameField.name}
              value={productNameField.state.value}
              onBlur={productNameField.handleBlur}
              onChange={(event) => {
                productNameField.handleChange(event.target.value)
              }}
              aria-invalid={isInvalid}
            />
            {isInvalid ? <FieldError errors={productNameField.state.meta.errors} /> : null}
          </Field>
        )
      }}
    </form.Field>
  )
}

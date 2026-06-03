import { productCategoryOptions } from "@/features/recipes/model/recipe-form-options"
import { ingredientFieldName } from "@/features/recipes/ui/recipe-form-fields"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Field, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select } from "@/shared/ui/select"

interface RecipeIngredientMetaFieldsProps {
  form: RecipeFormApi
  index: number
}

export function RecipeIngredientMetaFields({ form, index }: RecipeIngredientMetaFieldsProps) {
  return (
    <>
      <form.Field name={ingredientFieldName(index, "category")}>
        {(categoryField) => (
          <Field className="md:col-span-2">
            <FieldLabel htmlFor={categoryField.name}>Категория</FieldLabel>
            <Select
              id={categoryField.name}
              name={categoryField.name}
              value={categoryField.state.value}
              onBlur={categoryField.handleBlur}
              onChange={(event) => {
                categoryField.handleChange(event.target.value)
              }}
            >
              {productCategoryOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </Select>
          </Field>
        )}
      </form.Field>

      <form.Field name={ingredientFieldName(index, "comment")}>
        {(commentField) => (
          <Field className="md:col-span-4">
            <FieldLabel htmlFor={commentField.name}>Комментарий</FieldLabel>
            <Input
              id={commentField.name}
              name={commentField.name}
              value={commentField.state.value}
              onBlur={commentField.handleBlur}
              onChange={(event) => {
                commentField.handleChange(event.target.value)
              }}
            />
          </Field>
        )}
      </form.Field>

      <form.Field name={ingredientFieldName(index, "isOptional")}>
        {(optionalField) => (
          <Field className="flex items-center gap-2 pt-6 md:col-span-2">
            <input
              id={optionalField.name}
              name={optionalField.name}
              type="checkbox"
              className="border-input text-primary focus-visible:ring-ring size-4 rounded border focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
              checked={optionalField.state.value}
              onBlur={optionalField.handleBlur}
              onChange={(event) => {
                optionalField.handleChange(event.target.checked)
              }}
            />
            <FieldLabel htmlFor={optionalField.name}>Необязательный</FieldLabel>
          </Field>
        )}
      </form.Field>
    </>
  )
}

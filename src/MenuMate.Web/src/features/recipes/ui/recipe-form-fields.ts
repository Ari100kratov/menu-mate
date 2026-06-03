import type { RecipeFormValues } from "@/features/recipes/model/recipe-form"

export type IngredientFormField = Extract<keyof RecipeFormValues["ingredients"][number], string>
export type StepFormField = Extract<keyof RecipeFormValues["steps"][number], string>

export function ingredientFieldName<TField extends IngredientFormField>(
  index: number,
  field: TField,
): `ingredients[${number}].${TField}` {
  return `ingredients[${String(index)}].${field}` as `ingredients[${number}].${TField}`
}

export function stepFieldName<TField extends StepFormField>(
  index: number,
  field: TField,
): `steps[${number}].${TField}` {
  return `steps[${String(index)}].${field}` as `steps[${number}].${TField}`
}

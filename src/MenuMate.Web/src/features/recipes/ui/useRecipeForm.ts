import { useForm } from "@tanstack/react-form"

import { recipeFormSchema, type RecipeFormValues } from "@/features/recipes/model/recipe-form"

interface UseRecipeFormOptions {
  initialValues: RecipeFormValues
  onSubmit: (values: RecipeFormValues) => void
}

export function useRecipeForm({ initialValues, onSubmit }: UseRecipeFormOptions) {
  return useForm({
    defaultValues: initialValues,
    validators: {
      onSubmit: recipeFormSchema,
    },
    onSubmit: ({ value }) => {
      onSubmit(value)
    },
  })
}

export type RecipeFormApi = ReturnType<typeof useRecipeForm>

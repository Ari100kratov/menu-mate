import { useForm } from "@tanstack/react-form"

import { recipeFormSchema, type RecipeFormValues } from "@/features/recipes/model/recipe-form"

interface UseRecipeFormOptions {
  initialValues: RecipeFormValues
}

export function useRecipeForm({ initialValues }: UseRecipeFormOptions) {
  return useForm({
    defaultValues: initialValues,
    validators: {
      onSubmit: recipeFormSchema,
    },
  })
}

export type RecipeFormApi = ReturnType<typeof useRecipeForm>

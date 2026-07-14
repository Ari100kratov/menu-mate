import { useNavigate } from "react-router-dom"
import { useState } from "react"
import { toast } from "sonner"
import { useQueryClient } from "@tanstack/react-query"

import { generateRecipeCoverImage } from "@/features/imports/api/imports.api"
import { type Recipe, uploadRecipeImage } from "@/features/recipes/api/recipes.api"
import { recipeQueryKeys, useCreateRecipeMutation } from "@/features/recipes/api/recipes.queries"
import {
  createEmptyRecipeFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"

export default function RecipeCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const createRecipeMutation = useCreateRecipeMutation()
  const [coverError, setCoverError] = useState<unknown>()

  function handleSubmit(values: RecipeFormValues, coverFile: File | null) {
    setCoverError(undefined)
    createRecipeMutation.mutate(toRecipeRequest(values), {
      onSuccess: (recipe) => {
        void uploadCoverAndNavigate(recipe.id, values.title, coverFile)
      },
    })
  }

  async function uploadCoverAndNavigate(recipeId: string, title: string, coverFile: File | null) {
    try {
      if (coverFile) {
        const image = await uploadRecipeImage(recipeId, {
          file: coverFile,
          scope: "Cover",
          altText: title,
        })
        queryClient.setQueryData<Recipe>(recipeQueryKeys.detail(recipeId), (recipe) =>
          recipe
            ? {
                ...recipe,
                images: [
                  ...recipe.images.filter((existingImage) => existingImage.scope !== "Cover"),
                  image,
                ],
              }
            : recipe,
        )
        void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
      }
      toast.success("Рецепт создан")
      void navigate(`/recipes/${recipeId}`, { replace: true })
    } catch (error) {
      setCoverError(error)
      toast.warning("Рецепт создан, но обложку загрузить не удалось")
      void navigate(`/recipes/${recipeId}/edit`, { replace: true })
    }
  }

  return (
    <RecipeForm
      initialValues={createEmptyRecipeFormValues()}
      submitLabel="Создать рецепт"
      isSubmitting={createRecipeMutation.isPending}
      error={createRecipeMutation.error ?? coverError}
      generateCover={(values) => generateRecipeCoverImage(toRecipeRequest(values))}
      onSubmit={handleSubmit}
    />
  )
}

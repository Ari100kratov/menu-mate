import { useLocation, useNavigate } from "react-router-dom"
import { toast } from "sonner"
import { useQueryClient } from "@tanstack/react-query"
import { useRef } from "react"

import { generateRecipeCoverImage } from "@/features/imports/api/imports.api"
import { type Recipe, updateRecipe, uploadRecipeImage } from "@/features/recipes/api/recipes.api"
import { recipeQueryKeys, useCreateRecipeMutation } from "@/features/recipes/api/recipes.queries"
import {
  createEmptyRecipeFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { getBackNavigationState } from "@/shared/lib/back-navigation"

export default function RecipeCreatePage() {
  const navigate = useNavigate()
  const location = useLocation()
  const queryClient = useQueryClient()
  const createRecipeMutation = useCreateRecipeMutation()
  const createdRecipeIdRef = useRef<string>(undefined)

  async function handleSubmit(
    values: RecipeFormValues,
    coverFile: File | null,
    onSaved: () => void,
  ) {
    let recipeId = createdRecipeIdRef.current
    if (recipeId) {
      await updateRecipe(recipeId, toRecipeRequest(values))
    } else {
      recipeId = (await createRecipeMutation.mutateAsync(toRecipeRequest(values))).id
      createdRecipeIdRef.current = recipeId
    }
    if (coverFile) await uploadCover(recipeId, values.title, coverFile)
    onSaved()
    toast.success("Рецепт создан")
    await navigate(`/recipes/${recipeId}`, {
      replace: true,
      state: getBackNavigationState(location.state),
    })
  }

  async function uploadCover(recipeId: string, title: string, coverFile: File) {
    const image = await uploadRecipeImage(recipeId, {
      file: coverFile,
      scope: "Cover",
      altText: title,
    })
    queryClient.setQueriesData<Recipe>({ queryKey: recipeQueryKeys.details() }, (recipe) =>
      recipe?.id === recipeId
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

  return (
    <RecipeForm
      initialValues={createEmptyRecipeFormValues()}
      submitLabel="Создать рецепт"
      isSubmitting={createRecipeMutation.isPending}
      error={createRecipeMutation.error}
      generateCover={(values) => generateRecipeCoverImage(toRecipeRequest(values))}
      onSubmit={handleSubmit}
    />
  )
}

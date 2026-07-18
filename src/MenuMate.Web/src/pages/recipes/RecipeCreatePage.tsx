import { useLocation, useNavigate } from "react-router-dom"
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
import { getBackNavigationState } from "@/shared/lib/back-navigation"

export default function RecipeCreatePage() {
  const navigate = useNavigate()
  const location = useLocation()
  const queryClient = useQueryClient()
  const createRecipeMutation = useCreateRecipeMutation()

  function handleSubmit(values: RecipeFormValues, coverFile: File | null) {
    createRecipeMutation.mutate(toRecipeRequest(values), {
      onSuccess: (recipe) => {
        toast.success("Рецепт создан")
        void navigate(`/recipes/${recipe.id}`, {
          replace: true,
          state: getBackNavigationState(location.state),
        })
        if (coverFile) {
          void uploadCover(recipe.id, values.title, coverFile)
        }
      },
    })
  }

  async function uploadCover(recipeId: string, title: string, coverFile: File) {
    try {
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
    } catch {
      toast.warning("Рецепт создан, но обложку загрузить не удалось")
    }
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

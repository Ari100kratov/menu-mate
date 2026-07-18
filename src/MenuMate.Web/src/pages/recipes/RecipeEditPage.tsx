import { Navigate, useLocation, useNavigate, useParams } from "react-router-dom"
import { toast } from "sonner"
import { useQueryClient } from "@tanstack/react-query"

import { generateRecipeCoverImage } from "@/features/imports/api/imports.api"
import { uploadRecipeImage } from "@/features/recipes/api/recipes.api"
import {
  recipeQueryKeys,
  useRecipeQuery,
  useUpdateRecipeMutation,
} from "@/features/recipes/api/recipes.queries"
import {
  recipeToFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { findCoverImage } from "@/features/recipes/model/recipe-images"
import { RecipeFormSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import { getParentBackState } from "@/shared/lib/back-navigation"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function RecipeEditPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const navigate = useNavigate()
  const location = useLocation()
  const queryClient = useQueryClient()
  const normalizedRecipeId = recipeId ?? ""
  const recipeQuery = useRecipeQuery(recipeId)
  const updateRecipeMutation = useUpdateRecipeMutation(normalizedRecipeId)

  if (!recipeId) {
    return <Navigate to="/recipes" replace />
  }

  if (recipeQuery.data && !recipeQuery.data.isOwnedByCurrentUser) {
    return <Navigate to={`/recipes/${recipeQuery.data.id}`} replace />
  }

  function handleSubmit(values: RecipeFormValues, coverFile: File | null) {
    updateRecipeMutation.mutate(toRecipeRequest(values), {
      onSuccess: () => {
        if (coverFile) {
          void uploadCover(normalizedRecipeId, values.title, coverFile)
        }

        toast.success("Рецепт сохранен")
        void navigate(`/recipes/${normalizedRecipeId}`, {
          replace: true,
          state: getParentBackState(location.state),
        })
      },
    })
  }

  async function uploadCover(recipeId: string, title: string, coverFile: File) {
    try {
      await uploadRecipeImage(recipeId, {
        file: coverFile,
        scope: "Cover",
        altText: title,
      })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.details() })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    } catch {
      toast.warning("Рецепт сохранен, но обложку загрузить не удалось")
    }
  }

  return (
    <div className="space-y-5">
      {recipeQuery.isPending ? <RecipeFormSkeleton /> : null}
      {recipeQuery.error ? <ErrorAlert error={recipeQuery.error} /> : null}
      {recipeQuery.data ? (
        <RecipeForm
          key={recipeQuery.data.id}
          initialValues={recipeToFormValues(recipeQuery.data)}
          coverImageUrl={findCoverImage(recipeQuery.data.images)?.readUrl ?? undefined}
          submitLabel="Сохранить"
          isSubmitting={updateRecipeMutation.isPending}
          error={updateRecipeMutation.error}
          generateCover={(values) => generateRecipeCoverImage(toRecipeRequest(values))}
          onSubmit={handleSubmit}
        />
      ) : null}
    </div>
  )
}

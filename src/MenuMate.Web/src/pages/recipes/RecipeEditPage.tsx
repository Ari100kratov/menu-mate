import { Navigate, useParams } from "react-router-dom"
import { toast } from "sonner"

import { generateRecipeCoverImage } from "@/features/imports/api/imports.api"
import {
  useRecipeQuery,
  useUpdateRecipeMutation,
  useUploadRecipeImageMutation,
} from "@/features/recipes/api/recipes.queries"
import {
  recipeToFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { findCoverImage } from "@/features/recipes/model/recipe-images"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"

export default function RecipeEditPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const normalizedRecipeId = recipeId ?? ""
  const recipeQuery = useRecipeQuery(recipeId)
  const updateRecipeMutation = useUpdateRecipeMutation(normalizedRecipeId)
  const uploadImageMutation = useUploadRecipeImageMutation(normalizedRecipeId)

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
          uploadImageMutation.mutate(
            {
              file: coverFile,
              scope: "Cover",
              altText: values.title,
            },
            {
              onSuccess: () => {
                toast.success("Рецепт сохранён")
              },
            },
          )
          return
        }

        toast.success("Рецепт сохранён")
      },
    })
  }

  return (
    <div className="space-y-5">
      {recipeQuery.isPending ? <PageSkeleton /> : null}
      {recipeQuery.error ? <ErrorAlert error={recipeQuery.error} /> : null}
      {recipeQuery.data ? (
        <RecipeForm
          key={recipeQuery.data.id}
          initialValues={recipeToFormValues(recipeQuery.data)}
          coverImageUrl={findCoverImage(recipeQuery.data.images)?.readUrl ?? undefined}
          submitLabel="Сохранить"
          isSubmitting={updateRecipeMutation.isPending || uploadImageMutation.isPending}
          error={updateRecipeMutation.error ?? uploadImageMutation.error}
          generateCover={(values) => generateRecipeCoverImage(toRecipeRequest(values))}
          onSubmit={handleSubmit}
        />
      ) : null}
    </div>
  )
}

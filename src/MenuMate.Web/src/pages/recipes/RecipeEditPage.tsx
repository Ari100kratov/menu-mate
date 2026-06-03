import { ArrowLeft } from "lucide-react"
import { Link, Navigate, useParams } from "react-router-dom"

import { useRecipeQuery, useUpdateRecipeMutation } from "@/features/recipes/api/recipes.queries"
import {
  recipeToFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { RecipeImagesPanel } from "@/features/recipes/ui/RecipeImagesPanel"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { PageHeader } from "@/shared/ui/page"

export default function RecipeEditPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const normalizedRecipeId = recipeId ?? ""
  const recipeQuery = useRecipeQuery(recipeId)
  const updateRecipeMutation = useUpdateRecipeMutation(normalizedRecipeId)

  if (!recipeId) {
    return <Navigate to="/recipes" replace />
  }

  function handleSubmit(values: RecipeFormValues) {
    updateRecipeMutation.mutate(toRecipeRequest(values))
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={recipeQuery.data?.title ?? "Редактирование рецепта"}
        description="Обновите состав, шаги и теги рецепта."
        action={
          <Button asChild variant="outline">
            <Link to="/recipes">
              <ArrowLeft />К списку
            </Link>
          </Button>
        }
      />

      {recipeQuery.isPending ? <PageSkeleton /> : null}
      {recipeQuery.error ? <ErrorAlert error={recipeQuery.error} /> : null}
      {recipeQuery.data ? (
        <>
          <RecipeImagesPanel recipe={recipeQuery.data} />
          <RecipeForm
            key={recipeQuery.data.id}
            initialValues={recipeToFormValues(recipeQuery.data)}
            submitLabel="Сохранить"
            isSubmitting={updateRecipeMutation.isPending}
            error={updateRecipeMutation.error}
            onSubmit={handleSubmit}
          />
        </>
      ) : null}
    </div>
  )
}

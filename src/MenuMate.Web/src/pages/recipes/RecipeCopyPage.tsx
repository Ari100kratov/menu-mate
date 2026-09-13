import { useQueryClient } from "@tanstack/react-query"
import { useRef } from "react"
import { Navigate, useLocation, useNavigate, useParams, useSearchParams } from "react-router-dom"
import { toast } from "sonner"

import { generateRecipeCoverImage } from "@/features/imports/api/imports.api"
import { type Recipe, updateRecipe, uploadRecipeImage } from "@/features/recipes/api/recipes.api"
import {
  recipeQueryKeys,
  useCopyRecipeMutation,
  useRecipeQuery,
} from "@/features/recipes/api/recipes.queries"
import { recipeToFormValues, toRecipeRequest } from "@/features/recipes/model/recipe-form"
import type { RecipeFormValues } from "@/features/recipes/model/recipe-form"
import { findCoverImage } from "@/features/recipes/model/recipe-images"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { RecipeFormSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import { getParentBackState } from "@/shared/lib/back-navigation"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function RecipeCopyPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const [searchParams] = useSearchParams()
  const revisionId = searchParams.get("revisionId") ?? undefined
  const navigate = useNavigate()
  const location = useLocation()
  const queryClient = useQueryClient()
  const sourceQuery = useRecipeQuery(recipeId, revisionId)
  const copyMutation = useCopyRecipeMutation()
  const createdRecipeIdRef = useRef<string>(undefined)

  if (!recipeId || !revisionId) {
    return <Navigate to="/recipes" replace />
  }

  const sourceRecipeId = recipeId
  const sourceRevisionId = revisionId

  async function handleSubmit(
    values: RecipeFormValues,
    coverFile: File | null,
    onSaved: () => void,
  ) {
    const sourceCover = sourceQuery.data ? findCoverImage(sourceQuery.data.images) : undefined
    let targetRecipeId = createdRecipeIdRef.current
    if (targetRecipeId) {
      await updateRecipe(targetRecipeId, toRecipeRequest(values))
    } else {
      const recipe = await copyMutation.mutateAsync({
        recipeId: sourceRecipeId,
        request: {
          sourceRevisionId,
          recipe: toRecipeRequest(values),
          copySourceCover: Boolean(sourceCover && !coverFile),
        },
      })
      targetRecipeId = recipe.id
      createdRecipeIdRef.current = targetRecipeId
    }
    if (coverFile) await uploadCover(targetRecipeId, values.title, coverFile)
    onSaved()
    toast.success("Копия рецепта создана")
    await navigate(`/recipes/${targetRecipeId}`, {
      replace: true,
      state: getParentBackState(location.state),
    })
  }

  async function uploadCover(targetRecipeId: string, title: string, coverFile: File) {
    const image = await uploadRecipeImage(targetRecipeId, {
      file: coverFile,
      scope: "Cover",
      altText: title,
    })
    queryClient.setQueriesData<Recipe>({ queryKey: recipeQueryKeys.details() }, (recipe) =>
      recipe?.id === targetRecipeId
        ? {
            ...recipe,
            images: [
              ...recipe.images.filter((existingImage) => existingImage.scope !== "Cover"),
              image,
            ],
          }
        : recipe,
    )
    void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.details() })
    void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
  }

  const source = sourceQuery.data
  const initialValues = source
    ? {
        ...recipeToFormValues(source),
        visibility: "Private" as const,
      }
    : undefined

  return (
    <div className="space-y-5">
      {sourceQuery.isPending ? <RecipeFormSkeleton /> : null}
      {sourceQuery.error ? <ErrorAlert error={sourceQuery.error} /> : null}
      {copyMutation.error ? <ErrorAlert error={copyMutation.error} /> : null}
      {source && initialValues ? (
        <RecipeForm
          key={`${source.id}:${source.revisionId}`}
          initialValues={initialValues}
          coverImageUrl={findCoverImage(source.images)?.readUrl ?? undefined}
          submitLabel="Создать копию"
          isSubmitting={copyMutation.isPending}
          error={copyMutation.error}
          generateCover={(values) => generateRecipeCoverImage(toRecipeRequest(values))}
          onSubmit={handleSubmit}
        />
      ) : null}
    </div>
  )
}

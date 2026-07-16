import { Navigate, useLocation, useNavigate, useParams } from "react-router-dom"

import {
  useCopyRecipeMutation,
  useDeleteRecipeMutation,
  useRecipeQuery,
  useSetRecipeFavoriteMutation,
  useSetRecipeSavedMutation,
} from "@/features/recipes/api/recipes.queries"
import { RecipeDetailsContent } from "@/features/recipes/ui/RecipeDetailsContent"
import { RecipeDetailsSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import { createBackNavigationState, getBackNavigationState } from "@/shared/lib/back-navigation"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function RecipeDetailsPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const navigate = useNavigate()
  const location = useLocation()
  const recipeQuery = useRecipeQuery(recipeId)
  const deleteRecipeMutation = useDeleteRecipeMutation()
  const favoriteMutation = useSetRecipeFavoriteMutation()
  const savedMutation = useSetRecipeSavedMutation()
  const copyMutation = useCopyRecipeMutation()

  if (!recipeId) {
    return <Navigate to="/recipes" replace />
  }

  const recipe = recipeQuery.data

  function handleDelete() {
    if (!recipe) {
      return
    }

    deleteRecipeMutation.mutate(recipe.id, {
      onSuccess: () => {
        const backNavigation = getBackNavigationState(location.state)
        void navigate(backNavigation?.backTo ?? "/recipes", {
          replace: true,
          state: backNavigation?.backState,
        })
      },
    })
  }

  function toggleFavorite() {
    if (!recipe) {
      return
    }

    favoriteMutation.mutate({
      recipeId: recipe.id,
      isFavorite: !recipe.isFavorite,
    })
  }

  function toggleSaved() {
    if (!recipe) {
      return
    }

    savedMutation.mutate({
      recipeId: recipe.id,
      isSaved: !recipe.isSaved,
    })
  }

  function copy() {
    if (!recipe) {
      return
    }

    copyMutation.mutate(recipe.id, {
      onSuccess: (createdRecipe) => {
        void navigate(`/recipes/${createdRecipe.id}/edit`, {
          state: createBackNavigationState(location),
        })
      },
    })
  }

  return (
    <div className="space-y-5">
      {recipeQuery.isPending ? <RecipeDetailsSkeleton /> : null}
      {recipeQuery.error ? <ErrorAlert error={recipeQuery.error} /> : null}
      {deleteRecipeMutation.error ? <ErrorAlert error={deleteRecipeMutation.error} /> : null}
      {favoriteMutation.error ? <ErrorAlert error={favoriteMutation.error} /> : null}
      {savedMutation.error ? <ErrorAlert error={savedMutation.error} /> : null}
      {copyMutation.error ? <ErrorAlert error={copyMutation.error} /> : null}

      {recipe ? (
        <RecipeDetailsContent
          recipe={recipe}
          isFavoritePending={favoriteMutation.isPending}
          isSavedPending={savedMutation.isPending}
          isCopyPending={copyMutation.isPending}
          isDeletePending={deleteRecipeMutation.isPending}
          onToggleFavorite={toggleFavorite}
          onToggleSaved={toggleSaved}
          onCopy={copy}
          onDelete={handleDelete}
        />
      ) : null}
    </div>
  )
}

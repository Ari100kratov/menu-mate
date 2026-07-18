import { Navigate, useLocation, useNavigate, useParams, useSearchParams } from "react-router-dom"

import {
  useDeleteRecipeMutation,
  useRecipeQuery,
  useSetRecipeFavoriteMutation,
} from "@/features/recipes/api/recipes.queries"
import { RecipeDetailsContent } from "@/features/recipes/ui/RecipeDetailsContent"
import { RecipeDetailsSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import {
  createBackNavigationState,
  getBackNavigationState,
  getParentBackState,
} from "@/shared/lib/back-navigation"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function RecipeDetailsPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const [searchParams] = useSearchParams()
  const revisionId = searchParams.get("revisionId") ?? undefined
  const navigate = useNavigate()
  const location = useLocation()
  const recipeQuery = useRecipeQuery(recipeId, revisionId)
  const deleteRecipeMutation = useDeleteRecipeMutation()
  const favoriteMutation = useSetRecipeFavoriteMutation()

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

    favoriteMutation.mutate(
      {
        recipeId: recipe.id,
        isFavorite: !recipe.isFavorite,
        revisionId: recipe.isFavorite ? undefined : recipe.revisionId,
      },
      {
        onSuccess: () => {
          if (recipe.revisionState === "SourceUnavailable" && recipe.isFavorite) {
            const backNavigation = getBackNavigationState(location.state)
            void navigate(backNavigation?.backTo ?? "/recipes", {
              replace: true,
              state: backNavigation?.backState,
            })
          }
        },
      },
    )
  }

  function updateSavedRevision() {
    if (!recipe) {
      return
    }

    favoriteMutation.mutate(
      {
        recipeId: recipe.id,
        isFavorite: true,
        revisionId: recipe.revisionId,
      },
      {
        onSuccess: () => {
          void navigate(`${location.pathname}${location.search}${location.hash}`, {
            replace: true,
            state: getParentBackState(location.state),
          })
        },
      },
    )
  }

  function openCopyDraft() {
    if (!recipe) {
      return
    }

    void navigate(
      `/recipes/${recipe.id}/copy?revisionId=${encodeURIComponent(recipe.revisionId)}`,
      { state: createBackNavigationState(location) },
    )
  }

  return (
    <div className="space-y-5">
      {recipeQuery.isPending ? <RecipeDetailsSkeleton /> : null}
      {recipeQuery.error ? <ErrorAlert error={recipeQuery.error} /> : null}
      {deleteRecipeMutation.error ? <ErrorAlert error={deleteRecipeMutation.error} /> : null}
      {favoriteMutation.error ? <ErrorAlert error={favoriteMutation.error} /> : null}

      {recipe ? (
        <RecipeDetailsContent
          recipe={recipe}
          isFavoritePending={favoriteMutation.isPending}
          isDeletePending={deleteRecipeMutation.isPending}
          onToggleFavorite={toggleFavorite}
          onUpdateSavedRevision={updateSavedRevision}
          onCopy={openCopyDraft}
          onDelete={handleDelete}
        />
      ) : null}
    </div>
  )
}

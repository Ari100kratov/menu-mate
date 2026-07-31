import { Lightbulb } from "lucide-react"
import { useState } from "react"
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
import { Button } from "@/shared/ui/button"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/shared/ui/dialog"
import { PageFloatingActions } from "@/shared/ui/page-floating-actions"

export default function RecipeDetailsPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const [searchParams] = useSearchParams()
  const revisionId = searchParams.get("revisionId") ?? undefined
  const menuServings = parseMenuServings(searchParams.get("menuServings"))
  const navigate = useNavigate()
  const location = useLocation()
  const recipeQuery = useRecipeQuery(recipeId, revisionId)
  const deleteRecipeMutation = useDeleteRecipeMutation()
  const favoriteMutation = useSetRecipeFavoriteMutation()
  const [isAdviceOpen, setIsAdviceOpen] = useState(false)

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
        <>
          <RecipeDetailsContent
            key={`${recipe.revisionId}:${String(menuServings ?? "original")}`}
            recipe={recipe}
            menuServings={menuServings}
            isFavoritePending={favoriteMutation.isPending}
            isDeletePending={deleteRecipeMutation.isPending}
            onToggleFavorite={toggleFavorite}
            onUpdateSavedRevision={updateSavedRevision}
            onCopy={openCopyDraft}
            onDelete={handleDelete}
          />
          {recipe.advice ? (
            <Dialog open={isAdviceOpen} onOpenChange={setIsAdviceOpen}>
              <PageFloatingActions>
                <Button
                  type="button"
                  size="icon-lg"
                  className="size-12 rounded-full shadow-lg"
                  aria-label="Показать советы"
                  title="Советы"
                  onClick={() => {
                    setIsAdviceOpen(true)
                  }}
                >
                  <Lightbulb />
                </Button>
              </PageFloatingActions>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Советы</DialogTitle>
                </DialogHeader>
                <div className="max-h-[60svh] overflow-y-auto px-5 pb-5">
                  <p className="type-body break-words whitespace-pre-wrap">{recipe.advice}</p>
                </div>
              </DialogContent>
            </Dialog>
          ) : null}
        </>
      ) : null}
    </div>
  )
}

function parseMenuServings(value: string | null) {
  if (!value) {
    return null
  }

  const servings = Number(value)
  return Number.isInteger(servings) && servings >= 1 && servings <= 100 ? servings : null
}

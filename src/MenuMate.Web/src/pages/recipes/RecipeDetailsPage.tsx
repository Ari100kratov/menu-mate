import { ArrowLeft, Pencil } from "lucide-react"
import { Link, Navigate, useNavigate, useParams } from "react-router-dom"

import { RecipeDetailsContent } from "@/features/recipes/ui/RecipeDetailsContent"
import {
  useDeleteRecipeMutation,
  useRecipeQuery,
  useSetRecipeFavoriteMutation,
} from "@/features/recipes/api/recipes.queries"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { PageHeader } from "@/shared/ui/page"

export default function RecipeDetailsPage() {
  const { recipeId } = useParams<{ recipeId: string }>()
  const navigate = useNavigate()
  const recipeQuery = useRecipeQuery(recipeId)
  const deleteRecipeMutation = useDeleteRecipeMutation()
  const favoriteMutation = useSetRecipeFavoriteMutation()

  if (!recipeId) {
    return <Navigate to="/recipes" replace />
  }

  const recipe = recipeQuery.data

  function handleDelete() {
    if (!recipe || !window.confirm(`Удалить рецепт «${recipe.title}»?`)) {
      return
    }

    deleteRecipeMutation.mutate(recipe.id, {
      onSuccess: () => {
        void navigate("/recipes", { replace: true })
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

  return (
    <div className="space-y-6">
      <PageHeader
        title={recipe?.title ?? "Рецепт"}
        description="Состав, шаги приготовления и быстрые действия."
        action={
          <>
            <Button asChild variant="outline">
              <Link to="/recipes">
                <ArrowLeft />К списку
              </Link>
            </Button>
            {recipe ? (
              <Button asChild>
                <Link to={`/recipes/${recipe.id}/edit`}>
                  <Pencil />
                  Изменить
                </Link>
              </Button>
            ) : null}
          </>
        }
      />

      {recipeQuery.isPending ? <PageSkeleton /> : null}
      {recipeQuery.error ? <ErrorAlert error={recipeQuery.error} /> : null}
      {deleteRecipeMutation.error ? <ErrorAlert error={deleteRecipeMutation.error} /> : null}
      {favoriteMutation.error ? <ErrorAlert error={favoriteMutation.error} /> : null}

      {recipe ? (
        <RecipeDetailsContent
          recipe={recipe}
          isFavoritePending={favoriteMutation.isPending}
          isDeletePending={deleteRecipeMutation.isPending}
          onToggleFavorite={toggleFavorite}
          onDelete={handleDelete}
        />
      ) : null}
    </div>
  )
}

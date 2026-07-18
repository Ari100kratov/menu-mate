import { ArrowRight, Info } from "lucide-react"
import { Link, useLocation } from "react-router-dom"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import { createBackNavigationState } from "@/shared/lib/back-navigation"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { RecipeDetailsActions } from "./RecipeDetailsActions"
import { RecipeIngredients, RecipeSteps, RecipeTags } from "./RecipeDetailsSections"
import { RecipeHero } from "./RecipeHero"

interface RecipeDetailsContentProps {
  recipe: Recipe
  isFavoritePending: boolean
  isDeletePending: boolean
  onToggleFavorite: () => void
  onUpdateSavedRevision: () => void
  onCopy: () => void
  onDelete: () => void
}

export function RecipeDetailsContent({
  recipe,
  isFavoritePending,
  isDeletePending,
  onToggleFavorite,
  onUpdateSavedRevision,
  onCopy,
  onDelete,
}: RecipeDetailsContentProps) {
  const location = useLocation()
  const canUpdateSavedRevision =
    recipe.revisionState === "Current" &&
    recipe.isFavorite &&
    recipe.savedRevisionId !== recipe.revisionId

  return (
    <div className="mx-auto max-w-3xl space-y-5">
      {recipe.revisionState === "SourceUnavailable" ? (
        <Alert>
          <Info />
          <AlertTitle>Рецепт недоступен</AlertTitle>
          <AlertDescription>
            Владелец скрыл или удалил рецепт. Сохраненная версия остается доступна для создания
            собственной копии.
          </AlertDescription>
        </Alert>
      ) : null}

      {recipe.revisionState === "UpdateAvailable" && recipe.currentRevisionId ? (
        <Alert>
          <Info />
          <AlertTitle>Доступна новая версия</AlertTitle>
          <AlertDescription className="flex flex-wrap items-center justify-between gap-3">
            <span>Владелец изменил рецепт после того, как вы добавили его в избранное.</span>
            <Button asChild variant="outline" size="sm">
              <Link
                to={`/recipes/${recipe.id}?revisionId=${encodeURIComponent(recipe.currentRevisionId)}`}
                state={createBackNavigationState(location)}
              >
                Посмотреть новую версию
                <ArrowRight />
              </Link>
            </Button>
          </AlertDescription>
        </Alert>
      ) : null}

      {recipe.revisionState === "Historical" ? (
        <Alert>
          <Info />
          <AlertTitle>Историческая версия</AlertTitle>
          <AlertDescription>
            Эта версия больше не закреплена в вашем избранном. Ее можно просмотреть или сохранить
            как отдельную копию.
          </AlertDescription>
        </Alert>
      ) : null}

      {canUpdateSavedRevision ? (
        <Alert>
          <Info />
          <AlertTitle>Новая версия рецепта</AlertTitle>
          <AlertDescription className="flex flex-wrap items-center justify-between gap-3">
            <span>Замените сохраненную версию этой актуальной ревизией.</span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={isFavoritePending}
              onClick={onUpdateSavedRevision}
            >
              Обновить сохраненную версию
            </Button>
          </AlertDescription>
        </Alert>
      ) : null}

      <div className="bg-card space-y-5 overflow-hidden rounded-xl border p-4 shadow-sm md:p-6">
        <RecipeHero
          recipe={recipe}
          actions={
            <RecipeDetailsActions
              recipe={recipe}
              isFavoritePending={isFavoritePending}
              isDeletePending={isDeletePending}
              onToggleFavorite={onToggleFavorite}
              onCopy={onCopy}
              onDelete={onDelete}
            />
          }
        />
        <RecipeTags tags={recipe.tags} />
        <RecipeIngredients recipe={recipe} />
        <RecipeSteps recipe={recipe} />
      </div>
    </div>
  )
}

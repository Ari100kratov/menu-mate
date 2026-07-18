import { CalendarPlus, Copy, Heart, Pencil, Trash2 } from "lucide-react"
import type { ReactNode } from "react"
import { Link, useLocation } from "react-router-dom"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import { createBackNavigationState } from "@/shared/lib/back-navigation"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/ui/alert-dialog"
import { Button } from "@/shared/ui/button"
import { Tooltip, TooltipContent, TooltipTrigger } from "@/shared/ui/tooltip"

interface RecipeDetailsActionsProps {
  recipe: Recipe
  isFavoritePending: boolean
  isDeletePending: boolean
  onToggleFavorite: () => void
  onCopy: () => void
  onDelete: () => void
}

export function RecipeDetailsActions({
  recipe,
  isFavoritePending,
  isDeletePending,
  onToggleFavorite,
  onCopy,
  onDelete,
}: RecipeDetailsActionsProps) {
  const location = useLocation()
  const isCurrent = recipe.revisionState === "Current"
  const isHistorical = recipe.revisionState === "Historical"
  const sourceUnavailable = recipe.revisionState === "SourceUnavailable"
  const canFavoriteDisplayedRevision = isCurrent || recipe.isDisplayedRevisionSaved
  const showFavorite =
    !isHistorical && (recipe.isFavorite || (!sourceUnavailable && canFavoriteDisplayedRevision))
  const showCopy = !recipe.isOwnedByCurrentUser || sourceUnavailable || isHistorical

  return (
    <div className="flex shrink-0 flex-wrap justify-end gap-1">
      {!sourceUnavailable && !isHistorical ? (
        <ActionTooltip label="Добавить в меню">
          <Button asChild variant="ghost" size="icon">
            <Link to={getMenuPlacementUrl(recipe)} aria-label="Добавить в меню">
              <CalendarPlus className="size-4" />
            </Link>
          </Button>
        </ActionTooltip>
      ) : null}

      {recipe.isOwnedByCurrentUser && isCurrent ? (
        <ActionTooltip label="Изменить рецепт">
          <Button asChild variant="ghost" size="icon">
            <Link
              to={`/recipes/${recipe.id}/edit`}
              state={createBackNavigationState(location)}
              aria-label="Изменить рецепт"
            >
              <Pencil className="size-4" />
            </Link>
          </Button>
        </ActionTooltip>
      ) : null}

      {showFavorite ? (
        <ActionTooltip label={recipe.isFavorite ? "Убрать из избранного" : "В избранное"}>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            aria-label={recipe.isFavorite ? "Убрать из избранного" : "В избранное"}
            disabled={isFavoritePending}
            onClick={onToggleFavorite}
          >
            <Heart className={recipe.isFavorite ? "fill-primary text-primary size-4" : "size-4"} />
          </Button>
        </ActionTooltip>
      ) : null}

      {showCopy ? (
        <ActionTooltip label="Создать свою копию">
          <Button
            type="button"
            variant="ghost"
            size="icon"
            aria-label="Создать свою копию"
            onClick={onCopy}
          >
            <Copy className="size-4" />
          </Button>
        </ActionTooltip>
      ) : null}

      {recipe.isOwnedByCurrentUser && isCurrent ? (
        <AlertDialog>
          <ActionTooltip label="Удалить рецепт">
            <AlertDialogTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="text-destructive hover:text-destructive"
                aria-label="Удалить рецепт"
                disabled={isDeletePending}
              >
                <Trash2 className="size-4" />
              </Button>
            </AlertDialogTrigger>
          </ActionTooltip>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Удалить рецепт?</AlertDialogTitle>
              <AlertDialogDescription>
                «{recipe.title}» будет скрыт. Пользователи, сохранившие точную версию в избранном
                или меню, смогут открыть ее только для копирования.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Отмена</AlertDialogCancel>
              <AlertDialogAction disabled={isDeletePending} onClick={onDelete}>
                {isDeletePending ? "Удаляем..." : "Удалить рецепт"}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      ) : null}
    </div>
  )
}

function getMenuPlacementUrl(recipe: Recipe) {
  const params = new URLSearchParams({
    recipeId: recipe.id,
    revisionId: recipe.revisionId,
    title: recipe.title,
    servings: String(recipe.servings),
  })
  return `/menu?${params.toString()}`
}

function ActionTooltip({ label, children }: { label: string; children: ReactNode }) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>{children}</TooltipTrigger>
      <TooltipContent>{label}</TooltipContent>
    </Tooltip>
  )
}

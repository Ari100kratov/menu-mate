import { Bookmark, CalendarPlus, Copy, Heart, Pencil, Trash2 } from "lucide-react"
import type { ReactNode } from "react"
import { Link } from "react-router-dom"

import type { Recipe } from "@/features/recipes/api/recipes.api"
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
  isSavedPending: boolean
  isCopyPending: boolean
  isDeletePending: boolean
  onToggleFavorite: () => void
  onToggleSaved: () => void
  onCopy: () => void
  onDelete: () => void
}

export function RecipeDetailsActions({
  recipe,
  isFavoritePending,
  isSavedPending,
  isCopyPending,
  isDeletePending,
  onToggleFavorite,
  onToggleSaved,
  onCopy,
  onDelete,
}: RecipeDetailsActionsProps) {
  return (
    <div className="flex shrink-0 flex-wrap justify-end gap-1">
      <ActionTooltip label="Добавить в меню">
        <Button asChild variant="ghost" size="icon">
          <Link to={getMenuPlacementUrl(recipe)} aria-label="Добавить в меню">
            <CalendarPlus className="size-4" />
          </Link>
        </Button>
      </ActionTooltip>

      {recipe.isOwnedByCurrentUser ? (
        <ActionTooltip label="Изменить рецепт">
          <Button asChild variant="ghost" size="icon">
            <Link to={`/recipes/${recipe.id}/edit`} aria-label="Изменить рецепт">
              <Pencil className="size-4" />
            </Link>
          </Button>
        </ActionTooltip>
      ) : null}

      <ActionTooltip label={recipe.isFavorite ? "Убрать из избранного" : "В избранное"}>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label={recipe.isFavorite ? "Убрать из избранного" : "В избранное"}
          disabled={isFavoritePending}
          onClick={onToggleFavorite}
        >
          <Heart className={recipe.isFavorite ? "size-4 fill-primary text-primary" : "size-4"} />
        </Button>
      </ActionTooltip>

      {!recipe.isOwnedByCurrentUser ? (
        <>
          <ActionTooltip label={recipe.isSaved ? "Убрать из библиотеки" : "Сохранить в библиотеку"}>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label={recipe.isSaved ? "Убрать из библиотеки" : "Сохранить в библиотеку"}
              disabled={isSavedPending}
              onClick={onToggleSaved}
            >
              <Bookmark className={recipe.isSaved ? "size-4 fill-primary text-primary" : "size-4"} />
            </Button>
          </ActionTooltip>
          <ActionTooltip label="Создать свою копию">
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label="Создать свою копию"
              disabled={isCopyPending}
              onClick={onCopy}
            >
              <Copy className="size-4" />
            </Button>
          </ActionTooltip>
        </>
      ) : null}

      {recipe.isOwnedByCurrentUser ? (
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
                «{recipe.title}» будет удалён без возможности восстановления.
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
    revisionId: recipe.currentRevisionId,
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

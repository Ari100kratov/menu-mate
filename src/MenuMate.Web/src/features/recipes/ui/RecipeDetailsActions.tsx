import { ExternalLink, Pencil, Star, Trash2 } from "lucide-react"
import { Link } from "react-router-dom"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import { Button } from "@/shared/ui/button"
import { MobileStickyActions } from "@/shared/ui/page"

interface RecipeDetailsActionsProps {
  recipe: Recipe
  isFavoritePending: boolean
  isDeletePending: boolean
  onToggleFavorite: () => void
  onDelete: () => void
}

export function RecipeDetailsActions(props: RecipeDetailsActionsProps) {
  return (
    <>
      <MobileRecipeActions {...props} />
      <DesktopRecipeActions {...props} />
    </>
  )
}

function MobileRecipeActions({
  recipe,
  isFavoritePending,
  onToggleFavorite,
}: RecipeDetailsActionsProps) {
  return (
    <MobileStickyActions>
      <Button
        type="button"
        variant="outline"
        disabled={isFavoritePending}
        onClick={onToggleFavorite}
      >
        <Star className={recipe.isFavorite ? "fill-primary text-primary" : undefined} />
        {recipe.isFavorite ? "Убрать" : "В избранное"}
      </Button>
      <Button asChild>
        <Link to={`/recipes/${recipe.id}/edit`}>
          <Pencil />
          Изменить
        </Link>
      </Button>
    </MobileStickyActions>
  )
}

function DesktopRecipeActions({
  recipe,
  isFavoritePending,
  isDeletePending,
  onToggleFavorite,
  onDelete,
}: RecipeDetailsActionsProps) {
  return (
    <div className="hidden flex-wrap gap-2 md:flex">
      <Button
        type="button"
        variant="outline"
        disabled={isFavoritePending}
        onClick={onToggleFavorite}
      >
        <Star className={recipe.isFavorite ? "fill-primary text-primary" : undefined} />
        {recipe.isFavorite ? "Убрать из избранного" : "В избранное"}
      </Button>

      {recipe.sourceUrl ? (
        <Button asChild variant="outline">
          <a href={recipe.sourceUrl} target="_blank" rel="noreferrer">
            <ExternalLink />
            Источник
          </a>
        </Button>
      ) : null}

      <Button type="button" variant="ghost" disabled={isDeletePending} onClick={onDelete}>
        <Trash2 />
        Удалить
      </Button>
    </div>
  )
}

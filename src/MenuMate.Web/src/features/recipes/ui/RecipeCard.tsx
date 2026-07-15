import { Bookmark, Clock3, Heart, ImageIcon, Timer, Users } from "lucide-react"
import { Link } from "react-router-dom"

import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { getRecipeCategoryLabel } from "@/features/recipes/model/recipe-form-options"
import { Button } from "@/shared/ui/button"
import { RecipeImage } from "./RecipeImage"

interface RecipeCardProps {
  recipe: RecipeListItem
  isFavoritePending: boolean
  onToggleFavorite: () => void
}

export function RecipeCard({ recipe, isFavoritePending, onToggleFavorite }: RecipeCardProps) {
  return (
    <article className="bg-card group hover:border-primary/30 relative grid min-h-28 grid-cols-[7rem_minmax(0,1fr)] overflow-hidden rounded-xl border shadow-sm transition hover:shadow-md sm:min-h-32 sm:grid-cols-[8rem_minmax(0,1fr)]">
      <Link
        to={`/recipes/${recipe.id}`}
        className="bg-muted block h-28 self-center overflow-hidden outline-none sm:h-32"
      >
        {recipe.coverImage?.readUrl ? (
          <RecipeImage
            frameClassName="size-full"
            imageClassName="object-cover transition duration-300 group-hover:scale-[1.03]"
            src={recipe.coverImage.readUrl}
            alt={recipe.coverImage.altText ?? recipe.title}
          />
        ) : (
          <span className="text-muted-foreground flex size-full items-center justify-center">
            <ImageIcon className="size-6" />
          </span>
        )}
      </Link>

      <Link
        to={`/recipes/${recipe.id}`}
        className="flex min-w-0 flex-col justify-center gap-2 p-3 outline-none sm:p-4"
      >
        <div className="space-y-0.5">
          <h2 className="type-subsection-title line-clamp-2 pr-8">{recipe.title}</h2>
          <div className="flex flex-wrap items-center gap-2">
            <span className="type-label text-primary">
              {getRecipeCategoryLabel(recipe.category)}
            </span>
            {!recipe.isOwnedByCurrentUser && recipe.isSaved ? (
              <span className="type-label text-muted-foreground flex items-center gap-1">
                <Bookmark className="size-4 fill-current" />
                Сохранён
              </span>
            ) : null}
          </div>
        </div>
        <div className="type-supporting text-muted-foreground flex flex-wrap gap-x-3 gap-y-1">
          {recipe.totalTimeMinutes == null ? null : (
            <span className="flex items-center gap-1">
              <Clock3 className="size-4" />
              {formatMinutes(Number(recipe.totalTimeMinutes))}
            </span>
          )}
          {recipe.activeTimeMinutes == null ? null : (
            <span
              className="text-primary flex items-center gap-1"
              aria-label={`Активное время: ${formatMinutes(Number(recipe.activeTimeMinutes))}`}
              title="Активное время"
            >
              <Timer className="size-4" />
              {formatMinutes(Number(recipe.activeTimeMinutes))}
            </span>
          )}
          <span className="flex items-center gap-1">
            <Users className="size-4" />
            {String(recipe.servings)}
          </span>
        </div>
      </Link>

      <Button
        type="button"
        variant="ghost"
        size="icon-sm"
        className="absolute top-2 right-2 rounded-full"
        aria-label={recipe.isFavorite ? "Убрать из избранного" : "Добавить в избранное"}
        disabled={isFavoritePending}
        onClick={onToggleFavorite}
      >
        <Heart className={recipe.isFavorite ? "fill-primary text-primary size-4" : "size-4"} />
      </Button>
    </article>
  )
}

function formatMinutes(minutes: number) {
  const hours = Math.floor(minutes / 60)
  const rest = minutes % 60
  if (hours === 0) return `${String(rest)} мин`
  return rest === 0 ? `${String(hours)} ч` : `${String(hours)} ч ${String(rest)} мин`
}

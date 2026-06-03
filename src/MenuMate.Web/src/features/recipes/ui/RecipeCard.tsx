import { BookOpen, ImageIcon, Pencil, Star, Trash2 } from "lucide-react"
import { Link } from "react-router-dom"

import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { normalizeTagValue } from "@/features/tags/model/tag-values"
import { Button } from "@/shared/ui/button"

interface RecipeCardProps {
  recipe: RecipeListItem
  isMutationPending: boolean
  activeTag: string
  onDelete: () => void
  onToggleFavorite: () => void
  onSelectTag: (tag: string) => void
}

export function RecipeCard({
  recipe,
  isMutationPending,
  activeTag,
  onDelete,
  onToggleFavorite,
  onSelectTag,
}: RecipeCardProps) {
  return (
    <article className="bg-card text-card-foreground flex min-h-full flex-col rounded-md border">
      <Link
        to={`/recipes/${recipe.id}`}
        className="focus-visible:ring-ring block overflow-hidden rounded-t-md outline-none focus-visible:ring-2 focus-visible:ring-offset-2"
      >
        {recipe.coverImage?.readUrl ? (
          <img
            className="bg-muted aspect-[4/3] w-full object-cover"
            src={recipe.coverImage.readUrl}
            alt={recipe.coverImage.altText ?? recipe.title}
          />
        ) : (
          <div className="bg-muted text-muted-foreground flex aspect-[4/3] w-full flex-col items-center justify-center gap-2">
            <ImageIcon className="size-8" />
            <span className="text-sm">Нет обложки</span>
          </div>
        )}
      </Link>

      <div className="flex flex-1 flex-col justify-between p-4">
        <div className="space-y-3">
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0">
              <h2 className="line-clamp-2 text-lg font-semibold tracking-normal">
                <Link to={`/recipes/${recipe.id}`} className="hover:text-primary hover:underline">
                  {recipe.title}
                </Link>
              </h2>
              <p className="text-muted-foreground text-sm">{recipe.servings} порц.</p>
            </div>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label={recipe.isFavorite ? "Убрать из избранного" : "Добавить в избранное"}
              disabled={isMutationPending}
              onClick={onToggleFavorite}
            >
              <Star className={recipe.isFavorite ? "fill-primary text-primary" : undefined} />
            </Button>
          </div>

          {recipe.description ? (
            <p className="text-muted-foreground line-clamp-3 text-sm">{recipe.description}</p>
          ) : null}

          {recipe.tags.length > 0 ? (
            <div className="flex flex-wrap gap-2">
              {recipe.tags.map((tag) => (
                <button
                  key={tag}
                  type="button"
                  className="bg-secondary text-secondary-foreground hover:bg-primary/10 hover:text-primary focus-visible:ring-ring rounded-md px-2 py-1 text-xs outline-none focus-visible:ring-2"
                  aria-pressed={normalizeTagValue(tag) === normalizeTagValue(activeTag)}
                  onClick={() => {
                    onSelectTag(tag)
                  }}
                >
                  {tag}
                </button>
              ))}
            </div>
          ) : null}
        </div>

        <div className="mt-4 grid grid-cols-2 gap-2">
          <Button asChild variant="outline" size="sm">
            <Link to={`/recipes/${recipe.id}`}>
              <BookOpen />
              Открыть
            </Link>
          </Button>
          <Button asChild variant="outline" size="sm">
            <Link to={`/recipes/${recipe.id}/edit`}>
              <Pencil />
              Изменить
            </Link>
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="col-span-2"
            disabled={isMutationPending}
            onClick={onDelete}
          >
            <Trash2 />
            Удалить
          </Button>
        </div>
      </div>
    </article>
  )
}

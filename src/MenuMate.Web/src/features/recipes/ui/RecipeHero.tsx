import { ChefHat, Clock3, ExternalLink, Globe2, LockKeyhole, Timer, Users } from "lucide-react"
import type { ReactNode } from "react"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import { findCoverImage } from "@/features/recipes/model/recipe-images"
import { getRecipeCategoryLabel } from "@/features/recipes/model/recipe-form-options"
import { RecipeImagePreview } from "./RecipeImagePreview"

interface RecipeHeroProps {
  recipe: Recipe
  actions: ReactNode
}

export function RecipeHero({ recipe, actions }: RecipeHeroProps) {
  const coverImage = findCoverImage(recipe.images)

  return (
    <section className="grid gap-5 md:grid-cols-[minmax(15rem,20rem)_minmax(0,1fr)] md:items-start">
      <div className="mx-auto w-full max-w-md">
        <RecipeImagePreview image={coverImage} fallbackTitle={recipe.title} />
      </div>
      <div className="space-y-5">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0 space-y-2">
            <h2 className="type-recipe-title">{recipe.title}</h2>
            {recipe.description ? (
              <p className="type-body text-muted-foreground">{recipe.description}</p>
            ) : null}
          </div>
          {actions}
        </div>

        <div className="grid grid-cols-2 gap-x-4 gap-y-4">
          <RecipeMeta
            icon={ChefHat}
            label="Категория"
            value={getRecipeCategoryLabel(recipe.category)}
          />
          <RecipeMeta icon={Users} label="Количество" value={`${String(recipe.servings)} порции`} />
          {recipe.totalTimeMinutes === null ? null : (
            <RecipeMeta
              icon={Clock3}
              label="Общее время"
              value={formatMinutes(Number(recipe.totalTimeMinutes))}
            />
          )}
          {recipe.activeTimeMinutes === null ? null : (
            <RecipeMeta
              icon={Timer}
              label="Активное время"
              value={formatMinutes(Number(recipe.activeTimeMinutes))}
            />
          )}
          <RecipeMeta
            icon={recipe.visibility === "Public" ? Globe2 : LockKeyhole}
            label="Доступ"
            value={recipe.visibility === "Public" ? "Публичный рецепт" : "Личный рецепт"}
          />
        </div>

        {recipe.sourceUrl ? (
          <div className="space-y-1">
            <h3 className="type-label">Источник</h3>
            <a
              className="type-supporting text-primary inline-flex max-w-full items-center gap-1.5 hover:underline"
              href={recipe.sourceUrl}
              target="_blank"
              rel="noreferrer"
            >
              <span className="truncate">Открыть оригинал рецепта</span>
              <ExternalLink className="size-4 shrink-0" />
            </a>
          </div>
        ) : null}
      </div>
    </section>
  )
}

function formatMinutes(minutes: number) {
  const hours = Math.floor(minutes / 60)
  const rest = minutes % 60

  if (hours === 0) {
    return `${String(rest)} мин`
  }

  return rest === 0 ? `${String(hours)} ч` : `${String(hours)} ч ${String(rest)} мин`
}

function RecipeMeta({
  icon: Icon,
  label,
  value,
}: {
  icon: typeof ChefHat
  label: string
  value: string
}) {
  return (
    <div className="flex min-w-0 items-start gap-2.5">
      <Icon className="text-primary mt-0.5 size-4 shrink-0" />
      <div className="min-w-0">
        <div className="type-supporting text-muted-foreground">{label}</div>
        <div className="type-label break-words">{value}</div>
      </div>
    </div>
  )
}

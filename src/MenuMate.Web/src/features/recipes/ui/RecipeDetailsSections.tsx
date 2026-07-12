import { CircleDashed, MessageSquareText, Tag } from "lucide-react"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import { formatRecipeIngredientQuantity } from "@/features/recipes/model/recipe-ingredient-format"
import { getProductCategoryLabel } from "@/features/recipes/model/recipe-form-options"
import { findStepImage } from "@/features/recipes/model/recipe-images"
import { RecipeImagePreview } from "@/features/recipes/ui/RecipeImagePreview"

interface RecipeDetailsSectionsProps {
  recipe: Recipe
}

export function RecipeTags({ tags }: { tags: readonly string[] }) {
  if (tags.length === 0) {
    return null
  }

  return (
    <section className="space-y-3 border-t pt-5">
      <h3 className="type-section-title flex items-center gap-2">
        <Tag className="text-primary size-4" />
        Теги
      </h3>
      <div className="flex flex-wrap gap-2">
        {tags.map((tag) => (
          <span
            key={tag}
            className="type-supporting bg-secondary text-secondary-foreground rounded-full px-3 py-1"
          >
            {tag}
          </span>
        ))}
      </div>
    </section>
  )
}

export function RecipeIngredients({ recipe }: RecipeDetailsSectionsProps) {
  return (
    <section className="space-y-3 border-t pt-5">
      <h3 className="type-section-title">Ингредиенты</h3>
      <div className="space-y-2">
        {recipe.ingredients.map((ingredient) => (
          <div
            key={`${ingredient.productName}-${ingredient.comment ?? ""}`}
            className="rounded-lg border px-3 py-3"
          >
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div className="type-body font-medium">{ingredient.productName}</div>
              {ingredient.isOptional ? (
                <span className="type-supporting bg-accent text-accent-foreground inline-flex items-center gap-1 rounded-full px-2 py-0.5">
                  <CircleDashed className="size-3.5" />
                  Можно пропустить
                </span>
              ) : null}
            </div>
            <div className="type-supporting text-muted-foreground mt-1 flex flex-wrap items-center gap-x-2 gap-y-1">
              <span>{formatRecipeIngredientQuantity(ingredient)}</span>
              <span className="text-muted-foreground/50">·</span>
              <span>{getProductCategoryLabel(ingredient.category)}</span>
            </div>
            {ingredient.comment ? (
              <div className="type-supporting bg-muted/50 text-muted-foreground mt-2 flex items-start gap-2 rounded-md px-2.5 py-2">
                <MessageSquareText className="mt-0.5 size-3.5 shrink-0" />
                <span>{ingredient.comment}</span>
              </div>
            ) : null}
          </div>
        ))}
      </div>
    </section>
  )
}

export function RecipeSteps({ recipe }: RecipeDetailsSectionsProps) {
  return (
    <section className="space-y-3 border-t pt-5">
      <h3 className="type-section-title">Приготовление</h3>
      <div className="space-y-3">
        {recipe.steps.map((step) => {
          const image = findStepImage(recipe.images, Number(step.number))

          return (
            <article key={step.number} className="grid grid-cols-[2.25rem_1fr] gap-3">
              <div className="bg-secondary text-secondary-foreground flex size-9 items-center justify-center rounded-full text-sm font-semibold">
                {step.number}
              </div>
              <div className="space-y-3">
                <p className="type-body">{step.text}</p>
                {image ? (
                  <div className="max-w-md">
                    <RecipeImagePreview image={image} fallbackTitle={`Шаг ${String(step.number)}`} />
                  </div>
                ) : null}
              </div>
            </article>
          )
        })}
      </div>
    </section>
  )
}

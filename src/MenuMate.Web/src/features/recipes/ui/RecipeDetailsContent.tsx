import type { Recipe } from "@/features/recipes/api/recipes.api"
import { RecipeDetailsActions } from "./RecipeDetailsActions"
import { RecipeIngredients, RecipeSteps, RecipeTags } from "./RecipeDetailsSections"
import { RecipeHero } from "./RecipeHero"

interface RecipeDetailsContentProps {
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

export function RecipeDetailsContent({
  recipe,
  isFavoritePending,
  isSavedPending,
  isCopyPending,
  isDeletePending,
  onToggleFavorite,
  onToggleSaved,
  onCopy,
  onDelete,
}: RecipeDetailsContentProps) {
  return (
    <div className="mx-auto max-w-3xl space-y-5">
      <div className="bg-card space-y-5 overflow-hidden rounded-xl border p-4 shadow-sm md:p-6">
        <RecipeHero
          recipe={recipe}
          actions={
            <RecipeDetailsActions
              recipe={recipe}
              isFavoritePending={isFavoritePending}
              isSavedPending={isSavedPending}
              isCopyPending={isCopyPending}
              isDeletePending={isDeletePending}
              onToggleFavorite={onToggleFavorite}
              onToggleSaved={onToggleSaved}
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

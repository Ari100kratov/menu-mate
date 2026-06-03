import { AddRecipeToMenuPanel } from "@/features/menu-planning/ui/AddRecipeToMenuPanel"
import type { Recipe } from "@/features/recipes/api/recipes.api"
import { RecipeDetailsActions } from "./RecipeDetailsActions"
import { RecipeIngredients, RecipeSteps, RecipeTags } from "./RecipeDetailsSections"
import { RecipeHero } from "./RecipeHero"

interface RecipeDetailsContentProps {
  recipe: Recipe
  isFavoritePending: boolean
  isDeletePending: boolean
  onToggleFavorite: () => void
  onDelete: () => void
}

export function RecipeDetailsContent({
  recipe,
  isFavoritePending,
  isDeletePending,
  onToggleFavorite,
  onDelete,
}: RecipeDetailsContentProps) {
  return (
    <>
      <RecipeDetailsActions
        recipe={recipe}
        isFavoritePending={isFavoritePending}
        isDeletePending={isDeletePending}
        onToggleFavorite={onToggleFavorite}
        onDelete={onDelete}
      />
      <RecipeHero recipe={recipe} />
      <RecipeTags tags={recipe.tags} />
      <AddRecipeToMenuPanel recipe={recipe} />
      <RecipeIngredients recipe={recipe} />
      <RecipeSteps recipe={recipe} />
    </>
  )
}

import type { Recipe } from "@/features/recipes/api/recipes.api"
import { findCoverImage } from "@/features/recipes/model/recipe-images"
import { PageSection } from "@/shared/ui/page"
import { RecipeImagePreview } from "./RecipeImagePreview"

interface RecipeHeroProps {
  recipe: Recipe
}

export function RecipeHero({ recipe }: RecipeHeroProps) {
  const coverImage = findCoverImage(recipe.images)

  return (
    <section className="grid gap-4 md:grid-cols-[minmax(0,22rem)_1fr] md:items-start">
      <RecipeImagePreview image={coverImage} fallbackTitle={recipe.title} />

      <PageSection className="border-0 p-0">
        <div className="flex flex-wrap gap-2 text-sm">
          <RecipeStat value={`${String(recipe.servings)} порц.`} />
          <RecipeStat value={`${String(recipe.ingredients.length)} инг.`} />
          <RecipeStat value={`${String(recipe.steps.length)} шаг.`} />
          {recipe.isFavorite ? <RecipeStat value="Избранное" isPrimary /> : null}
        </div>

        {recipe.description ? (
          <p className="text-muted-foreground max-w-3xl text-sm md:text-base">
            {recipe.description}
          </p>
        ) : null}
      </PageSection>
    </section>
  )
}

function RecipeStat({ value, isPrimary = false }: { value: string; isPrimary?: boolean }) {
  return (
    <span
      className={
        isPrimary
          ? "bg-primary/10 text-primary rounded-md px-2 py-1"
          : "bg-secondary text-secondary-foreground rounded-md px-2 py-1"
      }
    >
      {value}
    </span>
  )
}

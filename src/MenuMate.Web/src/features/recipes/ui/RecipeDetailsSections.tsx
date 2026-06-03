import type { Recipe } from "@/features/recipes/api/recipes.api"
import { findStepImage } from "@/features/recipes/model/recipe-images"
import { PageSection } from "@/shared/ui/page"

interface RecipeDetailsSectionsProps {
  recipe: Recipe
}

export function RecipeTags({ tags }: { tags: readonly string[] }) {
  if (tags.length === 0) {
    return null
  }

  return (
    <section className="flex flex-wrap gap-2">
      {tags.map((tag) => (
        <span
          key={tag}
          className="bg-secondary text-secondary-foreground rounded-md px-2 py-1 text-xs"
        >
          {tag}
        </span>
      ))}
    </section>
  )
}

export function RecipeIngredients({ recipe }: RecipeDetailsSectionsProps) {
  return (
    <PageSection title="Ингредиенты">
      <div className="grid gap-2 md:grid-cols-2">
        {recipe.ingredients.map((ingredient) => (
          <div
            key={`${ingredient.productName}-${ingredient.comment ?? ""}`}
            className="rounded-md border p-3"
          >
            <div className="font-medium">{ingredient.productName}</div>
            <div className="text-muted-foreground text-sm">
              {formatIngredientAmount(ingredient)}
              {ingredient.comment ? `, ${ingredient.comment}` : ""}
              {ingredient.isOptional ? ", необязательно" : ""}
            </div>
          </div>
        ))}
      </div>
    </PageSection>
  )
}

export function RecipeSteps({ recipe }: RecipeDetailsSectionsProps) {
  return (
    <PageSection title="Шаги">
      <div className="space-y-3">
        {recipe.steps.map((step) => {
          const image = findStepImage(recipe.images, Number(step.number))

          return (
            <article
              key={step.number}
              className="grid gap-3 rounded-md border p-3 md:grid-cols-[4rem_1fr]"
            >
              <div className="bg-secondary text-secondary-foreground flex size-10 items-center justify-center rounded-md font-semibold">
                {step.number}
              </div>
              <div className="space-y-3">
                <p className="text-sm md:text-base">{step.text}</p>
                {image?.readUrl ? (
                  <img
                    className="bg-muted aspect-[4/3] w-full max-w-md rounded-md border object-cover"
                    src={image.readUrl}
                    alt={image.altText ?? `Шаг ${String(step.number)}`}
                  />
                ) : null}
              </div>
            </article>
          )
        })}
      </div>
    </PageSection>
  )
}

function formatIngredientAmount(ingredient: Recipe["ingredients"][number]) {
  const amount = ingredient.amount !== null ? `${String(ingredient.amount)} ` : ""
  const unit = ingredient.unit === "None" ? "" : ingredient.unit

  if (!amount && !unit) {
    return ingredient.quantityKind
  }

  return `${amount}${unit}`.trim()
}

import { Plus } from "lucide-react"
import { useMemo, useState } from "react"

import { RecipeCreateMenu } from "@/features/imports/ui/RecipeCreateMenu"
import {
  useRecipesQuery,
  useSetRecipeFavoriteMutation,
} from "@/features/recipes/api/recipes.queries"
import { RecipeCard } from "@/features/recipes/ui/RecipeCard"
import { RecipeFiltersSection } from "@/features/recipes/ui/RecipeFiltersSection"
import { RecipeListSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import { ErrorAlert } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"

export default function RecipesPage() {
  const [search, setSearch] = useState("")
  const [scope, setScope] = useState<"library" | "catalog">("library")
  const [category, setCategory] = useState("")
  const [favoritesOnly, setFavoritesOnly] = useState(false)
  const recipesQuery = useRecipesQuery({ scope, search, favoritesOnly })
  const favoriteMutation = useSetRecipeFavoriteMutation()
  const recipes = useMemo(
    () =>
      category
        ? (recipesQuery.data ?? []).filter((recipe) => recipe.category === category)
        : (recipesQuery.data ?? []),
    [category, recipesQuery.data],
  )

  function resetFilters() {
    setSearch("")
    setCategory("")
    setFavoritesOnly(false)
  }

  return (
    <div className="space-y-5">
      <div className="hidden justify-end sm:flex">
        <RecipeCreateMenu />
      </div>

      <RecipeFiltersSection
        scope={scope}
        search={search}
        category={category}
        favoritesOnly={favoritesOnly}
        recipesCount={recipesQuery.data ? recipes.length : undefined}
        onScopeChange={setScope}
        onSearchChange={setSearch}
        onCategoryChange={setCategory}
        onFavoritesOnlyChange={setFavoritesOnly}
        onReset={resetFilters}
      />

      {recipesQuery.error ? <ErrorAlert error={recipesQuery.error} /> : null}
      {favoriteMutation.error ? <ErrorAlert error={favoriteMutation.error} /> : null}

      {recipesQuery.isPending ? (
        <RecipeListSkeleton />
      ) : recipes.length > 0 ? (
        <section className="grid gap-3 lg:grid-cols-2">
          {recipes.map((recipe) => (
            <RecipeCard
              key={recipe.id}
              recipe={recipe}
              isFavoritePending={favoriteMutation.isPending}
              onToggleFavorite={() => {
                favoriteMutation.mutate({
                  recipeId: recipe.id,
                  isFavorite: !recipe.isFavorite,
                })
              }}
            />
          ))}
        </section>
      ) : (
        <EmptyState
          icon={Plus}
          title="Рецептов пока нет"
          description="Добавьте первый рецепт, чтобы использовать его в меню и списках покупок."
          action={<RecipeCreateMenu />}
        />
      )}

      <RecipeCreateMenu
        iconOnly
        className="fixed right-4 bottom-20 z-30 rounded-full shadow-lg sm:hidden"
      />
    </div>
  )
}

import { Plus } from "lucide-react"
import { useMemo, useState } from "react"
import { Link } from "react-router-dom"

import { useRecipesQuery, useSetRecipeFavoriteMutation } from "@/features/recipes/api/recipes.queries"
import { RecipeCard } from "@/features/recipes/ui/RecipeCard"
import { RecipeFiltersSection } from "@/features/recipes/ui/RecipeFiltersSection"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
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
        <Button asChild>
          <Link to="/recipes/new">
            <Plus />
            Добавить
          </Link>
        </Button>
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
        <PageSkeleton />
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
          action={
            <Button asChild>
              <Link to="/recipes/new">
                <Plus />
                Добавить рецепт
              </Link>
            </Button>
          }
        />
      )}

      <Button
        asChild
        size="icon-lg"
        className="fixed right-4 bottom-20 z-30 rounded-full shadow-lg sm:hidden"
      >
        <Link to="/recipes/new" aria-label="Добавить рецепт">
          <Plus />
        </Link>
      </Button>
    </div>
  )
}

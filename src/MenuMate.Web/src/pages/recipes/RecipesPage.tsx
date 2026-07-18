import { Plus, Search } from "lucide-react"

import { RecipeCreateMenu } from "@/features/imports/ui/RecipeCreateMenu"
import {
  useInfiniteRecipesQuery,
  useSetRecipeFavoriteMutation,
} from "@/features/recipes/api/recipes.queries"
import { useRecipeListFilterState } from "@/features/recipes/model/recipe-list-filter-state"
import { RecipeCard } from "@/features/recipes/ui/RecipeCard"
import { RecipeFiltersSection } from "@/features/recipes/ui/RecipeFiltersSection"
import { RecipeInfiniteScrollStatus } from "@/features/recipes/ui/RecipeInfiniteScrollStatus"
import { RecipeListSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import { useDebouncedValue } from "@/shared/lib/use-debounced-value"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"
import { ScrollToTopButton } from "@/shared/ui/scroll-to-top-button"

export default function RecipesPage() {
  const {
    scope,
    search,
    category,
    selectedTags,
    favoritesOnly,
    setScope,
    setSearch,
    setCategory,
    setTags,
    setFavoritesOnly,
    resetActiveFilters,
  } = useRecipeListFilterState("menumate:recipes:filters:v1")
  const debouncedSearch = useDebouncedValue(search, 350)
  const recipesQuery = useInfiniteRecipesQuery({
    scope,
    search: debouncedSearch,
    category,
    tagIds: selectedTags.map((tag) => tag.id),
    favoritesOnly,
  })
  const favoriteMutation = useSetRecipeFavoriteMutation()
  const recipes = recipesQuery.data?.pages.flat() ?? []
  const hasActiveFilters = Boolean(
    search.trim() || category || selectedTags.length > 0 || favoritesOnly,
  )
  const isSearchPending = search.trim() !== debouncedSearch.trim()

  return (
    <div className="space-y-5">
      <div className="hidden justify-end sm:flex">
        <RecipeCreateMenu />
      </div>

      <RecipeFiltersSection
        scope={scope}
        search={search}
        category={category}
        selectedTags={selectedTags}
        favoritesOnly={favoritesOnly}
        recipesCount={recipesQuery.data ? recipes.length : undefined}
        hasMoreRecipes={recipesQuery.hasNextPage}
        isSearchPending={isSearchPending}
        onScopeChange={setScope}
        onSearchChange={setSearch}
        onCategoryChange={setCategory}
        onTagsChange={setTags}
        onFavoritesOnlyChange={setFavoritesOnly}
        onReset={resetActiveFilters}
      />

      {recipesQuery.error ? <ErrorAlert error={recipesQuery.error} /> : null}
      {favoriteMutation.error ? <ErrorAlert error={favoriteMutation.error} /> : null}

      {recipesQuery.isPending ? (
        <RecipeListSkeleton />
      ) : recipes.length > 0 ? (
        <>
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
          <RecipeInfiniteScrollStatus
            hasNextPage={recipesQuery.hasNextPage}
            isFetchingNextPage={recipesQuery.isFetchingNextPage}
            hasError={recipesQuery.isFetchNextPageError}
            loadedCount={recipes.length}
            onLoadMore={recipesQuery.fetchNextPage}
          />
        </>
      ) : hasActiveFilters ? (
        <EmptyState
          icon={Search}
          title="Ничего не найдено"
          description="Попробуйте изменить запрос или сбросить фильтры этой вкладки."
          action={
            <Button type="button" variant="outline" onClick={resetActiveFilters}>
              Сбросить фильтры
            </Button>
          }
        />
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
      <ScrollToTopButton className="fixed right-4 bottom-[10rem] z-30 rounded-full shadow-lg sm:right-6 sm:bottom-6" />
    </div>
  )
}

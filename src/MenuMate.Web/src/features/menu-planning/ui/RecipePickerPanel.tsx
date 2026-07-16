import { ArrowLeft, ImageIcon, Plus } from "lucide-react"
import { useState } from "react"

import type { PlacementRecipe } from "@/features/menu-planning/model/menu-calendar"
import { RecipePickerListSkeleton } from "@/features/menu-planning/ui/MenuSkeletons"
import { useInfiniteRecipesQuery } from "@/features/recipes/api/recipes.queries"
import { getRecipeCategoryLabel } from "@/features/recipes/model/recipe-form-options"
import { useRecipeListFilterState } from "@/features/recipes/model/recipe-list-filter-state"
import { RecipeFiltersSection } from "@/features/recipes/ui/RecipeFiltersSection"
import { RecipeInfiniteScrollStatus } from "@/features/recipes/ui/RecipeInfiniteScrollStatus"
import { useDebouncedValue } from "@/shared/lib/use-debounced-value"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Input } from "@/shared/ui/input"

interface RecipePickerPanelProps {
  onSelect: (recipe: PlacementRecipe) => void
  onAddText: (text: string) => void
  onBack: () => void
}

export function RecipePickerPanel({ onSelect, onAddText, onBack }: RecipePickerPanelProps) {
  const {
    scope,
    search,
    category,
    favoritesOnly,
    setScope,
    setSearch,
    setCategory,
    setFavoritesOnly,
    resetActiveFilters,
  } = useRecipeListFilterState("menumate:menu:recipe-picker-filters:v1")
  const debouncedSearch = useDebouncedValue(search, 350)
  const [text, setText] = useState("")
  const recipesQuery = useInfiniteRecipesQuery({
    scope,
    search: debouncedSearch,
    category,
    favoritesOnly,
  })
  const recipes = recipesQuery.data?.pages.flat() ?? []

  return (
    <section className="mx-auto max-w-3xl space-y-4">
      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Назад"
          title="Назад"
          onClick={onBack}
        >
          <ArrowLeft />
        </Button>
        <div>
          <h2 className="type-section-title">Выбрать блюдо</h2>
        </div>
      </div>

      <RecipeFiltersSection
        scope={scope}
        search={search}
        category={category}
        favoritesOnly={favoritesOnly}
        recipesCount={recipesQuery.data ? recipes.length : undefined}
        hasMoreRecipes={recipesQuery.hasNextPage}
        isSearchPending={search.trim() !== debouncedSearch.trim()}
        onScopeChange={setScope}
        onSearchChange={setSearch}
        onCategoryChange={setCategory}
        onFavoritesOnlyChange={setFavoritesOnly}
        onReset={resetActiveFilters}
      />

      <form
        className="bg-card flex gap-2 rounded-xl border p-3 shadow-sm"
        onSubmit={(event) => {
          event.preventDefault()
          const normalized = text.trim()
          if (normalized) {
            onAddText(normalized)
          }
        }}
      >
        <Input
          value={text}
          placeholder="Или добавить блюдо текстом"
          onChange={(event) => {
            setText(event.target.value)
          }}
        />
        <Button type="submit" size="icon" aria-label="Добавить текстом" disabled={!text.trim()}>
          <Plus />
        </Button>
      </form>

      {recipesQuery.error ? <ErrorAlert error={recipesQuery.error} /> : null}
      {recipesQuery.isPending ? <RecipePickerListSkeleton /> : null}

      <div className="space-y-2">
        {recipes.map((recipe) => (
          <button
            key={recipe.id}
            type="button"
            className="bg-card hover:border-primary/40 focus-visible:ring-ring grid w-full grid-cols-[4rem_minmax(0,1fr)] items-center gap-3 rounded-xl border p-2 text-left shadow-sm transition outline-none focus-visible:ring-2"
            onClick={() => {
              onSelect({
                id: recipe.id,
                currentRevisionId: recipe.currentRevisionId,
                title: recipe.title,
                servings: Number(recipe.servings),
              })
            }}
          >
            <span className="bg-muted flex size-16 overflow-hidden rounded-lg">
              {recipe.coverImage?.readUrl ? (
                <img
                  src={recipe.coverImage.readUrl}
                  alt={recipe.coverImage.altText ?? recipe.title}
                  className="size-full object-cover"
                />
              ) : (
                <ImageIcon className="text-muted-foreground m-auto size-5" />
              )}
            </span>
            <span className="min-w-0">
              <span className="type-label block truncate">{recipe.title}</span>
              <span className="type-supporting text-muted-foreground block truncate">
                {getRecipeCategoryLabel(recipe.category)} · {String(recipe.servings)} порции
              </span>
            </span>
          </button>
        ))}
      </div>

      {!recipesQuery.isPending && recipes.length === 0 ? (
        <p className="text-muted-foreground py-6 text-center text-sm">
          Подходящих рецептов не найдено.
        </p>
      ) : null}

      <RecipeInfiniteScrollStatus
        hasNextPage={recipesQuery.hasNextPage}
        isFetchingNextPage={recipesQuery.isFetchingNextPage}
        hasError={recipesQuery.isFetchNextPageError}
        loadedCount={recipes.length}
        onLoadMore={recipesQuery.fetchNextPage}
      />
    </section>
  )
}

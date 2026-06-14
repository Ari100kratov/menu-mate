import { ArrowLeft, ImageIcon, Plus } from "lucide-react"
import { useMemo, useState } from "react"

import type { PlacementRecipe } from "@/features/menu-planning/model/menu-calendar"
import { useRecipesQuery } from "@/features/recipes/api/recipes.queries"
import { getRecipeCategoryLabel } from "@/features/recipes/model/recipe-form-options"
import { RecipeFiltersSection } from "@/features/recipes/ui/RecipeFiltersSection"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { Input } from "@/shared/ui/input"

interface RecipePickerPanelProps {
  onSelect: (recipe: PlacementRecipe) => void
  onAddText: (text: string) => void
  onBack: () => void
}

export function RecipePickerPanel({ onSelect, onAddText, onBack }: RecipePickerPanelProps) {
  const [scope, setScope] = useState<"library" | "catalog">("library")
  const [search, setSearch] = useState("")
  const [category, setCategory] = useState("")
  const [favoritesOnly, setFavoritesOnly] = useState(false)
  const [text, setText] = useState("")
  const recipesQuery = useRecipesQuery({ scope, search, favoritesOnly })
  const recipes = useMemo(
    () =>
      category
        ? (recipesQuery.data ?? []).filter((recipe) => recipe.category === category)
        : (recipesQuery.data ?? []),
    [category, recipesQuery.data],
  )

  return (
    <section className="mx-auto max-w-3xl space-y-4">
      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Назад к меню"
          onClick={onBack}
        >
          <ArrowLeft />
        </Button>
        <div>
          <h2 className="type-section-title">Выбрать блюдо</h2>
          <p className="type-supporting text-muted-foreground">
            Поиск начинается без автоматических фильтров.
          </p>
        </div>
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
        onReset={() => {
          setSearch("")
          setCategory("")
          setFavoritesOnly(false)
        }}
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
      {recipesQuery.isPending ? <PageSkeleton /> : null}

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
    </section>
  )
}

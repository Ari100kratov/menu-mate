import {
  ArrowUpDown,
  Check,
  Compass,
  Library,
  LoaderCircle,
  Search,
  SlidersHorizontal,
  X,
} from "lucide-react"
import { useEffect, useRef, useState, type WheelEvent } from "react"

import { recipeCategoryOptions } from "@/features/recipes/model/recipe-form-options"
import type {
  RecipeListSort,
  RecipeListTagFilter,
  RecipeOwnershipFilter,
} from "@/features/recipes/model/recipe-list-filter-state"
import { RecipeAdvancedFiltersDialog } from "@/features/recipes/ui/RecipeAdvancedFiltersDialog"
import { RecipeTagFilter } from "@/features/recipes/ui/RecipeTagFilter"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from "@/shared/ui/dropdown-menu"
import { Input } from "@/shared/ui/input"

interface RecipeFiltersSectionProps {
  scope: "library" | "catalog"
  search: string
  category: string
  selectedTags: RecipeListTagFilter[]
  favoritesOnly: boolean
  sort: RecipeListSort
  ownership: RecipeOwnershipFilter
  showTagsInMain: boolean
  recipesCount: number | undefined
  totalCount: number | undefined
  isSearchPending: boolean
  onScopeChange: (value: "library" | "catalog") => void
  onSearchChange: (value: string) => void
  onCategoryChange: (value: string) => void
  onTagsChange: (tags: RecipeListTagFilter[]) => void
  onFavoritesOnlyChange: (value: boolean) => void
  onSortChange: (value: RecipeListSort) => void
  onOwnershipChange: (value: RecipeOwnershipFilter) => void
  onShowTagsInMainChange: (value: boolean) => void
  onReset: () => void
}

export function RecipeFiltersSection({
  scope,
  search,
  category,
  selectedTags,
  favoritesOnly,
  sort,
  ownership,
  showTagsInMain,
  recipesCount,
  totalCount,
  isSearchPending,
  onScopeChange,
  onSearchChange,
  onCategoryChange,
  onTagsChange,
  onFavoritesOnlyChange,
  onSortChange,
  onOwnershipChange,
  onShowTagsInMainChange,
  onReset,
}: RecipeFiltersSectionProps) {
  const [advancedFiltersOpen, setAdvancedFiltersOpen] = useState(false)
  const hasActiveFilters = Boolean(
    search.trim() ||
    category ||
    selectedTags.length > 0 ||
    favoritesOnly ||
    ownership !== "all" ||
    sort !== "alphabetical",
  )
  const hasActiveAdvancedFilters =
    favoritesOnly || ownership !== "all" || (selectedTags.length > 0 && !showTagsInMain)
  const categoriesRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const container = categoriesRef.current
    const activeChip = container?.querySelector<HTMLElement>("[data-active='true']")
    if (!container || !activeChip) {
      return
    }

    const containerBounds = container.getBoundingClientRect()
    const activeChipBounds = activeChip.getBoundingClientRect()
    const centeredScrollLeft =
      container.scrollLeft +
      activeChipBounds.left -
      containerBounds.left -
      (container.clientWidth - activeChip.clientWidth) / 2
    container.scrollTo({ left: Math.max(0, centeredScrollLeft), behavior: "auto" })
  }, [category, scope])

  return (
    <section className="space-y-3">
      <div
        className="bg-card grid grid-cols-2 gap-1 rounded-xl border p-1 shadow-sm"
        role="tablist"
        aria-label="Раздел рецептов"
      >
        <Button
          type="button"
          variant={scope === "library" ? "default" : "ghost"}
          className="rounded-lg px-3"
          role="tab"
          aria-selected={scope === "library"}
          onClick={() => {
            onScopeChange("library")
          }}
        >
          <Library className="size-4" />
          <span className="truncate">Моя библиотека</span>
          {scope === "library" ? <Check className="ml-auto size-4" /> : null}
        </Button>
        <Button
          type="button"
          variant={scope === "catalog" ? "default" : "ghost"}
          className="rounded-lg px-3"
          role="tab"
          aria-selected={scope === "catalog"}
          onClick={() => {
            onScopeChange("catalog")
          }}
        >
          <Compass className="size-4" />
          <span className="truncate">Каталог</span>
          {scope === "catalog" ? <Check className="ml-auto size-4" /> : null}
        </Button>
      </div>

      <div className="grid grid-cols-[minmax(0,1fr)_auto] gap-2">
        <div className="relative">
          {isSearchPending ? (
            <LoaderCircle className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 animate-spin" />
          ) : (
            <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
          )}
          <Input
            type="search"
            className={cn(
              "bg-card h-11 rounded-xl pl-9 [&::-webkit-search-cancel-button]:appearance-none",
              search ? "pr-10" : "pr-3",
            )}
            value={search}
            placeholder="Название или описание"
            aria-label="Поиск по названию и описанию рецепта"
            aria-busy={isSearchPending}
            autoComplete="off"
            onChange={(event) => {
              onSearchChange(event.target.value)
            }}
          />
          {search ? (
            <Button
              type="button"
              variant="ghost"
              size="icon-sm"
              className="absolute top-1/2 right-1 -translate-y-1/2 rounded-lg"
              aria-label="Очистить поиск"
              title="Очистить поиск"
              onClick={() => {
                onSearchChange("")
              }}
            >
              <X />
            </Button>
          ) : null}
        </div>

        <Button
          type="button"
          variant="outline"
          size="icon-lg"
          className="relative size-11 rounded-xl"
          aria-label="Расширенные фильтры"
          title="Расширенные фильтры"
          onClick={() => {
            setAdvancedFiltersOpen(true)
          }}
        >
          <SlidersHorizontal />
          {hasActiveAdvancedFilters ? (
            <span
              className="bg-destructive absolute top-1.5 right-1.5 size-2 rounded-full"
              aria-hidden="true"
            />
          ) : null}
        </Button>
      </div>

      {showTagsInMain ? (
        <div className="min-w-0">
          <RecipeTagFilter selectedTags={selectedTags} onChange={onTagsChange} />
        </div>
      ) : null}

      <div
        ref={categoriesRef}
        className="-mx-4 flex [scrollbar-width:none] gap-2 overflow-x-auto px-4 pb-1 md:mx-0 md:px-0 [&::-webkit-scrollbar]:hidden"
        aria-label="Категории рецептов"
        onWheel={scrollCategoriesWithMouseWheel}
      >
        <FilterChip
          active={!category}
          label="Все"
          onClick={() => {
            onCategoryChange("")
          }}
        />
        {recipeCategoryOptions.map((option) => (
          <FilterChip
            key={option.value}
            active={category === option.value}
            label={option.label}
            onClick={() => {
              onCategoryChange(option.value)
            }}
          />
        ))}
        {hasActiveFilters ? (
          <Button type="button" variant="ghost" size="sm" onClick={onReset}>
            <X />
            Сбросить все
          </Button>
        ) : null}
      </div>

      <div className="flex min-h-8 min-w-0 items-center justify-between gap-2">
        {recipesCount === undefined ? (
          <span />
        ) : (
          <p className="type-supporting text-muted-foreground whitespace-nowrap" aria-live="polite">
            {getResultsLabel(recipesCount, totalCount)}
          </p>
        )}
        <SortMenu sort={sort} onSortChange={onSortChange} />
      </div>

      <RecipeAdvancedFiltersDialog
        key={String(advancedFiltersOpen)}
        open={advancedFiltersOpen}
        favoritesOnly={favoritesOnly}
        ownership={ownership}
        selectedTags={selectedTags}
        showTagsInMain={showTagsInMain}
        onOpenChange={setAdvancedFiltersOpen}
        onApply={(filters) => {
          onFavoritesOnlyChange(filters.favoritesOnly)
          onOwnershipChange(filters.ownership)
          onTagsChange(filters.tags)
          onShowTagsInMainChange(filters.showTagsInMain)
        }}
      />
    </section>
  )
}

function SortMenu({
  sort,
  onSortChange,
}: {
  sort: RecipeListSort
  onSortChange: (sort: RecipeListSort) => void
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="text-muted-foreground hover:text-foreground h-auto max-w-40 justify-end gap-1 rounded-lg px-1.5 py-1"
          aria-label={`Сортировка: ${getSortLabel(sort)}`}
          title={`Сортировка: ${getSortLabel(sort)}`}
        >
          <ArrowUpDown className="size-3.5" />
          <span className="truncate">{getSortLabel(sort)}</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuRadioGroup
          value={sort}
          onValueChange={(value) => {
            onSortChange(value as RecipeListSort)
          }}
        >
          <DropdownMenuRadioItem value="alphabetical">По алфавиту</DropdownMenuRadioItem>
          <DropdownMenuRadioItem value="newest">Сначала новые</DropdownMenuRadioItem>
          <DropdownMenuRadioItem value="popular">По популярности</DropdownMenuRadioItem>
        </DropdownMenuRadioGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}

function getSortLabel(sort: RecipeListSort) {
  switch (sort) {
    case "newest":
      return "Сначала новые"
    case "popular":
      return "По популярности"
    default:
      return "По алфавиту"
  }
}

function getResultsLabel(loadedCount: number, totalCount: number | undefined) {
  if (totalCount === undefined || loadedCount >= totalCount) {
    return `Найдено: ${String(totalCount ?? loadedCount)}`
  }

  return `Показано: ${String(loadedCount)} из ${String(totalCount)}`
}

function scrollCategoriesWithMouseWheel(event: WheelEvent<HTMLDivElement>) {
  const container = event.currentTarget
  const maxScrollLeft = container.scrollWidth - container.clientWidth
  const isHorizontalGesture = Math.abs(event.deltaX) >= Math.abs(event.deltaY)
  const delta = isHorizontalGesture ? event.deltaX : event.deltaY
  const canScroll =
    (delta < 0 && container.scrollLeft > 0) || (delta > 0 && container.scrollLeft < maxScrollLeft)

  if (!canScroll) {
    return
  }

  event.preventDefault()
  container.scrollLeft += delta
}

function FilterChip({
  active,
  label,
  onClick,
}: {
  active: boolean
  label: string
  onClick: () => void
}) {
  return (
    <Button
      type="button"
      variant={active ? "default" : "outline"}
      size="sm"
      className="shrink-0 rounded-full"
      data-active={active}
      aria-pressed={active}
      onClick={onClick}
    >
      {label}
    </Button>
  )
}

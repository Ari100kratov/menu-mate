import { Check, Compass, Heart, Library, Search, X } from "lucide-react"
import type { WheelEvent } from "react"

import { recipeCategoryOptions } from "@/features/recipes/model/recipe-form-options"
import { Button } from "@/shared/ui/button"
import { Input } from "@/shared/ui/input"

interface RecipeFiltersSectionProps {
  scope: "library" | "catalog"
  search: string
  category: string
  favoritesOnly: boolean
  recipesCount: number | undefined
  onScopeChange: (value: "library" | "catalog") => void
  onSearchChange: (value: string) => void
  onCategoryChange: (value: string) => void
  onFavoritesOnlyChange: (value: boolean) => void
  onReset: () => void
}

export function RecipeFiltersSection({
  scope,
  search,
  category,
  favoritesOnly,
  recipesCount,
  onScopeChange,
  onSearchChange,
  onCategoryChange,
  onFavoritesOnlyChange,
  onReset,
}: RecipeFiltersSectionProps) {
  const hasActiveFilters = Boolean(search.trim() || category || favoritesOnly)

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
          <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
          <Input
            className="bg-card h-11 rounded-xl pl-9"
            value={search}
            placeholder="Найти рецепт"
            onChange={(event) => {
              onSearchChange(event.target.value)
            }}
          />
        </div>
        <Button
          type="button"
          variant={favoritesOnly ? "default" : "outline"}
          size="icon-lg"
          className="size-11 rounded-xl"
          aria-label={favoritesOnly ? "Показать все рецепты" : "Показать только избранные"}
          aria-pressed={favoritesOnly}
          title={favoritesOnly ? "Показаны избранные" : "Только избранные"}
          onClick={() => {
            onFavoritesOnlyChange(!favoritesOnly)
          }}
        >
          <Heart className={favoritesOnly ? "fill-current" : undefined} />
        </Button>
      </div>

      <div
        className="-mx-4 flex gap-2 overflow-x-auto px-4 pb-1 [scrollbar-width:none] md:mx-0 md:px-0 [&::-webkit-scrollbar]:hidden"
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
            Сбросить
          </Button>
        ) : null}
      </div>

      {recipesCount === undefined ? null : (
        <p className="type-supporting text-muted-foreground">Найдено: {recipesCount}</p>
      )}
    </section>
  )
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
  icon,
  onClick,
}: {
  active: boolean
  label: string
  icon?: React.ReactNode
  onClick: () => void
}) {
  return (
    <Button
      type="button"
      variant={active ? "default" : "outline"}
      size="sm"
      className="shrink-0 rounded-full"
      aria-pressed={active}
      onClick={onClick}
    >
      {icon}
      {label}
    </Button>
  )
}

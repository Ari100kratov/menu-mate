import { Search, Star, X } from "lucide-react"

import { Button } from "@/shared/ui/button"
import { Input } from "@/shared/ui/input"
import { PageSection } from "@/shared/ui/page"
import { Select } from "@/shared/ui/select"

interface RecipeFiltersSectionProps {
  search: string
  tag: string
  tagOptions: readonly string[]
  favoritesOnly: boolean
  recipesCount: number | undefined
  onSearchChange: (value: string) => void
  onTagChange: (value: string) => void
  onFavoritesOnlyChange: (value: boolean) => void
  onReset: () => void
}

export function RecipeFiltersSection({
  search,
  tag,
  tagOptions,
  favoritesOnly,
  recipesCount,
  onSearchChange,
  onTagChange,
  onFavoritesOnlyChange,
  onReset,
}: RecipeFiltersSectionProps) {
  const hasActiveFilters = Boolean(search.trim() || tag || favoritesOnly)

  return (
    <PageSection
      title="Быстрый отбор"
      description="Поиск, теги и избранное работают без отдельного справочника в навигации."
      action={
        hasActiveFilters ? (
          <Button type="button" variant="ghost" onClick={onReset}>
            <X />
            Сбросить
          </Button>
        ) : null
      }
    >
      <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_minmax(12rem,18rem)_auto] md:items-end">
        <label className="space-y-2">
          <span className="text-sm font-medium">Поиск</span>
          <div className="relative">
            <Search className="text-muted-foreground absolute top-1/2 left-3 size-4 -translate-y-1/2" />
            <Input
              className="pl-9"
              value={search}
              onChange={(event) => {
                onSearchChange(event.target.value)
              }}
            />
          </div>
        </label>

        <label className="space-y-2">
          <span className="text-sm font-medium">Тег</span>
          <Select
            value={tag}
            onChange={(event) => {
              onTagChange(event.target.value)
            }}
          >
            <option value="">Все теги</option>
            {tagOptions.map((tagName) => (
              <option key={tagName} value={tagName}>
                {tagName}
              </option>
            ))}
          </Select>
        </label>

        <Button
          type="button"
          variant={favoritesOnly ? "default" : "outline"}
          aria-pressed={favoritesOnly}
          onClick={() => {
            onFavoritesOnlyChange(!favoritesOnly)
          }}
        >
          <Star className={favoritesOnly ? "fill-primary-foreground" : undefined} />
          Избранные
        </Button>
      </div>
      {recipesCount === undefined ? null : (
        <div className="text-muted-foreground text-sm">Найдено: {recipesCount}</div>
      )}
    </PageSection>
  )
}

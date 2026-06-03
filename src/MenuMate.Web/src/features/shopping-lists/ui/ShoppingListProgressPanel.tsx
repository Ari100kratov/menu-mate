import type {
  ShoppingFilterOption,
  ShoppingItemFilter,
  ShoppingListProgress,
} from "@/features/shopping-lists/model/shopping-list-state"
import { Button } from "@/shared/ui/button"
import { PageSection } from "@/shared/ui/page"

interface ShoppingListProgressPanelProps {
  progress: ShoppingListProgress
  filterOptions: readonly ShoppingFilterOption[]
  isStoreMode: boolean
  itemFilter: ShoppingItemFilter
  onFilterChange: (value: ShoppingItemFilter) => void
}

export function ShoppingListProgressPanel({
  progress,
  filterOptions,
  isStoreMode,
  itemFilter,
  onFilterChange,
}: ShoppingListProgressPanelProps) {
  return (
    <PageSection
      title={isStoreMode ? "Режим магазина" : "Прогресс покупок"}
      description={
        isStoreMode
          ? "Показываются только позиции, которые еще надо купить или проверить дома."
          : "Фильтры меняют только отображение, состояние позиций хранится на backend."
      }
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="text-muted-foreground text-sm">
          Осталось {progress.remaining} из {progress.total}
        </div>
        <div className="text-muted-foreground text-sm">{progress.percent}%</div>
      </div>
      <div className="bg-muted h-2 overflow-hidden rounded-full">
        <div
          className="bg-primary h-full rounded-full transition-all"
          style={{ width: `${String(progress.percent)}%` }}
        />
      </div>
      <div className="flex flex-wrap gap-2">
        {filterOptions.map((option) => (
          <Button
            key={option.value}
            type="button"
            size="sm"
            variant={!isStoreMode && itemFilter === option.value ? "default" : "outline"}
            aria-pressed={!isStoreMode && itemFilter === option.value}
            disabled={isStoreMode}
            onClick={() => {
              onFilterChange(option.value)
            }}
          >
            {option.label}
            <span className="text-xs opacity-80">{option.count}</span>
          </Button>
        ))}
      </div>
    </PageSection>
  )
}

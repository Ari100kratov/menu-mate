import { LoaderCircle, Plus, Settings2, Utensils } from "lucide-react"

import type {
  MealSlot,
  MenuCalendarItem,
  UpdateMenuCalendarItemRequest,
} from "@/features/menu-planning/api/menu-calendar.api"
import {
  formatDayHeading,
  getRangeDates,
  type MenuDateRange,
} from "@/features/menu-planning/model/menu-calendar"
import { Button } from "@/shared/ui/button"
import { EmptyState } from "@/shared/ui/page"
import { MenuItemRow } from "./MenuItemRow"

interface MenuCalendarViewProps {
  range: MenuDateRange
  mealSlots: readonly MealSlot[]
  items: readonly MenuCalendarItem[]
  isPlacementMode: boolean
  isPending: boolean
  isItemsLoading: boolean
  onEditMealSlots: () => void
  onAdd: (date: string, mealSlotId: string) => void
  onUpdate: (itemId: string, request: UpdateMenuCalendarItemRequest) => void
  onRemove: (itemId: string) => void
}

export function MenuCalendarView({
  range,
  mealSlots,
  items,
  isPlacementMode,
  isPending,
  isItemsLoading,
  onEditMealSlots,
  onAdd,
  onUpdate,
  onRemove,
}: MenuCalendarViewProps) {
  if (mealSlots.length === 0) {
    return (
      <EmptyState
        icon={Utensils}
        title="Добавьте приемы пищи"
        description="Настройте хотя бы один прием пищи, чтобы добавлять блюда в календарь."
        action={
          <Button type="button" variant="outline" onClick={onEditMealSlots}>
            <Settings2 />
            Настроить приемы пищи
          </Button>
        }
      />
    )
  }

  return (
    <section className="space-y-3">
      {isItemsLoading ? (
        <div className="type-supporting text-muted-foreground flex items-center justify-end gap-2 px-1">
          <LoaderCircle className="size-4 animate-spin" />
          Обновляем блюда
        </div>
      ) : null}
      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        {getRangeDates(range).map((date) => (
          <article
            key={date}
            className="bg-secondary/35 border-primary/10 min-w-0 space-y-3 rounded-2xl border p-3"
          >
            <h2 className="type-subsection-title text-secondary-foreground px-1">
              {formatDayHeading(date)}
            </h2>
            <div className="space-y-2">
              {mealSlots.map((mealSlot) => {
                const slotItems = items.filter(
                  (item) => item.date === date && item.mealSlotId === mealSlot.id,
                )

                return (
                  <section
                    key={mealSlot.id}
                    className="bg-card/90 border-primary/10 space-y-2 rounded-xl border p-3 shadow-sm"
                  >
                    <header className="flex items-center justify-between gap-2">
                      <h3 className="type-label">{mealSlot.name}</h3>
                      <Button
                        type="button"
                        variant={isPlacementMode ? "secondary" : "ghost"}
                        size={isPlacementMode ? "sm" : "icon-sm"}
                        className={
                          isPlacementMode
                            ? undefined
                            : "border-border bg-background hover:bg-accent sm:w-auto sm:border sm:px-2.5 sm:shadow-xs"
                        }
                        aria-label={
                          isPlacementMode ? undefined : `Добавить блюдо в ${mealSlot.name}`
                        }
                        title={isPlacementMode ? undefined : `Добавить блюдо в ${mealSlot.name}`}
                        disabled={isPending}
                        onClick={() => {
                          onAdd(date, mealSlot.id)
                        }}
                      >
                        <Plus />
                        {isPlacementMode ? (
                          "Добавить сюда"
                        ) : (
                          <span className="hidden sm:inline">Добавить</span>
                        )}
                      </Button>
                    </header>
                    {slotItems.length > 0 ? (
                      <div className="space-y-2">
                        {slotItems.map((item) => (
                          <MenuItemRow
                            key={item.id}
                            item={item}
                            mealSlots={mealSlots}
                            isPending={isPending}
                            onUpdate={(request) => {
                              onUpdate(item.id, request)
                            }}
                            onRemove={() => {
                              onRemove(item.id)
                            }}
                          />
                        ))}
                      </div>
                    ) : (
                      <p className="type-supporting text-muted-foreground px-1 py-1">Пока пусто</p>
                    )}
                  </section>
                )
              })}
            </div>
          </article>
        ))}
      </div>
    </section>
  )
}

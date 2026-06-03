import { useMemo, useState } from "react"

import type { MenuPlan, MenuPlanItem } from "@/features/menu-planning/api/menu-plans.api"
import {
  formatDate,
  formatWeekday,
  getDateRangeInputValues,
  mealTypeOptions,
} from "@/features/menu-planning/model/menu-planning"
import type { MenuPlanItemFormValues } from "@/features/menu-planning/model/menu-plan-form"
import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { cn } from "@/shared/lib/utils"
import { MenuPlanItemCard } from "./MenuPlanItemCard"

interface MenuPlanWeekGridProps {
  menuPlan: MenuPlan
  recipes: readonly RecipeListItem[]
  editingItemId: string | null
  isMutationPending: boolean
  onEdit: (itemId: string) => void
  onCancelEdit: () => void
  onRemove: (itemId: string) => void
  onQuickAdd: (date: string, mealType: string) => void
  onCopy: (item: MenuPlanItem) => void
  onMove: (item: MenuPlanItem, date: string) => void
  onUpdate: (itemId: string, values: MenuPlanItemFormValues) => void
}

export function MenuPlanWeekGrid({
  menuPlan,
  recipes,
  editingItemId,
  isMutationPending,
  onEdit,
  onCancelEdit,
  onRemove,
  onQuickAdd,
  onCopy,
  onMove,
  onUpdate,
}: MenuPlanWeekGridProps) {
  const days = useMemo(
    () => getDateRangeInputValues(menuPlan.startDate, menuPlan.endDate),
    [menuPlan.endDate, menuPlan.startDate],
  )
  const [requestedActiveDate, setRequestedActiveDate] = useState(days[0] ?? menuPlan.startDate)
  const activeDate = days.includes(requestedActiveDate)
    ? requestedActiveDate
    : (days[0] ?? menuPlan.startDate)

  return (
    <div className="space-y-3">
      <div className="-mx-1 overflow-x-auto pb-1 md:hidden">
        <div className="flex gap-2 px-1">
          {days.map((date) => {
            const itemsCount = menuPlan.items.filter((item) => item.date === date).length

            return (
              <button
                key={date}
                type="button"
                className={cn(
                  "min-w-28 rounded-md border px-3 py-2 text-left text-sm",
                  date === activeDate ? "bg-primary text-primary-foreground" : "bg-card",
                )}
                onClick={() => {
                  setRequestedActiveDate(date)
                }}
              >
                <div className="font-medium">{formatWeekday(date)}</div>
                <div className="text-xs opacity-80">{formatDate(date)}</div>
                <div className="text-xs opacity-80">{itemsCount} поз.</div>
              </button>
            )
          })}
        </div>
      </div>

      <section className="grid gap-3 md:grid-cols-2 xl:grid-cols-7">
        {days.map((date) => (
          <article
            key={date}
            className={cn(
              "space-y-3 rounded-md border p-3",
              date !== activeDate && "hidden md:block",
            )}
          >
            <header>
              <div className="text-sm font-semibold uppercase">{formatWeekday(date)}</div>
              <div className="text-muted-foreground text-sm">{formatDate(date)}</div>
            </header>

            <div className="space-y-3">
              {mealTypeOptions.map((mealType) => {
                const items = menuPlan.items.filter(
                  (item) => item.date === date && item.mealType === mealType.value,
                )

                return (
                  <section key={mealType.value} className="space-y-2">
                    <div className="bg-muted/60 flex items-center justify-between gap-2 rounded-md px-2 py-1 text-xs font-medium">
                      <span>{mealType.label}</span>
                      <button
                        type="button"
                        className="text-primary hover:underline"
                        onClick={() => {
                          onQuickAdd(date, mealType.value)
                        }}
                      >
                        Добавить
                      </button>
                    </div>

                    {items.length > 0 ? (
                      <div className="space-y-2">
                        {items.map((item) => (
                          <MenuPlanItemCard
                            key={item.id}
                            item={item}
                            menuPlan={menuPlan}
                            recipes={recipes}
                            isEditing={editingItemId === item.id}
                            isPending={isMutationPending}
                            onEdit={() => {
                              onEdit(item.id)
                            }}
                            onCancelEdit={onCancelEdit}
                            onRemove={() => {
                              onRemove(item.id)
                            }}
                            availableDates={days}
                            onCopy={() => {
                              onCopy(item)
                            }}
                            onMove={(targetDate) => {
                              onMove(item, targetDate)
                            }}
                            onUpdate={(values) => {
                              onUpdate(item.id, values)
                            }}
                          />
                        ))}
                      </div>
                    ) : (
                      <div className="text-muted-foreground px-1 text-xs">Пусто</div>
                    )}
                  </section>
                )
              })}
            </div>
          </article>
        ))}
      </section>
    </div>
  )
}

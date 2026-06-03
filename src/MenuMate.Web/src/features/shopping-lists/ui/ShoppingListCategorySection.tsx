import { CheckCheck, ChevronDown, ChevronRight, Home, RotateCcw } from "lucide-react"

import {
  type ShoppingItemFormValues,
  toShoppingListItemFormValues,
} from "@/features/shopping-lists/model/shopping-list-ui"
import type {
  ShoppingListCategory,
  ShoppingListItemState,
} from "@/features/shopping-lists/model/shopping-list-state"
import { Button } from "@/shared/ui/button"
import { ShoppingListItemForm } from "./ShoppingListItemForm"
import { ShoppingListItemRow } from "./ShoppingListItemRow"

interface ShoppingListCategorySectionProps {
  category: ShoppingListCategory
  isCollapsed: boolean
  isPending: boolean
  isStoreMode: boolean
  editingItemId: string | null
  onToggle: () => void
  onMarkPurchased: () => void
  onMarkInStock: () => void
  onReset: () => void
  onEdit: (itemId: string) => void
  onCancelEdit: () => void
  onUpdate: (itemId: string, values: ShoppingItemFormValues) => void
  onStateChange: (itemId: string, nextState: ShoppingListItemState) => void
  onRemove: (itemId: string) => void
}

export function ShoppingListCategorySection({
  category,
  isCollapsed,
  isPending,
  isStoreMode,
  editingItemId,
  onToggle,
  onMarkPurchased,
  onMarkInStock,
  onReset,
  onEdit,
  onCancelEdit,
  onUpdate,
  onStateChange,
  onRemove,
}: ShoppingListCategorySectionProps) {
  const doneCount = category.items.filter((item) => item.isPurchased || item.isInStock).length

  return (
    <section className="space-y-3 rounded-md border p-3">
      <button
        type="button"
        className="focus-visible:ring-ring flex w-full items-center gap-2 rounded-md text-left outline-none focus-visible:ring-2"
        aria-expanded={!isCollapsed}
        onClick={onToggle}
      >
        {isCollapsed ? <ChevronRight className="size-4" /> : <ChevronDown className="size-4" />}
        <span className="min-w-0 flex-1 font-semibold tracking-normal">{category.name}</span>
        <span className="text-muted-foreground text-sm">
          {doneCount} / {category.items.length}
        </span>
      </button>

      {isCollapsed ? null : (
        <>
          <div className="flex flex-wrap gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={isPending}
              onClick={onMarkPurchased}
            >
              <CheckCheck />
              Куплено
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={isPending}
              onClick={onMarkInStock}
            >
              <Home />
              Есть дома
            </Button>
            <Button type="button" variant="ghost" size="sm" disabled={isPending} onClick={onReset}>
              <RotateCcw />
              Сбросить
            </Button>
          </div>

          <div className="space-y-2">
            {category.items.map((item) =>
              editingItemId === item.id ? (
                <ShoppingListItemForm
                  key={item.id}
                  title="Редактировать позицию"
                  submitLabel="Сохранить"
                  submitIcon="save"
                  isSubmitting={isPending}
                  initialValues={toShoppingListItemFormValues(item)}
                  onSubmit={(values) => {
                    onUpdate(item.id, values)
                  }}
                  onCancel={onCancelEdit}
                />
              ) : (
                <ShoppingListItemRow
                  key={item.id}
                  item={item}
                  isPending={isPending}
                  isStoreMode={isStoreMode}
                  onEdit={() => {
                    onEdit(item.id)
                  }}
                  onStateChange={(nextState) => {
                    onStateChange(item.id, nextState)
                  }}
                  onRemove={() => {
                    onRemove(item.id)
                  }}
                />
              ),
            )}
          </div>
        </>
      )}
    </section>
  )
}

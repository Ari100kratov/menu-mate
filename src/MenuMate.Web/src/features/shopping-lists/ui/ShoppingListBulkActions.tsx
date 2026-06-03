import { CheckCheck, Home, RotateCcw } from "lucide-react"

import { Button } from "@/shared/ui/button"

interface ShoppingListBulkActionsProps {
  itemCount: number
  doneCount: number
  isPending: boolean
  isStoreMode: boolean
  onMarkPurchased: () => void
  onMarkInStock: () => void
  onReset: () => void
}

export function ShoppingListBulkActions({
  itemCount,
  doneCount,
  isPending,
  isStoreMode,
  onMarkPurchased,
  onMarkInStock,
  onReset,
}: ShoppingListBulkActionsProps) {
  if (itemCount === 0) {
    return null
  }

  return (
    <section className="bg-muted/30 flex flex-col gap-3 rounded-md border p-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="space-y-1">
        <h3 className="text-sm font-semibold tracking-normal">
          {isStoreMode ? "Быстро закрыть поход" : "Массовые действия"}
        </h3>
        <p className="text-muted-foreground text-sm">
          {doneCount} / {itemCount} позиций в текущей выборке.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-2 sm:flex sm:flex-wrap sm:justify-end">
        <Button type="button" size="sm" disabled={isPending} onClick={onMarkPurchased}>
          <CheckCheck />
          Куплено
        </Button>
        <Button
          type="button"
          size="sm"
          variant="outline"
          disabled={isPending}
          onClick={onMarkInStock}
        >
          <Home />
          Есть дома
        </Button>
        {isStoreMode ? null : (
          <Button
            type="button"
            size="sm"
            variant="ghost"
            className="col-span-2 sm:col-span-1"
            disabled={isPending}
            onClick={onReset}
          >
            <RotateCcw />
            Сбросить
          </Button>
        )}
      </div>
    </section>
  )
}

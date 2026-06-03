import { ClipboardCopy, ShoppingCart, Trash2, X } from "lucide-react"

import type { ShoppingListProgress } from "@/features/shopping-lists/model/shopping-list-state"
import { Button } from "@/shared/ui/button"
import { MobileStickyActions } from "@/shared/ui/page"

interface ShoppingListToolbarProps {
  progress: ShoppingListProgress
  isStoreMode: boolean
  isDeletePending: boolean
  isItemActionPending: boolean
  onStoreModeChange: (value: boolean) => void
  onMarkRemainingPurchased: () => void
  onCopy: () => void
  onDelete: () => void
}

export function ShoppingListToolbar({
  progress,
  isStoreMode,
  isDeletePending,
  isItemActionPending,
  onStoreModeChange,
  onMarkRemainingPurchased,
  onCopy,
  onDelete,
}: ShoppingListToolbarProps) {
  return (
    <>
      <div className="flex flex-col gap-3 rounded-md border p-4 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-xl font-semibold tracking-normal">Список покупок</h2>
          <p className="text-muted-foreground text-sm">
            {progress.done} / {progress.total} поз.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button
            type="button"
            variant={isStoreMode ? "default" : "outline"}
            aria-pressed={isStoreMode}
            onClick={() => {
              onStoreModeChange(!isStoreMode)
            }}
          >
            <ShoppingCart />В магазине
          </Button>
          <Button type="button" variant="outline" onClick={onCopy}>
            <ClipboardCopy />
            Копировать
          </Button>
          <Button type="button" variant="ghost" disabled={isDeletePending} onClick={onDelete}>
            <Trash2 />
            Удалить
          </Button>
        </div>
      </div>

      {isStoreMode ? (
        <MobileStickyActions>
          <Button
            type="button"
            variant="outline"
            onClick={() => {
              onStoreModeChange(false)
            }}
          >
            <X />
            Обычный режим
          </Button>
          <Button
            type="button"
            disabled={progress.remaining === 0 || isItemActionPending}
            onClick={onMarkRemainingPurchased}
          >
            <ShoppingCart />
            Куплено все
          </Button>
          <span className="text-muted-foreground flex items-center text-sm">
            Осталось {progress.remaining}
          </span>
        </MobileStickyActions>
      ) : null}
    </>
  )
}

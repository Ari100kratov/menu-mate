import { CheckCheck, Home, Pencil, Trash2 } from "lucide-react"

import type { ShoppingListItem } from "@/features/shopping-lists/api/shopping-lists.api"
import type { ShoppingListItemState } from "@/features/shopping-lists/model/shopping-list-state"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/ui/button"

interface ShoppingListItemRowProps {
  item: ShoppingListItem
  isPending: boolean
  isStoreMode: boolean
  onEdit: () => void
  onStateChange: (nextState: ShoppingListItemState) => void
  onRemove: () => void
}

export function ShoppingListItemRow({
  item,
  isPending,
  isStoreMode,
  onEdit,
  onStateChange,
  onRemove,
}: ShoppingListItemRowProps) {
  const isDone = item.isPurchased || item.isInStock

  return (
    <article
      className={cn("bg-card text-card-foreground rounded-md border p-3", isDone && "bg-muted/30")}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h4
            className={cn(
              "font-medium tracking-normal",
              isDone && "text-muted-foreground line-through",
            )}
          >
            {item.name}
          </h4>
          <p className="text-muted-foreground text-sm">
            {item.amountText}
            {item.comment ? `, ${item.comment}` : ""}
          </p>
        </div>
        <div className="flex shrink-0">
          {isStoreMode ? null : (
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label="Редактировать позицию"
              disabled={isPending}
              onClick={onEdit}
            >
              <Pencil />
            </Button>
          )}
          <Button
            type="button"
            variant="ghost"
            size="icon"
            aria-label="Удалить позицию"
            disabled={isPending}
            onClick={onRemove}
          >
            <Trash2 />
          </Button>
        </div>
      </div>

      {isStoreMode ? (
        <div className="mt-3 grid grid-cols-2 gap-2">
          <Button
            type="button"
            variant="outline"
            disabled={isPending}
            onClick={() => {
              onStateChange({ isPurchased: true, isInStock: false })
            }}
          >
            <CheckCheck />
            Куплено
          </Button>
          <Button
            type="button"
            variant="outline"
            disabled={isPending}
            onClick={() => {
              onStateChange({ isPurchased: false, isInStock: true })
            }}
          >
            <Home />
            Есть дома
          </Button>
        </div>
      ) : (
        <div className="mt-3 flex flex-wrap gap-4 text-sm">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              className="border-input text-primary focus-visible:ring-ring size-4 rounded border focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
              checked={item.isPurchased}
              disabled={isPending}
              onChange={(event) => {
                onStateChange({
                  isPurchased: event.target.checked,
                  isInStock: item.isInStock,
                })
              }}
            />
            Куплено
          </label>

          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              className="border-input text-primary focus-visible:ring-ring size-4 rounded border focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
              checked={item.isInStock}
              disabled={isPending}
              onChange={(event) => {
                onStateChange({
                  isPurchased: item.isPurchased,
                  isInStock: event.target.checked,
                })
              }}
            />
            Есть дома
          </label>
        </div>
      )}
    </article>
  )
}

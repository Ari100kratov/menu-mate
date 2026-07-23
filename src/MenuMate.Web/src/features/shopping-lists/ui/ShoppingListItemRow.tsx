import { Pencil, Trash2 } from "lucide-react"

import type { ShoppingListItem } from "@/features/shopping-lists/api/shopping-lists.api"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/ui/button"
import { Checkbox } from "@/shared/ui/checkbox"

interface ShoppingListItemRowProps {
  item: ShoppingListItem
  isPending: boolean
  onCheckedChange: (checked: boolean) => void
  onEdit: () => void
  onRemove: () => void
}

export function ShoppingListItemRow({
  item,
  isPending,
  onCheckedChange,
  onEdit,
  onRemove,
}: ShoppingListItemRowProps) {
  return (
    <div className="group flex items-center gap-3 px-2 py-3">
      <Checkbox
        checked={item.isPurchased}
        disabled={isPending}
        aria-label={`Отметить «${item.name}»`}
        onCheckedChange={(checked) => {
          onCheckedChange(checked === true)
        }}
      />
      <button
        type="button"
        className="hover:bg-muted/60 min-w-0 flex-1 rounded-md px-1 py-0.5 text-left transition-colors"
        disabled={isPending}
        aria-label={`${item.isPurchased ? "Снять отметку с" : "Отметить"} «${item.name}»`}
        onClick={() => {
          onCheckedChange(!item.isPurchased)
        }}
      >
        <span
          className={cn(
            "type-body block",
            item.isPurchased && "text-muted-foreground line-through",
          )}
        >
          {item.name}
        </span>
        <span className="type-supporting text-muted-foreground block">
          {item.amountText}
          {item.comment ? ` · ${item.comment}` : ""}
        </span>
      </button>
      <div className="flex shrink-0">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Редактировать"
          disabled={isPending}
          onClick={onEdit}
        >
          <Pencil className="size-4" />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Удалить"
          disabled={isPending}
          onClick={onRemove}
        >
          <Trash2 className="size-4" />
        </Button>
      </div>
    </div>
  )
}

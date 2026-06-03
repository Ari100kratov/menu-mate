import type { ShoppingListSummary } from "@/features/shopping-lists/api/shopping-lists.api"
import { formatDateTime } from "@/features/shopping-lists/model/shopping-list-ui"

interface ShoppingListSummaryButtonProps {
  summary: ShoppingListSummary
  isActive: boolean
  onClick: () => void
}

export function ShoppingListSummaryButton({
  summary,
  isActive,
  onClick,
}: ShoppingListSummaryButtonProps) {
  const total = Number(summary.itemsCount)
  const purchased = Number(summary.purchasedItemsCount)
  const percent = total > 0 ? Math.round((purchased / total) * 100) : 0

  return (
    <button
      type="button"
      className="bg-card text-card-foreground hover:bg-accent focus-visible:ring-ring w-full rounded-md border p-3 text-left outline-none focus-visible:ring-2"
      data-active={isActive}
      onClick={onClick}
    >
      <div className="font-medium">{formatDateTime(summary.updatedAt)}</div>
      <div className="bg-muted mt-2 h-1.5 overflow-hidden rounded-full">
        <div className="bg-primary h-full rounded-full" style={{ width: `${String(percent)}%` }} />
      </div>
      <div className="text-muted-foreground text-sm">
        {purchased} / {total} куплено
      </div>
    </button>
  )
}

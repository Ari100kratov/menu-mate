import { useShoppingListQuery } from "@/features/shopping-lists/api/shopping-lists.queries"
import { ShoppingListWorkspace } from "@/features/shopping-lists/ui/ShoppingListWorkspace"
import { ShoppingListSkeleton } from "@/features/shopping-lists/ui/ShoppingSkeletons"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function ShoppingPage() {
  const shoppingListQuery = useShoppingListQuery()

  if (shoppingListQuery.isPending) {
    return <ShoppingListSkeleton />
  }

  return (
    <div className="mx-auto max-w-3xl">
      {shoppingListQuery.error ? <ErrorAlert error={shoppingListQuery.error} /> : null}
      {shoppingListQuery.data ? (
        <ShoppingListWorkspace shoppingList={shoppingListQuery.data} />
      ) : null}
    </div>
  )
}

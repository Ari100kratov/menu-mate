import { useShoppingListQuery } from "@/features/shopping-lists/api/shopping-lists.queries"
import { ShoppingListWorkspace } from "@/features/shopping-lists/ui/ShoppingListWorkspace"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"

export default function ShoppingPage() {
  const shoppingListQuery = useShoppingListQuery()

  if (shoppingListQuery.isPending) {
    return <PageSkeleton />
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

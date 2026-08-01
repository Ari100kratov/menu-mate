import { useShoppingListOffline } from "@/features/shopping-lists/model/use-shopping-list-offline"
import { ShoppingOfflineStatusBar } from "@/features/shopping-lists/ui/ShoppingOfflineStatusBar"
import { ShoppingListWorkspace } from "@/features/shopping-lists/ui/ShoppingListWorkspace"
import { ShoppingListSkeleton } from "@/features/shopping-lists/ui/ShoppingSkeletons"
import { ErrorAlert } from "@/shared/ui/feedback"

export default function ShoppingPage() {
  const shoppingListState = useShoppingListOffline()

  if (shoppingListState.isPending) {
    return <ShoppingListSkeleton />
  }

  return (
    <div className="mx-auto max-w-3xl">
      {shoppingListState.error ? <ErrorAlert error={shoppingListState.error} /> : null}
      {shoppingListState.shoppingList ? (
        <ShoppingListWorkspace
          shoppingList={shoppingListState.shoppingList}
          isReadOnly={shoppingListState.isReadOnly}
          onItemStateChange={shoppingListState.setItemPurchasedState}
        />
      ) : null}
      {shoppingListState.offlineStatus ? (
        <ShoppingOfflineStatusBar
          status={shoppingListState.offlineStatus}
          onRetry={() => {
            window.location.reload()
          }}
        />
      ) : null}
    </div>
  )
}

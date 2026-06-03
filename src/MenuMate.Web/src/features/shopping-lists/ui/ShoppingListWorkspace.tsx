import { useMemo, useState } from "react"

import type {
  ShoppingList,
  ShoppingListItem,
} from "@/features/shopping-lists/api/shopping-lists.api"
import {
  useAddShoppingListItemMutation,
  useRemoveShoppingListItemMutation,
  useSetShoppingListItemStateMutation,
  useUpdateShoppingListItemMutation,
} from "@/features/shopping-lists/api/shopping-lists.queries"
import { toShoppingListItemRequest } from "@/features/shopping-lists/model/shopping-list-ui"
import {
  calculateProgress,
  filterShoppingListCategories,
  getItemsForStateChange,
  getShoppingFilterOptions,
  getShoppingListItems,
  type ShoppingItemFilter,
  type ShoppingListCategory,
  type ShoppingListItemState,
} from "@/features/shopping-lists/model/shopping-list-state"
import { ErrorAlert } from "@/shared/ui/feedback"
import { ShoppingListAddItemPanel } from "./ShoppingListAddItemPanel"
import { ShoppingListBulkActions } from "./ShoppingListBulkActions"
import { ShoppingListCategoriesPanel } from "./ShoppingListCategoriesPanel"
import { ShoppingListProgressPanel } from "./ShoppingListProgressPanel"
import { ShoppingListToolbar } from "./ShoppingListToolbar"

interface ShoppingListWorkspaceProps {
  shoppingList: ShoppingList
  isDeletePending: boolean
  onDelete: () => void
}

export function ShoppingListWorkspace({
  shoppingList,
  isDeletePending,
  onDelete,
}: ShoppingListWorkspaceProps) {
  const addItemMutation = useAddShoppingListItemMutation(shoppingList.id)
  const updateItemMutation = useUpdateShoppingListItemMutation(shoppingList.id)
  const stateMutation = useSetShoppingListItemStateMutation(shoppingList.id)
  const removeItemMutation = useRemoveShoppingListItemMutation(shoppingList.id)
  const [itemFilter, setItemFilter] = useState<ShoppingItemFilter>("all")
  const [isStoreMode, setIsStoreMode] = useState(false)
  const [editingItemId, setEditingItemId] = useState<string | null>(null)
  const [collapsedCategoryNames, setCollapsedCategoryNames] = useState<ReadonlySet<string>>(
    () => new Set(),
  )
  const progress = useMemo(() => calculateProgress(shoppingList), [shoppingList])
  const effectiveFilter: ShoppingItemFilter = isStoreMode ? "remaining" : itemFilter
  const visibleCategories = useMemo(
    () => filterShoppingListCategories(shoppingList, effectiveFilter),
    [effectiveFilter, shoppingList],
  )
  const visibleItems = useMemo(() => getShoppingListItems(visibleCategories), [visibleCategories])
  const visibleDoneCount = useMemo(
    () => visibleItems.filter((item) => item.isPurchased || item.isInStock).length,
    [visibleItems],
  )
  const filterOptions = useMemo(() => getShoppingFilterOptions(progress), [progress])
  const isItemMutationPending =
    addItemMutation.isPending ||
    updateItemMutation.isPending ||
    stateMutation.isPending ||
    removeItemMutation.isPending

  async function copyText() {
    await navigator.clipboard.writeText(shoppingList.text)
  }

  function toggleCategory(categoryName: string) {
    setCollapsedCategoryNames((current) => {
      const next = new Set(current)

      if (next.has(categoryName)) {
        next.delete(categoryName)
      } else {
        next.add(categoryName)
      }

      return next
    })
  }

  function setItemsState(items: readonly ShoppingListItem[], nextState: ShoppingListItemState) {
    getItemsForStateChange(items, nextState).forEach((item) => {
      stateMutation.mutate({
        itemId: item.id,
        request: nextState,
      })
    })
  }

  function setCategoryState(category: ShoppingListCategory, nextState: ShoppingListItemState) {
    setItemsState(category.items, nextState)
  }

  return (
    <div className="space-y-5">
      {addItemMutation.error ? <ErrorAlert error={addItemMutation.error} /> : null}
      {updateItemMutation.error ? <ErrorAlert error={updateItemMutation.error} /> : null}
      {stateMutation.error ? <ErrorAlert error={stateMutation.error} /> : null}
      {removeItemMutation.error ? <ErrorAlert error={removeItemMutation.error} /> : null}

      <ShoppingListToolbar
        progress={progress}
        isStoreMode={isStoreMode}
        isDeletePending={isDeletePending}
        isItemActionPending={isItemMutationPending}
        onStoreModeChange={setIsStoreMode}
        onMarkRemainingPurchased={() => {
          setItemsState(visibleItems, { isPurchased: true, isInStock: false })
        }}
        onCopy={() => {
          void copyText()
        }}
        onDelete={onDelete}
      />

      <ShoppingListProgressPanel
        progress={progress}
        filterOptions={filterOptions}
        isStoreMode={isStoreMode}
        itemFilter={itemFilter}
        onFilterChange={setItemFilter}
      />

      <ShoppingListBulkActions
        itemCount={visibleItems.length}
        doneCount={visibleDoneCount}
        isPending={isItemMutationPending}
        isStoreMode={isStoreMode}
        onMarkPurchased={() => {
          setItemsState(visibleItems, { isPurchased: true, isInStock: false })
        }}
        onMarkInStock={() => {
          setItemsState(visibleItems, { isPurchased: false, isInStock: true })
        }}
        onReset={() => {
          setItemsState(visibleItems, { isPurchased: false, isInStock: false })
        }}
      />

      <ShoppingListAddItemPanel
        isHidden={isStoreMode}
        isSubmitting={addItemMutation.isPending}
        onSubmit={(values) => {
          addItemMutation.mutate(toShoppingListItemRequest(values))
        }}
      />

      <ShoppingListCategoriesPanel
        shoppingList={shoppingList}
        visibleCategories={visibleCategories}
        collapsedCategoryNames={collapsedCategoryNames}
        isPending={isItemMutationPending}
        isStoreMode={isStoreMode}
        editingItemId={editingItemId}
        onToggleCategory={toggleCategory}
        onCategoryStateChange={setCategoryState}
        onEdit={setEditingItemId}
        onCancelEdit={() => {
          setEditingItemId(null)
        }}
        onUpdate={(itemId, values) => {
          updateItemMutation.mutate(
            {
              itemId,
              request: toShoppingListItemRequest(values),
            },
            {
              onSuccess: () => {
                setEditingItemId(null)
              },
            },
          )
        }}
        onStateChange={(itemId, nextState) => {
          stateMutation.mutate({
            itemId,
            request: nextState,
          })
        }}
        onRemove={(itemId) => {
          removeItemMutation.mutate(itemId)
        }}
      />
    </div>
  )
}

import type {
  ShoppingList,
  ShoppingListItem,
} from "@/features/shopping-lists/api/shopping-lists.api"

export type ShoppingItemFilter = "all" | "remaining" | "purchased" | "inStock"
export type ShoppingListCategory = ShoppingList["categories"][number]

export interface ShoppingListItemState {
  isPurchased: boolean
  isInStock: boolean
}

export interface ShoppingListProgress {
  total: number
  done: number
  remaining: number
  purchased: number
  inStock: number
  percent: number
}

export interface ShoppingFilterOption {
  value: ShoppingItemFilter
  label: string
  count: number
}

export function getShoppingListItems(
  categories: readonly ShoppingListCategory[],
): ShoppingListItem[] {
  return categories.flatMap((category) => category.items)
}

export function calculateProgress(shoppingList: ShoppingList): ShoppingListProgress {
  const items = getShoppingListItems(shoppingList.categories)
  const done = items.filter((item) => item.isPurchased || item.isInStock).length
  const total = items.length

  return {
    total,
    done,
    remaining: total - done,
    purchased: items.filter((item) => item.isPurchased).length,
    inStock: items.filter((item) => item.isInStock).length,
    percent: total > 0 ? Math.round((done / total) * 100) : 0,
  }
}

export function getShoppingFilterOptions(progress: ShoppingListProgress): ShoppingFilterOption[] {
  return [
    { value: "all", label: "Все", count: progress.total },
    { value: "remaining", label: "Осталось", count: progress.remaining },
    { value: "purchased", label: "Куплено", count: progress.purchased },
    { value: "inStock", label: "Есть дома", count: progress.inStock },
  ]
}

export function filterShoppingListCategories(
  shoppingList: ShoppingList,
  itemFilter: ShoppingItemFilter,
): ShoppingListCategory[] {
  return shoppingList.categories
    .map((category) => ({
      ...category,
      items: category.items.filter((item) => isShoppingListItemVisible(item, itemFilter)),
    }))
    .filter((category) => category.items.length > 0)
}

export function getItemsForStateChange(
  items: readonly ShoppingListItem[],
  nextState: ShoppingListItemState,
): ShoppingListItem[] {
  return items.filter(
    (item) => item.isPurchased !== nextState.isPurchased || item.isInStock !== nextState.isInStock,
  )
}

export function isShoppingListItemVisible(item: ShoppingListItem, itemFilter: ShoppingItemFilter) {
  switch (itemFilter) {
    case "all":
      return true
    case "remaining":
      return !item.isPurchased && !item.isInStock
    case "purchased":
      return item.isPurchased
    case "inStock":
      return item.isInStock
  }
}

import type {
  ShoppingList,
  ShoppingListItem,
} from "@/features/shopping-lists/api/shopping-lists.api"

type ShoppingListTextScope = "all" | "remaining"

export function refreshShoppingListText(shoppingList: ShoppingList): ShoppingList {
  return {
    ...shoppingList,
    text: formatShoppingListText(shoppingList, "all"),
    remainingText: formatShoppingListText(shoppingList, "remaining"),
  }
}

export function setShoppingListItemPurchasedState(
  shoppingList: ShoppingList,
  itemId: string,
  isPurchased: boolean,
) {
  return refreshShoppingListText({
    ...shoppingList,
    categories: shoppingList.categories.map((category) => ({
      ...category,
      items: category.items.map((item) =>
        item.id === itemId
          ? {
              ...item,
              isPurchased,
            }
          : item,
      ),
    })),
  })
}

export function findShoppingListItem(shoppingList: ShoppingList, itemId: string) {
  return shoppingList.categories
    .flatMap((category) => category.items)
    .find((item) => item.id === itemId)
}

function formatShoppingListText(shoppingList: ShoppingList, scope: ShoppingListTextScope) {
  return shoppingList.categories
    .map((category) => {
      const items = category.items.filter((item) => scope === "all" || !item.isPurchased)

      if (items.length === 0) {
        return null
      }

      return [category.name, ...items.map(formatShoppingListItem)].join("\n")
    })
    .filter((categoryText): categoryText is string => categoryText !== null)
    .join("\n\n")
}

function formatShoppingListItem(item: ShoppingListItem) {
  const purchasedMarker = item.isPurchased ? "✓ " : ""
  const amount = item.amountText.trim() ? ` ${item.amountText.trim()}` : ""
  const comment = item.comment?.trim() ? ` (${item.comment.trim()})` : ""
  return `- ${purchasedMarker}${item.name}${amount}${comment}`
}

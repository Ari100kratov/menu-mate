import { removePersistentValue } from "@/shared/lib/persistent-state"

export const shoppingCategoryCollapseStorageKey = "menumate:shopping:collapsed-categories:v1"

export type CollapsedShoppingCategoriesByList = Record<string, string[]>

export function isCollapsedShoppingCategoriesByList(
  value: unknown,
): value is CollapsedShoppingCategoriesByList {
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    return false
  }

  return Object.values(value).every(
    (categoryNames) =>
      Array.isArray(categoryNames) &&
      categoryNames.every((categoryName) => typeof categoryName === "string"),
  )
}

export function clearCollapsedShoppingCategories() {
  removePersistentValue(shoppingCategoryCollapseStorageKey)
}

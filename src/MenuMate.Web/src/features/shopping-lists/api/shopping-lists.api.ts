import { apiFetchJson } from "@/shared/api/client"

export interface ShoppingListItem {
  id: string
  productId: string
  name: string
  amount: number | string | null
  unit: string
  category: string
  amountText: string
  comment: string | null
  isPurchased: boolean
}

export interface ShoppingListCategory {
  name: string
  items: ShoppingListItem[]
}

export interface ShoppingList {
  id: string
  sourceStartDate: string
  sourceEndDate: string
  createdAt: string
  updatedAt: string
  categories: ShoppingListCategory[]
  text: string
}

export interface ShoppingListItemRequest {
  productId: string | null
  name: string
  amount: number | null
  unit: string
  category: string
  comment: string | null
}

export interface ShoppingListItemStateRequest {
  isPurchased: boolean
}

export interface MenuShoppingPreviewIngredient {
  ingredientId: string
  productId: string
  name: string
  amount: number | string | null
  unit: string
  category: string
  amountText: string
  comment: string | null
  isOptional: boolean
}

export interface MenuShoppingPreviewRecipe {
  menuItemId: string
  title: string
  baseServings: number
  servings: number
  ingredients: MenuShoppingPreviewIngredient[]
}

export interface MenuShoppingPreview {
  startDate: string
  endDate: string
  recipes: MenuShoppingPreviewRecipe[]
}

export interface ReplaceShoppingListFromMenuRequest {
  startDate: string
  endDate: string
  recipes: {
    menuItemId: string
    servings: number
    ingredientIds: string[]
  }[]
}

export function getShoppingList() {
  return apiFetchJson<ShoppingList>("/api/shopping-list")
}

export function getMenuShoppingPreview(startDate: string, endDate: string) {
  const query = new URLSearchParams({ startDate, endDate })
  return apiFetchJson<MenuShoppingPreview>(`/api/shopping-list/menu-preview?${query.toString()}`)
}

export function replaceShoppingListFromMenu(request: ReplaceShoppingListFromMenuRequest) {
  return apiFetchJson<ShoppingList>("/api/shopping-list/from-menu", {
    method: "PUT",
    body: JSON.stringify(request),
  })
}

export function addShoppingListItem(request: ShoppingListItemRequest) {
  return apiFetchJson<ShoppingList>("/api/shopping-list/items", {
    method: "POST",
    body: JSON.stringify(request),
  })
}

export function updateShoppingListItem(itemId: string, request: ShoppingListItemRequest) {
  return apiFetchJson<ShoppingList>(`/api/shopping-list/items/${itemId}`, {
    method: "PUT",
    body: JSON.stringify(request),
  })
}

export function setShoppingListItemState(itemId: string, request: ShoppingListItemStateRequest) {
  return apiFetchJson<ShoppingList>(`/api/shopping-list/items/${itemId}/checked`, {
    method: "PATCH",
    body: JSON.stringify(request),
  })
}

export async function removeShoppingListItem(itemId: string) {
  await apiFetchJson<unknown>(`/api/shopping-list/items/${itemId}`, { method: "DELETE" })
}

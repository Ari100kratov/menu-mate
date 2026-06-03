import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import type { components } from "@/shared/api/generated/schema"

export type ShoppingListSummary = components["schemas"]["ShoppingListSummaryResponse"]
export type ShoppingList = components["schemas"]["ShoppingListResponse"]
export type ShoppingListItem = components["schemas"]["ShoppingListItemResponse"]
export type ShoppingListItemRequest = components["schemas"]["ShoppingListItemRequest"]
export type ShoppingListItemStateRequest = components["schemas"]["ShoppingListItemStateRequest"]
export type GenerateShoppingListRequest = components["schemas"]["GenerateShoppingListRequest"]

export async function getShoppingLists() {
  return unwrapApiResponse<ShoppingListSummary[]>(
    apiClient.GET("/api/shopping-lists", {
      params: {},
    }),
  )
}

export async function getShoppingList(shoppingListId: string) {
  return unwrapApiResponse<ShoppingList>(
    apiClient.GET("/api/shopping-lists/{shoppingListId}", {
      params: {
        path: {
          shoppingListId,
        },
      },
    }),
  )
}

export async function generateShoppingList(request: GenerateShoppingListRequest) {
  return unwrapApiResponse<ShoppingList>(
    apiClient.POST("/api/shopping-lists", {
      body: request,
    }),
  )
}

export async function deleteShoppingList(shoppingListId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/shopping-lists/{shoppingListId}", {
      params: {
        path: {
          shoppingListId,
        },
      },
    }),
  )
}

export async function addShoppingListItem(
  shoppingListId: string,
  request: ShoppingListItemRequest,
) {
  return unwrapApiResponse<ShoppingList>(
    apiClient.POST("/api/shopping-lists/{shoppingListId}/items", {
      params: {
        path: {
          shoppingListId,
        },
      },
      body: request,
    }),
  )
}

export async function updateShoppingListItem(
  shoppingListId: string,
  itemId: string,
  request: ShoppingListItemRequest,
) {
  return unwrapApiResponse<ShoppingList>(
    apiClient.PUT("/api/shopping-lists/{shoppingListId}/items/{itemId}", {
      params: {
        path: {
          shoppingListId,
          itemId,
        },
      },
      body: request,
    }),
  )
}

export async function setShoppingListItemState(
  shoppingListId: string,
  itemId: string,
  request: ShoppingListItemStateRequest,
) {
  return unwrapApiResponse<ShoppingList>(
    apiClient.PATCH("/api/shopping-lists/{shoppingListId}/items/{itemId}/state", {
      params: {
        path: {
          shoppingListId,
          itemId,
        },
      },
      body: request,
    }),
  )
}

export async function removeShoppingListItem(shoppingListId: string, itemId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/shopping-lists/{shoppingListId}/items/{itemId}", {
      params: {
        path: {
          shoppingListId,
          itemId,
        },
      },
    }),
  )
}

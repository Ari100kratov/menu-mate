import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  addShoppingListItem,
  deleteShoppingList,
  generateShoppingList,
  getShoppingList,
  getShoppingLists,
  removeShoppingListItem,
  setShoppingListItemState,
  updateShoppingListItem,
  type GenerateShoppingListRequest,
  type ShoppingListItemRequest,
  type ShoppingListItemStateRequest,
} from "@/features/shopping-lists/api/shopping-lists.api"

export const shoppingListQueryKeys = {
  all: ["shopping-lists"] as const,
  lists: () => [...shoppingListQueryKeys.all, "list"] as const,
  detail: (shoppingListId: string) =>
    [...shoppingListQueryKeys.all, "detail", shoppingListId] as const,
}

export function useShoppingListsQuery() {
  return useQuery({
    queryKey: shoppingListQueryKeys.lists(),
    queryFn: getShoppingLists,
    staleTime: 30_000,
  })
}

export function useShoppingListQuery(shoppingListId: string | undefined) {
  return useQuery({
    queryKey: shoppingListQueryKeys.detail(shoppingListId ?? ""),
    queryFn: () => getShoppingList(shoppingListId ?? ""),
    enabled: Boolean(shoppingListId),
    staleTime: 30_000,
  })
}

export function useGenerateShoppingListMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: GenerateShoppingListRequest) => generateShoppingList(request),
    onSuccess: (shoppingList) => {
      queryClient.setQueryData(shoppingListQueryKeys.detail(shoppingList.id), shoppingList)
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.lists() })
    },
  })
}

export function useDeleteShoppingListMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deleteShoppingList,
    onSuccess: (_data, shoppingListId) => {
      queryClient.removeQueries({ queryKey: shoppingListQueryKeys.detail(shoppingListId) })
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.lists() })
    },
  })
}

export function useAddShoppingListItemMutation(shoppingListId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: ShoppingListItemRequest) => addShoppingListItem(shoppingListId, request),
    onSuccess: (shoppingList) => {
      queryClient.setQueryData(shoppingListQueryKeys.detail(shoppingListId), shoppingList)
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.lists() })
    },
  })
}

export function useUpdateShoppingListItemMutation(shoppingListId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ itemId, request }: { itemId: string; request: ShoppingListItemRequest }) =>
      updateShoppingListItem(shoppingListId, itemId, request),
    onSuccess: (shoppingList) => {
      queryClient.setQueryData(shoppingListQueryKeys.detail(shoppingListId), shoppingList)
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.lists() })
    },
  })
}

export function useSetShoppingListItemStateMutation(shoppingListId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ itemId, request }: { itemId: string; request: ShoppingListItemStateRequest }) =>
      setShoppingListItemState(shoppingListId, itemId, request),
    onSuccess: (shoppingList) => {
      queryClient.setQueryData(shoppingListQueryKeys.detail(shoppingListId), shoppingList)
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.lists() })
    },
  })
}

export function useRemoveShoppingListItemMutation(shoppingListId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (itemId: string) => removeShoppingListItem(shoppingListId, itemId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.detail(shoppingListId) })
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.lists() })
    },
  })
}

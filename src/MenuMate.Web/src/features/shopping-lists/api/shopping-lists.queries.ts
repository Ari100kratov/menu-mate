import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  addShoppingListItem,
  getMenuShoppingPreview,
  getShoppingList,
  removeShoppingListItem,
  replaceShoppingListFromMenu,
  setShoppingListItemState,
  updateShoppingListItem,
  type ReplaceShoppingListFromMenuRequest,
  type ShoppingListItemRequest,
  type ShoppingListItemStateRequest,
} from "@/features/shopping-lists/api/shopping-lists.api"

export const shoppingListQueryKeys = {
  current: ["shopping-list"] as const,
  preview: (startDate: string, endDate: string) =>
    ["shopping-list", "menu-preview", startDate, endDate] as const,
}

export function useShoppingListQuery() {
  return useQuery({
    queryKey: shoppingListQueryKeys.current,
    queryFn: getShoppingList,
    staleTime: 30_000,
  })
}

export function useMenuShoppingPreviewQuery(startDate: string, endDate: string) {
  return useQuery({
    queryKey: shoppingListQueryKeys.preview(startDate, endDate),
    queryFn: () => getMenuShoppingPreview(startDate, endDate),
    enabled: Boolean(startDate && endDate),
    staleTime: 30_000,
  })
}

function useShoppingListMutation<TVariables>(
  mutationFn: (variables: TVariables) => Promise<Awaited<ReturnType<typeof getShoppingList>>>,
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: (shoppingList) => {
      queryClient.setQueryData(shoppingListQueryKeys.current, shoppingList)
    },
  })
}

export function useReplaceShoppingListFromMenuMutation() {
  return useShoppingListMutation((request: ReplaceShoppingListFromMenuRequest) =>
    replaceShoppingListFromMenu(request),
  )
}

export function useAddShoppingListItemMutation() {
  return useShoppingListMutation((request: ShoppingListItemRequest) => addShoppingListItem(request))
}

export function useUpdateShoppingListItemMutation() {
  return useShoppingListMutation(
    ({ itemId, request }: { itemId: string; request: ShoppingListItemRequest }) =>
      updateShoppingListItem(itemId, request),
  )
}

export function useSetShoppingListItemStateMutation() {
  return useShoppingListMutation(
    ({ itemId, request }: { itemId: string; request: ShoppingListItemStateRequest }) =>
      setShoppingListItemState(itemId, request),
  )
}

export function useRemoveShoppingListItemMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: removeShoppingListItem,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: shoppingListQueryKeys.current })
    },
  })
}

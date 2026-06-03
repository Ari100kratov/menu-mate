import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  addMenuPlanItem,
  createMenuPlan,
  deleteMenuPlan,
  getMenuPlan,
  getMenuPlans,
  removeMenuPlanItem,
  updateMenuPlanItem,
  updateMenuPlan,
  type CreateMenuPlanItemRequest,
  type CreateMenuPlanRequest,
  type UpdateMenuPlanItemRequest,
  type UpdateMenuPlanRequest,
} from "@/features/menu-planning/api/menu-plans.api"

export const menuPlanQueryKeys = {
  all: ["menu-plans"] as const,
  lists: () => [...menuPlanQueryKeys.all, "list"] as const,
  detail: (menuPlanId: string) => [...menuPlanQueryKeys.all, "detail", menuPlanId] as const,
}

export function useMenuPlansQuery() {
  return useQuery({
    queryKey: menuPlanQueryKeys.lists(),
    queryFn: getMenuPlans,
    staleTime: 30_000,
  })
}

export function useMenuPlanQuery(menuPlanId: string | undefined) {
  return useQuery({
    queryKey: menuPlanQueryKeys.detail(menuPlanId ?? ""),
    queryFn: () => getMenuPlan(menuPlanId ?? ""),
    enabled: Boolean(menuPlanId),
    staleTime: 30_000,
  })
}

export function useCreateMenuPlanMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateMenuPlanRequest) => createMenuPlan(request),
    onSuccess: (menuPlan) => {
      queryClient.setQueryData(menuPlanQueryKeys.detail(menuPlan.id), menuPlan)
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.lists() })
    },
  })
}

export function useUpdateMenuPlanMutation(menuPlanId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UpdateMenuPlanRequest) => updateMenuPlan(menuPlanId, request),
    onSuccess: (menuPlan) => {
      queryClient.setQueryData(menuPlanQueryKeys.detail(menuPlanId), menuPlan)
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.lists() })
    },
  })
}

export function useDeleteMenuPlanMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deleteMenuPlan,
    onSuccess: (_data, menuPlanId) => {
      queryClient.removeQueries({ queryKey: menuPlanQueryKeys.detail(menuPlanId) })
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.lists() })
    },
  })
}

export function useAddMenuPlanItemMutation(menuPlanId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateMenuPlanItemRequest) => addMenuPlanItem(menuPlanId, request),
    onSuccess: (menuPlan) => {
      queryClient.setQueryData(menuPlanQueryKeys.detail(menuPlanId), menuPlan)
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.lists() })
    },
  })
}

export function useRemoveMenuPlanItemMutation(menuPlanId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (itemId: string) => removeMenuPlanItem(menuPlanId, itemId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.detail(menuPlanId) })
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.lists() })
    },
  })
}

export function useUpdateMenuPlanItemMutation(menuPlanId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ itemId, request }: { itemId: string; request: UpdateMenuPlanItemRequest }) =>
      updateMenuPlanItem(menuPlanId, itemId, request),
    onSuccess: (menuPlan) => {
      queryClient.setQueryData(menuPlanQueryKeys.detail(menuPlanId), menuPlan)
      void queryClient.invalidateQueries({ queryKey: menuPlanQueryKeys.lists() })
    },
  })
}

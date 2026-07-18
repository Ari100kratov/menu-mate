import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  addMenuCalendarItem,
  clearMenuCalendar,
  createMealSlot,
  deleteMealSlot,
  getMenuCalendar,
  getMealSlots,
  removeMenuCalendarItem,
  reorderMealSlots,
  updateMealSlot,
  updateMenuCalendarItem,
  type UpdateMenuCalendarItemRequest,
} from "@/features/menu-planning/api/menu-calendar.api"

export const menuCalendarQueryKeys = {
  all: ["menu-calendar"] as const,
  range: (startDate: string, endDate: string) =>
    [...menuCalendarQueryKeys.all, startDate, endDate] as const,
}

export function useMenuCalendarQuery(startDate: string, endDate: string) {
  return useQuery({
    queryKey: menuCalendarQueryKeys.range(startDate, endDate),
    queryFn: () => getMenuCalendar(startDate, endDate),
    placeholderData: keepPreviousData,
    staleTime: 15_000,
  })
}

export function useMealSlotsQuery() {
  return useQuery({
    queryKey: [...menuCalendarQueryKeys.all, "meal-slots"],
    queryFn: getMealSlots,
    staleTime: 300_000,
  })
}

function useCalendarMutation<TVariables, TResult>(
  mutationFn: (variables: TVariables) => Promise<TResult>,
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: menuCalendarQueryKeys.all })
    },
  })
}

export function useAddMenuCalendarItemMutation() {
  return useCalendarMutation(addMenuCalendarItem)
}

export function useUpdateMenuCalendarItemMutation() {
  return useCalendarMutation(
    ({ itemId, request }: { itemId: string; request: UpdateMenuCalendarItemRequest }) =>
      updateMenuCalendarItem(itemId, request),
  )
}

export function useRemoveMenuCalendarItemMutation() {
  return useCalendarMutation(removeMenuCalendarItem)
}

export function useClearMenuCalendarMutation() {
  return useCalendarMutation(({ startDate, endDate }: { startDate: string; endDate: string }) =>
    clearMenuCalendar(startDate, endDate),
  )
}

export function useCreateMealSlotMutation() {
  return useCalendarMutation((name: string) => createMealSlot(name))
}

export function useUpdateMealSlotMutation() {
  return useCalendarMutation(({ mealSlotId, name }: { mealSlotId: string; name: string }) =>
    updateMealSlot(mealSlotId, name),
  )
}

export function useDeleteMealSlotMutation() {
  return useCalendarMutation(deleteMealSlot)
}

export function useReorderMealSlotsMutation() {
  return useCalendarMutation((mealSlotIds: readonly string[]) => reorderMealSlots(mealSlotIds))
}

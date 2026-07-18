import { apiFetchJson } from "@/shared/api/client"

export interface MealSlot {
  id: string
  name: string
  sortOrder: number
}

export interface MenuCalendarItem {
  id: string
  date: string
  mealSlotId: string
  position: number
  recipeId: string | null
  recipeRevisionId: string | null
  recipeTitle: string | null
  text: string | null
  servings: number
  comment: string | null
  imageUrl: string | null
}

export interface MenuCalendar {
  startDate: string
  endDate: string
  mealSlots: MealSlot[]
  items: MenuCalendarItem[]
}

export interface CreateMenuCalendarItemRequest {
  date: string
  mealSlotId: string
  recipeId: string | null
  recipeRevisionId: string | null
  text: string | null
  servings: number
  comment: string | null
}

export interface UpdateMenuCalendarItemRequest {
  date: string
  mealSlotId: string
  text: string | null
  servings: number
  comment: string | null
}

export async function getMenuCalendar(startDate: string, endDate: string) {
  return apiFetchJson<MenuCalendar>(
    `/api/menu-calendar?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`,
  )
}

export async function getMealSlots() {
  return apiFetchJson<MealSlot[]>("/api/menu-calendar/meal-slots")
}

export async function addMenuCalendarItem(request: CreateMenuCalendarItemRequest) {
  return apiFetchJson<MenuCalendarItem>("/api/menu-calendar/items", {
    method: "POST",
    body: JSON.stringify(request),
  })
}

export async function updateMenuCalendarItem(
  itemId: string,
  request: UpdateMenuCalendarItemRequest,
) {
  return apiFetchJson<MenuCalendarItem>(`/api/menu-calendar/items/${itemId}`, {
    method: "PUT",
    body: JSON.stringify(request),
  })
}

export async function removeMenuCalendarItem(itemId: string) {
  await apiFetchJson<unknown>(`/api/menu-calendar/items/${itemId}`, {
    method: "DELETE",
  })
}

export async function clearMenuCalendar(startDate: string, endDate: string) {
  await apiFetchJson<unknown>(
    `/api/menu-calendar?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`,
    { method: "DELETE" },
  )
}

export async function createMealSlot(name: string) {
  return apiFetchJson<MealSlot[]>("/api/menu-calendar/meal-slots", {
    method: "POST",
    body: JSON.stringify({ name }),
  })
}

export async function updateMealSlot(mealSlotId: string, name: string) {
  return apiFetchJson<MealSlot[]>(`/api/menu-calendar/meal-slots/${mealSlotId}`, {
    method: "PUT",
    body: JSON.stringify({ name }),
  })
}

export async function deleteMealSlot(mealSlotId: string) {
  return apiFetchJson<MealSlot[]>(`/api/menu-calendar/meal-slots/${mealSlotId}`, {
    method: "DELETE",
  })
}

export async function reorderMealSlots(mealSlotIds: readonly string[]) {
  return apiFetchJson<MealSlot[]>("/api/menu-calendar/meal-slots/order", {
    method: "PUT",
    body: JSON.stringify({ mealSlotIds }),
  })
}

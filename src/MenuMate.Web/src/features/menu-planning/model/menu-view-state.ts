import { getRange, type MenuDateRange, type MenuRangeMode } from "./menu-calendar"

import { usePersistentState } from "@/shared/lib/persistent-state"

interface MenuViewState {
  rangeMode: MenuRangeMode
  range: MenuDateRange
}

const initialMenuViewState: MenuViewState = {
  rangeMode: "week",
  range: getRange("week"),
}

export function useMenuViewState() {
  return usePersistentState("menumate:menu:calendar-view:v1", initialMenuViewState, isMenuViewState)
}

function isMenuViewState(value: unknown): value is MenuViewState {
  if (!isRecord(value) || !isMenuRangeMode(value.rangeMode) || !isRecord(value.range)) {
    return false
  }

  const startDate = value.range.startDate
  const endDate = value.range.endDate
  return (
    typeof startDate === "string" &&
    typeof endDate === "string" &&
    isInputDate(startDate) &&
    isInputDate(endDate) &&
    startDate <= endDate
  )
}

function isMenuRangeMode(value: unknown): value is MenuRangeMode {
  return value === "week" || value === "month" || value === "custom"
}

function isInputDate(value: string) {
  return /^\d{4}-\d{2}-\d{2}$/.test(value) && !Number.isNaN(Date.parse(`${value}T00:00:00`))
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}

export type MenuRangeMode = "week" | "month" | "custom"

export interface MenuDateRange {
  startDate: string
  endDate: string
}

export interface PlacementRecipe {
  id: string
  revisionId: string
  title: string
  servings: number
}

export function formatInputDate(date: Date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const day = String(date.getDate()).padStart(2, "0")
  return `${String(year)}-${month}-${day}`
}

export function parseInputDate(value: string) {
  return new Date(`${value}T00:00:00`)
}

export function getRange(mode: MenuRangeMode, anchor = new Date()): MenuDateRange {
  if (mode === "month") {
    return {
      startDate: formatInputDate(new Date(anchor.getFullYear(), anchor.getMonth(), 1)),
      endDate: formatInputDate(new Date(anchor.getFullYear(), anchor.getMonth() + 1, 0)),
    }
  }

  const start = new Date(anchor)
  const day = start.getDay()
  start.setDate(start.getDate() - (day === 0 ? 6 : day - 1))
  const end = new Date(start)
  end.setDate(end.getDate() + 6)
  return { startDate: formatInputDate(start), endDate: formatInputDate(end) }
}

export function shiftRange(range: MenuDateRange, mode: MenuRangeMode, direction: -1 | 1) {
  const anchor = parseInputDate(range.startDate)
  if (mode === "month") {
    anchor.setMonth(anchor.getMonth() + direction)
    return getRange("month", anchor)
  }

  const days = mode === "week" ? 7 : getRangeLength(range)
  const start = parseInputDate(range.startDate)
  const end = parseInputDate(range.endDate)
  start.setDate(start.getDate() + days * direction)
  end.setDate(end.getDate() + days * direction)
  return { startDate: formatInputDate(start), endDate: formatInputDate(end) }
}

export function getRangeDates(range: MenuDateRange) {
  const result: string[] = []
  const current = parseInputDate(range.startDate)
  const end = parseInputDate(range.endDate)
  while (current <= end) {
    result.push(formatInputDate(current))
    current.setDate(current.getDate() + 1)
  }
  return result
}

export function formatRangeLabel(range: MenuDateRange) {
  const formatter = new Intl.DateTimeFormat("ru-RU", { day: "numeric", month: "short" })
  const yearFormatter = new Intl.DateTimeFormat("ru-RU", { year: "numeric" })
  return `${formatter.format(parseInputDate(range.startDate))} - ${formatter.format(parseInputDate(range.endDate))}, ${yearFormatter.format(parseInputDate(range.endDate))}`
}

export function formatDayHeading(value: string) {
  const date = parseInputDate(value)
  const weekday = new Intl.DateTimeFormat("ru-RU", { weekday: "long" }).format(date)
  const dateLabel = new Intl.DateTimeFormat("ru-RU", { day: "numeric", month: "long" }).format(date)
  return `${capitalize(weekday)}, ${dateLabel}`
}

function getRangeLength(range: MenuDateRange) {
  return Math.max(
    1,
    Math.round(
      (parseInputDate(range.endDate).getTime() - parseInputDate(range.startDate).getTime()) /
        86_400_000,
    ) + 1,
  )
}

function capitalize(value: string) {
  return value.charAt(0).toLocaleUpperCase("ru-RU") + value.slice(1)
}

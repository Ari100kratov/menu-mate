export const mealTypeOptions = [
  { value: "Breakfast", label: "Завтрак" },
  { value: "Lunch", label: "Обед" },
  { value: "Dinner", label: "Ужин" },
  { value: "Snack", label: "Перекус" },
] as const

export function getMealTypeLabel(mealType: string) {
  return mealTypeOptions.find((option) => option.value === mealType)?.label ?? mealType
}

export function formatDate(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
  }).format(new Date(`${value}T00:00:00`))
}

export function formatInputDate(date: Date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const day = String(date.getDate()).padStart(2, "0")
  return `${String(year)}-${month}-${day}`
}

export function getTodayDateInputValue() {
  return formatInputDate(new Date())
}

export function addDays(dateInputValue: string, days: number) {
  const date = new Date(`${dateInputValue}T00:00:00`)
  date.setDate(date.getDate() + days)
  return formatInputDate(date)
}

export function getDateRangeInputValues(startDate: string, endDate: string) {
  const dates: string[] = []
  const current = new Date(`${startDate}T00:00:00`)
  const end = new Date(`${endDate}T00:00:00`)

  while (current <= end) {
    dates.push(formatInputDate(current))
    current.setDate(current.getDate() + 1)
  }

  return dates
}

export function formatWeekday(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    weekday: "short",
  }).format(new Date(`${value}T00:00:00`))
}

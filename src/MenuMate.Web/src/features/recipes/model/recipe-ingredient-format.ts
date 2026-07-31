import { measurementUnitOptions } from "@/features/recipes/model/recipe-form-options"

interface IngredientQuantity {
  amount: null | number | string
  unit: string
}

export function formatRecipeIngredientQuantity(ingredient: IngredientQuantity) {
  if (ingredient.unit === "ToTaste") {
    return "По вкусу"
  }

  const amount =
    ingredient.amount === null || ingredient.amount === "" ? "" : String(ingredient.amount)
  const unit =
    measurementUnitOptions.find((option) => option.value === ingredient.unit)?.label ??
    ingredient.unit
  const quantity = [amount, unit].filter(Boolean).join(" ")
  return quantity || "Количество не указано"
}

export function scaleRecipeIngredientAmount(
  amount: number | null,
  originalServings: number,
  targetServings: number,
) {
  if (amount === null || originalServings <= 0 || originalServings === targetServings) {
    return amount
  }

  return Math.round(((amount * targetServings) / originalServings) * 100) / 100
}

export function formatRecipeServings(servings: number) {
  const lastTwoDigits = Math.abs(servings) % 100
  const lastDigit = lastTwoDigits % 10
  const label =
    lastTwoDigits >= 11 && lastTwoDigits <= 14
      ? "порций"
      : lastDigit === 1
        ? "порция"
        : lastDigit >= 2 && lastDigit <= 4
          ? "порции"
          : "порций"

  return `${String(servings)} ${label}`
}

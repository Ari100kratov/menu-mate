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

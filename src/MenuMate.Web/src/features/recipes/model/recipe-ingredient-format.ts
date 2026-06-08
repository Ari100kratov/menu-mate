import {
  measurementUnitOptions,
  quantityKindOptions,
} from "@/features/recipes/model/recipe-form-options"

interface IngredientQuantity {
  amount: null | number | string
  unit: string
  quantityKind: string
}

export function formatRecipeIngredientQuantity(ingredient: IngredientQuantity) {
  if (ingredient.quantityKind === "ToTaste") {
    return "По вкусу"
  }

  const amount =
    ingredient.amount === null || ingredient.amount === "" ? "" : String(ingredient.amount)
  const unit =
    measurementUnitOptions.find((option) => option.value === ingredient.unit)?.label ??
    ingredient.unit
  const quantity = [amount, unit].filter(Boolean).join(" ")
  const quantityKind =
    quantityKindOptions.find((option) => option.value === ingredient.quantityKind)?.label ??
    ingredient.quantityKind

  if (ingredient.quantityKind === "Exact") {
    return quantity || "Количество не указано"
  }

  return `${quantityKind}: ${quantity || "количество не указано"}`
}

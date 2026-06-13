import {
  measurementUnitOptions,
  productCategoryOptions,
} from "@/shared/config/product-taxonomy"

export const defaultShoppingUnit = "Piece"
export const defaultShoppingCategory = "Other"

export const shoppingUnitOptions = measurementUnitOptions.filter(
  (option) => option.value !== "ToTaste" && option.value !== "Unknown",
)

export const shoppingCategoryOptions = productCategoryOptions

export type ShoppingUnitValue = (typeof shoppingUnitOptions)[number]["value"]
export type ShoppingCategoryValue = (typeof shoppingCategoryOptions)[number]["value"]

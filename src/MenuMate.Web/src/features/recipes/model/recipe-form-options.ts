export {
  getProductCategoryLabel,
  measurementUnitOptions,
  productCategoryOptions,
} from "@/shared/config/product-taxonomy"

export const recipeCategoryOptions = [
  { value: "Breakfast", label: "Завтрак" },
  { value: "Soup", label: "Суп" },
  { value: "MainCourse", label: "Основное блюдо" },
  { value: "SideDish", label: "Гарнир" },
  { value: "Salad", label: "Салат" },
  { value: "Appetizer", label: "Закуска" },
  { value: "Dessert", label: "Десерт" },
  { value: "Baking", label: "Выпечка" },
  { value: "Drink", label: "Напиток" },
  { value: "Sauce", label: "Соус" },
  { value: "Other", label: "Другое" },
] as const

export const recipeVisibilityOptions = [
  { value: "Private", label: "Личный", description: "Виден только вам" },
  {
    value: "Public",
    label: "Публичный",
    description: "Рецепт виден всем. Изменять оригинал можете только вы.",
  },
] as const

export function getRecipeCategoryLabel(category: string) {
  return recipeCategoryOptions.find((option) => option.value === category)?.label ?? category
}

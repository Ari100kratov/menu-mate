export const defaultShoppingUnit = "Piece"
export const defaultShoppingCategory = "Other"

export const shoppingUnitOptions = [
  { value: "Gram", label: "г" },
  { value: "Kilogram", label: "кг" },
  { value: "Milliliter", label: "мл" },
  { value: "Liter", label: "л" },
  { value: "Piece", label: "шт." },
  { value: "Pack", label: "уп." },
] as const

export const shoppingQuantityKindOptions = [
  { value: "Exact", label: "Точно" },
  { value: "Approximate", label: "Примерно" },
  { value: "ToTaste", label: "По вкусу" },
] as const

export const shoppingCategoryOptions = [
  { value: "Produce", label: "Овощи и фрукты" },
  { value: "Dairy", label: "Молочные" },
  { value: "MeatAndPoultry", label: "Мясо и птица" },
  { value: "FishAndSeafood", label: "Рыба" },
  { value: "Grocery", label: "Бакалея" },
  { value: "GrainsAndPasta", label: "Крупы и паста" },
  { value: "Spices", label: "Специи" },
  { value: "Bakery", label: "Хлеб" },
  { value: "Drinks", label: "Напитки" },
  { value: "Frozen", label: "Заморозка" },
  { value: "Other", label: "Прочее" },
] as const

export type ShoppingUnitValue = (typeof shoppingUnitOptions)[number]["value"]
export type ShoppingCategoryValue = (typeof shoppingCategoryOptions)[number]["value"]

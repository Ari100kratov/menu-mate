export const measurementUnitOptions = [
  { value: "Gram", label: "г" },
  { value: "Kilogram", label: "кг" },
  { value: "Milliliter", label: "мл" },
  { value: "Liter", label: "л" },
  { value: "Piece", label: "шт." },
  { value: "Teaspoon", label: "ч. л." },
  { value: "Tablespoon", label: "ст. л." },
  { value: "Pinch", label: "щепотка" },
  { value: "Pack", label: "упаковка" },
  { value: "ToTaste", label: "по вкусу" },
  { value: "Unknown", label: "без единицы" },
] as const

export const productCategoryOptions = [
  { value: "Produce", label: "Овощи и фрукты" },
  { value: "Dairy", label: "Молочные продукты" },
  { value: "MeatAndPoultry", label: "Мясо и птица" },
  { value: "FishAndSeafood", label: "Рыба и морепродукты" },
  { value: "Grocery", label: "Бакалея" },
  { value: "GrainsAndPasta", label: "Крупы и паста" },
  { value: "Spices", label: "Специи" },
  { value: "Bakery", label: "Выпечка и хлеб" },
  { value: "Drinks", label: "Напитки" },
  { value: "Frozen", label: "Заморозка" },
  { value: "Other", label: "Другое" },
] as const

export function getProductCategoryLabel(category: string) {
  return productCategoryOptions.find((option) => option.value === category)?.label ?? category
}

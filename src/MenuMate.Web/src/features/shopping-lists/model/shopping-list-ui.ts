import type {
  ShoppingListItem,
  ShoppingListItemRequest,
} from "@/features/shopping-lists/api/shopping-lists.api"
import { defaultShoppingCategory, defaultShoppingUnit } from "@/shared/config/shopping-taxonomy"
import { z } from "zod"

const positiveDecimalPattern = /^(?:\d+(?:[,.]\d+)?|[,.]\d+)$/

export interface ShoppingItemFormValues {
  productId: string
  name: string
  amount: string
  unit: string
  quantityKind: string
  category: string
  comment: string
}

export interface ShoppingItemDefaults {
  unit?: string
  category?: string
}

export const shoppingItemFormSchema = z
  .object({
    productId: z.string().trim(),
    name: z
      .string()
      .trim()
      .min(1, "Введите название продукта.")
      .max(160, "Название слишком длинное."),
    amount: z.string().trim(),
    unit: z.string().trim().min(1, "Выберите единицу."),
    quantityKind: z.string().trim().min(1, "Выберите тип количества."),
    category: z.string().trim().min(1, "Выберите категорию."),
    comment: z.string().trim(),
  })
  .superRefine((values, context) => {
    if (!values.amount || values.quantityKind === "ToTaste") {
      return
    }

    const amount = parseDecimalInput(values.amount)
    if (amount === null || amount <= 0 || !positiveDecimalPattern.test(values.amount)) {
      context.addIssue({
        code: "custom",
        path: ["amount"],
        message: "Введите число больше 0.",
      })
    }
  })

export function getDefaultShoppingItemFormValues(
  defaults: ShoppingItemDefaults = {},
): ShoppingItemFormValues {
  return {
    productId: "",
    name: "",
    amount: "",
    unit: defaults.unit ?? defaultShoppingUnit,
    quantityKind: "Exact",
    category: defaults.category ?? defaultShoppingCategory,
    comment: "",
  }
}

export function toShoppingListItemFormValues(item: ShoppingListItem): ShoppingItemFormValues {
  return {
    productId: item.productId,
    name: item.name,
    amount: item.amount === null ? "" : String(item.amount),
    unit: item.unit,
    quantityKind: item.quantityKind,
    category: item.category,
    comment: item.comment ?? "",
  }
}

export function toShoppingListItemRequest(values: ShoppingItemFormValues): ShoppingListItemRequest {
  const amountText = values.amount.trim()

  return {
    productId: normalizeOptionalText(values.productId),
    name: values.name.trim(),
    amount: amountText.length > 0 ? parseDecimalInput(amountText) : null,
    unit: values.unit,
    quantityKind: values.quantityKind,
    category: values.category,
    comment: normalizeOptionalText(values.comment),
  }
}

export function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value))
}

function normalizeOptionalText(value: string) {
  const normalized = value.trim()
  return normalized.length > 0 ? normalized : null
}

function parseDecimalInput(value: string) {
  if (!value) {
    return null
  }

  const amount = Number(value.replace(",", "."))

  return Number.isFinite(amount) ? amount : null
}

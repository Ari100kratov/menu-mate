import type {
  ShoppingListItem,
  ShoppingListItemRequest,
} from "@/features/shopping-lists/api/shopping-lists.api"
import type {
  ProductLineEditorErrors,
  ProductLineEditorValue,
} from "@/features/products/model/product-line"
import { defaultShoppingCategory, defaultShoppingUnit } from "@/shared/config/shopping-taxonomy"
import { z } from "zod"

const positiveDecimalPattern = /^(?:\d+(?:[,.]\d+)?|[,.]\d+)$/

export type ShoppingItemFormValues = ProductLineEditorValue
export type ShoppingItemFieldErrors = ProductLineEditorErrors

export interface ShoppingItemDefaults {
  unit?: string
  category?: string
}

export const shoppingItemFormSchema = z
  .object({
    productId: z.string().trim(),
    productName: z
      .string()
      .trim()
      .min(1, "Введите название продукта.")
      .max(160, "Название слишком длинное."),
    amount: z.string().trim(),
    unit: z.string().trim().min(1, "Выберите единицу."),
    isToTaste: z.boolean(),
    category: z.string().trim().min(1, "Выберите категорию."),
    comment: z.string().trim(),
  })
  .superRefine((values, context) => {
    if (values.isToTaste) {
      return
    }

    if (!values.amount) {
      context.addIssue({
        code: "custom",
        path: ["amount"],
        message: "Укажите количество или отметьте «по вкусу».",
      })
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
    productName: "",
    amount: "",
    unit: defaults.unit ?? defaultShoppingUnit,
    isToTaste: false,
    category: defaults.category ?? defaultShoppingCategory,
    comment: "",
  }
}

export function toShoppingListItemFormValues(item: ShoppingListItem): ShoppingItemFormValues {
  return {
    productId: item.productId,
    productName: item.name,
    amount: item.amount === null ? "" : String(item.amount),
    unit: item.unit,
    isToTaste: item.unit === "ToTaste",
    category: item.category,
    comment: item.comment ?? "",
  }
}

export function toShoppingListItemRequest(values: ShoppingItemFormValues): ShoppingListItemRequest {
  const amountText = values.amount.trim()

  return {
    productId: normalizeOptionalText(values.productId),
    name: values.productName.trim(),
    amount: values.isToTaste ? null : amountText.length > 0 ? parseDecimalInput(amountText) : null,
    unit: values.isToTaste ? "ToTaste" : values.unit,
    category: values.category,
    comment: normalizeOptionalText(values.comment),
  }
}

export function validateShoppingItem(values: ShoppingItemFormValues): ShoppingItemFieldErrors {
  const result = shoppingItemFormSchema.safeParse(values)

  if (result.success) {
    return {}
  }

  return result.error.issues.reduce<ShoppingItemFieldErrors>((errors, issue) => {
    const fieldName = issue.path[0]

    if (typeof fieldName === "string" && !(fieldName in errors)) {
      errors[fieldName as keyof ShoppingItemFieldErrors] = issue.message
    }

    return errors
  }, {})
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

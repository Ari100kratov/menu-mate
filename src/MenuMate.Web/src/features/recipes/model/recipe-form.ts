import { z } from "zod"

import type {
  CreateRecipeRequest,
  Recipe,
  UpdateRecipeRequest,
} from "@/features/recipes/api/recipes.api"
import { parseTagValues } from "@/features/tags/model/tag-values"

const positiveDecimalPattern = /^(?:\d+(?:[,.]\d+)?|[,.]\d+)$/
const sourceUrlSchema = z.url("Введите абсолютный URL источника.")

const recipeIngredientSchema = z
  .object({
    productName: z.string().trim().min(1, "Укажите продукт."),
    amount: z.string().trim(),
    unit: z.string().trim().min(1, "Выберите единицу."),
    quantityKind: z.string().trim().min(1, "Выберите тип количества."),
    category: z.string().trim().min(1, "Выберите категорию."),
    comment: z.string().trim(),
    isOptional: z.boolean(),
  })
  .superRefine((ingredient, context) => {
    if (ingredient.quantityKind === "ToTaste") {
      return
    }

    if (!ingredient.amount) {
      context.addIssue({
        code: "custom",
        path: ["amount"],
        message: "Укажите количество или выберите «по вкусу».",
      })
      return
    }

    const amount = parseDecimalInput(ingredient.amount)
    if (amount === null || amount <= 0 || !positiveDecimalPattern.test(ingredient.amount)) {
      context.addIssue({
        code: "custom",
        path: ["amount"],
        message: "Введите число больше 0.",
      })
    }
  })

export const recipeFormSchema = z.object({
  title: z.string().trim().min(1, "Введите название.").max(160, "Название слишком длинное."),
  description: z.string().trim(),
  servings: z
    .string()
    .trim()
    .refine(
      (value) => Number.isInteger(Number(value)) && Number(value) >= 1 && Number(value) <= 100,
      {
        message: "Введите количество порций от 1 до 100.",
      },
    ),
  sourceUrl: z
    .string()
    .trim()
    .refine((value) => value.length === 0 || sourceUrlSchema.safeParse(value).success, {
      message: "Введите абсолютный URL источника.",
    }),
  ingredients: z.array(recipeIngredientSchema).min(1, "Добавьте хотя бы один ингредиент."),
  steps: z
    .array(
      z.object({
        text: z.string().trim().min(1, "Опишите шаг приготовления."),
      }),
    )
    .min(1, "Добавьте хотя бы один шаг."),
  tags: z.string().trim(),
})

export type RecipeFormValues = z.infer<typeof recipeFormSchema>
export type RecipeIngredientFormValues = RecipeFormValues["ingredients"][number]
export type RecipeStepFormValues = RecipeFormValues["steps"][number]

export function createEmptyRecipeFormValues(): RecipeFormValues {
  return {
    title: "",
    description: "",
    servings: "2",
    sourceUrl: "",
    ingredients: [createEmptyIngredientFormValues()],
    steps: [createEmptyStepFormValues()],
    tags: "",
  }
}

export function createEmptyIngredientFormValues(): RecipeIngredientFormValues {
  return {
    productName: "",
    amount: "",
    unit: "Gram",
    quantityKind: "Exact",
    category: "Other",
    comment: "",
    isOptional: false,
  }
}

export function createEmptyStepFormValues(): RecipeStepFormValues {
  return {
    text: "",
  }
}

export function recipeToFormValues(recipe: Recipe): RecipeFormValues {
  return {
    title: recipe.title,
    description: recipe.description ?? "",
    servings: String(recipe.servings),
    sourceUrl: recipe.sourceUrl ?? "",
    ingredients: recipe.ingredients.map((ingredient) => ({
      productName: ingredient.productName,
      amount: ingredient.amount === null ? "" : String(ingredient.amount),
      unit: ingredient.unit,
      quantityKind: ingredient.quantityKind,
      category: ingredient.category,
      comment: ingredient.comment ?? "",
      isOptional: ingredient.isOptional,
    })),
    steps: recipe.steps.map((step) => ({
      text: step.text,
    })),
    tags: recipe.tags.join(", "),
  }
}

export function toRecipeRequest(
  values: RecipeFormValues,
): CreateRecipeRequest | UpdateRecipeRequest {
  return {
    title: values.title.trim(),
    description: normalizeOptionalString(values.description),
    servings: Number(values.servings),
    sourceUrl: normalizeOptionalString(values.sourceUrl),
    ingredients: values.ingredients.map((ingredient) => ({
      productName: ingredient.productName.trim(),
      amount:
        ingredient.quantityKind === "ToTaste" ? null : parseDecimalInput(ingredient.amount.trim()),
      unit: ingredient.quantityKind === "ToTaste" ? "ToTaste" : ingredient.unit,
      quantityKind: ingredient.quantityKind,
      category: ingredient.category,
      comment: normalizeOptionalString(ingredient.comment),
      isOptional: ingredient.isOptional,
    })),
    steps: values.steps.map((step) => ({
      text: step.text.trim(),
    })),
    tags: parseTagValues(values.tags),
  }
}

function normalizeOptionalString(value: string) {
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

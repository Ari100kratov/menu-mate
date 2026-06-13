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
    ingredientId: z.string().trim(),
    productName: z.string().trim().min(1, "Укажите продукт."),
    amount: z.string().trim(),
    unit: z.string().trim().min(1, "Выберите единицу."),
    isToTaste: z.boolean(),
    category: z.string().trim().min(1, "Выберите категорию."),
    comment: z.string().trim(),
    isOptional: z.boolean(),
  })
  .superRefine((ingredient, context) => {
    if (ingredient.isToTaste) {
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

export const recipeFormSchema = z
  .object({
    title: z.string().trim().min(1, "Введите название.").max(160, "Название слишком длинное."),
    description: z.string().trim(),
    category: z.string().trim().min(1, "Выберите категорию блюда."),
    visibility: z.enum(["Private", "Public"]),
    servings: z
      .string()
      .trim()
      .refine(
        (value) => Number.isInteger(Number(value)) && Number(value) >= 1 && Number(value) <= 100,
        {
          message: "Введите количество порций от 1 до 100.",
        },
      ),
    totalTimeMinutes: optionalPositiveInteger("Введите общее время в минутах."),
    activeTimeMinutes: optionalPositiveInteger("Введите активное время в минутах."),
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
  .superRefine((recipe, context) => {
    const totalTime = parseOptionalInteger(recipe.totalTimeMinutes)
    const activeTime = parseOptionalInteger(recipe.activeTimeMinutes)

    if (totalTime !== null && activeTime !== null && activeTime > totalTime) {
      context.addIssue({
        code: "custom",
        path: ["activeTimeMinutes"],
        message: "Активное время не может быть больше общего.",
      })
    }
  })

export type RecipeFormValues = z.infer<typeof recipeFormSchema>
export type RecipeIngredientFormValues = RecipeFormValues["ingredients"][number]
export type RecipeStepFormValues = RecipeFormValues["steps"][number]
export type RecipeIngredientFieldErrors = Partial<
  Record<Extract<keyof RecipeIngredientFormValues, string>, string>
>

export function createEmptyRecipeFormValues(): RecipeFormValues {
  return {
    title: "",
    description: "",
    category: "MainCourse",
    visibility: "Public",
    servings: "2",
    totalTimeMinutes: "",
    activeTimeMinutes: "",
    sourceUrl: "",
    ingredients: [],
    steps: [createEmptyStepFormValues()],
    tags: "",
  }
}

export function getRecipeIngredientErrorMessages(ingredient: RecipeIngredientFormValues) {
  return Object.values(validateRecipeIngredient(ingredient))
}

export function validateRecipeIngredient(
  ingredient: RecipeIngredientFormValues,
): RecipeIngredientFieldErrors {
  const result = recipeIngredientSchema.safeParse(ingredient)

  if (result.success) {
    return {}
  }

  return result.error.issues.reduce<RecipeIngredientFieldErrors>((errors, issue) => {
    const fieldName = issue.path[0]

    if (typeof fieldName === "string" && !(fieldName in errors)) {
      errors[fieldName as keyof RecipeIngredientFieldErrors] = issue.message
    }

    return errors
  }, {})
}

export function createEmptyIngredientFormValues(): RecipeIngredientFormValues {
  return {
    ingredientId: "",
    productName: "",
    amount: "",
    unit: "Gram",
    isToTaste: false,
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
    category: recipe.category,
    visibility: recipe.visibility as "Private" | "Public",
    servings: String(recipe.servings),
    totalTimeMinutes: recipe.totalTimeMinutes === null ? "" : String(recipe.totalTimeMinutes),
    activeTimeMinutes: recipe.activeTimeMinutes === null ? "" : String(recipe.activeTimeMinutes),
    sourceUrl: recipe.sourceUrl ?? "",
    ingredients: recipe.ingredients.map((ingredient) => ({
      ingredientId: ingredient.ingredientId ?? "",
      productName: ingredient.productName,
      amount: ingredient.amount === null ? "" : String(ingredient.amount),
      unit: ingredient.unit,
      isToTaste: ingredient.unit === "ToTaste",
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
    category: values.category,
    visibility: values.visibility,
    totalTimeMinutes: parseOptionalInteger(values.totalTimeMinutes),
    activeTimeMinutes: parseOptionalInteger(values.activeTimeMinutes),
    sourceUrl: normalizeOptionalString(values.sourceUrl),
    ingredients: values.ingredients.map((ingredient) => ({
      ingredientId: normalizeOptionalString(ingredient.ingredientId),
      productName: ingredient.productName.trim(),
      amount:
        ingredient.isToTaste ? null : parseDecimalInput(ingredient.amount.trim()),
      unit: ingredient.isToTaste ? "ToTaste" : ingredient.unit,
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

function optionalPositiveInteger(message: string) {
  return z
    .string()
    .trim()
    .refine(
      (value) =>
        value.length === 0 ||
        (Number.isInteger(Number(value)) && Number(value) >= 1 && Number(value) <= 10080),
      { message },
    )
}

function parseOptionalInteger(value: string) {
  const normalized = value.trim()
  return normalized ? Number(normalized) : null
}

import type {
  CreateMenuPlanItemRequest,
  MenuPlan,
  MenuPlanItem,
} from "@/features/menu-planning/api/menu-plans.api"
import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { z } from "zod"

const servingsSchema = z
  .string()
  .trim()
  .refine(
    (value) => Number.isInteger(Number(value)) && Number(value) >= 1 && Number(value) <= 100,
    {
      message: "Введите количество порций от 1 до 100.",
    },
  )

export interface CreatePlanFormValues {
  name: string
  startDate: string
  endDate: string
}

export interface MenuPlanItemFormValues {
  date: string
  mealType: string
  recipeId: string
  text: string
  servings: string
  comment: string
}

export interface RecipeOption {
  id: string
  title: string
}

export const createMenuPlanFormSchema = z
  .object({
    name: z.string().trim().min(1, "Введите название плана.").max(160, "Название слишком длинное."),
    startDate: z.string().trim().min(1, "Укажите дату начала."),
    endDate: z.string().trim().min(1, "Укажите дату окончания."),
  })
  .superRefine((values, context) => {
    if (!values.startDate || !values.endDate) {
      return
    }

    if (values.endDate < values.startDate) {
      context.addIssue({
        code: "custom",
        path: ["endDate"],
        message: "Дата окончания должна быть не раньше даты начала.",
      })
    }
  })

export function createMenuPlanItemFormSchema(menuPlan: MenuPlan) {
  return z
    .object({
      date: z.string().trim().min(1, "Укажите дату."),
      mealType: z.string().trim().min(1, "Выберите прием пищи."),
      recipeId: z.string().trim(),
      text: z.string().trim(),
      servings: servingsSchema,
      comment: z.string().trim(),
    })
    .superRefine((values, context) => {
      if (values.date < menuPlan.startDate || values.date > menuPlan.endDate) {
        context.addIssue({
          code: "custom",
          path: ["date"],
          message: "Дата должна попадать в выбранный план.",
        })
      }

      if (!values.recipeId && !values.text.trim()) {
        context.addIssue({
          code: "custom",
          path: ["text"],
          message: "Выберите рецепт или заполните свободный текст.",
        })
      }
    })
}

export function toMenuPlanItemFormValues(item: MenuPlanItem): MenuPlanItemFormValues {
  return {
    date: item.date,
    mealType: item.mealType,
    recipeId: item.recipeId ?? "",
    text: item.text ?? "",
    servings: String(item.servings),
    comment: item.comment ?? "",
  }
}

export function getDefaultMenuPlanItemValues(menuPlan: MenuPlan): MenuPlanItemFormValues {
  return getMenuPlanItemPreset(menuPlan.startDate, "Dinner")
}

export function getMenuPlanItemPreset(date: string, mealType: string): MenuPlanItemFormValues {
  return {
    date,
    mealType,
    recipeId: "",
    text: "",
    servings: "2",
    comment: "",
  }
}

export function toMenuPlanItemRequest(
  values: MenuPlanItemFormValues,
  recipes: readonly RecipeOption[],
): CreateMenuPlanItemRequest {
  const recipe = recipes.find((candidate) => candidate.id === values.recipeId)
  const text = values.text.trim()
  const comment = values.comment.trim()

  return {
    date: values.date,
    mealType: values.mealType,
    recipeId: recipe?.id ?? null,
    recipeTitle: recipe?.title ?? null,
    text: recipe ? null : text.length > 0 ? text : null,
    servings: Number(values.servings),
    comment: comment.length > 0 ? comment : null,
  }
}

export function getFilteredRecipeOptions(
  recipes: readonly RecipeOption[],
  search: string,
  selectedRecipeId: string,
) {
  const normalizedSearch = search.trim().toLocaleLowerCase("ru-RU")
  const selectedRecipe = recipes.find((recipe) => recipe.id === selectedRecipeId)
  const filteredRecipes = normalizedSearch
    ? recipes.filter((recipe) => recipe.title.toLocaleLowerCase("ru-RU").includes(normalizedSearch))
    : [...recipes]

  if (selectedRecipe && !filteredRecipes.some((recipe) => recipe.id === selectedRecipe.id)) {
    return [selectedRecipe, ...filteredRecipes]
  }

  return filteredRecipes
}

export function getRecipeOptions(
  recipes: readonly RecipeListItem[],
  values: MenuPlanItemFormValues,
): RecipeOption[] {
  if (!values.recipeId || recipes.some((recipe) => recipe.id === values.recipeId)) {
    return [...recipes]
  }

  return [
    {
      id: values.recipeId,
      title: "Сохраненный рецепт",
    },
    ...recipes,
  ]
}

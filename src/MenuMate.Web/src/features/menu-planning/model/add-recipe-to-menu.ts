import type { CreateMenuPlanItemRequest } from "@/features/menu-planning/api/menu-plans.api"
import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { z } from "zod"

export interface AddRecipeToMenuRecipe {
  id: string
  currentRevisionId: string
  title: string
  servings: number | string
}

export interface AddRecipeToMenuFormValues {
  date: string
  mealType: string
  servings: string
  comment: string
}

export function createAddRecipeToMenuFormSchema(menuPlan: MenuPlan) {
  return z
    .object({
      date: z.string().trim().min(1, "Укажите дату."),
      mealType: z.string().trim().min(1, "Выберите прием пищи."),
      servings: z
        .string()
        .trim()
        .refine(
          (value) => Number.isInteger(Number(value)) && Number(value) >= 1 && Number(value) <= 100,
          {
            message: "Введите количество порций от 1 до 100.",
          },
        ),
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
    })
}

export function toAddRecipeMenuItemRequest(
  values: AddRecipeToMenuFormValues,
  recipe: AddRecipeToMenuRecipe,
): CreateMenuPlanItemRequest {
  const comment = values.comment.trim()

  return {
    date: values.date,
    mealType: values.mealType,
    recipeId: recipe.id,
    recipeRevisionId: recipe.currentRevisionId,
    recipeTitle: recipe.title,
    text: null,
    servings: Number(values.servings),
    comment: comment.length > 0 ? comment : null,
  }
}

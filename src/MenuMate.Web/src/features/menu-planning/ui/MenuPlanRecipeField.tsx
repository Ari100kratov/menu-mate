import { Search } from "lucide-react"
import { useMemo, useState } from "react"

import {
  getFilteredRecipeOptions,
  getRecipeOptions,
  type MenuPlanItemFormValues,
} from "@/features/menu-planning/model/menu-plan-form"
import type { MenuPlanItemFormApi } from "@/features/menu-planning/ui/useMenuPlanItemForm"
import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { Field, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

const NO_RECIPE_SELECT_VALUE = "__no_recipe__"

interface MenuPlanRecipeFieldProps {
  form: MenuPlanItemFormApi
  recipes: readonly RecipeListItem[]
  initialValues: MenuPlanItemFormValues
}

export function MenuPlanRecipeField({ form, recipes, initialValues }: MenuPlanRecipeFieldProps) {
  const recipeOptions = useMemo(
    () => getRecipeOptions(recipes, initialValues),
    [initialValues, recipes],
  )
  const [recipeSearch, setRecipeSearch] = useState("")
  const filteredRecipeOptions = useMemo(
    () => getFilteredRecipeOptions(recipeOptions, recipeSearch, initialValues.recipeId),
    [initialValues.recipeId, recipeOptions, recipeSearch],
  )

  return (
    <form.Field name="recipeId">
      {(field) => (
        <Field className="md:col-span-3">
          <FieldLabel htmlFor={field.name}>Рецепт</FieldLabel>
          <div className="relative">
            <Search className="text-muted-foreground absolute top-1/2 left-3 size-4 -translate-y-1/2" />
            <Input
              className="pl-9"
              value={recipeSearch}
              placeholder="Быстрый поиск рецепта"
              onChange={(event) => {
                setRecipeSearch(event.target.value)
              }}
            />
          </div>
          <Select
            name={field.name}
            value={field.state.value || NO_RECIPE_SELECT_VALUE}
            onValueChange={(value) => {
              const recipeId = value === NO_RECIPE_SELECT_VALUE ? "" : value
              const recipe = recipeOptions.find((option) => option.id === recipeId)
              field.handleChange(recipeId)
              form.setFieldValue("recipeRevisionId", recipe?.currentRevisionId ?? "")
            }}
          >
            <SelectTrigger id={field.name} className="w-full" onBlur={field.handleBlur}>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={NO_RECIPE_SELECT_VALUE}>Без рецепта</SelectItem>
              {filteredRecipeOptions.map((recipe) => (
                <SelectItem key={recipe.id} value={recipe.id}>
                  {recipe.title}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
      )}
    </form.Field>
  )
}

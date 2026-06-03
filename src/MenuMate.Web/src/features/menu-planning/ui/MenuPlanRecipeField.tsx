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
import { Select } from "@/shared/ui/select"

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
            id={field.name}
            name={field.name}
            value={field.state.value}
            onBlur={field.handleBlur}
            onChange={(event) => {
              field.handleChange(event.target.value)
            }}
          >
            <option value="">Без рецепта</option>
            {filteredRecipeOptions.map((recipe) => (
              <option key={recipe.id} value={recipe.id}>
                {recipe.title}
              </option>
            ))}
          </Select>
        </Field>
      )}
    </form.Field>
  )
}

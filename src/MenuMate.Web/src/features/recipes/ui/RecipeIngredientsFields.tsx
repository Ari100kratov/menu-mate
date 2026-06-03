import { Plus } from "lucide-react"

import { createEmptyIngredientFormValues } from "@/features/recipes/model/recipe-form"
import { RecipeIngredientCard } from "@/features/recipes/ui/RecipeIngredientCard"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { FieldError } from "@/shared/ui/field"

interface RecipeIngredientsFieldsProps {
  form: RecipeFormApi
}

export function RecipeIngredientsFields({ form }: RecipeIngredientsFieldsProps) {
  return (
    <form.Field name="ingredients" mode="array">
      {(field) => (
        <section className="space-y-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold tracking-normal">Ингредиенты</h2>
              <FieldError errors={field.state.meta.errors} />
            </div>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                field.pushValue(createEmptyIngredientFormValues())
              }}
            >
              <Plus />
              Добавить
            </Button>
          </div>

          <div className="space-y-3">
            {field.state.value.map((_ingredient, index) => (
              <RecipeIngredientCard
                key={index}
                form={form}
                index={index}
                canRemove={field.state.value.length > 1}
                onRemove={() => {
                  field.removeValue(index)
                }}
              />
            ))}
          </div>
        </section>
      )}
    </form.Field>
  )
}

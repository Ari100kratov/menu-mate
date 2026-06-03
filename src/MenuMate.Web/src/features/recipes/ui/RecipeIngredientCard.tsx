import { Trash2 } from "lucide-react"

import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { RecipeIngredientMetaFields } from "./RecipeIngredientMetaFields"
import { RecipeIngredientNameField } from "./RecipeIngredientNameField"
import { RecipeIngredientQuantityFields } from "./RecipeIngredientQuantityFields"

interface RecipeIngredientCardProps {
  form: RecipeFormApi
  index: number
  canRemove: boolean
  onRemove: () => void
}

export function RecipeIngredientCard({
  form,
  index,
  canRemove,
  onRemove,
}: RecipeIngredientCardProps) {
  return (
    <div className="rounded-md border p-4">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h3 className="font-medium tracking-normal">Ингредиент {index + 1}</h3>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Удалить ингредиент"
          disabled={!canRemove}
          onClick={onRemove}
        >
          <Trash2 />
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-6">
        <RecipeIngredientNameField form={form} index={index} />
        <RecipeIngredientQuantityFields form={form} index={index} />
        <RecipeIngredientMetaFields form={form} index={index} />
      </div>
    </div>
  )
}

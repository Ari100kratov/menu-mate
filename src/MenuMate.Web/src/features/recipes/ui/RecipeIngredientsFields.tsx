import { CircleDashed, MessageSquareText, Pencil, Plus, Trash2 } from "lucide-react"
import { useState } from "react"

import {
  getRecipeIngredientErrorMessages,
  type RecipeIngredientFormValues,
} from "@/features/recipes/model/recipe-form"
import { getProductCategoryLabel } from "@/features/recipes/model/recipe-form-options"
import { formatRecipeIngredientQuantity } from "@/features/recipes/model/recipe-ingredient-format"
import { RecipeIngredientDialog } from "@/features/recipes/ui/RecipeIngredientDialog"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { FieldError } from "@/shared/ui/field"
import { PageSection } from "@/shared/ui/page"

interface RecipeIngredientsFieldsProps {
  form: RecipeFormApi
  showValidationErrors: boolean
}

export function RecipeIngredientsFields({
  form,
  showValidationErrors,
}: RecipeIngredientsFieldsProps) {
  const [editor, setEditor] = useState<{ mode: "create" } | { mode: "edit"; index: number } | null>(
    null,
  )

  return (
    <form.Field name="ingredients" mode="array">
      {(field) => {
        return (
          <div>
            <PageSection
              title="Ингредиенты"
              description="Добавляйте продукты по одному. Категория каталога подставится автоматически."
              className="rounded-none border-0 border-b p-4 md:p-6"
            >
              <FieldError errors={field.state.meta.errors} />

              {field.state.value.length === 0 ? (
                <button
                  type="button"
                  className="border-input hover:border-primary/50 hover:bg-muted/40 flex w-full flex-col items-center gap-2 rounded-xl border border-dashed px-4 py-8 text-center transition-colors"
                  onClick={() => {
                    setEditor({ mode: "create" })
                  }}
                >
                  <span className="bg-primary/10 text-primary flex size-10 items-center justify-center rounded-full">
                    <Plus className="size-5" />
                  </span>
                  <span className="type-subsection-title">Добавьте первый ингредиент</span>
                  <span className="type-supporting text-muted-foreground">
                    Он появится в списке только после заполнения и подтверждения.
                  </span>
                </button>
              ) : (
                <>
                  <div className="space-y-2">
                    {field.state.value.map((ingredient, index) => (
                      <IngredientRow
                        key={index}
                        ingredient={ingredient}
                        index={index}
                        showValidationError={showValidationErrors}
                        onEdit={() => {
                          setEditor({ mode: "edit", index })
                        }}
                        onRemove={() => {
                          field.removeValue(index)
                        }}
                      />
                    ))}
                  </div>
                  <Button
                    type="button"
                    variant="outline"
                    className="w-full border-dashed"
                    onClick={() => {
                      setEditor({ mode: "create" })
                    }}
                  >
                    <Plus className="size-4" />
                    Добавить ингредиент
                  </Button>
                </>
              )}

              {editor ? (
                <RecipeIngredientDialog
                  key={editor.mode === "edit" ? `edit-${String(editor.index)}` : "create"}
                  open
                  mode={editor.mode}
                  initialValue={
                    editor.mode === "edit" ? field.state.value[editor.index] : undefined
                  }
                  onOpenChange={(open) => {
                    if (!open) {
                      setEditor(null)
                    }
                  }}
                  onSave={(ingredient) => {
                    if (editor.mode === "edit") {
                      field.replaceValue(editor.index, ingredient)
                      return
                    }

                    field.pushValue(ingredient)
                  }}
                />
              ) : null}
            </PageSection>
          </div>
        )
      }}
    </form.Field>
  )
}

function IngredientRow({
  ingredient,
  index,
  showValidationError,
  onEdit,
  onRemove,
}: {
  ingredient: RecipeIngredientFormValues
  index: number
  showValidationError: boolean
  onEdit: () => void
  onRemove: () => void
}) {
  const errorMessages = showValidationError ? getRecipeIngredientErrorMessages(ingredient) : []
  const isInvalid = errorMessages.length > 0

  return (
    <div
      className="data-[invalid=true]:border-destructive/70 data-[invalid=true]:bg-destructive/5 flex min-h-16 items-center gap-3 rounded-lg border px-3 py-2"
      data-invalid={isInvalid}
      tabIndex={isInvalid ? -1 : undefined}
      aria-label={isInvalid ? `Ингредиент ${String(index + 1)}: ${errorMessages[0]}` : undefined}
    >
      <button
        type="button"
        className="min-w-0 flex-1 text-left outline-none"
        aria-invalid={isInvalid}
        onClick={onEdit}
      >
        <span className="flex min-w-0 flex-wrap items-center gap-x-2 gap-y-1">
          <span className="type-body truncate font-medium">
            {ingredient.productName || `Ингредиент ${String(index + 1)}`}
          </span>
          {ingredient.isOptional ? (
            <span className="type-supporting bg-accent text-accent-foreground inline-flex items-center gap-1 rounded-full px-2 py-0.5">
              <CircleDashed className="size-3.5" />
              Можно пропустить
            </span>
          ) : null}
        </span>
        <span className="mt-0.5 flex min-w-0 flex-wrap items-center gap-x-2 gap-y-1">
          <span
            className={
              isInvalid
                ? "type-supporting text-destructive"
                : "type-supporting text-muted-foreground"
            }
          >
            {errorMessages[0] ?? formatRecipeIngredientQuantity(ingredient)}
          </span>
          <span className="text-muted-foreground/50">·</span>
          <span className="type-supporting text-muted-foreground">
            {getProductCategoryLabel(ingredient.category)}
          </span>
        </span>
        {ingredient.comment ? (
          <span className="type-supporting text-muted-foreground mt-1 flex items-center gap-1.5">
            <MessageSquareText className="size-3.5 shrink-0" />
            <span className="truncate">{ingredient.comment}</span>
          </span>
        ) : null}
      </button>
      <Button type="button" variant="ghost" size="icon-sm" aria-label="Изменить" onClick={onEdit}>
        <Pencil />
      </Button>
      <Button type="button" variant="ghost" size="icon-sm" aria-label="Удалить" onClick={onRemove}>
        <Trash2 />
      </Button>
    </div>
  )
}

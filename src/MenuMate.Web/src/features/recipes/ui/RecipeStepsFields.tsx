import { Plus, Trash2 } from "lucide-react"

import { createEmptyStepFormValues } from "@/features/recipes/model/recipe-form"
import { stepFieldName } from "@/features/recipes/ui/recipe-form-fields"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Textarea } from "@/shared/ui/textarea"

interface RecipeStepsFieldsProps {
  form: RecipeFormApi
}

export function RecipeStepsFields({ form }: RecipeStepsFieldsProps) {
  return (
    <form.Field name="steps" mode="array">
      {(field) => (
        <section className="space-y-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold tracking-normal">Шаги</h2>
              <FieldError errors={field.state.meta.errors} />
            </div>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                field.pushValue(createEmptyStepFormValues())
              }}
            >
              <Plus />
              Добавить
            </Button>
          </div>

          <div className="space-y-3">
            {field.state.value.map((_step, index) => (
              <RecipeStepCard
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

function RecipeStepCard({
  form,
  index,
  canRemove,
  onRemove,
}: {
  form: RecipeFormApi
  index: number
  canRemove: boolean
  onRemove: () => void
}) {
  return (
    <div className="rounded-md border p-4">
      <div className="mb-3 flex items-center justify-between gap-3">
        <h3 className="font-medium tracking-normal">Шаг {index + 1}</h3>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Удалить шаг"
          disabled={!canRemove}
          onClick={onRemove}
        >
          <Trash2 />
        </Button>
      </div>

      <form.Field name={stepFieldName(index, "text")}>
        {(stepField) => {
          const isInvalid = stepField.state.meta.isTouched && !stepField.state.meta.isValid

          return (
            <Field data-invalid={isInvalid}>
              <FieldLabel htmlFor={stepField.name}>Описание шага</FieldLabel>
              <Textarea
                id={stepField.name}
                name={stepField.name}
                value={stepField.state.value}
                onBlur={stepField.handleBlur}
                onChange={(event) => {
                  stepField.handleChange(event.target.value)
                }}
                aria-invalid={isInvalid}
              />
              {isInvalid ? <FieldError errors={stepField.state.meta.errors} /> : null}
            </Field>
          )
        }}
      </form.Field>
    </div>
  )
}

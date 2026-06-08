import { Plus, Trash2 } from "lucide-react"

import { createEmptyStepFormValues } from "@/features/recipes/model/recipe-form"
import { stepFieldName } from "@/features/recipes/ui/recipe-form-fields"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Textarea } from "@/shared/ui/textarea"
import { PageSection } from "@/shared/ui/page"

interface RecipeStepsFieldsProps {
  form: RecipeFormApi
}

export function RecipeStepsFields({ form }: RecipeStepsFieldsProps) {
  return (
    <form.Field name="steps" mode="array">
      {(field) => {
        function addStep() {
          field.pushValue(createEmptyStepFormValues())
        }

        return (
          <PageSection
            title="Шаги приготовления"
            className="rounded-none border-0 border-b p-4 md:p-6"
          >
            <FieldError errors={field.state.meta.errors} />

            <div className="divide-y">
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

            <Button
              type="button"
              variant="outline"
              className="w-full border-dashed"
              onClick={addStep}
            >
              <Plus className="size-4" />
              Добавить шаг
            </Button>
          </PageSection>
        )
      }}
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
    <div className="grid grid-cols-[2rem_minmax(0,1fr)_2rem] gap-3 py-3">
      <div className="bg-secondary text-secondary-foreground flex size-8 items-center justify-center rounded-full text-sm font-semibold">
        {index + 1}
      </div>

      <form.Field name={stepFieldName(index, "text")}>
        {(stepField) => {
          const isInvalid = stepField.state.meta.isTouched && !stepField.state.meta.isValid

          return (
            <Field data-invalid={isInvalid}>
              <FieldLabel className="sr-only" htmlFor={stepField.name}>
                Описание шага {index + 1}
              </FieldLabel>
              <Textarea
                id={stepField.name}
                name={stepField.name}
                className="min-h-20 resize-y"
                value={stepField.state.value}
                placeholder="Что нужно сделать?"
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

      <div>
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          aria-label="Удалить шаг"
          disabled={!canRemove}
          onClick={onRemove}
        >
          <Trash2 />
        </Button>
      </div>
    </div>
  )
}

import type { RecipeFormValues } from "@/features/recipes/model/recipe-form"
import { RecipeIngredientsFields } from "@/features/recipes/ui/RecipeIngredientsFields"
import { RecipeMainFields } from "@/features/recipes/ui/RecipeMainFields"
import { RecipeStepsFields } from "@/features/recipes/ui/RecipeStepsFields"
import { useRecipeForm } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"

interface RecipeFormProps {
  initialValues: RecipeFormValues
  submitLabel: string
  isSubmitting: boolean
  error?: unknown
  onSubmit: (values: RecipeFormValues) => void
}

export function RecipeForm({
  initialValues,
  submitLabel,
  isSubmitting,
  error,
  onSubmit,
}: RecipeFormProps) {
  const form = useRecipeForm({ initialValues, onSubmit })

  return (
    <form
      className="space-y-8"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      {error ? <ErrorAlert error={error} /> : null}

      <RecipeMainFields form={form} />
      <RecipeIngredientsFields form={form} />
      <RecipeStepsFields form={form} />

      <div className="flex flex-col-reverse gap-3 border-t pt-5 sm:flex-row sm:justify-end">
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Сохраняем..." : submitLabel}
        </Button>
      </div>
    </form>
  )
}

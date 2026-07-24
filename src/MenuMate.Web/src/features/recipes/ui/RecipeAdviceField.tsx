import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Field } from "@/shared/ui/field"
import { PageSection } from "@/shared/ui/page"
import { Textarea } from "@/shared/ui/textarea"

interface RecipeAdviceFieldProps {
  form: RecipeFormApi
}

export function RecipeAdviceField({ form }: RecipeAdviceFieldProps) {
  return (
    <PageSection
      title="Советы"
      className="rounded-none border-0 border-b p-4 md:p-6"
      data-recipe-form-field="advice"
    >
      <form.Field name="advice">
        {(field) => (
          <Field>
            <Textarea
              id={field.name}
              name={field.name}
              className="min-h-32"
              value={field.state.value}
              placeholder="Например, дайте блюду настояться 10 минут перед подачей."
              onBlur={field.handleBlur}
              onChange={(event) => {
                field.handleChange(event.target.value)
              }}
            />
          </Field>
        )}
      </form.Field>
    </PageSection>
  )
}

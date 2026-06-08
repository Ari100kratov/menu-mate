import { ChevronDown } from "lucide-react"

import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { TagPicker } from "@/features/tags/ui/TagPicker"
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

export function RecipeAdditionalFields({ form }: { form: RecipeFormApi }) {
  return (
    <details className="group p-4 md:p-6">
      <summary className="type-section-title flex cursor-pointer list-none items-center justify-between gap-3">
        <span>
          Дополнительно
          <span className="type-supporting text-muted-foreground ml-2">
            источник и теги
          </span>
        </span>
        <ChevronDown className="size-4 transition group-open:rotate-180" />
      </summary>

      <FieldGroup className="mt-5 grid gap-4">
        <form.Field name="sourceUrl">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Ссылка на источник</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="url"
                  inputMode="url"
                  value={field.state.value}
                  placeholder="https://..."
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    field.handleChange(event.target.value)
                  }}
                  aria-invalid={isInvalid}
                />
                {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
              </Field>
            )
          }}
        </form.Field>

        <form.Field name="tags">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>Теги</FieldLabel>
              <TagPicker
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={field.handleChange}
              />
              <FieldDescription>
                Добавьте теги, чтобы быстрее находить и фильтровать рецепты.
              </FieldDescription>
            </Field>
          )}
        </form.Field>
      </FieldGroup>
    </details>
  )
}

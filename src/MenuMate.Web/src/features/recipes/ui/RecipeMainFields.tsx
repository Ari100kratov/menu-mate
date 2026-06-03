import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { TagPicker } from "@/features/tags/ui/TagPicker"
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Textarea } from "@/shared/ui/textarea"

interface RecipeMainFieldsProps {
  form: RecipeFormApi
}

export function RecipeMainFields({ form }: RecipeMainFieldsProps) {
  return (
    <section className="space-y-4">
      <h2 className="text-lg font-semibold tracking-normal">Основное</h2>
      <FieldGroup className="grid gap-4 md:grid-cols-2">
        <form.Field name="title">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field className="md:col-span-2" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Название</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
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

        <form.Field name="servings">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Порции</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="number"
                  inputMode="numeric"
                  min={1}
                  max={100}
                  value={field.state.value}
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

        <form.Field name="sourceUrl">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Источник</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="url"
                  inputMode="url"
                  value={field.state.value}
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

        <form.Field name="description">
          {(field) => (
            <Field className="md:col-span-2">
              <FieldLabel htmlFor={field.name}>Описание</FieldLabel>
              <Textarea
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
              />
            </Field>
          )}
        </form.Field>

        <form.Field name="tags">
          {(field) => (
            <Field className="md:col-span-2">
              <FieldLabel htmlFor={field.name}>Теги</FieldLabel>
              <TagPicker
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={field.handleChange}
              />
              <FieldDescription>
                Выбирайте существующие теги или создавайте новые прямо здесь.
              </FieldDescription>
            </Field>
          )}
        </form.Field>
      </FieldGroup>
    </section>
  )
}

import { Minus, Plus } from "lucide-react"

import {
  recipeCategoryOptions,
  recipeVisibilityOptions,
} from "@/features/recipes/model/recipe-form-options"
import { RecipeDurationField } from "@/features/recipes/ui/RecipeDurationField"
import type { RecipeFormApi } from "@/features/recipes/ui/useRecipeForm"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { PageSection } from "@/shared/ui/page"
import { Button } from "@/shared/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"
import { Textarea } from "@/shared/ui/textarea"

interface RecipeMainFieldsProps {
  form: RecipeFormApi
}

export function RecipeMainFields({ form }: RecipeMainFieldsProps) {
  return (
    <PageSection title="О рецепте" className="rounded-none border-0 border-b p-4 md:p-6">
      <FieldGroup className="grid gap-4 md:grid-cols-2">
        <form.Field name="title">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field className="md:col-span-2" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Название блюда</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
                  placeholder="Например, паста с морепродуктами"
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

        <form.Field name="category">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>Категория блюда</FieldLabel>
              <Select
                name={field.name}
                value={field.state.value}
                onValueChange={field.handleChange}
              >
                <SelectTrigger id={field.name} className="w-full" onBlur={field.handleBlur}>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {recipeCategoryOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
          )}
        </form.Field>

        <form.Field name="visibility">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>Доступ</FieldLabel>
              <Select
                name={field.name}
                value={field.state.value}
                onValueChange={(value) => {
                  field.handleChange(value as "Private" | "Public")
                }}
              >
                <SelectTrigger id={field.name} className="w-full" onBlur={field.handleBlur}>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {recipeVisibilityOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <p className="type-supporting text-muted-foreground">
                {
                  recipeVisibilityOptions.find((option) => option.value === field.state.value)
                    ?.description
                }
              </p>
            </Field>
          )}
        </form.Field>

        <form.Field name="servings">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
            const servings = Number(field.state.value)

            return (
              <Field className="w-fit max-w-full" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Порции</FieldLabel>
                <div className="grid h-10 w-fit grid-cols-[2.5rem_4rem_2.5rem] items-center overflow-hidden rounded-lg border">
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="size-10 rounded-none"
                    disabled={servings <= 1}
                    aria-label="Уменьшить количество порций"
                    onClick={() => {
                      field.handleChange(String(Math.max(1, servings - 1)))
                    }}
                  >
                    <Minus />
                  </Button>
                  <Input
                    id={field.name}
                    name={field.name}
                    className="h-10 w-16 rounded-none border-x border-y-0 px-1 text-center shadow-none focus-visible:ring-0"
                    inputMode="numeric"
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(event) => {
                      field.handleChange(event.target.value)
                    }}
                    aria-invalid={isInvalid}
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="size-10 rounded-none"
                    disabled={servings >= 100}
                    aria-label="Увеличить количество порций"
                    onClick={() => {
                      field.handleChange(String(Math.min(100, servings + 1)))
                    }}
                  >
                    <Plus />
                  </Button>
                </div>
                {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
              </Field>
            )
          }}
        </form.Field>

        <div className="space-y-4 md:col-span-2">
          <h3 className="type-subsection-title">Время приготовления</h3>
          <div className="grid gap-4 md:grid-cols-2">
            <RecipeDurationField
              form={form}
              name="totalTimeMinutes"
              label="Общее время"
              hint="до подачи"
            />
            <RecipeDurationField
              form={form}
              name="activeTimeMinutes"
              label="Активное время"
              hint="ваша работа"
            />
          </div>
        </div>

        <form.Field name="description">
          {(field) => (
            <Field className="md:col-span-2">
              <FieldLabel htmlFor={field.name}>Описание</FieldLabel>
              <Textarea
                id={field.name}
                name={field.name}
                value={field.state.value}
                placeholder="Что важно знать об этом блюде"
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
              />
            </Field>
          )}
        </form.Field>
      </FieldGroup>
    </PageSection>
  )
}

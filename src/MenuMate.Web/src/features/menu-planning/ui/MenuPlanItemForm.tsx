import { ListPlus, Pencil, X } from "lucide-react"

import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { mealTypeOptions } from "@/features/menu-planning/model/menu-planning"
import type { MenuPlanItemFormValues } from "@/features/menu-planning/model/menu-plan-form"
import { MenuPlanRecipeField } from "@/features/menu-planning/ui/MenuPlanRecipeField"
import { useMenuPlanItemForm } from "@/features/menu-planning/ui/useMenuPlanItemForm"
import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { Button } from "@/shared/ui/button"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

interface MenuPlanItemFormProps {
  formId?: string
  title: string
  submitLabel: string
  submitIcon: "add" | "save"
  menuPlan: MenuPlan
  recipes: readonly RecipeListItem[]
  isSubmitting: boolean
  initialValues: MenuPlanItemFormValues
  resetAfterSubmit?: boolean
  onSubmit: (values: MenuPlanItemFormValues) => void
  onCancel?: () => void
}

export function MenuPlanItemForm({
  formId,
  title,
  submitLabel,
  submitIcon,
  menuPlan,
  recipes,
  isSubmitting,
  initialValues,
  resetAfterSubmit = false,
  onSubmit,
  onCancel,
}: MenuPlanItemFormProps) {
  const form = useMenuPlanItemForm({
    menuPlan,
    initialValues,
    resetAfterSubmit,
    onSubmit,
  })

  return (
    <form
      id={formId}
      className="space-y-4 rounded-md border p-4"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      <h3 className="font-semibold tracking-normal">{title}</h3>
      <FieldGroup className="grid gap-3 md:grid-cols-6">
        <form.Field name="date">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field className="md:col-span-2" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Дата</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="date"
                  min={menuPlan.startDate}
                  max={menuPlan.endDate}
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

        <form.Field name="mealType">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field className="md:col-span-2" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Прием пищи</FieldLabel>
                <Select
                  name={field.name}
                  value={field.state.value}
                  onValueChange={field.handleChange}
                >
                  <SelectTrigger
                    id={field.name}
                    className="w-full"
                    onBlur={field.handleBlur}
                    aria-invalid={isInvalid}
                  >
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {mealTypeOptions.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
              </Field>
            )
          }}
        </form.Field>

        <form.Field name="servings">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field className="md:col-span-2" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Порции</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="number"
                  inputMode="numeric"
                  min={1}
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

        <MenuPlanRecipeField form={form} recipes={recipes} initialValues={initialValues} />

        <form.Field name="text">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field className="md:col-span-3" data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Текст</FieldLabel>
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

        <form.Field name="comment">
          {(field) => (
            <Field className="md:col-span-6">
              <FieldLabel htmlFor={field.name}>Комментарий</FieldLabel>
              <Input
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
      </FieldGroup>

      <div className="flex flex-wrap gap-2">
        <Button type="submit" disabled={isSubmitting}>
          {submitIcon === "add" ? <ListPlus /> : <Pencil />}
          {isSubmitting ? "Сохраняем..." : submitLabel}
        </Button>
        {onCancel ? (
          <Button type="button" variant="ghost" onClick={onCancel}>
            <X />
            Отмена
          </Button>
        ) : null}
      </div>
    </form>
  )
}

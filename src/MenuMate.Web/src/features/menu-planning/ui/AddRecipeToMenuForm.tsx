import { useForm } from "@tanstack/react-form"
import { CalendarPlus } from "lucide-react"
import { Link } from "react-router-dom"

import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { useAddMenuPlanItemMutation } from "@/features/menu-planning/api/menu-plans.queries"
import {
  type AddRecipeToMenuFormValues,
  type AddRecipeToMenuRecipe,
  createAddRecipeToMenuFormSchema,
  toAddRecipeMenuItemRequest,
} from "@/features/menu-planning/model/add-recipe-to-menu"
import { mealTypeOptions } from "@/features/menu-planning/model/menu-planning"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"
import { AddRecipeToMenuPlanField } from "./AddRecipeToMenuPlanField"

interface AddRecipeToMenuFormProps {
  menuPlans: readonly MenuPlan[]
  selectedMenuPlan: MenuPlan
  recipe: AddRecipeToMenuRecipe
  onSelectedMenuPlanChange: (menuPlanId: string) => void
}

export function AddRecipeToMenuForm({
  menuPlans,
  selectedMenuPlan,
  recipe,
  onSelectedMenuPlanChange,
}: AddRecipeToMenuFormProps) {
  const addItemMutation = useAddMenuPlanItemMutation(selectedMenuPlan.id)
  const form = useForm({
    defaultValues: {
      date: selectedMenuPlan.startDate,
      mealType: "Dinner",
      servings: String(recipe.servings),
      comment: "",
    } satisfies AddRecipeToMenuFormValues,
    validators: {
      onSubmit: createAddRecipeToMenuFormSchema(selectedMenuPlan),
    },
    onSubmit: ({ value }) => {
      addItemMutation.mutate(toAddRecipeMenuItemRequest(value, recipe))
    },
  })

  return (
    <section className="space-y-4 rounded-md border p-4 md:p-5">
      <div className="space-y-1">
        <h2 className="text-lg font-semibold tracking-normal">Добавить в меню</h2>
        <p className="text-muted-foreground text-sm">
          Быстрый путь из рецепта в недельный план без перехода в общий раздел меню.
        </p>
      </div>

      {addItemMutation.error ? <ErrorAlert error={addItemMutation.error} /> : null}
      {addItemMutation.isSuccess ? (
        <p className="text-primary text-sm font-medium">Рецепт добавлен в план меню.</p>
      ) : null}

      <form
        className="space-y-4"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        <AddRecipeToMenuPlanField
          menuPlans={menuPlans}
          selectedMenuPlan={selectedMenuPlan}
          onSelectedMenuPlanChange={onSelectedMenuPlanChange}
        />

        <FieldGroup className="grid gap-3 sm:grid-cols-3">
          <form.Field name="date">
            {(field) => {
              const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>Дата</FieldLabel>
                  <Input
                    id={field.name}
                    name={field.name}
                    type="date"
                    min={selectedMenuPlan.startDate}
                    max={selectedMenuPlan.endDate}
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
                <Field data-invalid={isInvalid}>
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
                <Field data-invalid={isInvalid}>
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
        </FieldGroup>

        <form.Field name="comment">
          {(field) => (
            <Field>
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

        <div className="flex flex-col gap-2 sm:flex-row">
          <Button type="submit" disabled={addItemMutation.isPending}>
            <CalendarPlus />
            {addItemMutation.isPending ? "Добавляем..." : "Добавить в меню"}
          </Button>
          <Button asChild type="button" variant="outline">
            <Link to="/menu">Открыть меню</Link>
          </Button>
        </div>
      </form>
    </section>
  )
}

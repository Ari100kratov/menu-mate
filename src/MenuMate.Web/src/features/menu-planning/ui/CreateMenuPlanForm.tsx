import { useForm } from "@tanstack/react-form"
import { CalendarPlus } from "lucide-react"

import { addDays, getTodayDateInputValue } from "@/features/menu-planning/model/menu-planning"
import {
  createMenuPlanFormSchema,
  type CreatePlanFormValues,
} from "@/features/menu-planning/model/menu-plan-form"
import { Button } from "@/shared/ui/button"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

interface CreateMenuPlanFormProps {
  isSubmitting: boolean
  onSubmit: (values: CreatePlanFormValues) => void
}

export function CreateMenuPlanForm({ isSubmitting, onSubmit }: CreateMenuPlanFormProps) {
  const today = getTodayDateInputValue()
  const form = useForm({
    defaultValues: {
      name: "Меню на неделю",
      startDate: today,
      endDate: addDays(today, 6),
    } satisfies CreatePlanFormValues,
    validators: {
      onSubmit: createMenuPlanFormSchema,
    },
    onSubmit: ({ value }) => {
      onSubmit(value)
    },
  })

  return (
    <form
      className="grid gap-3 rounded-md border p-4 md:grid-cols-[minmax(0,1fr)_10rem_10rem_auto] md:items-end"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      <form.Field name="name">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field data-invalid={isInvalid}>
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

      <form.Field name="startDate">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Начало</FieldLabel>
              <Input
                id={field.name}
                name={field.name}
                type="date"
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

      <form.Field name="endDate">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

          return (
            <Field data-invalid={isInvalid}>
              <FieldLabel htmlFor={field.name}>Окончание</FieldLabel>
              <Input
                id={field.name}
                name={field.name}
                type="date"
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

      <Button type="submit" disabled={isSubmitting}>
        <CalendarPlus />
        {isSubmitting ? "Создаем..." : "Создать"}
      </Button>
    </form>
  )
}

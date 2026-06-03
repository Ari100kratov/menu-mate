import { useForm } from "@tanstack/react-form"
import { Link } from "react-router-dom"
import { z } from "zod"

import { useRegisterMutation } from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

const registerFormSchema = z.object({
  displayName: z
    .string()
    .trim()
    .min(1, "Введите имя.")
    .max(120, "Имя не должно быть длиннее 120 символов."),
  email: z.string().trim().pipe(z.email("Введите корректный email.")),
  password: z.string().min(8, "Пароль должен быть не короче 8 символов."),
})

type RegisterFormValues = z.infer<typeof registerFormSchema>

export default function RegisterPage() {
  const registerMutation = useRegisterMutation()
  const form = useForm({
    defaultValues: {
      displayName: "",
      email: "",
      password: "",
    } satisfies RegisterFormValues,
    validators: {
      onSubmit: registerFormSchema,
    },
    onSubmit: ({ value }) => {
      registerMutation.mutate(value)
    },
  })

  return (
    <AuthFormLayout title="Регистрация" description="Создайте рабочее пространство для меню.">
      <form
        className="space-y-4"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {registerMutation.error ? <ErrorAlert error={registerMutation.error} /> : null}

        <FieldGroup>
          <form.Field name="displayName">
            {(field) => {
              const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>Имя</FieldLabel>
                  <Input
                    id={field.name}
                    name={field.name}
                    autoComplete="name"
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

          <form.Field name="email">
            {(field) => {
              const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>Email</FieldLabel>
                  <Input
                    id={field.name}
                    name={field.name}
                    type="email"
                    autoComplete="email"
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

          <form.Field name="password">
            {(field) => {
              const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>Пароль</FieldLabel>
                  <Input
                    id={field.name}
                    name={field.name}
                    type="password"
                    autoComplete="new-password"
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

        <Button type="submit" className="w-full" disabled={registerMutation.isPending}>
          {registerMutation.isPending ? "Создаем..." : "Создать аккаунт"}
        </Button>

        <p className="text-muted-foreground text-center text-sm">
          Уже есть аккаунт?{" "}
          <Link className="text-primary font-medium underline-offset-4 hover:underline" to="/login">
            Войти
          </Link>
        </p>
      </form>
    </AuthFormLayout>
  )
}

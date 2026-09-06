import { useForm } from "@tanstack/react-form"
import { Link, useLocation } from "react-router-dom"
import { z } from "zod"

import { useLoginMutation } from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { PasswordField } from "@/features/auth/ui/PasswordField"
import { ApiException } from "@/shared/api/errors"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

const loginFormSchema = z.object({
  email: z.string().trim().pipe(z.email("Введите корректный email.")),
  password: z.string().min(1, "Введите пароль."),
})

type LoginFormValues = z.infer<typeof loginFormSchema>

export default function LoginPage() {
  const loginMutation = useLoginMutation()
  const location = useLocation()
  const state = location.state as Record<string, unknown> | null
  const form = useForm({
    defaultValues: {
      email: "",
      password: "",
    } satisfies LoginFormValues,
    validators: {
      onSubmit: loginFormSchema,
    },
    onSubmit: ({ value }) => {
      loginMutation.mutate(value)
    },
  })

  return (
    <AuthFormLayout title="Вход">
      <form
        className="space-y-5"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {state?.emailVerified ||
        state?.passwordReset ||
        state?.passwordChanged ||
        state?.emailChanged ||
        state?.accountDeleted ? (
          <Alert>
            <AlertTitle>Готово</AlertTitle>
            <AlertDescription>
              {state.accountDeleted
                ? "Аккаунт и связанные данные удалены."
                : state.emailVerified
                  ? "Email подтвержден. Теперь можно войти."
                  : "Данные учетной записи обновлены. Войдите заново."}
            </AlertDescription>
          </Alert>
        ) : null}
        {loginMutation.error ? <ErrorAlert error={loginMutation.error} /> : null}
        {loginMutation.error instanceof ApiException &&
        loginMutation.error.code === "Auth.EmailNotVerified" ? (
          <Link
            className="text-primary block text-sm font-medium underline-offset-4 hover:underline"
            to={`/verify-email?email=${encodeURIComponent(form.getFieldValue("email"))}`}
          >
            Ввести код подтверждения
          </Link>
        ) : null}

        <FieldGroup>
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
                  <PasswordField
                    id={field.name}
                    name={field.name}
                    autoComplete="current-password"
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

        <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
          {loginMutation.isPending ? "Входим..." : "Войти"}
        </Button>

        <div className="text-center">
          <Link
            className="text-primary text-sm font-medium underline-offset-4 hover:underline"
            to="/forgot-password"
          >
            Забыли пароль?
          </Link>
        </div>

        <p className="text-muted-foreground border-t pt-5 text-center text-sm">
          Нет аккаунта?{" "}
          <Link
            className="text-primary font-medium underline-offset-4 hover:underline"
            to="/register"
          >
            Зарегистрироваться
          </Link>
        </p>
      </form>
    </AuthFormLayout>
  )
}

import { useForm } from "@tanstack/react-form"
import { Link } from "react-router-dom"
import { z } from "zod"

import { useLoginMutation } from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
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
    <AuthFormLayout title="Вход" description="Продолжите работу с рецептами и меню.">
      <form
        className="space-y-4"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {loginMutation.error ? <ErrorAlert error={loginMutation.error} /> : null}

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
                  <Input
                    id={field.name}
                    name={field.name}
                    type="password"
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

        <p className="text-muted-foreground text-center text-sm">
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

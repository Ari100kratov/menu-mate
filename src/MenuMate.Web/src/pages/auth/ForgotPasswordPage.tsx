import { useForm } from "@tanstack/react-form"
import { Link } from "react-router-dom"
import { z } from "zod"

import { useRequestPasswordResetMutation } from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

const formSchema = z.object({
  email: z.string().trim().pipe(z.email("Введите корректный email.")),
})

export default function ForgotPasswordPage() {
  const mutation = useRequestPasswordResetMutation()
  const form = useForm({
    defaultValues: { email: "" },
    validators: { onSubmit: formSchema },
    onSubmit: ({ value }) => {
      mutation.mutate(value.email)
    },
  })

  return (
    <AuthFormLayout title="Восстановление пароля">
      <form
        className="space-y-5"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          void form.handleSubmit()
        }}
      >
        <p className="text-muted-foreground text-sm">
          Если для адреса доступно восстановление, мы отправим ссылку для нового пароля.
        </p>
        {mutation.error ? <ErrorAlert error={mutation.error} /> : null}
        {mutation.isSuccess ? (
          <Alert>
            <AlertTitle>Проверьте почту</AlertTitle>
            <AlertDescription>
              Запрос принят. Ссылка действует 30 минут и может быть использована один раз.
            </AlertDescription>
          </Alert>
        ) : null}
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
        <Button className="w-full" type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Отправляем..." : "Получить ссылку"}
        </Button>
        <Link className="text-primary block text-center text-sm hover:underline" to="/login">
          Вернуться ко входу
        </Link>
      </form>
    </AuthFormLayout>
  )
}

import { useForm } from "@tanstack/react-form"
import { Link, useLocation } from "react-router-dom"

import { useCompletePasswordResetMutation } from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { PasswordField } from "@/features/auth/ui/PasswordField"
import { passwordPairSchema } from "@/features/auth/model/password-policy"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"

const formSchema = passwordPairSchema

export default function ResetPasswordPage() {
  const location = useLocation()
  const token = new URLSearchParams(location.hash.replace(/^#/, "")).get("token") ?? ""
  const mutation = useCompletePasswordResetMutation()
  const form = useForm({
    defaultValues: { password: "", confirmPassword: "" },
    validators: { onSubmit: formSchema },
    onSubmit: ({ value }) => {
      mutation.mutate({ token, newPassword: value.password })
    },
  })

  return (
    <AuthFormLayout title="Новый пароль">
      <form
        className="space-y-5"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          void form.handleSubmit()
        }}
      >
        {!token ? <ErrorAlert error={new Error("В ссылке отсутствует токен сброса.")} /> : null}
        {mutation.error ? <ErrorAlert error={mutation.error} /> : null}
        <FieldGroup>
          {(["password", "confirmPassword"] as const).map((name) => (
            <form.Field key={name} name={name}>
              {(field) => {
                const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                return (
                  <Field data-invalid={isInvalid}>
                    <FieldLabel htmlFor={field.name}>
                      {name === "password" ? "Новый пароль" : "Повторите пароль"}
                    </FieldLabel>
                    <PasswordField
                      id={field.name}
                      name={field.name}
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
          ))}
        </FieldGroup>
        <Button className="w-full" type="submit" disabled={!token || mutation.isPending}>
          {mutation.isPending ? "Сохраняем..." : "Сохранить пароль"}
        </Button>
        <Link className="text-primary block text-center text-sm hover:underline" to="/login">
          Вернуться ко входу
        </Link>
      </form>
    </AuthFormLayout>
  )
}

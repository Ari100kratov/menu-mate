import { useForm } from "@tanstack/react-form"
import { Link } from "react-router-dom"
import { z } from "zod"

import { usePrivacyPolicyQuery, useRegisterMutation } from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { PasswordField } from "@/features/auth/ui/PasswordField"
import { passwordSchema } from "@/features/auth/model/password-policy"
import { Button } from "@/shared/ui/button"
import { Checkbox } from "@/shared/ui/checkbox"
import { ApiException } from "@/shared/api/errors"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

const registerFormSchema = z
  .object({
    displayName: z
      .string()
      .trim()
      .min(1, "Введите имя.")
      .max(120, "Имя не должно быть длиннее 120 символов."),
    email: z.string().trim().pipe(z.email("Введите корректный email.")),
    password: passwordSchema,
    confirmPassword: z.string(),
    privacyAccepted: z.boolean().refine((accepted) => accepted, {
      message: "Примите политику конфиденциальности.",
    }),
  })
  .refine((value) => value.password === value.confirmPassword, {
    path: ["confirmPassword"],
    message: "Пароли не совпадают.",
  })

type RegisterFormValues = z.infer<typeof registerFormSchema>

export default function RegisterPage() {
  const registerMutation = useRegisterMutation()
  const policyQuery = usePrivacyPolicyQuery()
  const registrationWasSaved =
    registerMutation.error instanceof ApiException &&
    registerMutation.error.code === "Auth.RegistrationEmailDeliveryFailed"
  const form = useForm({
    defaultValues: {
      displayName: "",
      email: "",
      password: "",
      confirmPassword: "",
      privacyAccepted: false as boolean,
    } satisfies RegisterFormValues,
    validators: {
      onSubmit: registerFormSchema,
    },
    onSubmit: ({ value }) => {
      registerMutation.mutate({
        displayName: value.displayName,
        email: value.email,
        password: value.password,
        privacyPolicyVersion: policyQuery.data?.version ?? "",
      })
    },
  })

  return (
    <AuthFormLayout title="Регистрация">
      <form
        className="space-y-5"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {registerMutation.error ? <ErrorAlert error={registerMutation.error} /> : null}
        {registrationWasSaved ? (
          <Button asChild className="w-full" variant="outline">
            <Link to={`/verify-email?email=${encodeURIComponent(form.state.values.email.trim())}`}>
              Перейти к подтверждению
            </Link>
          </Button>
        ) : null}

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

          <form.Field name="confirmPassword">
            {(field) => {
              const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>Повторите пароль</FieldLabel>
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

          <form.Field name="privacyAccepted">
            {(field) => {
              const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

              return (
                <Field data-invalid={isInvalid}>
                  <label className="flex cursor-pointer items-start gap-3 rounded-md border p-3 text-sm">
                    <Checkbox
                      className="mt-0.5"
                      checked={field.state.value}
                      onBlur={field.handleBlur}
                      onCheckedChange={(checked) => {
                        field.handleChange(checked === true)
                      }}
                      aria-invalid={isInvalid}
                    />
                    <span>
                      Я ознакомился и принимаю{" "}
                      <Link
                        className="text-primary underline"
                        to="/privacy"
                        target="_blank"
                        rel="noreferrer"
                      >
                        политику конфиденциальности
                      </Link>
                      {policyQuery.data ? ` версии ${policyQuery.data.version}` : ""}.
                    </span>
                  </label>
                  {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
                </Field>
              )
            }}
          </form.Field>
        </FieldGroup>

        {policyQuery.error ? <ErrorAlert error={policyQuery.error} /> : null}

        <Button
          type="submit"
          className="w-full"
          disabled={registerMutation.isPending || !policyQuery.data}
        >
          {registerMutation.isPending ? "Создаем..." : "Создать аккаунт"}
        </Button>

        <p className="text-muted-foreground border-t pt-5 text-center text-sm">
          Уже есть аккаунт?{" "}
          <Link className="text-primary font-medium underline-offset-4 hover:underline" to="/login">
            Войти
          </Link>
        </p>
      </form>
    </AuthFormLayout>
  )
}

import { useForm } from "@tanstack/react-form"
import { Link } from "react-router-dom"
import { z } from "zod"

import type { UserProfile } from "@/features/auth/api/auth.api"
import {
  useChangePasswordMutation,
  useRequestEmailChangeMutation,
  useUpdateDisplayNameMutation,
} from "@/features/auth/api/auth.queries"
import { PasswordField } from "@/features/auth/ui/PasswordField"
import { passwordSchema } from "@/features/auth/model/password-policy"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"
import { PageSection } from "@/shared/ui/page"

export function ProfileAccountSettings({ user }: { user: UserProfile }) {
  return (
    <div className="grid gap-4">
      {user.emailVerificationStatus === "LegacyUnverified" ? (
        <Alert>
          <AlertTitle>Email пока не подтвержден</AlertTitle>
          <AlertDescription className="space-y-2">
            <p>Вход доступен, но автоматическое восстановление пароля отключено.</p>
            <Button asChild size="sm" variant="outline">
              <Link to={`/verify-email?email=${encodeURIComponent(user.email)}`}>
                Подтвердить текущий email
              </Link>
            </Button>
          </AlertDescription>
        </Alert>
      ) : null}
      <DisplayNameForm user={user} />
      <EmailChangeForm />
      <PasswordChangeForm />
    </div>
  )
}

function DisplayNameForm({ user }: { user: UserProfile }) {
  const mutation = useUpdateDisplayNameMutation()
  const form = useForm({
    defaultValues: { displayName: user.displayName },
    validators: {
      onSubmit: z.object({
        displayName: z.string().trim().min(1, "Введите имя.").max(120, "Максимум 120 символов."),
      }),
    },
    onSubmit: ({ value }) => {
      mutation.mutate(value.displayName)
    },
  })

  return (
    <PageSection title="Имя" description="Имя видно в вашем профиле.">
      <AccountForm onSubmit={() => form.handleSubmit()}>
        {mutation.error ? <ErrorAlert error={mutation.error} /> : null}
        {mutation.isSuccess ? (
          <Alert>
            <AlertTitle>Имя сохранено</AlertTitle>
          </Alert>
        ) : null}
        <form.Field name="displayName">
          {(field) => (
            <Field data-invalid={field.state.meta.isTouched && !field.state.meta.isValid}>
              <FieldLabel htmlFor={field.name}>Отображаемое имя</FieldLabel>
              <Input
                id={field.name}
                value={field.state.value}
                autoComplete="name"
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
              />
              <FieldError errors={field.state.meta.errors} />
            </Field>
          )}
        </form.Field>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Сохраняем..." : "Сохранить имя"}
        </Button>
      </AccountForm>
    </PageSection>
  )
}

function EmailChangeForm() {
  const mutation = useRequestEmailChangeMutation()
  const form = useForm({
    defaultValues: { newEmail: "", currentPassword: "" },
    validators: {
      onSubmit: z.object({
        newEmail: z.string().trim().pipe(z.email("Введите корректный email.")),
        currentPassword: z.string().min(1, "Введите текущий пароль."),
      }),
    },
    onSubmit: ({ value }) => {
      mutation.mutate(value)
    },
  })

  return (
    <PageSection
      title="Email"
      description="Письмо с кодом придет на новый адрес. Доступ к старому адресу не требуется."
    >
      <AccountForm onSubmit={() => form.handleSubmit()}>
        {mutation.error ? <ErrorAlert error={mutation.error} /> : null}
        <FieldGroup>
          <form.Field name="newEmail">
            {(field) => (
              <Field data-invalid={field.state.meta.isTouched && !field.state.meta.isValid}>
                <FieldLabel htmlFor={field.name}>Новый email</FieldLabel>
                <Input
                  id={field.name}
                  type="email"
                  autoComplete="email"
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    field.handleChange(event.target.value)
                  }}
                />
                <FieldError errors={field.state.meta.errors} />
              </Field>
            )}
          </form.Field>
          <form.Field name="currentPassword">
            {(field) => (
              <Field data-invalid={field.state.meta.isTouched && !field.state.meta.isValid}>
                <FieldLabel htmlFor={field.name}>Текущий пароль</FieldLabel>
                <PasswordField
                  id={field.name}
                  autoComplete="current-password"
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    field.handleChange(event.target.value)
                  }}
                />
                <FieldError errors={field.state.meta.errors} />
              </Field>
            )}
          </form.Field>
        </FieldGroup>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Отправляем..." : "Подтвердить новый email"}
        </Button>
      </AccountForm>
    </PageSection>
  )
}

function PasswordChangeForm() {
  const mutation = useChangePasswordMutation()
  const profilePasswordSchema = z
    .object({
      currentPassword: z.string().min(1, "Введите текущий пароль."),
      newPassword: passwordSchema,
      confirmPassword: z.string(),
    })
    .refine((value) => value.newPassword === value.confirmPassword, {
      path: ["confirmPassword"],
      message: "Пароли не совпадают.",
    })
  const form = useForm({
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
    validators: { onSubmit: profilePasswordSchema },
    onSubmit: ({ value }) => {
      mutation.mutate({ currentPassword: value.currentPassword, newPassword: value.newPassword })
    },
  })

  return (
    <PageSection
      title="Пароль"
      description="После изменения все refresh-сессии будут завершены и потребуется новый вход."
    >
      <AccountForm onSubmit={() => form.handleSubmit()}>
        {mutation.error ? <ErrorAlert error={mutation.error} /> : null}
        <FieldGroup>
          {(["currentPassword", "newPassword", "confirmPassword"] as const).map((name) => (
            <form.Field key={name} name={name}>
              {(field) => (
                <Field data-invalid={field.state.meta.isTouched && !field.state.meta.isValid}>
                  <FieldLabel htmlFor={field.name}>
                    {name === "currentPassword"
                      ? "Текущий пароль"
                      : name === "newPassword"
                        ? "Новый пароль"
                        : "Повторите новый пароль"}
                  </FieldLabel>
                  <PasswordField
                    id={field.name}
                    autoComplete={name === "currentPassword" ? "current-password" : "new-password"}
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(event) => {
                      field.handleChange(event.target.value)
                    }}
                  />
                  <FieldError errors={field.state.meta.errors} />
                </Field>
              )}
            </form.Field>
          ))}
        </FieldGroup>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Сохраняем..." : "Изменить пароль"}
        </Button>
      </AccountForm>
    </PageSection>
  )
}

function AccountForm({
  children,
  onSubmit,
}: {
  children: React.ReactNode
  onSubmit: () => Promise<void>
}) {
  return (
    <form
      className="space-y-4"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        void onSubmit()
      }}
    >
      {children}
    </form>
  )
}

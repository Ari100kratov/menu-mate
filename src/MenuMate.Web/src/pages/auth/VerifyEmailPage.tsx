import { useForm } from "@tanstack/react-form"
import { Link, useSearchParams } from "react-router-dom"
import { REGEXP_ONLY_DIGITS } from "input-otp"
import { z } from "zod"

import {
  useConfirmEmailVerificationMutation,
  useResendEmailVerificationMutation,
} from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { InputOTP, InputOTPGroup, InputOTPSlot } from "@/shared/ui/input-otp"

const formSchema = z.object({
  code: z.string().regex(/^\d{6}$/, "Введите шестизначный код."),
})

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const email = searchParams.get("email")?.trim() ?? ""
  const confirmation = useConfirmEmailVerificationMutation()
  const resend = useResendEmailVerificationMutation()
  const form = useForm({
    defaultValues: { code: "" },
    validators: { onSubmit: formSchema },
    onSubmit: ({ value }) => {
      confirmation.mutate({ email, code: value.code })
    },
  })

  return (
    <AuthFormLayout title="Подтверждение email">
      <form
        className="space-y-5"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          void form.handleSubmit()
        }}
      >
        <p className="text-muted-foreground text-sm">
          Введите код из письма для <span className="text-foreground font-medium">{email}</span>.
        </p>
        {!email ? (
          <ErrorAlert error={new Error("Email не указан. Вернитесь к регистрации.")} />
        ) : null}
        {confirmation.error ? <ErrorAlert error={confirmation.error} /> : null}
        {resend.error ? <ErrorAlert error={resend.error} /> : null}
        {resend.isSuccess ? (
          <Alert>
            <AlertTitle>Запрос принят</AlertTitle>
            <AlertDescription>
              Если адрес ожидает подтверждения, новое письмо отправлено.
            </AlertDescription>
          </Alert>
        ) : null}

        <form.Field name="code">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel>Код из письма</FieldLabel>
                <InputOTP
                  maxLength={6}
                  pattern={REGEXP_ONLY_DIGITS}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={field.handleChange}
                  aria-invalid={isInvalid}
                  autoComplete="one-time-code"
                >
                  <InputOTPGroup>
                    {Array.from({ length: 6 }, (_, index) => (
                      <InputOTPSlot key={index} index={index} />
                    ))}
                  </InputOTPGroup>
                </InputOTP>
                {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
              </Field>
            )
          }}
        </form.Field>

        <Button className="w-full" type="submit" disabled={!email || confirmation.isPending}>
          {confirmation.isPending ? "Проверяем..." : "Подтвердить"}
        </Button>
        <Button
          className="w-full"
          type="button"
          variant="outline"
          disabled={!email || resend.isPending}
          onClick={() => {
            resend.mutate(email)
          }}
        >
          {resend.isPending ? "Отправляем..." : "Отправить код повторно"}
        </Button>
        <Link className="text-primary block text-center text-sm hover:underline" to="/login">
          Вернуться ко входу
        </Link>
      </form>
    </AuthFormLayout>
  )
}

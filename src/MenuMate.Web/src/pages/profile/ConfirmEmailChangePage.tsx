import { useForm } from "@tanstack/react-form"
import { REGEXP_ONLY_DIGITS } from "input-otp"
import { useSearchParams } from "react-router-dom"
import { z } from "zod"

import { useConfirmEmailChangeMutation } from "@/features/auth/api/auth.queries"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldError, FieldLabel } from "@/shared/ui/field"
import { InputOTP, InputOTPGroup, InputOTPSlot } from "@/shared/ui/input-otp"
import { PageSection } from "@/shared/ui/page"

export default function ConfirmEmailChangePage() {
  const [searchParams] = useSearchParams()
  const email = searchParams.get("email") ?? "новый адрес"
  const mutation = useConfirmEmailChangeMutation()
  const form = useForm({
    defaultValues: { code: "" },
    validators: { onSubmit: z.object({ code: z.string().regex(/^\d{6}$/, "Введите код.") }) },
    onSubmit: ({ value }) => {
      mutation.mutate(value.code)
    },
  })

  return (
    <PageSection
      title="Подтверждение нового email"
      description={`Введите код, отправленный на ${email}. После подтверждения потребуется новый вход.`}
    >
      <form
        className="space-y-4"
        onSubmit={(event) => {
          event.preventDefault()
          void form.handleSubmit()
        }}
      >
        {mutation.error ? <ErrorAlert error={mutation.error} /> : null}
        <form.Field name="code">
          {(field) => (
            <Field data-invalid={field.state.meta.isTouched && !field.state.meta.isValid}>
              <FieldLabel>Код из письма</FieldLabel>
              <InputOTP
                maxLength={6}
                pattern={REGEXP_ONLY_DIGITS}
                value={field.state.value}
                onChange={field.handleChange}
                autoComplete="one-time-code"
              >
                <InputOTPGroup>
                  {Array.from({ length: 6 }, (_, index) => (
                    <InputOTPSlot key={index} index={index} />
                  ))}
                </InputOTPGroup>
              </InputOTP>
              <FieldError errors={field.state.meta.errors} />
            </Field>
          )}
        </form.Field>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Проверяем..." : "Изменить email"}
        </Button>
      </form>
    </PageSection>
  )
}

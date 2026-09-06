import { useState } from "react"
import { Link } from "react-router-dom"

import {
  useAcceptPrivacyPolicyMutation,
  usePrivacyPolicyQuery,
} from "@/features/auth/api/auth.queries"
import { AuthFormLayout } from "@/features/auth/ui/AuthFormLayout"
import { Button } from "@/shared/ui/button"
import { Checkbox } from "@/shared/ui/checkbox"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Skeleton } from "@/shared/ui/skeleton"

export default function PrivacyConsentPage() {
  const [accepted, setAccepted] = useState(false)
  const policyQuery = usePrivacyPolicyQuery()
  const acceptMutation = useAcceptPrivacyPolicyMutation()

  return (
    <AuthFormLayout title="Обновление политики">
      <div className="space-y-5">
        <p className="text-muted-foreground text-sm leading-6">
          Чтобы продолжить работу, ознакомьтесь с актуальной политикой конфиденциальности и
          подтвердите ее принятие.
        </p>
        {policyQuery.isPending ? <Skeleton className="h-20 w-full" /> : null}
        {policyQuery.error ? <ErrorAlert error={policyQuery.error} /> : null}
        {acceptMutation.error ? <ErrorAlert error={acceptMutation.error} /> : null}
        {policyQuery.data ? (
          <>
            <label className="flex cursor-pointer items-start gap-3 rounded-md border p-4 text-sm">
              <Checkbox
                className="mt-0.5"
                checked={accepted}
                onCheckedChange={(checked) => {
                  setAccepted(checked === true)
                }}
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
                </Link>{" "}
                версии {policyQuery.data.version}.
              </span>
            </label>
            <Button
              className="w-full"
              disabled={!accepted || acceptMutation.isPending}
              onClick={() => {
                acceptMutation.mutate(policyQuery.data.version)
              }}
            >
              {acceptMutation.isPending ? "Сохраняем..." : "Принять и продолжить"}
            </Button>
          </>
        ) : null}
      </div>
    </AuthFormLayout>
  )
}

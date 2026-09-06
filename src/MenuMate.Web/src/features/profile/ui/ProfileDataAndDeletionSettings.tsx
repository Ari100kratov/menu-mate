import { Trash2 } from "lucide-react"
import { useState } from "react"
import { Link } from "react-router-dom"

import type { UserProfile } from "@/features/auth/api/auth.api"
import { useDeleteAccountMutation, usePrivacyPolicyQuery } from "@/features/auth/api/auth.queries"
import { PasswordField } from "@/features/auth/ui/PasswordField"
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/ui/alert-dialog"
import { Button } from "@/shared/ui/button"
import { Checkbox } from "@/shared/ui/checkbox"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Field, FieldLabel } from "@/shared/ui/field"
import { PageSection } from "@/shared/ui/page"

export function ProfileDataAndDeletionSettings({ user }: { user: UserProfile }) {
  const policyQuery = usePrivacyPolicyQuery()

  return (
    <div className="grid gap-4">
      <PageSection
        title="Конфиденциальность"
        description="Управление персональными данными и условиями их обработки."
      >
        <div className="text-muted-foreground space-y-2 text-sm">
          <p>
            Принятая версия: {user.privacyPolicyAcceptedVersion ?? "не зафиксирована"}.
            {policyQuery.data ? ` Актуальная версия: ${policyQuery.data.version}.` : ""}
          </p>
          <div className="flex flex-wrap gap-3">
            <Button asChild variant="outline" size="sm">
              <Link to="/privacy">Политика конфиденциальности</Link>
            </Button>
            <Button asChild variant="outline" size="sm">
              <Link to="/account-deletion">Правила удаления данных</Link>
            </Button>
          </div>
        </div>
      </PageSection>

      <DeleteAccountSection />
    </div>
  )
}

function DeleteAccountSection() {
  const [open, setOpen] = useState(false)
  const [currentPassword, setCurrentPassword] = useState("")
  const [confirmed, setConfirmed] = useState(false)
  const mutation = useDeleteAccountMutation()

  function resetDialog() {
    setCurrentPassword("")
    setConfirmed(false)
    mutation.reset()
  }

  return (
    <PageSection
      className="border-destructive/40"
      title="Удаление аккаунта"
      description="Необратимо удаляет учетную запись и связанные персональные данные."
    >
      <div className="text-muted-foreground space-y-2 text-sm leading-6">
        <p>
          Будут удалены ваши рецепты и изображения, черновики импорта, меню, покупки, избранное и
          настройки. Самостоятельные копии рецептов, созданные другими пользователями, сохранятся.
        </p>
        <p>Перед удалением экспортируйте все, что хотите сохранить.</p>
      </div>

      <AlertDialog
        open={open}
        onOpenChange={(nextOpen) => {
          setOpen(nextOpen)
          if (!nextOpen) {
            resetDialog()
          }
        }}
      >
        <AlertDialogTrigger asChild>
          <Button variant="destructive">
            <Trash2 />
            Удалить аккаунт
          </Button>
        </AlertDialogTrigger>
        <AlertDialogContent>
          <form
            className="space-y-4"
            onSubmit={(event) => {
              event.preventDefault()
              mutation.mutate(currentPassword)
            }}
          >
            <AlertDialogHeader>
              <AlertDialogTitle>Удалить аккаунт без возможности восстановления?</AlertDialogTitle>
              <AlertDialogDescription>
                Операция начнется сразу. Введите текущий пароль и подтвердите, что понимаете
                последствия.
              </AlertDialogDescription>
            </AlertDialogHeader>

            {mutation.error ? <ErrorAlert error={mutation.error} /> : null}

            <Field>
              <FieldLabel htmlFor="delete-account-password">Текущий пароль</FieldLabel>
              <PasswordField
                id="delete-account-password"
                autoComplete="current-password"
                value={currentPassword}
                onChange={(event) => {
                  setCurrentPassword(event.target.value)
                }}
              />
            </Field>

            <label className="flex cursor-pointer items-start gap-3 rounded-md border p-3 text-sm">
              <Checkbox
                className="mt-0.5"
                checked={confirmed}
                onCheckedChange={(checked) => {
                  setConfirmed(checked === true)
                }}
              />
              <span>Я понимаю, что данные и учетную запись нельзя будет восстановить.</span>
            </label>

            <AlertDialogFooter>
              <AlertDialogCancel type="button" disabled={mutation.isPending}>
                Отмена
              </AlertDialogCancel>
              <Button
                type="submit"
                variant="destructive"
                disabled={!currentPassword || !confirmed || mutation.isPending}
              >
                {mutation.isPending ? "Удаляем..." : "Удалить навсегда"}
              </Button>
            </AlertDialogFooter>
          </form>
        </AlertDialogContent>
      </AlertDialog>
    </PageSection>
  )
}

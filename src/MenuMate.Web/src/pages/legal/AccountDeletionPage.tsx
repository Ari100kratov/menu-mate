import { Link } from "react-router-dom"

import { usePrivacyPolicyQuery } from "@/features/auth/api/auth.queries"
import { LegalPageLayout, LegalSection } from "@/pages/legal/LegalPageLayout"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Skeleton } from "@/shared/ui/skeleton"

export default function AccountDeletionPage() {
  const policyQuery = usePrivacyPolicyQuery()

  return (
    <LegalPageLayout
      title="Удаление аккаунта"
      description="Публичная инструкция по удалению аккаунта План есть и связанных данных."
    >
      {policyQuery.isPending ? <Skeleton className="h-32 w-full" /> : null}
      {policyQuery.error ? <ErrorAlert error={policyQuery.error} /> : null}
      {policyQuery.data ? (
        <div className="space-y-8">
          <LegalSection title="Удаление в приложении">
            <ol className="list-decimal space-y-2 pl-5">
              <li>Войдите в аккаунт.</li>
              <li>Откройте профиль и раздел «Удаление аккаунта».</li>
              <li>Введите текущий пароль и подтвердите необратимое удаление.</li>
            </ol>
            <Button asChild>
              <Link to="/profile">Перейти в профиль</Link>
            </Button>
          </LegalSection>

          <LegalSection title="Если войти не получается">
            <p>
              Сначала попробуйте восстановить пароль через подтвержденный email. Если доступа к
              почте больше нет, отправьте запрос оператору {policyQuery.data.operatorName} с адреса{" "}
              <a
                className="text-primary underline"
                href={`mailto:${policyQuery.data.contactEmail}?subject=${encodeURIComponent("Удаление аккаунта План есть")}`}
              >
                {policyQuery.data.contactEmail}
              </a>
              . Для защиты аккаунта потребуется подтвердить, что запрос отправил его владелец.
            </p>
            <Button asChild variant="outline">
              <Link to="/forgot-password">Восстановить пароль</Link>
            </Button>
          </LegalSection>

          <LegalSection title="Что будет удалено">
            <p>
              Удаляются учетные данные, токены и коды, ваши рецепты и изображения, черновики
              импорта, меню, список покупок, избранное и настройки. Закладки других пользователей на
              удаленный оригинал исчезнут. Их самостоятельные копии рецепта сохранятся как
              принадлежащий им контент.
            </p>
            <p>
              Рабочие данные удаляются сразу. Остаточные данные в резервных копиях удаляются по
              циклу ротации не позднее чем через {policyQuery.data.backupRetentionDays} дней.
              Подробнее — в{" "}
              <Link className="text-primary underline" to="/privacy">
                политике конфиденциальности
              </Link>
              .
            </p>
          </LegalSection>
        </div>
      ) : null}
    </LegalPageLayout>
  )
}

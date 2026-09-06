import { Link } from "react-router-dom"

import type { PrivacyPolicy } from "@/features/auth/api/auth.api"
import { usePrivacyPolicyQuery } from "@/features/auth/api/auth.queries"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Skeleton } from "@/shared/ui/skeleton"
import { LegalPageLayout, LegalSection } from "@/pages/legal/LegalPageLayout"

export default function PrivacyPolicyPage() {
  const policyQuery = usePrivacyPolicyQuery()

  return (
    <LegalPageLayout
      title="Политика конфиденциальности"
      description="Как План есть обрабатывает и удаляет персональные данные."
    >
      {policyQuery.isPending ? <Skeleton className="h-32 w-full" /> : null}
      {policyQuery.error ? <ErrorAlert error={policyQuery.error} /> : null}
      {policyQuery.data ? <PrivacyPolicyContent policy={policyQuery.data} /> : null}
    </LegalPageLayout>
  )
}

export function PrivacyPolicyContent({ policy }: { policy: PrivacyPolicy }) {
  const effectiveDate = new Intl.DateTimeFormat("ru-RU", { dateStyle: "long" }).format(
    new Date(policy.effectiveAt),
  )

  return (
    <div className="space-y-8">
      <p className="rounded-md border p-4 text-sm">
        Версия {policy.version}, действует с {effectiveDate}. Оператор данных: {policy.operatorName}
        . По вопросам конфиденциальности:{" "}
        <a className="text-primary underline" href={`mailto:${policy.contactEmail}`}>
          {policy.contactEmail}
        </a>
        .
      </p>

      <LegalSection title="Какие данные обрабатываются">
        <p>
          Мы храним имя, email, защищенный хеш пароля, статус подтверждения email и факт принятия
          политики. Пароль в открытом виде не сохраняется.
        </p>
        <p>
          Для работы приложения хранятся ваши рецепты, изображения и черновики импорта, меню, список
          покупок и пользовательские настройки. В браузере может храниться офлайн-копия списка
          покупок.
        </p>
        <p>
          Сервер и прокси могут записывать технические журналы: время запроса, адрес сети, маршрут,
          код ответа и сведения об ошибке. Refresh token хранится в защищенной HttpOnly cookie.
        </p>
      </LegalSection>

      <LegalSection title="Зачем нужны данные">
        <p>
          Данные используются для регистрации и защиты аккаунта, восстановления доступа,
          синхронизации контента между устройствами, работы рецептов, меню и покупок, отправки
          сервисных писем, диагностики ошибок и защиты инфраструктуры.
        </p>
      </LegalSection>

      <LegalSection title="Сервисы и передача данных">
        <p>
          Для работы могут использоваться поставщик хостинга, PostgreSQL, S3-совместимое хранилище
          изображений, SMTP-провайдер сервисной почты и система технической телеметрии. Они получают
          только данные, необходимые для своей функции.
        </p>
        <p>
          Если вы сами запускаете распознавание рецепта или генерацию изображения, введенные данные
          и изображения передаются настроенному AI-провайдеру. Эта обработка не запускается скрыто.
          Актуальный перечень провайдеров и места обработки должен соответствовать конфигурации
          развернутого сервиса.
        </p>
      </LegalSection>

      <LegalSection title="Сроки хранения">
        <p>
          Данные аккаунта и созданный контент хранятся, пока аккаунт активен. Незавершенные
          черновики импорта автоматически удаляются по истечении настроенного срока. Технические
          журналы хранятся до {policy.technicalLogRetentionDays} дней.
        </p>
        <p>
          После удаления аккаунта рабочие данные удаляются сразу. Остаточные копии могут сохраняться
          в защищенных резервных копиях не более {policy.backupRetentionDays} дней и удаляются по
          циклу ротации. Они не возвращаются в рабочую систему, кроме восстановления после аварии.
        </p>
      </LegalSection>

      <LegalSection title="Удаление аккаунта и ваши права">
        <p>
          Удалить аккаунт можно в профиле после повторного ввода пароля. Будут удалены учетные
          данные, ваши рецепты и изображения, черновики, меню, список покупок и настройки.
          Самостоятельные копии рецептов, ранее созданные другими пользователями, принадлежат им и
          сохраняются.
        </p>
        <p>
          Публичная инструкция для удаления доступна на странице{" "}
          <Link className="text-primary underline" to="/account-deletion">
            «Удаление аккаунта»
          </Link>
          . Для запроса доступа, исправления или удаления данных напишите на{" "}
          <a className="text-primary underline" href={`mailto:${policy.contactEmail}`}>
            {policy.contactEmail}
          </a>
          . Перед выполнением запроса оператор может проверить личность заявителя.
        </p>
      </LegalSection>

      <LegalSection title="Безопасность и изменения">
        <p>
          Передача данных на хостинге должна быть защищена HTTPS. Доступ к инфраструктуре и секретам
          ограничивается, а пароли хешируются с солью алгоритмом PBKDF2.
        </p>
        <p>
          При существенном изменении политики приложение запросит принятие новой версии. Предыдущий
          факт принятия сохраняется в учетной записи для подтверждения версии условий.
        </p>
      </LegalSection>
    </div>
  )
}

import { AlertCircle } from "lucide-react"

import { getApiErrorMessages } from "@/shared/api/errors"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Skeleton } from "@/shared/ui/skeleton"

interface ErrorAlertProps {
  error: unknown
}

export function ErrorAlert({ error }: ErrorAlertProps) {
  const messages = getApiErrorMessages(error)

  return (
    <Alert variant="destructive" className="space-y-2">
      <div className="flex items-start gap-2">
        <AlertCircle className="mt-0.5 size-4 shrink-0" />
        <div>
          <AlertTitle>Не удалось выполнить действие</AlertTitle>
          <AlertDescription>{messages[0] ?? "Неизвестная ошибка"}</AlertDescription>
        </div>
      </div>
    </Alert>
  )
}

export function PageSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-8 w-2/3 max-w-80" />
      <Skeleton className="h-24 w-full" />
      <Skeleton className="h-24 w-full" />
    </div>
  )
}

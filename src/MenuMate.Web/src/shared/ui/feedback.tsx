import { AlertCircle } from "lucide-react"

import { getApiErrorMessages } from "@/shared/api/errors"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"

interface ErrorAlertProps {
  error: unknown
}

export function ErrorAlert({ error }: ErrorAlertProps) {
  const messages = getApiErrorMessages(error)

  return (
    <Alert variant="destructive">
      <AlertCircle />
      <AlertTitle className="line-clamp-none">Не удалось выполнить действие</AlertTitle>
      <AlertDescription className="min-w-0">{messages[0] ?? "Неизвестная ошибка"}</AlertDescription>
    </Alert>
  )
}

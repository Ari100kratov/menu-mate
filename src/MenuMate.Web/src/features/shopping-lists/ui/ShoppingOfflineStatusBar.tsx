import { CloudAlert, RefreshCw, WifiOff } from "lucide-react"
import { useState } from "react"

import type { ShoppingOfflineStatus } from "@/features/shopping-lists/model/use-shopping-list-offline"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/ui/button"

interface ShoppingOfflineStatusBarProps {
  status: ShoppingOfflineStatus
  onRetry: () => void
}

export function ShoppingOfflineStatusBar({ status, onRetry }: ShoppingOfflineStatusBarProps) {
  const [isRetrying, setIsRetrying] = useState(false)
  const content = getStatusContent(status)
  const Icon = content.icon
  const detail = getStatusDetail(status, content.savedText)

  function handleRetry() {
    if (isRetrying) {
      return
    }

    setIsRetrying(true)
    window.setTimeout(onRetry, 600)
  }

  return (
    <div className="fixed right-[4.5rem] bottom-[calc(5rem+env(safe-area-inset-bottom))] left-4 z-30 md:right-20 md:bottom-6 md:left-[17.5rem]">
      <div
        className={cn(
          "bg-card mx-auto flex min-h-11 max-w-3xl items-center gap-1.5 rounded-full border py-2 pr-1 pl-3 shadow-md",
          content.className,
        )}
      >
        <div className="min-w-0 flex-1" role="status" aria-live="polite">
          <div className="flex min-w-0 items-center gap-2">
            <Icon
              className={cn(
                "size-4 shrink-0",
                content.iconClassName,
                status.mode === "syncing" && "animate-spin",
              )}
            />
            <span className="block min-w-0 truncate text-[15px] leading-5 font-medium">
              {content.text}
            </span>
          </div>
          {detail ? (
            <span className="block min-w-0 text-xs leading-4 tracking-tight opacity-80">
              {detail}
            </span>
          ) : null}
        </div>
        {status.mode !== "syncing" ? (
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="hover:bg-background/50 size-9 shrink-0 rounded-full"
            aria-label={isRetrying ? "Проверяем соединение" : "Проверить соединение"}
            aria-disabled={isRetrying}
            title={isRetrying ? "Проверяем соединение" : "Проверить соединение"}
            onClick={handleRetry}
          >
            <RefreshCw className={cn("size-4", isRetrying && "animate-spin")} />
          </Button>
        ) : null}
      </div>
    </div>
  )
}

function getStatusContent(status: ShoppingOfflineStatus) {
  if (status.mode === "syncing") {
    return {
      icon: RefreshCw,
      iconClassName: "text-primary-foreground",
      className: "border-primary bg-primary text-primary-foreground",
      text: "Синхронизация",
      savedText: null,
    }
  }

  if (status.mode === "waiting") {
    return {
      icon: CloudAlert,
      iconClassName: "text-accent-foreground",
      className: "border-accent bg-accent text-accent-foreground",
      text: "Офлайн",
      savedText: "Список сохранен на устройстве",
    }
  }

  return {
    icon: WifiOff,
    iconClassName: "text-accent-foreground",
    className: "border-accent bg-accent text-accent-foreground",
    text: "Офлайн",
    savedText: "Список сохранен на устройстве",
  }
}

function getStatusDetail(status: ShoppingOfflineStatus, savedText: string | null) {
  if (status.pendingCount > 0) {
    return formatPendingSummary(status.pendingCount, status.mode === "syncing")
  }

  return savedText
}

function formatPendingCount(count: number) {
  const remainder10 = count % 10
  const remainder100 = count % 100
  const noun =
    remainder10 === 1 && remainder100 !== 11
      ? "изменение"
      : remainder10 >= 2 && remainder10 <= 4 && (remainder100 < 12 || remainder100 > 14)
        ? "изменения"
        : "изменений"

  return `${String(count)} ${noun}`
}

function formatPendingSummary(count: number, isSynchronizing: boolean) {
  return `${isSynchronizing ? "Отправляем" : "Ожидают"}: ${formatPendingCount(count)}`
}

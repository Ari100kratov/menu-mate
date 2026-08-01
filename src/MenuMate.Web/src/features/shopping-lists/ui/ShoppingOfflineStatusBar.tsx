import { CloudAlert, RefreshCw, WifiOff } from "lucide-react"

import type { ShoppingOfflineStatus } from "@/features/shopping-lists/model/use-shopping-list-offline"
import { cn } from "@/shared/lib/utils"

export function ShoppingOfflineStatusBar({ status }: { status: ShoppingOfflineStatus }) {
  const content = getStatusContent(status)
  const Icon = content.icon

  return (
    <div className="fixed right-20 bottom-[calc(5rem+env(safe-area-inset-bottom))] left-4 z-30 md:right-20 md:bottom-6 md:left-[17.5rem]">
      <div
        className={cn(
          "mx-auto flex min-h-11 max-w-3xl items-center gap-2 rounded-full border px-4 py-2 shadow-lg backdrop-blur",
          content.className,
        )}
        role="status"
        aria-live="polite"
      >
        <Icon className={cn("size-4 shrink-0", status.mode === "syncing" && "animate-spin")} />
        <span className="type-supporting min-w-0 truncate">{content.text}</span>
      </div>
    </div>
  )
}

function getStatusContent(status: ShoppingOfflineStatus) {
  if (status.mode === "syncing") {
    return {
      icon: RefreshCw,
      className: "border-primary/25 bg-primary/10 text-foreground",
      text:
        status.pendingCount > 0
          ? `Синхронизация · ${formatPendingCount(status.pendingCount)}`
          : "Синхронизация списка",
    }
  }

  if (status.mode === "waiting") {
    return {
      icon: CloudAlert,
      className: "border-accent/40 bg-accent/90 text-accent-foreground",
      text: `Ожидаем связь · ${formatPendingCount(status.pendingCount)}`,
    }
  }

  return {
    icon: WifiOff,
    className: "border-accent/40 bg-accent/90 text-accent-foreground",
    text:
      status.pendingCount > 0
        ? `Офлайн-режим · ${formatPendingCount(status.pendingCount)}`
        : "Офлайн-режим · список сохранен на устройстве",
  }
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

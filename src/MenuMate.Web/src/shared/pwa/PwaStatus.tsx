import { RefreshCw, X } from "lucide-react"
import { useEffect } from "react"
import { toast } from "sonner"
import { useRegisterSW } from "virtual:pwa-register/react"

import { Button } from "@/shared/ui/button"

export function PwaStatus() {
  const {
    needRefresh: [needRefresh, setNeedRefresh],
    offlineReady: [offlineReady, setOfflineReady],
    updateServiceWorker,
  } = useRegisterSW()

  useEffect(() => {
    if (!offlineReady) {
      return
    }

    toast.success("Приложение готово к работе без интернета", {
      id: "pwa-offline-ready",
      onDismiss: () => {
        setOfflineReady(false)
      },
    })
  }, [offlineReady, setOfflineReady])

  useEffect(() => {
    if (!needRefresh) {
      return
    }

    toast.custom(
      (toastId) => (
        <PwaUpdateToast
          onClose={() => {
            setNeedRefresh(false)
            toast.dismiss(toastId)
          }}
          onUpdate={() => {
            toast.dismiss(toastId)
            void updateServiceWorker(true)
          }}
        />
      ),
      {
        id: "pwa-update-ready",
        duration: Number.POSITIVE_INFINITY,
        onDismiss: () => {
          setNeedRefresh(false)
        },
      },
    )
  }, [needRefresh, setNeedRefresh, updateServiceWorker])

  return null
}

interface PwaUpdateToastProps {
  onClose: () => void
  onUpdate: () => void
}

function PwaUpdateToast({ onClose, onUpdate }: PwaUpdateToastProps) {
  return (
    <div className="bg-popover text-popover-foreground border-border flex w-[calc(100vw-2rem)] max-w-sm items-start gap-3 rounded-2xl border p-3 shadow-xl">
      <span className="bg-primary/10 text-primary flex size-9 shrink-0 items-center justify-center rounded-full">
        <RefreshCw className="size-4" aria-hidden="true" />
      </span>
      <div className="min-w-0 flex-1">
        <div className="flex items-start gap-2">
          <div className="min-w-0 flex-1">
            <p className="type-label">Доступна новая версия</p>
            <p className="type-supporting text-muted-foreground mt-0.5">
              Обновите приложение, чтобы получить последние изменения.
            </p>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="-mt-1 -mr-1 size-8 shrink-0 rounded-full"
            aria-label="Закрыть уведомление"
            onClick={onClose}
          >
            <X className="size-4" />
          </Button>
        </div>
        <Button type="button" size="sm" className="mt-3 w-full sm:w-auto" onClick={onUpdate}>
          Обновить
        </Button>
      </div>
    </div>
  )
}

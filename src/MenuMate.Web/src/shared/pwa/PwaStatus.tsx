import { useEffect } from "react"
import { toast } from "sonner"
import { useRegisterSW } from "virtual:pwa-register/react"

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

    toast("Доступно обновление приложения", {
      id: "pwa-update-ready",
      duration: Number.POSITIVE_INFINITY,
      action: {
        label: "Обновить",
        onClick: () => {
          void updateServiceWorker(true)
        },
      },
      cancel: {
        label: "Позже",
        onClick: () => {
          setNeedRefresh(false)
        },
      },
    })
  }, [needRefresh, setNeedRefresh, updateServiceWorker])

  return null
}

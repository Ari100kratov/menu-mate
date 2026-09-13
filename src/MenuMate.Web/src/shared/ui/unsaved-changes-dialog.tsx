import { useCallback, useEffect } from "react"
import { useBlocker } from "react-router-dom"

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/ui/alert-dialog"

export function UnsavedChangesDialog({ shouldBlock }: { shouldBlock: () => boolean }) {
  const blocker = useBlocker(useCallback(() => shouldBlock(), [shouldBlock]))

  useEffect(() => {
    function handleBeforeUnload(event: BeforeUnloadEvent) {
      if (shouldBlock()) {
        event.preventDefault()
        // Required by older Safari versions for the native unload confirmation.
        // eslint-disable-next-line @typescript-eslint/no-deprecated
        event.returnValue = ""
      }
    }
    window.addEventListener("beforeunload", handleBeforeUnload)
    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload)
    }
  }, [shouldBlock])

  return (
    <AlertDialog
      open={blocker.state === "blocked"}
      onOpenChange={(open) => {
        if (!open && blocker.state === "blocked") blocker.reset()
      }}
    >
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Уйти без сохранения?</AlertDialogTitle>
          <AlertDialogDescription>
            Несохраненные изменения и выбранное изображение будут потеряны.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={() => blocker.reset?.()}>
            Продолжить редактирование
          </AlertDialogCancel>
          <AlertDialogAction onClick={() => blocker.proceed?.()}>
            Уйти без сохранения
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

import { CheckCheck, ListChecks, Share2 } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog"
import { Button } from "@/shared/ui/button"

interface ShoppingListShareDialogProps {
  open: boolean
  allText: string
  remainingText: string
  onOpenChange: (open: boolean) => void
}

type ShareSelection = "remaining" | "all"
type OptionalShareNavigator = Omit<Navigator, "clipboard" | "share"> & {
  clipboard?: Clipboard
  share?: Navigator["share"]
}

export function ShoppingListShareDialog({
  open,
  allText,
  remainingText,
  onOpenChange,
}: ShoppingListShareDialogProps) {
  const [activeSelection, setActiveSelection] = useState<ShareSelection | null>(null)

  async function share(selection: ShareSelection) {
    const text = selection === "remaining" ? remainingText : allText
    if (!text) {
      return
    }

    setActiveSelection(selection)
    try {
      const optionalNavigator = navigator as OptionalShareNavigator
      if (shouldUseSystemShare(optionalNavigator)) {
        try {
          await optionalNavigator.share({
            title: selection === "remaining" ? "Осталось купить" : "Список покупок",
            text,
          })
          onOpenChange(false)
          return
        } catch (error) {
          if (isShareCancelled(error)) {
            return
          }
        }
      }

      await copyText(text)
      toast.success(
        selection === "remaining" ? "Оставшиеся покупки скопированы" : "Список покупок скопирован",
      )
      onOpenChange(false)
    } catch {
      toast.error("Не удалось поделиться списком")
    } finally {
      setActiveSelection(null)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Поделиться списком</DialogTitle>
          <DialogDescription className="sr-only">
            Два варианта для отправки списка покупок.
          </DialogDescription>
        </DialogHeader>
        <div className="grid gap-3 px-5 pb-5">
          <Button
            type="button"
            variant="outline"
            className="h-auto justify-start gap-3 px-4 py-3 text-left whitespace-normal"
            disabled={!remainingText || activeSelection !== null}
            onClick={() => {
              void share("remaining")
            }}
          >
            <ListChecks className="text-primary size-5" />
            <span className="min-w-0">
              <span className="type-label block">Осталось купить</span>
              <span className="type-supporting text-muted-foreground block">
                Только неотмеченные позиции
              </span>
            </span>
          </Button>
          <Button
            type="button"
            variant="outline"
            className="h-auto justify-start gap-3 px-4 py-3 text-left whitespace-normal"
            disabled={!allText || activeSelection !== null}
            onClick={() => {
              void share("all")
            }}
          >
            <CheckCheck className="text-primary size-5" />
            <span className="min-w-0">
              <span className="type-label block">Весь список</span>
              <span className="type-supporting text-muted-foreground block">
                Все позиции с текущими отметками
              </span>
            </span>
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}

export function ShoppingListShareButton({ onClick }: { onClick: () => void }) {
  return (
    <Button
      type="button"
      variant="secondary"
      size="icon-lg"
      className="size-12 rounded-full shadow-lg"
      aria-label="Поделиться списком"
      title="Поделиться списком"
      onClick={onClick}
    >
      <Share2 className="size-5" />
    </Button>
  )
}

async function copyText(text: string) {
  const optionalNavigator = navigator as OptionalShareNavigator
  if (!optionalNavigator.clipboard) {
    throw new Error("Буфер обмена недоступен")
  }

  await optionalNavigator.clipboard.writeText(text)
}

function isShareCancelled(error: unknown) {
  return error instanceof DOMException && error.name === "AbortError"
}

function shouldUseSystemShare(
  optionalNavigator: OptionalShareNavigator,
): optionalNavigator is OptionalShareNavigator & { share: NonNullable<Navigator["share"]> } {
  if (!optionalNavigator.share) {
    return false
  }

  const isMobileUserAgent = /Android|iPhone|iPad|iPod/i.test(optionalNavigator.userAgent)
  const isIPadWithDesktopUserAgent =
    optionalNavigator.userAgent.includes("Macintosh") && optionalNavigator.maxTouchPoints > 1

  return isMobileUserAgent || isIPadWithDesktopUserAgent
}

import { useRef, useState } from "react"
import { X } from "lucide-react"
import { Dialog } from "radix-ui"
import { toast } from "sonner"

import {
  createEmptyIngredientFormValues,
  type RecipeIngredientFieldErrors,
  type RecipeIngredientFormValues,
  validateRecipeIngredient,
} from "@/features/recipes/model/recipe-form"
import { RecipeIngredientEditorFields } from "@/features/recipes/ui/RecipeIngredientEditorFields"
import { Button } from "@/shared/ui/button"

interface RecipeIngredientDialogProps {
  open: boolean
  mode: "create" | "edit"
  initialValue?: RecipeIngredientFormValues
  onOpenChange: (open: boolean) => void
  onSave: (ingredient: RecipeIngredientFormValues) => void
}

export function RecipeIngredientDialog({
  open,
  mode,
  initialValue,
  onOpenChange,
  onSave,
}: RecipeIngredientDialogProps) {
  const contentRef = useRef<HTMLDivElement>(null)
  const [draft, setDraft] = useState<RecipeIngredientFormValues>(() => ({
    ...(initialValue ?? createEmptyIngredientFormValues()),
  }))
  const [errors, setErrors] = useState<RecipeIngredientFieldErrors>({})

  function handleSave() {
    const nextErrors = validateRecipeIngredient(draft)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      toast.error("Проверьте ингредиент", {
        description: "Мы выделили поля, которые нужно заполнить.",
      })

      requestAnimationFrame(() => {
        const invalidElement = contentRef.current?.querySelector<HTMLElement>(
          '[data-editor-invalid="true"]',
        )
        invalidElement?.scrollIntoView({ behavior: "smooth", block: "center" })
        invalidElement
          ?.querySelector<HTMLElement>("input, textarea, button, [tabindex]")
          ?.focus({ preventScroll: true })
      })
      return
    }

    onSave(draft)
    onOpenChange(false)
  }

  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/45 backdrop-blur-[2px]" />
        <Dialog.Content className="bg-background fixed inset-x-0 bottom-0 z-50 flex max-h-[92svh] flex-col overflow-hidden rounded-t-2xl border shadow-xl outline-none sm:inset-auto sm:top-1/2 sm:left-1/2 sm:h-[min(46rem,calc(100svh-2rem))] sm:w-[min(42rem,calc(100vw-2rem))] sm:-translate-x-1/2 sm:-translate-y-1/2 sm:rounded-2xl">
          <div className="flex items-start justify-between gap-4 border-b px-5 py-4">
            <div className="space-y-1">
              <Dialog.Title className="type-section-title">
                {mode === "create" ? "Добавить ингредиент" : "Редактировать ингредиент"}
              </Dialog.Title>
              <Dialog.Description className="type-supporting text-muted-foreground">
                Выберите продукт, укажите количество и полезную подсказку.
              </Dialog.Description>
            </div>
            <Dialog.Close asChild>
              <Button type="button" variant="ghost" size="icon" aria-label="Закрыть">
                <X />
              </Button>
            </Dialog.Close>
          </div>

          <div ref={contentRef} className="min-h-0 flex-1 overflow-y-auto px-5 py-5">
            <RecipeIngredientEditorFields
              value={draft}
              errors={errors}
              onChange={(value) => {
                setDraft(value)
                setErrors({})
              }}
            />
          </div>

          <div className="bg-background flex flex-col-reverse gap-2 border-t px-5 py-4 sm:flex-row sm:justify-end">
            <Dialog.Close asChild>
              <Button type="button" variant="outline">
                Отмена
              </Button>
            </Dialog.Close>
            <Button type="button" onClick={handleSave}>
              {mode === "create" ? "Добавить ингредиент" : "Сохранить изменения"}
            </Button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

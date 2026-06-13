import { ListPlus, Pencil, X } from "lucide-react"
import { useRef, useState } from "react"
import { toast } from "sonner"

import { ProductLineEditorFields } from "@/features/products/ui/ProductLineEditorFields"
import {
  validateShoppingItem,
  type ShoppingItemFieldErrors,
  type ShoppingItemFormValues,
} from "@/features/shopping-lists/model/shopping-list-ui"
import { Button } from "@/shared/ui/button"

interface ShoppingListItemFormProps {
  submitLabel: string
  submitIcon: "add" | "save"
  isSubmitting: boolean
  initialValues: ShoppingItemFormValues
  onSubmit: (values: ShoppingItemFormValues) => void
  onCancel?: () => void
}

export function ShoppingListItemForm({
  submitLabel,
  submitIcon,
  isSubmitting,
  initialValues,
  onSubmit,
  onCancel,
}: ShoppingListItemFormProps) {
  const contentRef = useRef<HTMLFormElement>(null)
  const [draft, setDraft] = useState<ShoppingItemFormValues>(() => ({ ...initialValues }))
  const [errors, setErrors] = useState<ShoppingItemFieldErrors>({})

  function handleSubmit() {
    const nextErrors = validateShoppingItem(draft)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      toast.error("Проверьте покупку", {
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

    onSubmit(draft)
  }

  return (
    <form
      ref={contentRef}
      className="space-y-6"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        handleSubmit()
      }}
    >
      <ProductLineEditorFields
        idPrefix="shopping-item"
        value={draft}
        errors={errors}
        commentLabel="Комментарий к покупке"
        commentPlaceholder="Например, взять спелые или определенной марки"
        commentDescription="Комментарий будет виден в списке покупок."
        onChange={(value) => {
          setDraft(value)
          setErrors({})
        }}
      />

      <div className="bg-background sticky bottom-0 -mx-5 flex flex-col-reverse gap-2 border-t px-5 pt-4 sm:flex-row sm:justify-end">
        {onCancel ? (
          <Button type="button" variant="outline" onClick={onCancel}>
            <X />
            Отмена
          </Button>
        ) : null}
        <Button type="submit" disabled={isSubmitting}>
          {submitIcon === "add" ? <ListPlus /> : <Pencil />}
          {isSubmitting ? "Сохраняем..." : submitLabel}
        </Button>
      </div>
    </form>
  )
}

import { Save } from "lucide-react"
import { useEffect, useRef, useState } from "react"
import { toast } from "sonner"

import { recipeFormSchema, type RecipeFormValues } from "@/features/recipes/model/recipe-form"
import { RecipeAdditionalFields } from "@/features/recipes/ui/RecipeAdditionalFields"
import {
  RecipeCoverPicker,
  type RecipeCoverSuggestion,
} from "@/features/recipes/ui/RecipeCoverPicker"
import { RecipeIngredientsFields } from "@/features/recipes/ui/RecipeIngredientsFields"
import { RecipeMainFields } from "@/features/recipes/ui/RecipeMainFields"
import { RecipeStepsFields } from "@/features/recipes/ui/RecipeStepsFields"
import { useRecipeForm } from "@/features/recipes/ui/useRecipeForm"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { getApiErrorMessages } from "@/shared/api/errors"

interface RecipeFormProps {
  initialValues: RecipeFormValues
  submitLabel: string
  isSubmitting: boolean
  error?: unknown
  coverImageUrl?: string
  suggestedCover?: RecipeCoverSuggestion
  onValuesChange?: (values: RecipeFormValues) => void
  generateCover?: (values: RecipeFormValues) => Promise<File>
  onSubmit: (values: RecipeFormValues, coverFile: File | null) => void
}

export function RecipeForm({
  initialValues,
  submitLabel,
  isSubmitting,
  error,
  coverImageUrl,
  suggestedCover,
  onValuesChange,
  generateCover,
  onSubmit,
}: RecipeFormProps) {
  const [coverFile, setCoverFile] = useState<File | null>(null)
  const coverFileRef = useRef<File | null>(null)
  const [showValidationErrors, setShowValidationErrors] = useState(false)
  const [isGeneratingCover, setIsGeneratingCover] = useState(false)
  const [generateCoverError, setGenerateCoverError] = useState<unknown>()
  const formElementRef = useRef<HTMLFormElement>(null)
  const form = useRecipeForm({ initialValues })

  function handleCoverFileChange(file: File | null) {
    coverFileRef.current = file
    setCoverFile(file)
  }

  useEffect(() => {
    if (!error) {
      return
    }

    toast.error(getApiErrorMessages(error)[0] ?? "Не удалось сохранить рецепт.")
  }, [error])

  async function handleSubmit() {
    const validationResult = recipeFormSchema.safeParse(form.state.values)

    if (validationResult.success) {
      setShowValidationErrors(false)
      onSubmit(validationResult.data, coverFileRef.current)
      return
    }

    const firstInvalidPath = validationResult.error.issues[0]?.path
    await form.validate("submit")
    setShowValidationErrors(true)
    toast.error("Проверьте корректность заполненных данных.")

    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        const invalidElement = getValidationTarget(formElementRef.current, firstInvalidPath)

        invalidElement?.scrollIntoView({ behavior: "smooth", block: "center" })
        const focusTarget = invalidElement?.matches("input, textarea, button, [tabindex]")
          ? invalidElement
          : invalidElement?.querySelector<HTMLElement>("input, textarea, button, [tabindex]")
        focusTarget?.focus({ preventScroll: true })
      })
    })
  }

  async function handleGenerateCover() {
    if (!generateCover) {
      return
    }

    setGenerateCoverError(undefined)
    setIsGeneratingCover(true)
    try {
      handleCoverFileChange(await generateCover(form.state.values))
      toast.success("Обложка сгенерирована")
    } catch (generationError) {
      setGenerateCoverError(generationError)
      toast.error(getApiErrorMessages(generationError)[0] ?? "Не удалось сгенерировать обложку.")
    } finally {
      setIsGeneratingCover(false)
    }
  }

  return (
    <form
      ref={formElementRef}
      className="mx-auto -mb-4 max-w-3xl md:mb-0"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void handleSubmit()
      }}
    >
      {error ? (
        <div className="mb-4">
          <ErrorAlert error={error} />
        </div>
      ) : null}
      {generateCoverError ? (
        <div className="mb-4">
          <ErrorAlert error={generateCoverError} />
        </div>
      ) : null}
      {onValuesChange ? (
        <form.Subscribe selector={(state) => state.values}>
          {(values) => <RecipeFormChangeObserver values={values} onChange={onValuesChange} />}
        </form.Subscribe>
      ) : null}

      <div className="bg-card overflow-hidden rounded-xl border shadow-sm">
        <RecipeCoverPicker
          existingImageUrl={coverImageUrl}
          file={coverFile}
          onFileChange={handleCoverFileChange}
          onGenerate={generateCover ? () => void handleGenerateCover() : undefined}
          isGenerating={isGeneratingCover}
          suggestedCover={suggestedCover}
        />
        <RecipeMainFields form={form} showValidationErrors={showValidationErrors} />
        <RecipeIngredientsFields form={form} showValidationErrors={showValidationErrors} />
        <RecipeStepsFields form={form} showValidationErrors={showValidationErrors} />
        <RecipeAdditionalFields form={form} showValidationErrors={showValidationErrors} />
      </div>

      <div className="bg-background/95 sticky bottom-18 z-30 -mx-4 mt-3 border-y px-4 py-2.5 backdrop-blur md:bottom-0 md:mx-0 md:mt-4 md:flex md:justify-end md:rounded-xl md:border md:p-3">
        <Button type="submit" className="h-11 w-full md:w-auto" disabled={isSubmitting}>
          <Save />
          {isSubmitting ? "Сохраняем..." : submitLabel}
        </Button>
      </div>
    </form>
  )
}

function getValidationTarget(
  formElement: HTMLFormElement | null,
  path: readonly PropertyKey[] | undefined,
) {
  if (!formElement) {
    return undefined
  }

  const fieldName = getFieldName(path)
  const namedElement = fieldName
    ? [...formElement.querySelectorAll<HTMLElement>("[name]")].find(
        (element) => element.getAttribute("name") === fieldName,
      )
    : undefined
  const fieldMarker = fieldName
    ? [...formElement.querySelectorAll<HTMLElement>("[data-recipe-form-field]")].reduce<
        HTMLElement | undefined
      >((closestMarker, element) => {
        const markerName = element.dataset.recipeFormField
        const isMatchingField =
          markerName &&
          (fieldName === markerName ||
            fieldName.startsWith(`${markerName}.`) ||
            fieldName.startsWith(`${markerName}[`))

        if (!isMatchingField) {
          return closestMarker
        }

        return !closestMarker ||
          markerName.length > (closestMarker.dataset.recipeFormField?.length ?? 0)
          ? element
          : closestMarker
      }, undefined)
    : undefined
  const target =
    namedElement ??
    fieldMarker ??
    formElement.querySelector<HTMLElement>('[aria-invalid="true"], [data-invalid="true"]')

  target?.closest("details")?.setAttribute("open", "")
  return target
}

function getFieldName(path: readonly PropertyKey[] | undefined): string {
  if (!path?.length) {
    return ""
  }

  return path.reduce<string>((fieldName, segment) => {
    if (typeof segment === "number") {
      return `${fieldName}[${String(segment)}]`
    }

    if (typeof segment !== "string") {
      return fieldName
    }

    return fieldName ? `${fieldName}.${segment}` : segment
  }, "")
}

function RecipeFormChangeObserver({
  values,
  onChange,
}: {
  values: RecipeFormValues
  onChange: (values: RecipeFormValues) => void
}) {
  useEffect(() => {
    onChange(values)
  }, [onChange, values])

  return null
}

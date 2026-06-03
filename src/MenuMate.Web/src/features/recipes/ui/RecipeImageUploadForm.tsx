import { useForm } from "@tanstack/react-form"
import { Upload } from "lucide-react"
import { useState } from "react"

import { Button } from "@/shared/ui/button"
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/shared/ui/field"
import { Input } from "@/shared/ui/input"

const supportedImageContentTypes = new Set(["image/jpeg", "image/png", "image/webp", "image/avif"])

interface ImageFormValues {
  file: File | null
  altText: string
}

interface RecipeImageUploadFormProps {
  formId: string
  title: string
  fileLabel: string
  initialAltText: string
  submitLabel: string
  isSubmitting: boolean
  onSubmit: (values: { file: File; altText?: string }) => void
}

export function RecipeImageUploadForm({
  formId,
  title,
  fileLabel,
  initialAltText,
  submitLabel,
  isSubmitting,
  onSubmit,
}: RecipeImageUploadFormProps) {
  const [fileInputKey, setFileInputKey] = useState(0)
  const defaultValues: ImageFormValues = {
    file: null,
    altText: initialAltText,
  }
  const form = useForm({
    defaultValues,
    onSubmit: ({ value }) => {
      if (!value.file) {
        return
      }

      onSubmit({
        file: value.file,
        altText: normalizeOptionalText(value.altText),
      })
      form.reset()
      setFileInputKey((currentKey) => currentKey + 1)
    },
  })

  return (
    <form
      className="space-y-4"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      <h4 className="font-medium tracking-normal">{title}</h4>
      <FieldGroup>
        <form.Field
          name="file"
          validators={{
            onChange: validateImageFile,
            onSubmit: validateImageFile,
          }}
        >
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid

            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={`${formId}-file`}>{fileLabel}</FieldLabel>
                <Input
                  key={fileInputKey}
                  id={`${formId}-file`}
                  name={field.name}
                  type="file"
                  accept="image/jpeg,image/png,image/webp,image/avif"
                  onBlur={field.handleBlur}
                  onChange={(event) => {
                    field.handleChange(event.target.files?.[0] ?? null)
                  }}
                  aria-invalid={isInvalid}
                />
                {field.state.value ? (
                  <FieldDescription>{field.state.value.name}</FieldDescription>
                ) : null}
                {isInvalid ? <FieldError errors={field.state.meta.errors} /> : null}
              </Field>
            )
          }}
        </form.Field>

        <form.Field name="altText">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={`${formId}-alt`}>Описание изображения</FieldLabel>
              <Input
                id={`${formId}-alt`}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event) => {
                  field.handleChange(event.target.value)
                }}
              />
            </Field>
          )}
        </form.Field>
      </FieldGroup>

      <form.Subscribe selector={(state) => state.values.file}>
        {(file) => (
          <Button type="submit" disabled={!file || isSubmitting}>
            <Upload />
            {isSubmitting ? "Загружаем..." : submitLabel}
          </Button>
        )}
      </form.Subscribe>
    </form>
  )
}

function normalizeOptionalText(value: string) {
  const normalized = value.trim()
  return normalized.length > 0 ? normalized : undefined
}

function validateImageFile({ value }: { value: File | null }) {
  if (!value) {
    return "Выберите изображение."
  }

  if (!supportedImageContentTypes.has(value.type)) {
    return "Поддерживаются JPEG, PNG, WebP и AVIF."
  }

  return undefined
}

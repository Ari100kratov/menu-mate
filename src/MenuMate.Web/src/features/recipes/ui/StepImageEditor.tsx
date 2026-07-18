import { Trash2 } from "lucide-react"

import type { Recipe, RecipeImage } from "@/features/recipes/api/recipes.api"
import { Button } from "@/shared/ui/button"
import { RecipeImagePreview } from "./RecipeImagePreview"
import { RecipeImageUploadForm } from "./RecipeImageUploadForm"

interface StepImageEditorProps {
  recipeTitle: string
  step: Recipe["steps"][number]
  image: RecipeImage | undefined
  isDeleting: boolean
  isUploading: boolean
  onDelete: (imageId: string) => void
  onUpload: (values: { file: File; scope: "Step"; stepNumber: number; altText?: string }) => void
}

export function StepImageEditor({
  recipeTitle,
  step,
  image,
  isDeleting,
  isUploading,
  onDelete,
  onUpload,
}: StepImageEditorProps) {
  return (
    <section className="grid gap-3 rounded-md border p-3 md:grid-cols-[minmax(0,14rem)_1fr]">
      <div className="space-y-2">
        <div className="type-label">Шаг {step.number}</div>
        <RecipeImagePreview
          image={image}
          fallbackTitle={`${recipeTitle}, шаг ${String(step.number)}`}
        />
        {image ? (
          <Button
            type="button"
            variant="outline"
            size="sm"
            className="w-full"
            disabled={isDeleting}
            onClick={() => {
              onDelete(image.id)
            }}
          >
            <Trash2 />
            {isDeleting ? "Удаляем..." : "Удалить изображение"}
          </Button>
        ) : null}
      </div>

      <div className="space-y-3">
        <p className="type-supporting text-muted-foreground line-clamp-2">{step.text}</p>
        <RecipeImageUploadForm
          formId={`step-${String(step.number)}-image`}
          title={image ? "Заменить изображение шага" : "Добавить изображение шага"}
          fileLabel="Файл шага"
          initialAltText={image?.altText ?? ""}
          submitLabel={image ? "Заменить" : "Загрузить"}
          isSubmitting={isUploading}
          onSubmit={(values) => {
            onUpload({
              ...values,
              scope: "Step",
              stepNumber: step.number,
            })
          }}
        />
      </div>
    </section>
  )
}

import { Trash2 } from "lucide-react"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import {
  useDeleteRecipeImageMutation,
  useUploadRecipeImageMutation,
} from "@/features/recipes/api/recipes.queries"
import { findCoverImage, findStepImage } from "@/features/recipes/model/recipe-images"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { PageSection } from "@/shared/ui/page"
import { RecipeImagePreview } from "./RecipeImagePreview"
import { RecipeImageUploadForm } from "./RecipeImageUploadForm"
import { StepImageEditor } from "./StepImageEditor"

interface RecipeImagesPanelProps {
  recipe: Recipe
}

export function RecipeImagesPanel({ recipe }: RecipeImagesPanelProps) {
  const uploadImageMutation = useUploadRecipeImageMutation(recipe.id)
  const deleteImageMutation = useDeleteRecipeImageMutation(recipe.id)
  const coverImage = findCoverImage(recipe.images)

  return (
    <PageSection
      title="Изображения"
      description="Обложка и изображения шагов хранятся в MinIO, загрузка всегда идет через backend."
    >
      {uploadImageMutation.error ? <ErrorAlert error={uploadImageMutation.error} /> : null}
      {deleteImageMutation.error ? <ErrorAlert error={deleteImageMutation.error} /> : null}

      <div className="grid gap-4 lg:grid-cols-[minmax(0,18rem)_1fr]">
        <div className="space-y-3">
          <RecipeImagePreview image={coverImage} fallbackTitle={recipe.title} />
          {coverImage ? (
            <Button
              type="button"
              variant="outline"
              className="w-full"
              disabled={deleteImageMutation.isPending}
              onClick={() => {
                deleteImageMutation.mutate(coverImage.id)
              }}
            >
              <Trash2 />
              {deleteImageMutation.isPending ? "Удаляем..." : "Удалить обложку"}
            </Button>
          ) : null}
        </div>

        <RecipeImageUploadForm
          formId="cover-image"
          title="Обложка"
          fileLabel="Файл обложки"
          initialAltText={coverImage?.altText ?? ""}
          submitLabel={coverImage ? "Заменить обложку" : "Загрузить обложку"}
          isSubmitting={uploadImageMutation.isPending}
          onSubmit={(values) => {
            uploadImageMutation.mutate({
              ...values,
              scope: "Cover",
            })
          }}
        />
      </div>

      <div className="space-y-3">
        <h3 className="font-semibold tracking-normal">Изображения шагов</h3>
        {recipe.steps.length > 0 ? (
          <div className="space-y-3">
            {recipe.steps.map((step) => {
              const stepNumber = Number(step.number)
              const image = findStepImage(recipe.images, stepNumber)

              return (
                <StepImageEditor
                  key={step.number}
                  recipeTitle={recipe.title}
                  step={step}
                  image={image}
                  isDeleting={deleteImageMutation.isPending}
                  isUploading={uploadImageMutation.isPending}
                  onDelete={(imageId) => {
                    deleteImageMutation.mutate(imageId)
                  }}
                  onUpload={(values) => {
                    uploadImageMutation.mutate(values)
                  }}
                />
              )
            })}
          </div>
        ) : (
          <p className="text-muted-foreground rounded-md border p-3 text-sm">
            Добавьте шаги приготовления, чтобы прикреплять к ним изображения.
          </p>
        )}
      </div>
    </PageSection>
  )
}

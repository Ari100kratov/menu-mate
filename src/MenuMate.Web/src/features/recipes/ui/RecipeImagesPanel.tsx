import { Trash2 } from "lucide-react"

import type { Recipe } from "@/features/recipes/api/recipes.api"
import {
  useDeleteRecipeImageMutation,
  useUploadRecipeImageMutation,
} from "@/features/recipes/api/recipes.queries"
import { findCoverImage } from "@/features/recipes/model/recipe-images"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { PageSection } from "@/shared/ui/page"
import { RecipeImagePreview } from "./RecipeImagePreview"
import { RecipeImageUploadForm } from "./RecipeImageUploadForm"

interface RecipeImagesPanelProps {
  recipe: Recipe
}

export function RecipeImagesPanel({ recipe }: RecipeImagesPanelProps) {
  const uploadImageMutation = useUploadRecipeImageMutation(recipe.id)
  const deleteImageMutation = useDeleteRecipeImageMutation(recipe.id)
  const coverImage = findCoverImage(recipe.images)

  return (
    <PageSection
      title="Обложка"
      description="Изображение используется в списке и карточке рецепта. Управление изображениями шагов временно скрыто."
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
          title="Файл обложки"
          fileLabel="Файл"
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
    </PageSection>
  )
}

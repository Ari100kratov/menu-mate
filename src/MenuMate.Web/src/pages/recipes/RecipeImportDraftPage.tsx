import { CheckCircle2, ExternalLink, Info, Trash2 } from "lucide-react"
import { useCallback, useEffect, useRef, useState } from "react"
import { Link, useNavigate, useParams } from "react-router-dom"
import { toast } from "sonner"
import { useQueryClient } from "@tanstack/react-query"

import {
  generateRecipeCoverImage,
  getRecipeImportSourceImage,
} from "@/features/imports/api/imports.api"
import {
  useConfirmRecipeImportDraftMutation,
  useDeleteRecipeImportDraftMutation,
  useRecipeImportDraftQuery,
  useUpdateRecipeImportDraftMutation,
} from "@/features/imports/api/imports.queries"
import { RecipeImportDraftPageSkeleton } from "@/features/imports/ui/RecipeImportSkeletons"
import { type Recipe, uploadRecipeImage } from "@/features/recipes/api/recipes.api"
import { recipeQueryKeys } from "@/features/recipes/api/recipes.queries"
import {
  recipeRequestToFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { RecipeImageLightbox } from "@/features/recipes/ui/RecipeImageLightbox"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { PageSection } from "@/shared/ui/page"

export default function RecipeImportDraftPage() {
  const { draftId } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const draftQuery = useRecipeImportDraftQuery(draftId)
  const updateMutation = useUpdateRecipeImportDraftMutation(draftId ?? "")
  const updateDraft = updateMutation.mutate
  const confirmMutation = useConfirmRecipeImportDraftMutation(draftId ?? "")
  const deleteMutation = useDeleteRecipeImportDraftMutation()
  const autosaveTimerRef = useRef<ReturnType<typeof setTimeout>>(undefined)
  const [autosaveError, setAutosaveError] = useState<unknown>()
  const [coverError, setCoverError] = useState<unknown>()

  useEffect(
    () => () => {
      if (autosaveTimerRef.current) {
        clearTimeout(autosaveTimerRef.current)
      }
    },
    [],
  )

  const handleValuesChange = useCallback(
    (values: RecipeFormValues) => {
      if (!draftId || draftQuery.data?.status !== "Ready") {
        return
      }

      if (autosaveTimerRef.current) {
        clearTimeout(autosaveTimerRef.current)
      }

      autosaveTimerRef.current = setTimeout(() => {
        setAutosaveError(undefined)
        updateDraft(toRecipeRequest(values), {
          onError: setAutosaveError,
        })
      }, 800)
    },
    [draftId, draftQuery.data?.status, updateDraft],
  )

  function handleConfirm(values: RecipeFormValues, coverFile: File | null) {
    setCoverError(undefined)
    confirmMutation.mutate(toRecipeRequest(values), {
      onSuccess: (recipe) => {
        void uploadCoverAndNavigate(recipe.id, values.title, coverFile)
      },
    })
  }

  async function uploadCoverAndNavigate(recipeId: string, title: string, coverFile: File | null) {
    try {
      if (coverFile) {
        const image = await uploadRecipeImage(recipeId, {
          file: coverFile,
          scope: "Cover",
          altText: title,
        })
        queryClient.setQueryData<Recipe>(recipeQueryKeys.detail(recipeId), (recipe) =>
          recipe
            ? {
                ...recipe,
                images: [
                  ...recipe.images.filter((existingImage) => existingImage.scope !== "Cover"),
                  image,
                ],
              }
            : recipe,
        )
        void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
      }
      toast.success("Рецепт создан")
      void navigate(`/recipes/${recipeId}`, { replace: true })
    } catch (error) {
      setCoverError(error)
      toast.warning("Рецепт создан, но обложку загрузить не удалось")
      void navigate(`/recipes/${recipeId}/edit`, { replace: true })
    }
  }

  function handleDelete() {
    if (!draftId) {
      return
    }

    deleteMutation.mutate(draftId, {
      onSuccess: () => {
        void navigate("/recipes/import", { replace: true })
      },
    })
  }

  if (draftQuery.isPending) {
    return <RecipeImportDraftPageSkeleton />
  }

  if (!draftQuery.data) {
    return <ErrorAlert error={draftQuery.error} />
  }

  const draft = draftQuery.data
  const suggestedCoverData = draft.evidence.suggestedCover
  const suggestedCoverSource = suggestedCoverData
    ? draft.sourceImages[suggestedCoverData.sourceImageIndex]
    : undefined
  const suggestedCover =
    suggestedCoverData && suggestedCoverSource
      ? {
          id: `${draft.id}:${String(suggestedCoverData.sourceImageIndex)}`,
          previewUrl: suggestedCoverSource.readUrl ?? undefined,
          loadSourceImage: (signal?: AbortSignal) =>
            getRecipeImportSourceImage(draft.id, suggestedCoverData.sourceImageIndex, signal),
          x: suggestedCoverData.x,
          y: suggestedCoverData.y,
          width: suggestedCoverData.width,
          height: suggestedCoverData.height,
        }
      : undefined

  return (
    <div className="space-y-5">
      <PageSection
        title="Исходные изображения"
        description="Сверьте распознанные поля со всеми оригиналами перед созданием рецепта."
        action={
          <Button
            type="button"
            variant="outline"
            onClick={handleDelete}
            disabled={deleteMutation.isPending}
          >
            <Trash2 />
            Удалить черновик
          </Button>
        }
      >
        <div className="grid gap-3 md:grid-cols-2">
          {draft.sourceImages.map((image, index) =>
            image.readUrl ? (
              <RecipeImageLightbox
                key={`${image.fileName}-${String(index)}`}
                imageUrl={image.readUrl}
                imageAlt={`Исходное изображение рецепта ${String(index + 1)}`}
              >
                <button
                  type="button"
                  className="focus-visible:ring-ring rounded-lg focus-visible:ring-2 focus-visible:outline-none"
                >
                  <img
                    src={image.readUrl}
                    alt={`Исходное изображение рецепта ${String(index + 1)}`}
                    className="max-h-[32rem] w-full rounded-lg border object-contain"
                  />
                </button>
              </RecipeImageLightbox>
            ) : null,
          )}
        </div>
        {draft.evidence.warnings.length > 0 ? (
          <Alert>
            <Info />
            <AlertTitle>Проверьте спорные места</AlertTitle>
            <AlertDescription>
              <ul className="list-disc pl-4">
                {draft.evidence.warnings.map((warning) => (
                  <li key={warning}>{warning}</li>
                ))}
              </ul>
            </AlertDescription>
          </Alert>
        ) : null}
      </PageSection>

      {draft.status === "Confirmed" && draft.createdRecipeId ? (
        <Alert>
          <CheckCircle2 />
          <AlertTitle>Рецепт уже создан</AlertTitle>
          <AlertDescription>
            <Button asChild variant="link" className="h-auto p-0">
              <Link to={`/recipes/${draft.createdRecipeId}`}>
                Открыть рецепт
                <ExternalLink />
              </Link>
            </Button>
          </AlertDescription>
        </Alert>
      ) : (
        <>
          {autosaveError ? <ErrorAlert error={autosaveError} /> : null}
          <p className="text-muted-foreground text-right text-sm">
            {updateMutation.isPending
              ? "Сохраняем черновик..."
              : "Изменения сохраняются автоматически"}
          </p>
          <RecipeForm
            initialValues={recipeRequestToFormValues(draft.recipe)}
            submitLabel="Создать рецепт"
            isSubmitting={confirmMutation.isPending}
            error={confirmMutation.error ?? coverError}
            onValuesChange={handleValuesChange}
            suggestedCover={suggestedCover}
            generateCover={(values) => generateRecipeCoverImage(toRecipeRequest(values))}
            onSubmit={handleConfirm}
          />
        </>
      )}
    </div>
  )
}

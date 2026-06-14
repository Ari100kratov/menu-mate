import { CheckCircle2, ExternalLink, Info, Trash2 } from "lucide-react"
import { useCallback, useEffect, useRef, useState } from "react"
import { Link, useNavigate, useParams } from "react-router-dom"
import { toast } from "sonner"

import {
  useConfirmRecipeImportDraftMutation,
  useDeleteRecipeImportDraftMutation,
  useRecipeImportDraftQuery,
  useUpdateRecipeImportDraftMutation,
} from "@/features/imports/api/imports.queries"
import { uploadRecipeImage } from "@/features/recipes/api/recipes.api"
import {
  recipeRequestToFormValues,
  toRecipeRequest,
  type RecipeFormValues,
} from "@/features/recipes/model/recipe-form"
import { RecipeForm } from "@/features/recipes/ui/RecipeForm"
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { PageSection } from "@/shared/ui/page"

export default function RecipeImportDraftPage() {
  const { draftId } = useParams()
  const navigate = useNavigate()
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
        await uploadRecipeImage(recipeId, {
          file: coverFile,
          scope: "Cover",
          altText: title,
        })
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
    return <PageSkeleton />
  }

  if (!draftQuery.data) {
    return <ErrorAlert error={draftQuery.error} />
  }

  const draft = draftQuery.data

  return (
    <div className="space-y-5">
      <PageSection
        title="Исходные скриншоты"
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
              <img
                key={`${image.fileName}-${String(index)}`}
                src={image.readUrl}
                alt={`Исходный скриншот рецепта ${String(index + 1)}`}
                className="max-h-[32rem] w-full rounded-lg border object-contain"
              />
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
            onSubmit={handleConfirm}
          />
        </>
      )}
    </div>
  )
}

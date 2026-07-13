import { FileImage, ImagePlus, Trash2 } from "lucide-react"
import { useEffect, useMemo, useState } from "react"
import { Link, useNavigate } from "react-router-dom"

import {
  useCreateRecipeImportDraftMutation,
  useDeleteRecipeImportDraftMutation,
  useRecipeImportDraftsQuery,
} from "@/features/imports/api/imports.queries"
import { RecipeImageLightbox } from "@/features/recipes/ui/RecipeImageLightbox"
import { cn } from "@/shared/lib/utils"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/ui/alert-dialog"
import { Button, buttonVariants } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { PageSection } from "@/shared/ui/page"

export default function RecipeImportPage() {
  const navigate = useNavigate()
  const draftsQuery = useRecipeImportDraftsQuery()
  const createMutation = useCreateRecipeImportDraftMutation()
  const deleteMutation = useDeleteRecipeImportDraftMutation()
  const [files, setFiles] = useState<File[]>([])
  const previewUrls = useMemo(() => files.map((file) => URL.createObjectURL(file)), [files])

  useEffect(
    () => () => {
      previewUrls.forEach((previewUrl) => {
        URL.revokeObjectURL(previewUrl)
      })
    },
    [previewUrls],
  )

  function addFiles(nextFiles: Iterable<File>) {
    setFiles((currentFiles) => [...currentFiles, ...nextFiles].slice(0, 8))
  }

  function handleFileChange(nextFiles: FileList | null) {
    if (nextFiles) {
      addFiles(Array.from(nextFiles))
    }
  }

  function removeFile(index: number) {
    setFiles((currentFiles) => currentFiles.filter((_, fileIndex) => fileIndex !== index))
  }

  useEffect(() => {
    function handlePaste(event: ClipboardEvent) {
      if (!event.clipboardData) {
        return
      }

      const pastedFiles = getPastedImageFiles(event.clipboardData)
      if (pastedFiles.length === 0) {
        return
      }

      event.preventDefault()
      setFiles((currentFiles) => [...currentFiles, ...pastedFiles].slice(0, 8))
    }

    window.addEventListener("paste", handlePaste)
    return () => {
      window.removeEventListener("paste", handlePaste)
    }
  }, [])

  function handleUpload() {
    if (files.length === 0) {
      return
    }

    createMutation.mutate(files, {
      onSuccess: (draft) => {
        void navigate(`/recipes/import/${draft.id}`)
      },
    })
  }

  return (
    <div className="space-y-5">
      <PageSection
        title="Изображения рецепта"
        description="Добавьте до 8 изображений рецепта: снимки экрана, страницы сайта или разворот книги. Поддерживаются JPEG, PNG и WebP до 10 МБ каждое и до 40 МБ суммарно. ИИ объединит их в один черновик."
      >
        <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_18rem]">
          <div className="space-y-3">
            <input
              id="recipe-import-files"
              className="sr-only"
              type="file"
              multiple
              accept="image/jpeg,image/png,image/webp"
              disabled={createMutation.isPending || files.length >= 8}
              onChange={(event) => {
                handleFileChange(event.target.files)
                event.target.value = ""
              }}
            />
            <div className="space-y-2">
              <label
                htmlFor="recipe-import-files"
                aria-disabled={createMutation.isPending}
                className={cn(
                  buttonVariants({ variant: "outline" }),
                  "h-10 w-full cursor-pointer justify-start sm:w-auto",
                  (createMutation.isPending || files.length >= 8) && "pointer-events-none opacity-50",
                )}
              >
                <FileImage />
                Добавить изображения
              </label>
              <p className="text-muted-foreground text-sm">
                {files.length > 0
                  ? `Добавлено файлов: ${String(files.length)} из 8. Изображение из буфера можно вставить через Ctrl+V.`
                  : "Можно добавлять изображения по одному или группой, а также вставить из буфера через Ctrl+V."}
              </p>
            </div>
            {createMutation.error ? <ErrorAlert error={createMutation.error} /> : null}
            <Button
              type="button"
              disabled={files.length === 0 || createMutation.isPending}
              onClick={handleUpload}
            >
              <ImagePlus />
              {createMutation.isPending ? "Распознаём рецепт..." : "Создать черновик"}
            </Button>
          </div>
          <div className="bg-muted flex min-h-48 overflow-hidden rounded-lg border p-2">
            {previewUrls.length > 0 ? (
              <div className="grid w-full grid-cols-2 gap-2">
                {previewUrls.map((previewUrl, index) => (
                  <div key={previewUrl} className="relative">
                    <RecipeImageLightbox
                      imageUrl={previewUrl}
                      imageAlt={`Предпросмотр выбранного изображения ${String(index + 1)}`}
                    >
                      <button type="button" className="focus-visible:ring-ring rounded-md focus-visible:ring-2 focus-visible:outline-none">
                        <img
                          src={previewUrl}
                          alt={`Предпросмотр выбранного изображения ${String(index + 1)}`}
                          className="max-h-64 w-full rounded-md object-contain"
                        />
                      </button>
                    </RecipeImageLightbox>
                    <Button
                      type="button"
                      variant="secondary"
                      size="icon"
                      className="absolute top-2 right-2 shadow-sm"
                      aria-label={`Удалить изображение ${String(index + 1)}`}
                      onClick={() => {
                        removeFile(index)
                      }}
                    >
                      <Trash2 />
                    </Button>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-muted-foreground flex min-h-44 flex-1 flex-col items-center justify-center gap-2 p-6 text-center text-sm">
                <FileImage className="size-8" />
                Предпросмотр появится после выбора изображений
              </div>
            )}
          </div>
        </div>
      </PageSection>

      <PageSection
        title="Недавние черновики"
        description="Исходные изображения и черновики автоматически удаляются через 7 дней после последнего изменения."
      >
        {draftsQuery.error ? <ErrorAlert error={draftsQuery.error} /> : null}
        {deleteMutation.error ? <ErrorAlert error={deleteMutation.error} /> : null}
        {draftsQuery.isPending ? (
          <PageSkeleton />
        ) : draftsQuery.data?.length ? (
          <div className="divide-y rounded-lg border">
            {draftsQuery.data.map((draft) => (
              <div key={draft.id} className="flex items-center gap-3 p-3">
                <div className="min-w-0 flex-1">
                  <Link to={`/recipes/import/${draft.id}`} className="font-medium hover:underline">
                    {draft.title}
                  </Link>
                  <p className="text-muted-foreground text-sm">
                    {draft.status === "Confirmed" ? "Рецепт создан" : "Готов к проверке"}
                  </p>
                </div>
                <DeleteDraftButton
                  title={draft.title}
                  disabled={deleteMutation.isPending}
                  onDelete={() => {
                    deleteMutation.mutate(draft.id)
                  }}
                />
              </div>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground text-sm">Черновиков пока нет.</p>
        )}
      </PageSection>
    </div>
  )
}

const supportedImageContentTypes = new Set(["image/jpeg", "image/png", "image/webp"])

function getPastedImageFiles(clipboardData: DataTransfer) {
  return Array.from(clipboardData.items)
    .filter((item) => item.kind === "file" && supportedImageContentTypes.has(item.type))
    .map((item) => item.getAsFile())
    .filter((file): file is File => file !== null)
}

function DeleteDraftButton({
  title,
  disabled,
  onDelete,
}: {
  title: string
  disabled: boolean
  onDelete: () => void
}) {
  return (
    <AlertDialog>
      <AlertDialogTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          disabled={disabled}
          aria-label="Удалить черновик"
        >
          <Trash2 />
        </Button>
      </AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Удалить черновик?</AlertDialogTitle>
          <AlertDialogDescription>
            Черновик «{title}» и исходные изображения будут удалены без возможности восстановления.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Отмена</AlertDialogCancel>
          <AlertDialogAction onClick={onDelete}>Удалить</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

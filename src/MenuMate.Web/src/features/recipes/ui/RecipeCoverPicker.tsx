import { ImagePlus, LoaderCircle, Sparkles } from "lucide-react"
import { lazy, Suspense, useEffect, useMemo, useRef, useState } from "react"

import { Button } from "@/shared/ui/button"
import { Skeleton } from "@/shared/ui/skeleton"
import type { RecipeCoverSuggestion } from "./RecipeCoverSuggestion"
import { RecipeImage } from "./RecipeImage"
import { RecipeImageLightbox } from "./RecipeImageLightbox"

const RecipeCoverSuggestionCard = lazy(async () => {
  const module = await import("./RecipeCoverSuggestion")
  return { default: module.RecipeCoverSuggestionCard }
})

interface RecipeCoverPickerProps {
  existingImageUrl?: string
  file: File | null
  onFileChange: (file: File | null) => void
  onGenerate?: () => void
  isGenerating?: boolean
  suggestedCover?: RecipeCoverSuggestion
}

export function RecipeCoverPicker({
  existingImageUrl,
  file,
  onFileChange,
  onGenerate,
  isGenerating = false,
  suggestedCover,
}: RecipeCoverPickerProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [dismissedSuggestionId, setDismissedSuggestionId] = useState<string>()
  const previewUrl = useMemo(() => (file ? URL.createObjectURL(file) : undefined), [file])

  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl)
      }
    }
  }, [previewUrl])

  const imageUrl = previewUrl ?? existingImageUrl

  return (
    <section className="border-b p-4 md:p-6">
      <input
        ref={inputRef}
        className="sr-only"
        type="file"
        accept="image/jpeg,image/png,image/webp,image/avif"
        onChange={(event) => {
          onFileChange(event.target.files?.[0] ?? null)
        }}
      />

      <div className="group bg-muted/50 hover:border-primary/50 grid w-full grid-cols-[5.5rem_minmax(0,1fr)] items-center gap-4 overflow-hidden rounded-xl border p-2 transition">
        {imageUrl ? (
          <RecipeImageLightbox imageUrl={imageUrl} imageAlt="Обложка рецепта">
            <button
              type="button"
              className="focus-visible:ring-ring rounded-lg focus-visible:ring-2 focus-visible:outline-none"
              aria-label="Открыть обложку на весь экран"
            >
              <RecipeImage
                frameClassName="aspect-square size-[5.5rem] rounded-lg"
                imageClassName="object-cover"
                src={imageUrl}
                alt=""
                loading="eager"
              />
            </button>
          </RecipeImageLightbox>
        ) : (
          <button
            type="button"
            className="bg-muted text-muted-foreground focus-visible:ring-ring flex aspect-square size-[5.5rem] items-center justify-center rounded-lg focus-visible:ring-2 focus-visible:outline-none"
            aria-label="Выбрать обложку"
            onClick={() => {
              inputRef.current?.click()
            }}
          >
            <ImagePlus className="size-7" />
          </button>
        )}
        <button
          type="button"
          className="min-w-0 text-left outline-none"
          onClick={() => {
            inputRef.current?.click()
          }}
        >
          <span>
            <span className="type-subsection-title block">
              {imageUrl ? "Обложка рецепта" : "Добавить обложку"}
            </span>
            <span className="type-supporting text-muted-foreground mt-1 block">
              {imageUrl ? "Нажмите, чтобы заменить изображение" : "JPEG, PNG, WebP или AVIF"}
            </span>
          </span>
        </button>
      </div>

      {suggestedCover && dismissedSuggestionId !== suggestedCover.id && !file ? (
        <Suspense fallback={<Skeleton className="mt-3 h-36 w-full rounded-lg" />}>
          <RecipeCoverSuggestionCard
            key={suggestedCover.id}
            suggestion={suggestedCover}
            onApply={onFileChange}
            onDismiss={() => {
              setDismissedSuggestionId(suggestedCover.id)
            }}
          />
        </Suspense>
      ) : null}

      {onGenerate ? (
        <Button
          type="button"
          variant="outline"
          className="mt-3 w-full"
          disabled={isGenerating}
          onClick={onGenerate}
        >
          {isGenerating ? <LoaderCircle className="animate-spin" /> : <Sparkles />}
          {isGenerating ? "Генерируем обложку..." : "Сгенерировать фото блюда с ИИ"}
        </Button>
      ) : null}

      {file ? (
        <div className="mt-2 flex items-center justify-between gap-3">
          <p className="type-supporting text-muted-foreground truncate">{file.name}</p>
          <Button
            type="button"
            variant="ghost"
            size="xs"
            onClick={() => {
              onFileChange(null)
              if (inputRef.current) {
                inputRef.current.value = ""
              }
            }}
          >
            {existingImageUrl ? "Отменить замену" : "Удалить изображение"}
          </Button>
        </div>
      ) : null}
    </section>
  )
}

export type { RecipeCoverSuggestion } from "./RecipeCoverSuggestion"

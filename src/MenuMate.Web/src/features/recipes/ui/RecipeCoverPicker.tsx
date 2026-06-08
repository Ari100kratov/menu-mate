import { ImagePlus } from "lucide-react"
import { useEffect, useMemo, useRef } from "react"

import { Button } from "@/shared/ui/button"

interface RecipeCoverPickerProps {
  existingImageUrl?: string
  file: File | null
  onFileChange: (file: File | null) => void
}

export function RecipeCoverPicker({
  existingImageUrl,
  file,
  onFileChange,
}: RecipeCoverPickerProps) {
  const inputRef = useRef<HTMLInputElement>(null)
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

      <button
        type="button"
        className="group bg-muted/50 grid w-full grid-cols-[5.5rem_minmax(0,1fr)] items-center gap-4 overflow-hidden rounded-xl border p-2 text-left outline-none transition hover:border-primary/50 focus-visible:ring-2 focus-visible:ring-ring"
        onClick={() => {
          inputRef.current?.click()
        }}
      >
        {imageUrl ? (
          <img className="bg-muted aspect-square size-[5.5rem] rounded-lg object-cover" src={imageUrl} alt="" />
        ) : (
          <span className="bg-muted text-muted-foreground flex aspect-square size-[5.5rem] items-center justify-center rounded-lg">
            <ImagePlus className="size-7" />
          </span>
        )}
        <span className="min-w-0">
          <span className="type-subsection-title block">
            {imageUrl ? "Обложка рецепта" : "Добавить обложку"}
          </span>
          <span className="type-supporting text-muted-foreground mt-1 block">
            {imageUrl ? "Нажмите, чтобы заменить изображение" : "JPEG, PNG, WebP или AVIF"}
          </span>
        </span>
      </button>

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
            Отменить замену
          </Button>
        </div>
      ) : null}
    </section>
  )
}

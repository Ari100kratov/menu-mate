import { ImageIcon } from "lucide-react"

import type { RecipeImage } from "@/features/recipes/api/recipes.api"

interface RecipeImagePreviewProps {
  image: RecipeImage | undefined
  fallbackTitle: string
}

export function RecipeImagePreview({ image, fallbackTitle }: RecipeImagePreviewProps) {
  if (image?.readUrl) {
    return (
      <img
        className="bg-muted aspect-[4/3] w-full rounded-md border object-cover"
        src={image.readUrl}
        alt={image.altText ?? fallbackTitle}
      />
    )
  }

  return (
    <div className="bg-muted text-muted-foreground flex aspect-[4/3] w-full flex-col items-center justify-center gap-2 rounded-md border">
      <ImageIcon className="size-8" />
      <span className="text-sm">Изображение не добавлено</span>
    </div>
  )
}

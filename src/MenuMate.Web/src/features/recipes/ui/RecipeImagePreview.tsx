import { ImageIcon } from "lucide-react"

import type { RecipeImage } from "@/features/recipes/api/recipes.api"

interface RecipeImagePreviewProps {
  image: RecipeImage | undefined
  fallbackTitle: string
}

export function RecipeImagePreview({ image, fallbackTitle }: RecipeImagePreviewProps) {
  if (image?.readUrl) {
    return (
      <figure className="space-y-1.5">
        <img
          className="bg-muted aspect-[4/3] w-full rounded-md border object-cover"
          src={image.readUrl}
          alt={image.altText ?? fallbackTitle}
        />
        <ImageAttribution image={image} />
      </figure>
    )
  }

  return (
    <div className="bg-muted text-muted-foreground flex aspect-[4/3] w-full flex-col items-center justify-center gap-2 rounded-md border">
      <ImageIcon className="size-8" />
      <span className="type-supporting">Изображение не добавлено</span>
    </div>
  )
}

function ImageAttribution({ image }: { image: RecipeImage }) {
  if (!image.authorName && !image.licenseName && !image.sourceUrl) {
    return null
  }

  return (
    <figcaption className="type-supporting text-muted-foreground flex flex-wrap gap-x-1">
      <span>Фото:</span>
      {image.authorName ? <span>{image.authorName}</span> : null}
      {image.licenseName ? (
        image.licenseUrl ? (
          <a
            className="hover:text-foreground underline"
            href={image.licenseUrl}
            target="_blank"
            rel="noreferrer"
          >
            {image.licenseName}
          </a>
        ) : (
          <span>{image.licenseName}</span>
        )
      ) : null}
      {image.sourceUrl ? (
        <a
          className="hover:text-foreground underline"
          href={image.sourceUrl}
          target="_blank"
          rel="noreferrer"
        >
          Источник
        </a>
      ) : null}
    </figcaption>
  )
}

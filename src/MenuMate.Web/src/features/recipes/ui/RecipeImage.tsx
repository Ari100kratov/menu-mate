import { useState, type ComponentProps } from "react"

import { cn } from "@/shared/lib/utils"

interface RecipeImageProps extends Omit<ComponentProps<"img">, "className" | "onLoad"> {
  frameClassName?: string
  imageClassName?: string
}

export function RecipeImage({
  src,
  alt,
  frameClassName,
  imageClassName,
  loading = "lazy",
  decoding = "async",
  ...props
}: RecipeImageProps) {
  const [loadedSource, setLoadedSource] = useState<string>()
  const isLoaded = loadedSource === src

  return (
    <span className={cn("bg-muted relative block overflow-hidden", frameClassName)}>
      <span
        aria-hidden="true"
        className={cn(
          "bg-accent absolute inset-0 transition-opacity",
          isLoaded
            ? "pointer-events-none opacity-0"
            : "animate-pulse opacity-100 motion-reduce:animate-none",
        )}
      />
      <img
        {...props}
        src={src}
        alt={alt}
        loading={loading}
        decoding={decoding}
        className={cn(
          "size-full opacity-0 transition-opacity duration-300",
          isLoaded && "opacity-100",
          imageClassName,
        )}
        onLoad={() => {
          setLoadedSource(src)
        }}
      />
    </span>
  )
}

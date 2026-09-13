import { useEffect, useRef, useState, type ComponentProps } from "react"

import { cn } from "@/shared/lib/utils"
import { observeImageVisibility } from "@/shared/lib/observe-image-visibility"
import { safeImageUrl } from "@/shared/lib/safe-image-url"

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
  const frameRef = useRef<HTMLSpanElement>(null)
  const [isNearViewport, setIsNearViewport] = useState(false)
  const shouldRender = loading === "eager" || isNearViewport
  const isLoaded = shouldRender && loadedSource === src

  useEffect(() => {
    if (loading === "eager" || !frameRef.current) return
    return observeImageVisibility(frameRef.current, (visible) => {
      setIsNearViewport(visible)
      if (!visible) setLoadedSource(undefined)
    })
  }, [loading])

  return (
    <span ref={frameRef} className={cn("bg-muted relative block overflow-hidden", frameClassName)}>
      <span
        aria-hidden="true"
        className={cn(
          "bg-accent absolute inset-0 transition-opacity",
          isLoaded
            ? "pointer-events-none opacity-0"
            : "animate-pulse opacity-100 motion-reduce:animate-none",
        )}
      />
      {shouldRender ? (
        <img
          {...props}
          src={safeImageUrl(src)}
          alt={alt}
          loading="eager"
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
      ) : null}
    </span>
  )
}

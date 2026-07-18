import { LoaderCircle, RefreshCw } from "lucide-react"
import { useEffect, useRef } from "react"

import { Button } from "@/shared/ui/button"

interface RecipeInfiniteScrollStatusProps {
  hasNextPage: boolean
  isFetchingNextPage: boolean
  hasError: boolean
  loadedCount: number
  onLoadMore: () => Promise<unknown>
}

export function RecipeInfiniteScrollStatus({
  hasNextPage,
  isFetchingNextPage,
  hasError,
  loadedCount,
  onLoadMore,
}: RecipeInfiniteScrollStatusProps) {
  const triggerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const trigger = triggerRef.current
    if (!trigger || !hasNextPage || isFetchingNextPage || hasError) {
      return
    }

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          void onLoadMore()
        }
      },
      { rootMargin: "320px 0px" },
    )

    observer.observe(trigger)
    return () => {
      observer.disconnect()
    }
  }, [hasError, hasNextPage, isFetchingNextPage, onLoadMore])

  if (loadedCount === 0) {
    return null
  }

  return (
    <div ref={triggerRef} className="flex min-h-12 items-center justify-center py-2">
      {isFetchingNextPage ? (
        <p className="type-supporting text-muted-foreground flex items-center gap-2" role="status">
          <LoaderCircle className="size-4 animate-spin" />
          Загружаем еще рецепты…
        </p>
      ) : hasError ? (
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => {
            void onLoadMore()
          }}
        >
          <RefreshCw />
          Повторить загрузку
        </Button>
      ) : hasNextPage ? (
        <span className="sr-only">Следующие рецепты загрузятся при прокрутке</span>
      ) : (
        <p className="type-supporting text-muted-foreground">Все рецепты загружены</p>
      )}
    </div>
  )
}

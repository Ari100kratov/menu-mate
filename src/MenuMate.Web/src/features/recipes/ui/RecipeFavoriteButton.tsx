import { Heart } from "lucide-react"

import { Button } from "@/shared/ui/button"
import { cn } from "@/shared/lib/utils"

interface RecipeFavoriteButtonProps {
  isFavorite: boolean
  favoriteCount: number
  disabled: boolean
  onClick: () => void
  presentation?: "card" | "action"
  className?: string
}

export function RecipeFavoriteButton({
  isFavorite,
  favoriteCount,
  disabled,
  onClick,
  presentation = "card",
  className,
}: RecipeFavoriteButtonProps) {
  const actionLabel = isFavorite ? "Убрать из избранного" : "Добавить в избранное"
  const likesLabel = favoriteCount > 0 ? `. Нравится: ${String(favoriteCount)}` : ""
  const isActionButton = presentation === "action"

  return (
    <Button
      type="button"
      variant="ghost"
      size={isActionButton ? "icon" : "sm"}
      className={cn(
        "tabular-nums",
        isActionButton
          ? favoriteCount > 0
            ? "h-9 w-auto gap-1.5 rounded-md px-2"
            : "size-9 rounded-md"
          : "gap-1.5 rounded-full",
        className,
      )}
      aria-label={`${actionLabel}${likesLabel}`}
      disabled={disabled}
      onClick={onClick}
    >
      {favoriteCount > 0 ? (
        <span
          className={cn(
            "min-w-[2ch] text-right leading-none",
            isActionButton ? "text-sm" : "text-xs",
          )}
        >
          {formatFavoriteCount(favoriteCount)}
        </span>
      ) : null}
      <Heart className={isFavorite ? "fill-primary text-primary size-4" : "size-4"} />
    </Button>
  )
}

function formatFavoriteCount(count: number) {
  return new Intl.NumberFormat("ru-RU", {
    notation: "compact",
    maximumFractionDigits: 1,
  }).format(count)
}

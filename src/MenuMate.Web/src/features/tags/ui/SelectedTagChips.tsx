import { X } from "lucide-react"

interface SelectedTagChipsProps {
  tags: readonly string[]
  disabled?: boolean
  onRemove: (tagName: string) => void
}

export function SelectedTagChips({ tags, disabled, onRemove }: SelectedTagChipsProps) {
  return (
    <>
      {tags.map((tag) => (
        <span
          key={tag}
          className="bg-secondary text-secondary-foreground inline-flex min-h-8 items-center gap-1 rounded-md px-2 text-xs"
        >
          {tag}
          <button
            type="button"
            className="hover:text-destructive focus-visible:ring-ring rounded-sm outline-none focus-visible:ring-2"
            aria-label={`Удалить тег ${tag}`}
            disabled={disabled}
            onClick={() => {
              onRemove(tag)
            }}
          >
            <X className="size-3" />
          </button>
        </span>
      ))}
    </>
  )
}

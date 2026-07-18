import { LoaderCircle, Plus } from "lucide-react"

import { Button } from "@/shared/ui/button"

interface TagSuggestionsProps {
  suggestions: readonly string[]
  draftTagName: string
  canCreateDraft: boolean
  isSearchPending: boolean
  isCreatePending: boolean
  disabled?: boolean
  onSelect: (tagName: string) => void
  onCreate: () => void
}

export function TagSuggestions({
  suggestions,
  draftTagName,
  canCreateDraft,
  isSearchPending,
  isCreatePending,
  disabled,
  onSelect,
  onCreate,
}: TagSuggestionsProps) {
  if (!draftTagName && suggestions.length === 0) {
    return null
  }

  if (draftTagName && isSearchPending) {
    return (
      <div className="type-supporting text-muted-foreground flex items-center gap-2 px-1">
        <LoaderCircle className="size-4 animate-spin" />
        Ищем существующие теги...
      </div>
    )
  }

  return (
    <div className="flex flex-wrap gap-2">
      {suggestions.map((tag) => (
        <Button
          key={tag}
          type="button"
          variant="secondary"
          size="sm"
          disabled={disabled}
          onMouseDown={(event) => {
            event.preventDefault()
          }}
          onClick={() => {
            onSelect(tag)
          }}
        >
          {tag}
        </Button>
      ))}

      {canCreateDraft ? (
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={(disabled ?? false) || isCreatePending}
          onMouseDown={(event) => {
            event.preventDefault()
          }}
          onClick={onCreate}
        >
          <Plus />
          {isCreatePending ? "Создаем..." : `Создать «${draftTagName}»`}
        </Button>
      ) : null}
    </div>
  )
}

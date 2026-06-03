import { Plus } from "lucide-react"

import type { TagItem } from "@/features/tags/api/tags.api"
import { Button } from "@/shared/ui/button"

interface TagSuggestionsProps {
  suggestions: readonly TagItem[]
  draftTagName: string
  canCreateDraft: boolean
  isCreatePending: boolean
  disabled?: boolean
  onSelect: (tagName: string) => void
  onCreate: () => void
}

export function TagSuggestions({
  suggestions,
  draftTagName,
  canCreateDraft,
  isCreatePending,
  disabled,
  onSelect,
  onCreate,
}: TagSuggestionsProps) {
  if (!draftTagName && suggestions.length === 0) {
    return null
  }

  return (
    <div className="flex flex-wrap gap-2">
      {suggestions.map((tag) => (
        <Button
          key={tag.id}
          type="button"
          variant="secondary"
          size="sm"
          disabled={disabled}
          onMouseDown={(event) => {
            event.preventDefault()
          }}
          onClick={() => {
            onSelect(tag.name)
          }}
        >
          {tag.name}
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

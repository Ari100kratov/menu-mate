import { Search } from "lucide-react"
import { useMemo, useState, type KeyboardEvent } from "react"

import { useTagsQuery } from "@/features/tags/api/tags.queries"
import { normalizeTagValue } from "@/features/tags/model/tag-values"
import { SelectedTagChips } from "@/features/tags/ui/SelectedTagChips"
import { TagSuggestions } from "@/features/tags/ui/TagSuggestions"
import type { RecipeListTagFilter } from "@/features/recipes/model/recipe-list-filter-state"
import { useDebouncedValue } from "@/shared/lib/use-debounced-value"
import { Input } from "@/shared/ui/input"

interface RecipeTagFilterProps {
  selectedTags: RecipeListTagFilter[]
  onChange: (tags: RecipeListTagFilter[]) => void
}

export function RecipeTagFilter({ selectedTags, onChange }: RecipeTagFilterProps) {
  const [draft, setDraft] = useState("")
  const selectedTagIds = useMemo(() => new Set(selectedTags.map((tag) => tag.id)), [selectedTags])
  const debouncedDraft = useDebouncedValue(draft, 250)
  const draftTagKey = normalizeTagValue(draft)
  const hasDraftSearch = draftTagKey.length > 0
  const hasDebouncedSearch = normalizeTagValue(debouncedDraft).length > 0
  const tagsQuery = useTagsQuery({ search: debouncedDraft }, hasDraftSearch && hasDebouncedSearch)
  const isSearchPending =
    hasDraftSearch && (draftTagKey !== normalizeTagValue(debouncedDraft) || tagsQuery.isFetching)
  const matchingTags = isSearchPending
    ? []
    : (tagsQuery.data ?? []).filter((tag) => !selectedTagIds.has(tag.id)).slice(0, 6)
  const suggestions = matchingTags.map((tag) => tag.name)

  function addTag(tagName: string) {
    const tag = matchingTags.find(
      (matchingTag) => normalizeTagValue(matchingTag.name) === normalizeTagValue(tagName),
    )
    if (!tag) {
      return
    }

    onChange([...selectedTags, { id: tag.id, name: tag.name }])
    setDraft("")
  }

  function removeTag(tagName: string) {
    const tagKey = normalizeTagValue(tagName)
    onChange(selectedTags.filter((tag) => normalizeTagValue(tag.name) !== tagKey))
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter" && matchingTags.length > 0) {
      event.preventDefault()
      addTag(matchingTags[0]?.name ?? "")
      return
    }

    if (event.key === "Backspace" && !draft && selectedTags.length > 0) {
      removeTag(selectedTags[selectedTags.length - 1]?.name ?? "")
    }
  }

  return (
    <div className="space-y-2">
      <div className="border-input bg-background focus-within:ring-ring rounded-xl border p-1.5 transition-colors focus-within:ring-2 focus-within:ring-offset-2">
        <div className="flex flex-wrap gap-2">
          <SelectedTagChips tags={selectedTags.map((tag) => tag.name)} onRemove={removeTag} />

          <div className="relative min-w-40 flex-1">
            <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-2 size-4 -translate-y-1/2" />
            <Input
              type="search"
              className="h-8 rounded-lg border-0 pl-8 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0"
              value={draft}
              placeholder={selectedTags.length > 0 ? "Добавить еще тег" : "Найти тег для фильтра"}
              aria-label="Поиск тегов для фильтра рецептов"
              aria-busy={isSearchPending}
              autoComplete="off"
              onChange={(event) => {
                setDraft(event.target.value)
              }}
              onKeyDown={handleKeyDown}
            />
          </div>
        </div>
      </div>

      <TagSuggestions
        suggestions={suggestions}
        draftTagName={draft.trim()}
        canCreateDraft={false}
        isSearchPending={isSearchPending}
        isCreatePending={false}
        onSelect={addTag}
        onCreate={() => undefined}
      />

      {tagsQuery.error ? (
        <p className="text-muted-foreground text-sm">Поиск тегов сейчас недоступен.</p>
      ) : null}
    </div>
  )
}

import { Search } from "lucide-react"
import { useMemo, useState, type KeyboardEvent } from "react"

import { useCreateTagMutation, useTagsQuery } from "@/features/tags/api/tags.queries"
import {
  normalizeTagValue,
  parseTagValues,
  serializeTagValues,
} from "@/features/tags/model/tag-values"
import { getApiErrorMessages } from "@/shared/api/errors"
import { cn } from "@/shared/lib/utils"
import { Input } from "@/shared/ui/input"
import { SelectedTagChips } from "./SelectedTagChips"
import { TagSuggestions } from "./TagSuggestions"

interface TagPickerProps {
  id: string
  name: string
  value: string
  onChange: (value: string) => void
  onBlur: () => void
  disabled?: boolean
  "aria-invalid"?: boolean
}

export function TagPicker({
  id,
  name,
  value,
  onChange,
  onBlur,
  disabled,
  "aria-invalid": ariaInvalid,
}: TagPickerProps) {
  const [draft, setDraft] = useState("")
  const selectedTags = useMemo(() => parseTagValues(value), [value])
  const selectedTagKeys = useMemo(() => new Set(selectedTags.map(normalizeTagName)), [selectedTags])
  const tagsQuery = useTagsQuery({ search: draft, includeHidden: false })
  const createTagMutation = useCreateTagMutation()
  const knownTags = tagsQuery.data ?? []
  const draftTagName = draft.trim()
  const draftTagKey = normalizeTagName(draftTagName)
  const existingDraftTag = knownTags.find((tag) => normalizeTagName(tag.name) === draftTagKey)
  const suggestions = knownTags
    .filter((tag) => !selectedTagKeys.has(normalizeTagName(tag.name)))
    .slice(0, 6)
  const canAddDraft = draftTagName.length > 0 && !selectedTagKeys.has(draftTagKey)
  const createTagError = createTagMutation.error
    ? (getApiErrorMessages(createTagMutation.error)[0] ?? "Не удалось создать тег.")
    : null

  function changeTags(tags: readonly string[]) {
    onChange(serializeTagValues(tags))
  }

  function addTag(tagName: string) {
    const normalizedTagName = tagName.trim()

    if (!normalizedTagName) {
      return
    }

    const normalizedKey = normalizeTagName(normalizedTagName)
    if (selectedTagKeys.has(normalizedKey)) {
      setDraft("")
      return
    }

    changeTags([...selectedTags, normalizedTagName])
    setDraft("")
  }

  function removeTag(tagName: string) {
    const tagKey = normalizeTagName(tagName)
    changeTags(selectedTags.filter((selectedTag) => normalizeTagName(selectedTag) !== tagKey))
  }

  function submitDraft() {
    if (!canAddDraft || createTagMutation.isPending) {
      return
    }

    if (existingDraftTag) {
      addTag(existingDraftTag.name)
      return
    }

    createTagMutation.mutate(
      {
        name: draftTagName,
        kind: "User",
      },
      {
        onSuccess: (tag) => {
          addTag(tag.name)
        },
      },
    )
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter" || event.key === "," || event.key === "Tab") {
      if (!draftTagName) {
        return
      }

      event.preventDefault()
      submitDraft()
      return
    }

    if (event.key === "Backspace" && !draft && selectedTags.length > 0) {
      removeTag(selectedTags[selectedTags.length - 1] ?? "")
    }
  }

  return (
    <div className="space-y-2">
      <div
        className={cn(
          "border-input bg-background focus-within:ring-ring rounded-md border p-2 transition-colors focus-within:ring-2 focus-within:ring-offset-2",
          ariaInvalid ? "border-destructive" : undefined,
        )}
      >
        <div className="flex flex-wrap gap-2">
          <SelectedTagChips tags={selectedTags} disabled={disabled} onRemove={removeTag} />

          <div className="relative min-w-40 flex-1">
            <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-2 size-4 -translate-y-1/2" />
            <Input
              id={id}
              name={name}
              className="h-8 border-0 pl-8 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0"
              value={draft}
              disabled={disabled}
              aria-invalid={ariaInvalid}
              placeholder={selectedTags.length > 0 ? "Еще тег" : "Найдите или создайте тег"}
              onBlur={onBlur}
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
        draftTagName={draftTagName}
        canCreateDraft={canAddDraft && !existingDraftTag}
        isCreatePending={createTagMutation.isPending}
        disabled={disabled}
        onSelect={addTag}
        onCreate={submitDraft}
      />

      {createTagError ? (
        <p className="text-destructive text-sm font-medium">{createTagError}</p>
      ) : null}
      {tagsQuery.error ? (
        <p className="text-muted-foreground text-sm">Подсказки тегов сейчас недоступны.</p>
      ) : null}
    </div>
  )
}

const normalizeTagName = normalizeTagValue

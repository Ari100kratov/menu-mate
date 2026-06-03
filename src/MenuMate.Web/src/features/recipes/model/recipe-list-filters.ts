import { normalizeTagValue } from "@/features/tags/model/tag-values"

export function getTagOptions(tags: readonly { name: string }[], activeTag: string) {
  const tagNames = tags.map((tag) => tag.name)

  if (!activeTag) {
    return tagNames
  }

  const hasActiveTag = tagNames.some(
    (tagName) => normalizeTagValue(tagName) === normalizeTagValue(activeTag),
  )

  return hasActiveTag ? tagNames : [activeTag, ...tagNames]
}

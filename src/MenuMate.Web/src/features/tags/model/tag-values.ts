export function parseTagValues(value: string) {
  const tags = value
    .split(/[\n,]+/)
    .map((tag) => tag.trim())
    .filter(Boolean)

  const uniqueTags = new Map<string, string>()
  for (const tag of tags) {
    uniqueTags.set(normalizeTagValue(tag), tag)
  }

  return [...uniqueTags.values()]
}

export function serializeTagValues(tags: readonly string[]) {
  return tags.join(", ")
}

export function normalizeTagValue(value: string) {
  return value.trim().toLocaleLowerCase("ru-RU")
}

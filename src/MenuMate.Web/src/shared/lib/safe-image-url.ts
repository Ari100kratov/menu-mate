/** Only web images and browser-created blob previews may reach an image sink. */
export function safeImageUrl(source: string | null | undefined): string | undefined {
  if (!source) return undefined

  // Reject control characters before parsing: URL normalizes tabs/newlines in schemes.
  for (let index = 0; index < source.length; index++) {
    const code = source.charCodeAt(index)
    if (code < 32 || code === 127) return undefined
  }
  if (
    !/^https?:\/\//i.test(source) &&
    !/^blob:https?:\/\//i.test(source) &&
    !/^\/(?!\/)/.test(source)
  ) {
    return undefined
  }

  try {
    const url = new URL(source, "https://menumate.invalid")
    if (!["https:", "http:", "blob:"].includes(url.protocol)) return undefined
    // Keep existing percent escapes and signed query parameters intact. Encode URL
    // metacharacters, not HTML entities (React escapes attributes itself).
    return encodeURI(source).replace(/%25/g, "%").replace(/['()]/g, encodeURIComponentCharacter)
  } catch {
    return undefined
  }
}

function encodeURIComponentCharacter(character: string) {
  return `%${character.charCodeAt(0).toString(16).toUpperCase()}`
}

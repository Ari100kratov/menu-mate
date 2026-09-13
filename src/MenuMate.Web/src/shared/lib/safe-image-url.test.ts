import { expect, it } from "vitest"

import { safeImageUrl } from "./safe-image-url"

it.each([
  "javascript:alert(1)",
  "java\nscript:alert(1)",
  "data:text/html,<script>alert(1)</script>",
  "data:image/svg+xml,<svg onload=alert(1)>",
  "blob:javascript:alert(1)",
  "//untrusted.example/image.jpg",
  "<img src=x onerror=alert(1)>",
  "https://",
])("rejects unsafe or malformed image source %s", (source) => {
  expect(safeImageUrl(source)).toBeUndefined()
})

it.each([
  "https://storage.example/image.webp?signature=a%2Fb%2Bc&expires=123",
  "http://localhost:9000/images/recipe.jpg",
  "/api/images/recipe.jpg?revision=2",
  "blob:https://menu.example/48e17edb-2a4a-4832-a0c6-48a8d47afe64",
])("preserves web images, signed URLs and blob previews: %s", (source) => {
  expect(safeImageUrl(source)).toBe(source)
})

it("encodes URL metacharacters without HTML entities or double percent encoding", () => {
  expect(safeImageUrl('https://images.example/еда <test>.jpg?q="x"&signature=a%2Fb')).toBe(
    "https://images.example/%D0%B5%D0%B4%D0%B0%20%3Ctest%3E.jpg?q=%22x%22&signature=a%2Fb",
  )
})

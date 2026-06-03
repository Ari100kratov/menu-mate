const apiBaseUrl = normalizeBaseUrl(import.meta.env.VITE_API_URL ?? "")

export function apiUrl(path: string): string {
  return joinUrl(apiBaseUrl, path)
}

function normalizeBaseUrl(value: string): string {
  return value.trim().replace(/\/+$/, "")
}

function joinUrl(baseUrl: string, path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`
  return `${baseUrl}${normalizedPath}`
}

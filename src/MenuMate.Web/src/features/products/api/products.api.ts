import { apiClient } from "@/shared/api/client"
import type { components } from "@/shared/api/generated/schema"
import { unwrapApiResponse } from "@/shared/api/unwrap"

export type Product = components["schemas"]["ProductResponse"]

export async function getProducts(search: string) {
  return unwrapApiResponse<Product[]>(
    apiClient.GET("/api/products", {
      params: {
        query: {
          search: normalizeProductSearch(search) || undefined,
        },
      },
    }),
  )
}

export function normalizeProductSearch(value: string) {
  return value.trim().toLocaleLowerCase("ru-RU").replaceAll("ё", "е").replaceAll(/\s+/g, " ")
}

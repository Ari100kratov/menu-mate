import { useQuery } from "@tanstack/react-query"

import { getProducts, normalizeProductSearch } from "@/features/products/api/products.api"

export function useProductsQuery(search: string, enabled = true) {
  const normalizedSearch = normalizeProductSearch(search)

  return useQuery({
    queryKey: ["products", normalizedSearch],
    queryFn: () => getProducts(search),
    enabled: enabled && Boolean(normalizedSearch),
    staleTime: 60_000,
  })
}

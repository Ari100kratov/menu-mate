import { keepPreviousData, useQuery } from "@tanstack/react-query"

import { getProducts, normalizeProductSearch } from "@/features/products/api/products.api"

export function useProductsQuery(search: string) {
  return useQuery({
    queryKey: ["products", normalizeProductSearch(search)],
    queryFn: () => getProducts(search),
    placeholderData: keepPreviousData,
    staleTime: 60_000,
  })
}

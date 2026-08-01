import { useQuery } from "@tanstack/react-query"

import { readOfflineShoppingRecord } from "@/features/shopping-lists/model/shopping-list-offline.storage"

export const offlineShoppingRecordQueryKey = ["shopping-list", "offline-record"] as const

export function useOfflineShoppingRecordQuery() {
  return useQuery({
    queryKey: offlineShoppingRecordQueryKey,
    queryFn: readOfflineShoppingRecord,
    staleTime: Number.POSITIVE_INFINITY,
    gcTime: Number.POSITIVE_INFINITY,
    networkMode: "always",
  })
}

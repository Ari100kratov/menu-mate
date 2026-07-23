import { useQuery } from "@tanstack/react-query"

import { getAdminUsers, type AdminUsersFilters } from "@/features/admin/api/admin.api"

export const adminQueryKeys = {
  users: (filters: AdminUsersFilters) =>
    [
      "admin",
      "users",
      {
        search: filters.search?.trim() ?? "",
        page: filters.page ?? 1,
        pageSize: filters.pageSize ?? 20,
      },
    ] as const,
}

export function useAdminUsersQuery(filters: AdminUsersFilters) {
  return useQuery({
    queryKey: adminQueryKeys.users(filters),
    queryFn: () => getAdminUsers(filters),
    staleTime: 30_000,
  })
}

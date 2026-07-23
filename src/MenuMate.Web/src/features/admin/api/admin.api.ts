import { apiFetchJson } from "@/shared/api/client"

export interface AdminUserListItem {
  id: string
  email: string
  displayName: string
  registeredAt: string
  roles: string[]
  recipesCount: number
}

export interface AdminUsersPage {
  items: AdminUserListItem[]
  totalCount: number
  page: number
  pageSize: number
}

export interface AdminUsersFilters {
  search?: string
  page?: number
  pageSize?: number
}

export async function getAdminUsers(filters: AdminUsersFilters) {
  const searchParams = new URLSearchParams({
    page: String(filters.page ?? 1),
    pageSize: String(filters.pageSize ?? 20),
  })
  const search = filters.search?.trim()
  if (search) {
    searchParams.set("search", search)
  }

  return apiFetchJson<AdminUsersPage>(`/api/admin/users?${searchParams.toString()}`)
}

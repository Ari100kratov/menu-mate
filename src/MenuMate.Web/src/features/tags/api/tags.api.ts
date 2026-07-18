import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse } from "@/shared/api/unwrap"
import type { components } from "@/shared/api/generated/schema"

export type TagItem = components["schemas"]["TagResponse"]
export type CreateTagRequest = components["schemas"]["CreateTagRequest"]

export interface TagListFilters {
  search?: string
}

export async function getTags(filters: TagListFilters) {
  return unwrapApiResponse<TagItem[]>(
    apiClient.GET("/api/tags", {
      params: {
        query: {
          search: normalizeQueryValue(filters.search),
        },
      },
    }),
  )
}

export async function createTag(request: CreateTagRequest) {
  return unwrapApiResponse<TagItem>(
    apiClient.POST("/api/tags", {
      body: request,
    }),
  )
}

function normalizeQueryValue(value: string | undefined) {
  const normalized = value?.trim()

  if (!normalized) {
    return undefined
  }

  return normalized
}

import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import type { components } from "@/shared/api/generated/schema"

export type TagItem = components["schemas"]["TagResponse"]
export type CreateTagRequest = components["schemas"]["CreateTagRequest"]

export interface TagListFilters {
  search?: string
  includeHidden?: boolean
}

export async function getTags(filters: TagListFilters) {
  return unwrapApiResponse<TagItem[]>(
    apiClient.GET("/api/tags", {
      params: {
        query: {
          search: normalizeQueryValue(filters.search),
          includeHidden: filters.includeHidden ?? false,
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

export async function confirmTag(tagId: string) {
  await unwrapEmptyApiResponse(
    apiClient.POST("/api/tags/{tagId}/confirm", {
      params: {
        path: {
          tagId,
        },
      },
    }),
  )
}

export async function hideTag(tagId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/tags/{tagId}", {
      params: {
        path: {
          tagId,
        },
      },
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

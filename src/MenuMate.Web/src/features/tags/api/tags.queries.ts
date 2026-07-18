import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  createTag,
  getTags,
  type CreateTagRequest,
  type TagListFilters,
} from "@/features/tags/api/tags.api"

const emptyTagFilters = {
  search: "",
} as const

export const tagQueryKeys = {
  all: ["tags"] as const,
  lists: () => [...tagQueryKeys.all, "list"] as const,
  list: (filters: TagListFilters) =>
    [
      ...tagQueryKeys.lists(),
      {
        search: filters.search?.trim() ?? emptyTagFilters.search,
      },
    ] as const,
}

export function useTagsQuery(filters: TagListFilters, enabled = true) {
  return useQuery({
    queryKey: tagQueryKeys.list(filters),
    queryFn: () => getTags(filters),
    enabled,
    staleTime: 30_000,
  })
}

export function useCreateTagMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateTagRequest) => createTag(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: tagQueryKeys.lists() })
    },
  })
}

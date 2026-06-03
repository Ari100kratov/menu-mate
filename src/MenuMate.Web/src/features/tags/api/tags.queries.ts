import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  confirmTag,
  createTag,
  getTags,
  hideTag,
  type CreateTagRequest,
  type TagListFilters,
} from "@/features/tags/api/tags.api"

const emptyTagFilters = {
  search: "",
  includeHidden: false,
} as const

export const tagQueryKeys = {
  all: ["tags"] as const,
  lists: () => [...tagQueryKeys.all, "list"] as const,
  list: (filters: TagListFilters) =>
    [
      ...tagQueryKeys.lists(),
      {
        search: filters.search?.trim() ?? emptyTagFilters.search,
        includeHidden: filters.includeHidden ?? emptyTagFilters.includeHidden,
      },
    ] as const,
}

export function useTagsQuery(filters: TagListFilters) {
  return useQuery({
    queryKey: tagQueryKeys.list(filters),
    queryFn: () => getTags(filters),
    placeholderData: keepPreviousData,
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

export function useConfirmTagMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: confirmTag,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: tagQueryKeys.lists() })
    },
  })
}

export function useHideTagMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: hideTag,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: tagQueryKeys.lists() })
    },
  })
}

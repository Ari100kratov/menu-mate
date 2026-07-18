import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  confirmRecipeImportDraft,
  createRecipeImportDraft,
  deleteRecipeImportDraft,
  getRecipeImportDraft,
  getRecipeImportDrafts,
  updateRecipeImportDraft,
} from "@/features/imports/api/imports.api"
import { recipeQueryKeys } from "@/features/recipes/api/recipes.queries"
import { tagQueryKeys } from "@/features/tags/api/tags.queries"
import type { CreateRecipeRequest } from "@/features/recipes/api/recipes.api"

export const recipeImportQueryKeys = {
  all: ["recipe-imports"] as const,
  list: () => [...recipeImportQueryKeys.all, "list"] as const,
  detail: (draftId: string) => [...recipeImportQueryKeys.all, "detail", draftId] as const,
}

export function useRecipeImportDraftsQuery() {
  return useQuery({
    queryKey: recipeImportQueryKeys.list(),
    queryFn: getRecipeImportDrafts,
  })
}

export function useRecipeImportDraftQuery(draftId: string | undefined) {
  return useQuery({
    queryKey: recipeImportQueryKeys.detail(draftId ?? ""),
    queryFn: () => getRecipeImportDraft(draftId ?? ""),
    enabled: Boolean(draftId),
  })
}

export function useCreateRecipeImportDraftMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createRecipeImportDraft,
    onSuccess: (draft) => {
      queryClient.setQueryData(recipeImportQueryKeys.detail(draft.id), draft)
      void queryClient.invalidateQueries({ queryKey: recipeImportQueryKeys.list() })
    },
  })
}

export function useUpdateRecipeImportDraftMutation(draftId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (recipe: CreateRecipeRequest) => updateRecipeImportDraft(draftId, recipe),
    onSuccess: (draft) => {
      queryClient.setQueryData(recipeImportQueryKeys.detail(draftId), draft)
      void queryClient.invalidateQueries({ queryKey: recipeImportQueryKeys.list() })
    },
  })
}

export function useConfirmRecipeImportDraftMutation(draftId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (recipe: CreateRecipeRequest) => confirmRecipeImportDraft(draftId, recipe),
    onSuccess: (recipe) => {
      queryClient.setQueryData(recipeQueryKeys.detail(recipe.id), recipe)
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
      void queryClient.invalidateQueries({ queryKey: tagQueryKeys.lists() })
      void queryClient.invalidateQueries({ queryKey: recipeImportQueryKeys.all })
    },
  })
}

export function useDeleteRecipeImportDraftMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deleteRecipeImportDraft,
    onSuccess: (_data, draftId) => {
      queryClient.removeQueries({ queryKey: recipeImportQueryKeys.detail(draftId) })
      void queryClient.invalidateQueries({ queryKey: recipeImportQueryKeys.list() })
    },
  })
}

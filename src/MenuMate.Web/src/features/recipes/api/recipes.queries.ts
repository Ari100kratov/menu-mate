import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { tagQueryKeys } from "@/features/tags/api/tags.queries"

import {
  copyRecipe,
  createRecipe,
  deleteRecipeImage,
  deleteRecipe,
  getRecipe,
  getRecipes,
  setRecipeFavorite,
  updateRecipe,
  uploadRecipeImage,
  type CreateRecipeRequest,
  type CopyRecipeRequest,
  type RecipeListFilters,
  type UpdateRecipeRequest,
  type UploadRecipeImageRequest,
} from "@/features/recipes/api/recipes.api"

const normalizedEmptyFilters = {
  scope: "library",
  search: "",
  tagIds: [] as string[],
  category: "",
  favoritesOnly: false,
  availableOnly: false,
} as const

export const recipeListPageSize = 20

export const recipeQueryKeys = {
  all: ["recipes"] as const,
  lists: () => [...recipeQueryKeys.all, "list"] as const,
  list: (filters: RecipeListFilters) =>
    [
      ...recipeQueryKeys.lists(),
      {
        scope: filters.scope ?? normalizedEmptyFilters.scope,
        search: filters.search?.trim() ?? normalizedEmptyFilters.search,
        tagIds: [...(filters.tagIds ?? normalizedEmptyFilters.tagIds)].sort(),
        category: filters.category?.trim() ?? normalizedEmptyFilters.category,
        favoritesOnly: filters.favoritesOnly ?? normalizedEmptyFilters.favoritesOnly,
        availableOnly: filters.availableOnly ?? normalizedEmptyFilters.availableOnly,
      },
    ] as const,
  details: () => [...recipeQueryKeys.all, "detail"] as const,
  detail: (recipeId: string, revisionId?: string) =>
    [...recipeQueryKeys.details(), recipeId, revisionId ?? "current"] as const,
}

export function useInfiniteRecipesQuery(filters: RecipeListFilters) {
  return useInfiniteQuery({
    queryKey: recipeQueryKeys.list(filters),
    queryFn: ({ pageParam }) =>
      getRecipes({
        ...filters,
        page: pageParam,
        pageSize: recipeListPageSize,
      }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) =>
      lastPage.length === recipeListPageSize ? allPages.length + 1 : undefined,
    staleTime: 2 * 60_000,
    gcTime: 30 * 60_000,
  })
}

export function useRecipeQuery(recipeId: string | undefined, revisionId?: string) {
  return useQuery({
    queryKey: recipeQueryKeys.detail(recipeId ?? "", revisionId),
    queryFn: () => getRecipe(recipeId ?? "", revisionId),
    enabled: Boolean(recipeId),
    staleTime: 30_000,
  })
}

export function useCreateRecipeMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateRecipeRequest) => createRecipe(request),
    onSuccess: (recipe) => {
      queryClient.setQueryData(recipeQueryKeys.detail(recipe.id), recipe)
      queryClient.setQueryData(recipeQueryKeys.detail(recipe.id, recipe.revisionId), recipe)
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
      void queryClient.invalidateQueries({ queryKey: tagQueryKeys.lists() })
    },
  })
}

export function useUpdateRecipeMutation(recipeId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UpdateRecipeRequest) => updateRecipe(recipeId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.details() })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
      void queryClient.invalidateQueries({ queryKey: tagQueryKeys.lists() })
    },
  })
}

export function useDeleteRecipeMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deleteRecipe,
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: recipeQueryKeys.details() })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useDeleteRecipeImageMutation(recipeId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (imageId: string) => deleteRecipeImage(recipeId, imageId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.details() })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useSetRecipeFavoriteMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      recipeId,
      isFavorite,
      revisionId,
    }: {
      recipeId: string
      isFavorite: boolean
      revisionId?: string
    }) => setRecipeFavorite(recipeId, isFavorite, revisionId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.details() })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useCopyRecipeMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ recipeId, request }: { recipeId: string; request: CopyRecipeRequest }) =>
      copyRecipe(recipeId, request),
    onSuccess: (recipe) => {
      queryClient.removeQueries({
        queryKey: recipeQueryKeys.detail(recipe.id, recipe.revisionId),
        exact: true,
      })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useUploadRecipeImageMutation(recipeId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UploadRecipeImageRequest) => uploadRecipeImage(recipeId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.details() })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

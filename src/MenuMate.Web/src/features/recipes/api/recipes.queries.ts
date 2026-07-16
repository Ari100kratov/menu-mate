import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  copyRecipe,
  createRecipe,
  deleteRecipeImage,
  deleteRecipe,
  getRecipe,
  getRecipes,
  setRecipeFavorite,
  setRecipeSaved,
  updateRecipe,
  uploadRecipeImage,
  type CreateRecipeRequest,
  type RecipeListFilters,
  type UpdateRecipeRequest,
  type UploadRecipeImageRequest,
} from "@/features/recipes/api/recipes.api"

const normalizedEmptyFilters = {
  scope: "library",
  search: "",
  tag: "",
  category: "",
  favoritesOnly: false,
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
        tag: filters.tag?.trim() ?? normalizedEmptyFilters.tag,
        category: filters.category?.trim() ?? normalizedEmptyFilters.category,
        favoritesOnly: filters.favoritesOnly ?? normalizedEmptyFilters.favoritesOnly,
      },
    ] as const,
  details: () => [...recipeQueryKeys.all, "detail"] as const,
  detail: (recipeId: string) => [...recipeQueryKeys.details(), recipeId] as const,
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

export function useRecipeQuery(recipeId: string | undefined) {
  return useQuery({
    queryKey: recipeQueryKeys.detail(recipeId ?? ""),
    queryFn: () => getRecipe(recipeId ?? ""),
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
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useUpdateRecipeMutation(recipeId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UpdateRecipeRequest) => updateRecipe(recipeId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(recipeId) })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useDeleteRecipeMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deleteRecipe,
    onSuccess: (_data, recipeId) => {
      queryClient.removeQueries({ queryKey: recipeQueryKeys.detail(recipeId) })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useDeleteRecipeImageMutation(recipeId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (imageId: string) => deleteRecipeImage(recipeId, imageId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(recipeId) })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useSetRecipeFavoriteMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ recipeId, isFavorite }: { recipeId: string; isFavorite: boolean }) =>
      setRecipeFavorite(recipeId, isFavorite),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(variables.recipeId) })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useSetRecipeSavedMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ recipeId, isSaved }: { recipeId: string; isSaved: boolean }) =>
      setRecipeSaved(recipeId, isSaved),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(variables.recipeId) })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useCopyRecipeMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: copyRecipe,
    onSuccess: (recipe) => {
      queryClient.setQueryData(recipeQueryKeys.detail(recipe.id), recipe)
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

export function useUploadRecipeImageMutation(recipeId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UploadRecipeImageRequest) => uploadRecipeImage(recipeId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.detail(recipeId) })
      void queryClient.invalidateQueries({ queryKey: recipeQueryKeys.lists() })
    },
  })
}

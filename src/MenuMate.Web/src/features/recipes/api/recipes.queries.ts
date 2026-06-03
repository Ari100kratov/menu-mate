import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"

import {
  createRecipe,
  deleteRecipeImage,
  deleteRecipe,
  getRecipe,
  getRecipes,
  setRecipeFavorite,
  updateRecipe,
  uploadRecipeImage,
  type CreateRecipeRequest,
  type RecipeListFilters,
  type UpdateRecipeRequest,
  type UploadRecipeImageRequest,
} from "@/features/recipes/api/recipes.api"

const normalizedEmptyFilters = {
  search: "",
  tag: "",
  favoritesOnly: false,
} as const

export const recipeQueryKeys = {
  all: ["recipes"] as const,
  lists: () => [...recipeQueryKeys.all, "list"] as const,
  list: (filters: RecipeListFilters) =>
    [
      ...recipeQueryKeys.lists(),
      {
        search: filters.search?.trim() ?? normalizedEmptyFilters.search,
        tag: filters.tag?.trim() ?? normalizedEmptyFilters.tag,
        favoritesOnly: filters.favoritesOnly ?? normalizedEmptyFilters.favoritesOnly,
      },
    ] as const,
  details: () => [...recipeQueryKeys.all, "detail"] as const,
  detail: (recipeId: string) => [...recipeQueryKeys.details(), recipeId] as const,
}

export function useRecipesQuery(filters: RecipeListFilters) {
  return useQuery({
    queryKey: recipeQueryKeys.list(filters),
    queryFn: () => getRecipes(filters),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
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

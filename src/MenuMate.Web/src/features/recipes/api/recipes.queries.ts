import {
  type InfiniteData,
  type QueryKey,
  useInfiniteQuery,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query"
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
  type RecipeListPage,
  type Recipe,
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
  sort: "alphabetical",
  ownership: "all",
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
        sort: filters.sort ?? normalizedEmptyFilters.sort,
        ownership: filters.ownership ?? normalizedEmptyFilters.ownership,
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
    getNextPageParam: (lastPage, allPages) => {
      const loadedCount = allPages.reduce((count, page) => count + page.items.length, 0)
      return loadedCount < lastPage.totalCount ? allPages.length + 1 : undefined
    },
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
    onMutate: async (variables) => {
      await Promise.all([
        queryClient.cancelQueries({ queryKey: recipeQueryKeys.lists() }),
        queryClient.cancelQueries({ queryKey: recipeQueryKeys.details() }),
      ])

      const listSnapshots = queryClient.getQueriesData<InfiniteData<RecipeListPage>>({
        queryKey: recipeQueryKeys.lists(),
      })
      const detailSnapshots = queryClient.getQueriesData<Recipe>({
        queryKey: recipeQueryKeys.details(),
      })

      updateCachedRecipeFavorite(queryClient, variables)

      return { listSnapshots, detailSnapshots }
    },
    onError: (_error, _variables, context) => {
      context?.listSnapshots.forEach(([queryKey, data]) => {
        queryClient.setQueryData(queryKey, data)
      })
      context?.detailSnapshots.forEach(([queryKey, data]) => {
        queryClient.setQueryData(queryKey, data)
      })
    },
  })
}

function updateCachedRecipeFavorite(
  queryClient: ReturnType<typeof useQueryClient>,
  variables: { recipeId: string; isFavorite: boolean },
) {
  queryClient
    .getQueriesData<InfiniteData<RecipeListPage>>({ queryKey: recipeQueryKeys.lists() })
    .forEach(([queryKey, data]) => {
      if (!data) {
        return
      }

      const removeFromFavorites = !variables.isFavorite && getFavoritesOnly(queryKey)
      const hasRecipe = data.pages.some((page) =>
        page.items.some((recipe) => recipe.id === variables.recipeId),
      )
      if (!hasRecipe) {
        return
      }

      queryClient.setQueryData<InfiniteData<RecipeListPage>>(queryKey, {
        ...data,
        pages: data.pages.map((page) => ({
          ...page,
          totalCount: removeFromFavorites ? Math.max(0, page.totalCount - 1) : page.totalCount,
          items: page.items.flatMap((recipe) => {
            if (recipe.id !== variables.recipeId) {
              return [recipe]
            }

            if (removeFromFavorites) {
              return []
            }

            return [updateRecipeFavorite(recipe, variables.isFavorite)]
          }),
        })),
      })
    })

  queryClient
    .getQueriesData<Recipe>({ queryKey: recipeQueryKeys.details() })
    .forEach(([queryKey, recipe]) => {
      if (recipe?.id === variables.recipeId) {
        queryClient.setQueryData<Recipe>(
          queryKey,
          updateRecipeFavorite(recipe, variables.isFavorite),
        )
      }
    })
}

function getFavoritesOnly(queryKey: QueryKey) {
  const filters = queryKey[2]
  return typeof filters === "object" && filters !== null && "favoritesOnly" in filters
    ? filters.favoritesOnly === true
    : false
}

function updateRecipeFavorite<T extends { isFavorite: boolean; favoriteCount: number }>(
  recipe: T,
  isFavorite: boolean,
) {
  const countDelta = recipe.isFavorite === isFavorite ? 0 : isFavorite ? 1 : -1
  return {
    ...recipe,
    isFavorite,
    favoriteCount: Math.max(0, recipe.favoriteCount + countDelta),
  }
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

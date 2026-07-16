import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import type { components, paths } from "@/shared/api/generated/schema"
import { optimizeRecipeImageForUpload } from "@/features/recipes/model/recipe-image-optimization"

export type RecipeListItem = components["schemas"]["RecipeListItemResponse"] & {
  activeTimeMinutes?: null | number | string
}
export type Recipe = components["schemas"]["RecipeResponse"]
export type RecipeImage = components["schemas"]["RecipeImageResponse"]
export type CreateRecipeRequest = components["schemas"]["CreateRecipeRequest"]
export type UpdateRecipeRequest = components["schemas"]["UpdateRecipeRequest"]

type UploadRecipeImageBody =
  paths["/api/recipes/{recipeId}/images"]["post"]["requestBody"]["content"]["multipart/form-data"]

export interface UploadRecipeImageRequest {
  file: File
  scope: "Cover" | "Step"
  stepNumber?: number
  altText?: string
}

export interface RecipeListFilters {
  scope?: "library" | "catalog"
  search?: string
  tag?: string
  category?: string
  favoritesOnly?: boolean
  page?: number
  pageSize?: number
}

export async function getRecipes(filters: RecipeListFilters) {
  return unwrapApiResponse<RecipeListItem[]>(
    apiClient.GET("/api/recipes", {
      params: {
        query: {
          scope: filters.scope ?? "library",
          search: normalizeQueryValue(filters.search),
          tag: normalizeQueryValue(filters.tag),
          category: normalizeQueryValue(filters.category),
          favoritesOnly: filters.favoritesOnly ?? false,
          page: filters.page ?? 1,
          pageSize: filters.pageSize ?? 20,
        },
      },
    }),
  )
}

export async function getRecipe(recipeId: string) {
  return unwrapApiResponse<Recipe>(
    apiClient.GET("/api/recipes/{recipeId}", {
      params: {
        path: {
          recipeId,
        },
      },
    }),
  )
}

export async function createRecipe(request: CreateRecipeRequest) {
  return unwrapApiResponse<Recipe>(
    apiClient.POST("/api/recipes", {
      body: request,
    }),
  )
}

export async function updateRecipe(recipeId: string, request: UpdateRecipeRequest) {
  await unwrapEmptyApiResponse(
    apiClient.PUT("/api/recipes/{recipeId}", {
      params: {
        path: {
          recipeId,
        },
      },
      body: request,
    }),
  )
}

export async function deleteRecipe(recipeId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/recipes/{recipeId}", {
      params: {
        path: {
          recipeId,
        },
      },
    }),
  )
}

export async function deleteRecipeImage(recipeId: string, imageId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/recipes/{recipeId}/images/{imageId}", {
      params: {
        path: {
          recipeId,
          imageId,
        },
      },
    }),
  )
}

export async function setRecipeFavorite(recipeId: string, isFavorite: boolean) {
  const requestOptions = {
    params: {
      path: {
        recipeId,
      },
    },
  }

  await unwrapEmptyApiResponse(
    isFavorite
      ? apiClient.POST("/api/recipes/{recipeId}/favorite", requestOptions)
      : apiClient.DELETE("/api/recipes/{recipeId}/favorite", requestOptions),
  )
}

export async function setRecipeSaved(recipeId: string, isSaved: boolean) {
  const requestOptions = {
    params: {
      path: {
        recipeId,
      },
    },
  }

  await unwrapEmptyApiResponse(
    isSaved
      ? apiClient.POST("/api/recipes/{recipeId}/library", requestOptions)
      : apiClient.DELETE("/api/recipes/{recipeId}/library", requestOptions),
  )
}

export async function copyRecipe(recipeId: string) {
  return unwrapApiResponse<Recipe>(
    apiClient.POST("/api/recipes/{recipeId}/copy", {
      params: {
        path: {
          recipeId,
        },
      },
    }),
  )
}

export async function uploadRecipeImage(recipeId: string, request: UploadRecipeImageRequest) {
  const file = await optimizeRecipeImageForUpload(request.file)
  const formData = new FormData()
  formData.append("file", file)
  formData.append("scope", request.scope)

  if (request.stepNumber !== undefined) {
    formData.append("stepNumber", String(request.stepNumber))
  }

  const normalizedAltText = normalizeQueryValue(request.altText)

  if (normalizedAltText) {
    formData.append("altText", normalizedAltText)
  }

  return unwrapApiResponse<RecipeImage>(
    apiClient.POST("/api/recipes/{recipeId}/images", {
      params: {
        path: {
          recipeId,
        },
      },
      body: formData as unknown as UploadRecipeImageBody,
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

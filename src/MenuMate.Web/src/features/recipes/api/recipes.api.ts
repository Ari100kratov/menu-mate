import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import type { components, paths } from "@/shared/api/generated/schema"

export type RecipeListItem = components["schemas"]["RecipeListItemResponse"]
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
  search?: string
  tag?: string
  favoritesOnly?: boolean
}

export async function getRecipes(filters: RecipeListFilters) {
  return unwrapApiResponse<RecipeListItem[]>(
    apiClient.GET("/api/recipes", {
      params: {
        query: {
          search: normalizeQueryValue(filters.search),
          tag: normalizeQueryValue(filters.tag),
          favoritesOnly: filters.favoritesOnly ?? false,
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

export async function uploadRecipeImage(recipeId: string, request: UploadRecipeImageRequest) {
  const formData = new FormData()
  formData.append("file", request.file)
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

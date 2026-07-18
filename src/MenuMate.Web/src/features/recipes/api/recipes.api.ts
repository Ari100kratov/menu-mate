import { apiClient, apiFetchJson } from "@/shared/api/client"
import { unwrapApiResponse } from "@/shared/api/unwrap"
import type { paths } from "@/shared/api/generated/schema"
import { optimizeRecipeImageForUpload } from "@/features/recipes/model/recipe-image-optimization"

export type RecipeRevisionState = "Current" | "UpdateAvailable" | "Historical" | "SourceUnavailable"

export interface RecipeImage {
  id: string
  scope: string
  stepNumber: number | string | null
  bucketName: string
  objectKey: string
  contentType: string
  sizeBytes: number | string
  altText: string | null
  readUrl: string | null
  sourceUrl: string | null
  authorName: string | null
  licenseName: string | null
  licenseUrl: string | null
}

export interface RecipeIngredient {
  ingredientId: string | null
  productName: string
  amount: number | null
  unit: string
  comment: string | null
  isOptional: boolean
  category: string
}

export interface RecipeStep {
  number: number
  text: string
}

interface RecipeRevisionSummary {
  id: string
  revisionId: string
  currentRevisionId: string | null
  revisionNumber: number
  isOwnedByCurrentUser: boolean
  isFavorite: boolean
  isDisplayedRevisionSaved: boolean
  revisionState: RecipeRevisionState
  title: string
  description: string | null
  servings: number
  category: string
  visibility: string
  totalTimeMinutes: number | null
  activeTimeMinutes: number | null
  tags: string[]
}

export interface RecipeListItem extends RecipeRevisionSummary {
  coverImage: RecipeImage | null
}

export interface Recipe extends RecipeRevisionSummary {
  savedRevisionId: string | null
  sourceRecipeId: string | null
  sourceRevisionId: string | null
  sourceUrl: string | null
  images: RecipeImage[]
  ingredients: RecipeIngredient[]
  steps: RecipeStep[]
}

export interface RecipeIngredientRequest {
  ingredientId: string | null
  productName: string
  amount: number | null
  unit: string
  category: string
  comment: string | null
  isOptional: boolean
}

export interface PreparationStepRequest {
  text: string
}

export interface CreateRecipeRequest {
  title: string
  description: string | null
  servings: number
  category: string
  visibility: string
  totalTimeMinutes: number | null
  activeTimeMinutes: number | null
  sourceUrl: string | null
  ingredients: RecipeIngredientRequest[]
  steps: PreparationStepRequest[]
  tags: string[]
}

export type UpdateRecipeRequest = CreateRecipeRequest

export interface CopyRecipeRequest {
  sourceRevisionId: string
  recipe: CreateRecipeRequest
  copySourceCover: boolean
}

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
  tagIds?: string[]
  category?: string
  favoritesOnly?: boolean
  availableOnly?: boolean
  page?: number
  pageSize?: number
}

export async function getRecipes(filters: RecipeListFilters) {
  const searchParams = new URLSearchParams({
    scope: filters.scope ?? "library",
    favoritesOnly: String(filters.favoritesOnly ?? false),
    availableOnly: String(filters.availableOnly ?? false),
    page: String(filters.page ?? 1),
    pageSize: String(filters.pageSize ?? 20),
  })
  appendQueryValue(searchParams, "search", filters.search)
  appendQueryValue(searchParams, "category", filters.category)
  for (const tagId of normalizeQueryValues(filters.tagIds) ?? []) {
    searchParams.append("tagIds", tagId)
  }

  return apiFetchJson<RecipeListItem[]>(`/api/recipes?${searchParams.toString()}`)
}

export async function getRecipe(recipeId: string, revisionId?: string) {
  const search = revisionId ? `?revisionId=${encodeURIComponent(revisionId)}` : ""
  return apiFetchJson<Recipe>(`/api/recipes/${encodeURIComponent(recipeId)}${search}`)
}

export async function createRecipe(request: CreateRecipeRequest) {
  return apiFetchJson<Recipe>("/api/recipes", {
    method: "POST",
    body: JSON.stringify(request),
  })
}

export async function updateRecipe(recipeId: string, request: UpdateRecipeRequest) {
  await apiFetchJson<unknown>(`/api/recipes/${encodeURIComponent(recipeId)}`, {
    method: "PUT",
    body: JSON.stringify(request),
  })
}

export async function deleteRecipe(recipeId: string) {
  await apiFetchJson<unknown>(`/api/recipes/${encodeURIComponent(recipeId)}`, {
    method: "DELETE",
  })
}

export async function deleteRecipeImage(recipeId: string, imageId: string) {
  await apiFetchJson<unknown>(
    `/api/recipes/${encodeURIComponent(recipeId)}/images/${encodeURIComponent(imageId)}`,
    { method: "DELETE" },
  )
}

export async function setRecipeFavorite(
  recipeId: string,
  isFavorite: boolean,
  revisionId?: string,
) {
  const query = isFavorite && revisionId ? `?revisionId=${encodeURIComponent(revisionId)}` : ""
  await apiFetchJson<unknown>(`/api/recipes/${encodeURIComponent(recipeId)}/favorite${query}`, {
    method: isFavorite ? "POST" : "DELETE",
  })
}

export async function copyRecipe(recipeId: string, request: CopyRecipeRequest) {
  return apiFetchJson<Recipe>(`/api/recipes/${encodeURIComponent(recipeId)}/copy`, {
    method: "POST",
    body: JSON.stringify(request),
  })
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

function appendQueryValue(params: URLSearchParams, key: string, value: string | undefined) {
  const normalized = normalizeQueryValue(value)
  if (normalized) {
    params.set(key, normalized)
  }
}

function normalizeQueryValue(value: string | undefined) {
  const normalized = value?.trim()
  return normalized === "" ? undefined : normalized
}

function normalizeQueryValues(values: string[] | undefined) {
  const normalizedValues = values
    ?.map((value) => normalizeQueryValue(value))
    .filter((value): value is string => Boolean(value))

  return normalizedValues && normalizedValues.length > 0 ? normalizedValues : undefined
}

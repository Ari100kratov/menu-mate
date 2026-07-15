import type { CreateRecipeRequest, Recipe } from "@/features/recipes/api/recipes.api"
import { apiFetchBlob, apiFetchJson } from "@/shared/api/client"

export interface RecipeImportDraftListItem {
  id: string
  status: "Ready" | "Confirmed"
  title: string
  createdRecipeId: string | null
  createdAt: string
  updatedAt: string
}

export interface RecipeImportEvidence {
  extractedText: string
  warnings: string[]
  provider: string
  model: string
  providerResponseId: string | null
  suggestedCover: RecipeImportSuggestedCover | null
}

export interface RecipeImportSuggestedCover {
  sourceImageIndex: number
  x: number
  y: number
  width: number
  height: number
  confidence: number
}

export interface RecipeImportSourceImage {
  readUrl: string | null
  contentType: string
  sizeBytes: number
  fileName: string
}

export interface RecipeImportDraft {
  id: string
  status: "Ready" | "Confirmed"
  recipe: CreateRecipeRequest
  evidence: RecipeImportEvidence
  sourceImages: RecipeImportSourceImage[]
  createdRecipeId: string | null
  createdAt: string
  updatedAt: string
}

export function getRecipeImportDrafts() {
  return apiFetchJson<RecipeImportDraftListItem[]>("/api/imports/recipe-drafts")
}

export function getRecipeImportDraft(draftId: string) {
  return apiFetchJson<RecipeImportDraft>(`/api/imports/recipe-drafts/${draftId}`)
}

export function getRecipeImportSourceImage(
  draftId: string,
  sourceImageIndex: number,
  signal?: AbortSignal,
) {
  return apiFetchBlob(
    `/api/imports/recipe-drafts/${draftId}/source-images/${String(sourceImageIndex)}/content`,
    { signal },
  )
}

export function createRecipeImportDraft(files: File[]) {
  const formData = new FormData()
  files.forEach((file) => {
    formData.append("files", file)
  })

  return apiFetchJson<RecipeImportDraft>("/api/imports/recipe-drafts", {
    method: "POST",
    body: formData,
  })
}

export function updateRecipeImportDraft(draftId: string, recipe: CreateRecipeRequest) {
  return apiFetchJson<RecipeImportDraft>(`/api/imports/recipe-drafts/${draftId}`, {
    method: "PUT",
    body: JSON.stringify({ recipe }),
  })
}

export function confirmRecipeImportDraft(draftId: string, recipe: CreateRecipeRequest) {
  return apiFetchJson<Recipe>(`/api/imports/recipe-drafts/${draftId}/confirm`, {
    method: "POST",
    body: JSON.stringify(recipe),
  })
}

export function deleteRecipeImportDraft(draftId: string) {
  return apiFetchJson<undefined>(`/api/imports/recipe-drafts/${draftId}`, {
    method: "DELETE",
  })
}

export async function generateRecipeCoverImage(recipe: CreateRecipeRequest) {
  const blob = await apiFetchBlob("/api/recipe-images/generate-cover", {
    method: "POST",
    body: JSON.stringify(recipe),
  })

  return new File([blob], "ai-recipe-cover.jpg", {
    type: blob.type || "image/jpeg",
  })
}

import type { RecipeImage } from "@/features/recipes/api/recipes.api"

export function findCoverImage(images: readonly RecipeImage[]) {
  return images.find((image) => image.scope === "Cover")
}

export function findStepImage(images: readonly RecipeImage[], stepNumber: number) {
  return images.find((image) => image.scope === "Step" && Number(image.stepNumber) === stepNumber)
}

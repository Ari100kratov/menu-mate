import { afterEach, expect, it, vi } from "vitest"

import { generateRecipeCoverImage } from "@/features/imports/api/imports.api"
import { createEmptyRecipeFormValues, toRecipeRequest } from "@/features/recipes/model/recipe-form"
import { apiFetchBlob } from "@/shared/api/client"

vi.mock("@/shared/api/client", () => ({ apiFetchBlob: vi.fn() }))

afterEach(() => vi.clearAllMocks())

it("sends the current dish title and complete recipe when generating a cover", async () => {
  vi.mocked(apiFetchBlob).mockResolvedValue(new Blob(["image"], { type: "image/jpeg" }))
  const values = {
    ...createEmptyRecipeFormValues(),
    title: " Тыквенный суп ",
    description: "Густой крем-суп",
    advice: "Подавать горячим",
    ingredients: [
      {
        ingredientId: "",
        productName: "Тыква",
        amount: "500",
        unit: "Gram",
        isToTaste: false,
        category: "Vegetables",
        comment: "Очистить",
        isOptional: false,
      },
    ],
    steps: [{ text: "Запечь тыкву" }, { text: "Измельчить в пюре" }],
  }

  const image = await generateRecipeCoverImage(toRecipeRequest(values))

  expect(apiFetchBlob).toHaveBeenCalledWith("/api/recipe-images/generate-cover", {
    method: "POST",
    body: JSON.stringify(toRecipeRequest(values)),
  })
  const body = vi.mocked(apiFetchBlob).mock.calls[0][1]?.body
  if (typeof body !== "string") {
    throw new Error("Expected a JSON request body")
  }
  const request: unknown = JSON.parse(body)
  expect(request).toMatchObject({
    title: "Тыквенный суп",
    description: values.description,
    advice: values.advice,
    servings: 2,
    ingredients: [{ productName: "Тыква", amount: 500, unit: "Gram", comment: "Очистить" }],
    steps: values.steps,
  })
  expect(image.name).toBe("ai-recipe-cover.jpg")
  expect(image.type).toBe("image/jpeg")
})

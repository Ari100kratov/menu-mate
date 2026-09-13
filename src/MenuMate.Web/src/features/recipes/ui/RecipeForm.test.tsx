// @vitest-environment jsdom
import { createRoot, type Root } from "react-dom/client"
import { createMemoryRouter, RouterProvider } from "react-router-dom"
import { afterEach, beforeEach, expect, it, vi } from "vitest"

import {
  createEmptyRecipeFormValues,
  createEmptyIngredientFormValues,
} from "@/features/recipes/model/recipe-form"
import { flushReact } from "@/shared/lib/react-test"
import { RecipeForm } from "./RecipeForm"

vi.mock("./RecipeIngredientsFields", () => ({ RecipeIngredientsFields: () => null }))
vi.mock("./RecipeStepsFields", () => ({ RecipeStepsFields: () => null }))
vi.mock("./RecipeAdditionalFields", () => ({ RecipeAdditionalFields: () => null }))

let root: Root
let container: HTMLDivElement
let router: ReturnType<typeof createMemoryRouter>
const submit = vi.fn<React.ComponentProps<typeof RecipeForm>["onSubmit"]>()
const initialValues = {
  ...createEmptyRecipeFormValues(),
  title: "Суп",
  category: "Soup",
  ingredients: [{ ...createEmptyIngredientFormValues(), productName: "Вода", amount: "1" }],
  steps: [{ text: "Сварить" }],
}

beforeEach(async () => {
  vi.stubGlobal("IS_REACT_ACT_ENVIRONMENT", true)
  vi.stubGlobal(
    "ResizeObserver",
    class {
      observe() {
        /* Layout is not needed in these tests. */
      }
      unobserve() {
        /* Layout is not needed in these tests. */
      }
      disconnect() {
        /* Layout is not needed in these tests. */
      }
    },
  )
  vi.spyOn(URL, "createObjectURL").mockReturnValue("blob:https://menu.example/preview")
  vi.spyOn(URL, "revokeObjectURL").mockImplementation(() => {
    /* No native blob resource. */
  })
  container = document.createElement("div")
  document.body.append(container)
  root = createRoot(container)
  router = createMemoryRouter(
    [
      { path: "/recipes", element: <p>Рецепты</p> },
      {
        path: "/edit",
        element: (
          <RecipeForm
            initialValues={initialValues}
            submitLabel="Сохранить"
            isSubmitting={false}
            onSubmit={submit}
          />
        ),
      },
    ],
    { initialEntries: ["/edit"] },
  )
  await flushReact(() => {
    root.render(<RouterProvider router={router} />)
  })
})

afterEach(async () => {
  await flushReact(() => {
    root.unmount()
  })
  router.dispose()
  container.remove()
  vi.restoreAllMocks()
  vi.unstubAllGlobals()
  submit.mockReset()
})

function unloadIsBlocked() {
  const event = new Event("beforeunload", { cancelable: true })
  window.dispatchEvent(event)
  return event.defaultPrevented
}

async function selectCover() {
  const input = container.querySelector<HTMLInputElement>('input[type="file"]')
  if (!input) throw new Error("Missing cover picker")
  const file = new File(["photo"], "cover.jpg", { type: "image/jpeg" })
  Object.defineProperty(input, "files", { value: [file], configurable: true })
  await flushReact(() => {
    input.dispatchEvent(new Event("change", { bubbles: true }))
  })
  return file
}

async function save() {
  await flushReact(() => {
    container
      .querySelector("form")
      ?.dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }))
  })
}

it("detects actual field edits and becomes clean when the original value is restored", async () => {
  const input = container.querySelector<HTMLInputElement>('input[name="title"]')
  if (!input) throw new Error("Missing recipe title")
  const valueDescriptor = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value")
  if (!valueDescriptor?.set) throw new Error("Missing input setter")
  expect(unloadIsBlocked()).toBe(false)
  await flushReact(() => {
    valueDescriptor.set?.call(input, "Новый суп")
    input.dispatchEvent(new Event("input", { bubbles: true }))
  })
  expect(unloadIsBlocked()).toBe(true)
  await flushReact(() => {
    valueDescriptor.set?.call(input, initialValues.title)
    input.dispatchEvent(new Event("input", { bubbles: true }))
  })
  expect(unloadIsBlocked()).toBe(false)
})

it("keeps the selected cover and the navigation guard after a failed save", async () => {
  submit.mockRejectedValue(new Error("Upload failed"))
  const file = await selectCover()
  expect(unloadIsBlocked()).toBe(true)
  await save()
  expect(submit.mock.calls[0]?.[1]).toBe(file)
  expect(container.textContent).toContain("cover.jpg")
  expect(unloadIsBlocked()).toBe(true)
  expect(container.querySelector("fieldset")?.disabled).toBe(false)
})

it("does not block the successful save redirect", async () => {
  await selectCover()
  submit.mockImplementation(async (_values, _file, onSaved) => {
    onSaved()
    await router.navigate("/recipes")
  })
  await save()
  expect(router.state.location.pathname).toBe("/recipes")
  expect(document.querySelector('[role="alertdialog"]')).toBeNull()
})

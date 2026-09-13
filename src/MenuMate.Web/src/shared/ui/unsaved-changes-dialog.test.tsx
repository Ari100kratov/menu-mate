// @vitest-environment jsdom
import { flushReact } from "@/shared/lib/react-test"
import { createRoot, type Root } from "react-dom/client"
import { createMemoryRouter, RouterProvider } from "react-router-dom"
import { afterEach, beforeEach, expect, it, vi } from "vitest"

import { UnsavedChangesDialog } from "./unsaved-changes-dialog"

let root: Root
let container: HTMLDivElement
let dirty: boolean
let router: ReturnType<typeof createMemoryRouter>

beforeEach(async () => {
  vi.stubGlobal("IS_REACT_ACT_ENVIRONMENT", true)
  dirty = false
  container = document.createElement("div")
  document.body.append(container)
  root = createRoot(container)
  router = createMemoryRouter(
    [
      { path: "/recipes", element: <p>Рецепты</p> },
      { path: "/edit", element: <UnsavedChangesDialog shouldBlock={() => dirty} /> },
    ],
    { initialEntries: ["/recipes", "/edit"], initialIndex: 1 },
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
  vi.unstubAllGlobals()
})

function clickButton(text: string) {
  const button = [...document.querySelectorAll("button")].find((item) => item.textContent === text)
  expect(button).toBeDefined()
  if (!button) throw new Error("Missing button")
  button.click()
}

it("allows leaving an unchanged form without a dialog", async () => {
  await flushReact(() => router.navigate(-1))
  expect(router.state.location.pathname).toBe("/recipes")
  expect(document.querySelector('[role="alertdialog"]')).toBeNull()
})

it("blocks Back, keeps the form on cancel, and proceeds only after confirmation", async () => {
  dirty = true
  await flushReact(() => router.navigate(-1))
  expect(router.state.location.pathname).toBe("/edit")
  expect(document.querySelector('[role="alertdialog"]')).not.toBeNull()
  await flushReact(() => {
    clickButton("Продолжить редактирование")
  })
  expect(router.state.location.pathname).toBe("/edit")
  await flushReact(() => router.navigate("/recipes"))
  await flushReact(() => {
    clickButton("Уйти без сохранения")
  })
  expect(router.state.location.pathname).toBe("/recipes")
})

it("guards tab closing only while changes are unsaved and removes the listener on unmount", async () => {
  const unload = () => {
    const event = new Event("beforeunload", { cancelable: true })
    window.dispatchEvent(event)
    return event.defaultPrevented
  }
  expect(unload()).toBe(false)
  dirty = true
  expect(unload()).toBe(true)
  dirty = false
  await flushReact(() => router.navigate("/recipes"))
  dirty = true
  expect(unload()).toBe(false)
})

// @vitest-environment jsdom
import { flushReact } from "@/shared/lib/react-test"
import { createRoot, type Root } from "react-dom/client"
import { afterEach, beforeEach, expect, it, vi } from "vitest"

import { RecipeImage } from "./RecipeImage"

let root: Root
let container: HTMLDivElement
let notifyVisibility: IntersectionObserverCallback
const observe = vi.fn()
const unobserve = vi.fn()
const disconnect = vi.fn()

beforeEach(() => {
  vi.stubGlobal("IS_REACT_ACT_ENVIRONMENT", true)
  vi.stubGlobal(
    "IntersectionObserver",
    class {
      constructor(callback: IntersectionObserverCallback, options: IntersectionObserverInit) {
        notifyVisibility = callback
        expect(options.rootMargin).toBe("600px 0px")
      }
      observe = observe
      unobserve = unobserve
      disconnect = disconnect
    },
  )
  container = document.createElement("div")
  document.body.append(container)
  root = createRoot(container)
})

afterEach(async () => {
  await flushReact(() => {
    root.unmount()
  })
  container.remove()
  vi.unstubAllGlobals()
  vi.clearAllMocks()
})

it("mounts only nearby images, removes distant ones and restores them when scrolling back", async () => {
  await flushReact(() => {
    root.render(<RecipeImage src="https://images.example/recipe.jpg" alt="Суп" />)
  })
  const frame = container.firstElementChild
  if (!frame) throw new Error("Missing image frame")
  expect(container.querySelector("img")).toBeNull()
  expect(observe).toHaveBeenCalledWith(frame)

  async function setVisible(isIntersecting: boolean) {
    await flushReact(() => {
      notifyVisibility(
        [{ target: frame, isIntersecting } as IntersectionObserverEntry],
        {} as IntersectionObserver,
      )
    })
  }
  await setVisible(true)
  expect(container.querySelector("img")?.src).toBe("https://images.example/recipe.jpg")
  await flushReact(() => {
    container.querySelector("img")?.dispatchEvent(new Event("load"))
  })
  await setVisible(false)
  expect(container.querySelector("img")).toBeNull()
  expect(container.firstElementChild).toBe(frame)
  await setVisible(true)
  expect(container.querySelectorAll("img")).toHaveLength(1)
  await flushReact(() => {
    root.unmount()
  })
  expect(unobserve).toHaveBeenCalledWith(frame)
  expect(disconnect).toHaveBeenCalledOnce()
})

it("loads eager images immediately", async () => {
  await flushReact(() => {
    root.render(<RecipeImage src="/cover.jpg" alt="" loading="eager" />)
  })
  expect(container.querySelector("img")).not.toBeNull()
  expect(observe).not.toHaveBeenCalled()
})

it("loads images when IntersectionObserver is unavailable", async () => {
  vi.stubGlobal("IntersectionObserver", undefined)
  await flushReact(() => {
    root.render(<RecipeImage src="/cover.jpg" alt="" />)
  })
  expect(container.querySelector("img")).not.toBeNull()
})

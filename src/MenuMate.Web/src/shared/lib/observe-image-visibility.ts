const listeners = new Map<Element, (visible: boolean) => void>()
let observer: IntersectionObserver | undefined

export function observeImageVisibility(element: Element, onChange: (visible: boolean) => void) {
  if (typeof IntersectionObserver === "undefined") {
    onChange(true)
    return () => {
      // No observer to dispose in older browsers.
    }
  }

  observer ??= new IntersectionObserver(
    (entries) => {
      for (const entry of entries) listeners.get(entry.target)?.(entry.isIntersecting)
    },
    { rootMargin: "600px 0px" },
  )
  listeners.set(element, onChange)
  observer.observe(element)

  return () => {
    observer?.unobserve(element)
    listeners.delete(element)
    if (listeners.size === 0) {
      observer?.disconnect()
      observer = undefined
    }
  }
}

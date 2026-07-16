import { ArrowUp } from "lucide-react"
import { useEffect, useState } from "react"

import { Button } from "@/shared/ui/button"

interface ScrollToTopButtonProps {
  threshold?: number
  className?: string
}

export function ScrollToTopButton({ threshold = 900, className }: ScrollToTopButtonProps) {
  const [visible, setVisible] = useState(() => window.scrollY > threshold)

  useEffect(() => {
    let animationFrame = 0

    function updateVisibility() {
      window.cancelAnimationFrame(animationFrame)
      animationFrame = window.requestAnimationFrame(() => {
        setVisible(window.scrollY > threshold)
      })
    }

    updateVisibility()
    window.addEventListener("scroll", updateVisibility, { passive: true })
    return () => {
      window.removeEventListener("scroll", updateVisibility)
      window.cancelAnimationFrame(animationFrame)
    }
  }, [threshold])

  if (!visible) {
    return null
  }

  return (
    <Button
      type="button"
      variant="secondary"
      size="icon-lg"
      className={className}
      aria-label="Вернуться наверх"
      title="Наверх"
      onClick={() => {
        const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches
        window.scrollTo({ top: 0, behavior: prefersReducedMotion ? "auto" : "smooth" })
      }}
    >
      <ArrowUp />
    </Button>
  )
}

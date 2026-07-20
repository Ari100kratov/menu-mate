import type { ReactNode } from "react"

interface PageFloatingActionsProps {
  children: ReactNode
}

export function PageFloatingActions({ children }: PageFloatingActionsProps) {
  return (
    <div className="fixed right-4 bottom-20 z-30 flex flex-col-reverse items-center gap-3 md:right-6 md:bottom-6">
      {children}
    </div>
  )
}

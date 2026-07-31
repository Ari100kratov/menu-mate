import type { ReactNode } from "react"

interface PageFloatingActionsProps {
  children: ReactNode
  reserveContentSpace?: boolean
}

export function PageFloatingActions({
  children,
  reserveContentSpace = false,
}: PageFloatingActionsProps) {
  return (
    <>
      {reserveContentSpace ? <div aria-hidden className="h-24 md:h-28" /> : null}
      <div className="fixed right-4 bottom-20 z-30 flex flex-col-reverse items-center gap-3 md:right-6 md:bottom-6">
        {children}
      </div>
    </>
  )
}

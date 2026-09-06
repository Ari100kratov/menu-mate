import { ArrowLeft } from "lucide-react"
import type { ReactNode } from "react"
import { Link } from "react-router-dom"

import { AppMark } from "@/shared/ui/app-mark"
import { Button } from "@/shared/ui/button"
import { ThemeToggle } from "@/shared/ui/theme-toggle"

export function LegalPageLayout({
  title,
  description,
  children,
}: {
  title: string
  description: string
  children: ReactNode
}) {
  return (
    <main className="bg-background min-h-svh">
      <header className="bg-background/95 sticky top-0 z-20 border-b backdrop-blur">
        <div className="mx-auto flex max-w-3xl items-center justify-between gap-3 px-4 py-3">
          <div className="flex items-center gap-3">
            <Button asChild size="icon" variant="ghost">
              <Link to="/" aria-label="Вернуться в приложение">
                <ArrowLeft />
              </Link>
            </Button>
            <AppMark className="size-9" />
            <span className="font-semibold">План есть</span>
          </div>
          <ThemeToggle />
        </div>
      </header>

      <article className="mx-auto max-w-3xl space-y-8 px-4 py-8 sm:py-12">
        <header className="space-y-2">
          <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
          <p className="text-muted-foreground">{description}</p>
        </header>
        {children}
      </article>
    </main>
  )
}

export function LegalSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="space-y-3">
      <h2 className="text-xl font-semibold tracking-tight">{title}</h2>
      <div className="text-muted-foreground space-y-3 leading-7">{children}</div>
    </section>
  )
}

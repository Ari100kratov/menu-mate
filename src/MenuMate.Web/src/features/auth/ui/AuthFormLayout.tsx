import type { ReactNode } from "react"

import { AppMark } from "@/shared/ui/app-mark"
import { ThemeToggle } from "@/shared/ui/theme-toggle"

interface AuthFormLayoutProps {
  title: string
  children: ReactNode
}

export function AuthFormLayout({ title, children }: AuthFormLayoutProps) {
  return (
    <main className="bg-background relative flex min-h-svh items-center justify-center px-4 py-8 sm:py-12">
      <div className="bg-background/85 absolute top-4 right-4 rounded-md border shadow-xs backdrop-blur">
        <ThemeToggle />
      </div>

      <section className="w-full max-w-md space-y-6">
        <header className="flex flex-col items-center text-center">
          <AppMark className="size-16 shadow-sm" />
          <div className="mt-4 space-y-1.5">
            <div className="text-foreground text-2xl font-semibold tracking-tight">План есть</div>
            <p className="text-muted-foreground text-sm">
              Рецепты, меню и покупки — в одном месте.
            </p>
          </div>
        </header>

        <div className="bg-card rounded-2xl border p-5 shadow-sm sm:p-7">
          <h1 className="text-foreground mb-6 text-xl font-semibold tracking-tight">{title}</h1>
          {children}
        </div>
      </section>
    </main>
  )
}

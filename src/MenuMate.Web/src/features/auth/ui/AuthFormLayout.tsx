import type { ReactNode } from "react"

interface AuthFormLayoutProps {
  title: string
  description: string
  children: ReactNode
}

export function AuthFormLayout({ title, description, children }: AuthFormLayoutProps) {
  return (
    <main className="bg-background flex min-h-svh items-center justify-center px-4 py-10">
      <section className="w-full max-w-sm space-y-6">
        <div className="space-y-2 text-center">
          <div className="text-primary text-2xl font-semibold tracking-normal">MenuMate</div>
          <h1 className="text-foreground text-xl font-semibold tracking-normal">{title}</h1>
          <p className="text-muted-foreground text-sm">{description}</p>
        </div>
        {children}
      </section>
    </main>
  )
}

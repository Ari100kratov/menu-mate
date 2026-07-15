import { Skeleton } from "@/shared/ui/skeleton"

export function AppShellSkeleton() {
  return (
    <div className="bg-background min-h-svh" role="status" aria-label="Загружаем приложение">
      <aside className="bg-sidebar fixed inset-y-0 left-0 hidden w-64 border-r md:block">
        <div className="space-y-2 border-b px-5 py-5">
          <Skeleton className="h-6 w-28" />
          <Skeleton className="h-4 w-40" />
        </div>
        <div className="space-y-2 px-3 py-4">
          {Array.from({ length: 4 }, (_, index) => (
            <Skeleton key={index} className="h-10 w-full rounded-lg" />
          ))}
        </div>
      </aside>

      <div className="md:pl-64">
        <header className="bg-background flex min-h-14 items-center justify-between gap-3 border-b px-3 py-2 md:min-h-16 md:px-6">
          <div className="flex min-w-0 items-center gap-2">
            <Skeleton className="size-9 shrink-0" />
            <div className="space-y-1.5">
              <Skeleton className="h-5 w-28" />
              <Skeleton className="hidden h-4 w-48 sm:block" />
            </div>
          </div>
          <div className="flex gap-1">
            <Skeleton className="size-9" />
            <Skeleton className="size-9" />
          </div>
        </header>

        <main className="mx-auto w-full max-w-6xl space-y-5 px-4 py-4 pb-24 md:px-6 md:py-6 md:pb-8">
          <Skeleton className="h-10 w-full rounded-xl" />
          <Skeleton className="h-11 w-full rounded-xl" />
          <div className="grid gap-3 lg:grid-cols-2">
            {Array.from({ length: 4 }, (_, index) => (
              <Skeleton key={index} className="h-36 w-full rounded-xl" />
            ))}
          </div>
        </main>
      </div>

      <nav className="bg-background fixed inset-x-0 bottom-0 grid h-16 grid-cols-4 border-t px-2 md:hidden">
        {Array.from({ length: 4 }, (_, index) => (
          <div key={index} className="flex flex-col items-center justify-center gap-1.5">
            <Skeleton className="size-5 rounded-full" />
            <Skeleton className="h-2.5 w-12" />
          </div>
        ))}
      </nav>
    </div>
  )
}

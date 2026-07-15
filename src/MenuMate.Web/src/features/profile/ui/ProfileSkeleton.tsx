import { Skeleton } from "@/shared/ui/skeleton"

export function ProfileSkeleton() {
  return (
    <div className="space-y-5" role="status" aria-label="Загружаем профиль">
      <div className="flex justify-end">
        <Skeleton className="h-10 w-full sm:w-28" />
      </div>

      <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_20rem]">
        <section className="min-w-0 space-y-4 rounded-md border p-4">
          <div className="flex items-start gap-3">
            <Skeleton className="size-11 shrink-0" />
            <div className="min-w-0 flex-1 space-y-2">
              <Skeleton className="h-6 w-48 max-w-full" />
              <Skeleton className="h-4 w-64 max-w-full" />
            </div>
          </div>
          <div className="grid gap-3 sm:grid-cols-2">
            {Array.from({ length: 2 }, (_, index) => (
              <div key={index} className="bg-muted/40 space-y-2 rounded-md p-3">
                <Skeleton className="h-3 w-20" />
                <Skeleton className="h-4 w-4/5" />
              </div>
            ))}
          </div>
        </section>

        <section className="min-w-0 space-y-4 rounded-md border p-4">
          <Skeleton className="h-6 w-16" />
          <div className="flex gap-2">
            <Skeleton className="h-7 w-20" />
            <Skeleton className="h-7 w-24" />
          </div>
        </section>
      </section>

      <section className="space-y-4 rounded-md border p-4">
        <Skeleton className="h-6 w-28" />
        <div className="grid grid-cols-3 gap-2">
          {Array.from({ length: 3 }, (_, index) => (
            <Skeleton key={index} className="h-10 w-full" />
          ))}
        </div>
        <div className="flex items-start gap-3 rounded-lg border p-3">
          <Skeleton className="size-9 shrink-0" />
          <div className="min-w-0 flex-1 space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-4/5" />
          </div>
        </div>
      </section>
    </div>
  )
}

import { Skeleton } from "@/shared/ui/skeleton"

interface SkeletonCountProps {
  count?: number
}

export function RecipeListSkeleton({ count = 4 }: SkeletonCountProps) {
  return (
    <section className="grid gap-3 lg:grid-cols-2" role="status" aria-label="Загружаем рецепты">
      {Array.from({ length: count }, (_, index) => (
        <RecipeCardSkeleton key={index} />
      ))}
    </section>
  )
}

export function RecipeDetailsSkeleton() {
  return (
    <div className="mx-auto max-w-3xl" role="status" aria-label="Загружаем рецепт">
      <div className="bg-card space-y-5 overflow-hidden rounded-xl border p-4 shadow-sm md:p-6">
        <section className="grid gap-5 md:grid-cols-[minmax(15rem,20rem)_minmax(0,1fr)] md:items-start">
          <Skeleton className="aspect-[4/3] w-full rounded-md" />
          <div className="space-y-5">
            <div className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-3">
              <div className="space-y-2">
                <Skeleton className="h-7 w-4/5" />
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-2/3" />
              </div>
              <Skeleton className="size-9 rounded-full" />
            </div>
            <div className="grid grid-cols-2 gap-x-4 gap-y-4">
              {Array.from({ length: 5 }, (_, index) => (
                <div key={index} className="flex items-start gap-2.5">
                  <Skeleton className="mt-0.5 size-4 shrink-0 rounded-full" />
                  <div className="w-full space-y-1.5">
                    <Skeleton className="h-3 w-16" />
                    <Skeleton className="h-4 w-24 max-w-full" />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section className="space-y-3 border-t pt-5">
          <Skeleton className="h-6 w-32" />
          <div className="space-y-2">
            {Array.from({ length: 3 }, (_, index) => (
              <div key={index} className="space-y-2 rounded-lg border px-3 py-3">
                <div className="flex justify-between gap-3">
                  <Skeleton className="h-5 w-2/5" />
                  <Skeleton className="h-5 w-20 rounded-full" />
                </div>
                <Skeleton className="h-4 w-1/3" />
              </div>
            ))}
          </div>
        </section>

        <section className="space-y-3 border-t pt-5">
          <Skeleton className="h-6 w-36" />
          {Array.from({ length: 2 }, (_, index) => (
            <div key={index} className="grid grid-cols-[2.25rem_1fr] gap-3">
              <Skeleton className="size-9 rounded-full" />
              <div className="space-y-2 pt-1">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-4/5" />
              </div>
            </div>
          ))}
        </section>
      </div>
    </div>
  )
}

export function RecipeFormSkeleton() {
  return (
    <div
      className="mx-auto -mb-4 max-w-3xl md:mb-0"
      role="status"
      aria-label="Загружаем форму рецепта"
    >
      <div className="bg-card overflow-hidden rounded-xl border shadow-sm">
        <section className="space-y-3 border-b p-4 md:p-6">
          <div className="grid grid-cols-[5.5rem_minmax(0,1fr)] items-center gap-4 rounded-xl border p-2">
            <Skeleton className="size-[5.5rem] rounded-lg" />
            <div className="space-y-2">
              <Skeleton className="h-5 w-36" />
              <Skeleton className="h-4 w-52 max-w-full" />
            </div>
          </div>
          <Skeleton className="h-10 w-full" />
        </section>

        <section className="space-y-4 border-b p-4 md:p-6">
          <Skeleton className="h-6 w-28" />
          <div className="grid gap-4 md:grid-cols-2">
            <FormFieldSkeleton className="md:col-span-2" />
            <FormFieldSkeleton />
            <FormFieldSkeleton />
            <FormFieldSkeleton className="max-w-40" />
            <div className="space-y-3 md:col-span-2">
              <Skeleton className="h-5 w-44" />
              <div className="grid gap-4 md:grid-cols-2">
                <FormFieldSkeleton />
                <FormFieldSkeleton />
              </div>
            </div>
            <div className="space-y-2 md:col-span-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-20 w-full" />
            </div>
          </div>
        </section>

        <section className="space-y-4 border-b p-4 md:p-6">
          <div className="space-y-2">
            <Skeleton className="h-6 w-28" />
            <Skeleton className="h-4 w-4/5 max-w-md" />
          </div>
          {Array.from({ length: 2 }, (_, index) => (
            <Skeleton key={index} className="h-16 w-full rounded-lg" />
          ))}
          <Skeleton className="h-10 w-full" />
        </section>

        <section className="space-y-4 border-b p-4 md:p-6">
          <Skeleton className="h-6 w-44" />
          {Array.from({ length: 2 }, (_, index) => (
            <div key={index} className="grid grid-cols-[2rem_minmax(0,1fr)_2rem] gap-3">
              <Skeleton className="size-8 rounded-full" />
              <Skeleton className="h-20 w-full" />
              <Skeleton className="size-8" />
            </div>
          ))}
          <Skeleton className="h-10 w-full" />
        </section>

        <section className="flex items-center justify-between p-4 md:p-6">
          <Skeleton className="h-6 w-48" />
          <Skeleton className="size-5" />
        </section>
      </div>

      <div className="bg-background/95 sticky bottom-18 -mx-4 mt-3 border-y px-4 py-2.5 md:bottom-0 md:mx-0 md:mt-4 md:flex md:justify-end md:rounded-xl md:border md:p-3">
        <Skeleton className="h-11 w-full md:w-32" />
      </div>
    </div>
  )
}

function RecipeCardSkeleton() {
  return (
    <article className="bg-card relative grid min-h-28 grid-cols-[7rem_minmax(0,1fr)] overflow-hidden rounded-xl border shadow-sm sm:min-h-32 sm:grid-cols-[8rem_minmax(0,1fr)]">
      <Skeleton className="h-28 w-full self-center rounded-none sm:h-32" />
      <div className="flex min-w-0 flex-col justify-center gap-2 p-3 sm:p-4">
        <div className="space-y-2">
          <Skeleton className="h-5 w-4/5" />
          <Skeleton className="h-4 w-24" />
        </div>
        <div className="flex gap-3">
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-4 w-14" />
          <Skeleton className="h-4 w-8" />
        </div>
      </div>
      <Skeleton className="absolute top-2 right-2 size-8 rounded-full" />
    </article>
  )
}

function FormFieldSkeleton({ className }: { className?: string }) {
  return (
    <div className={className ? `space-y-2 ${className}` : "space-y-2"}>
      <Skeleton className="h-4 w-24" />
      <Skeleton className="h-10 w-full" />
    </div>
  )
}

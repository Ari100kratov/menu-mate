import { RecipeFormSkeleton } from "@/features/recipes/ui/RecipeSkeletons"
import { Skeleton } from "@/shared/ui/skeleton"

export function RecipeImportDraftListSkeleton() {
  return (
    <div className="divide-y rounded-lg border" role="status" aria-label="Загружаем черновики">
      {Array.from({ length: 3 }, (_, index) => (
        <div key={index} className="flex items-center gap-3 p-3">
          <div className="min-w-0 flex-1 space-y-2">
            <Skeleton className="h-5 w-3/5" />
            <Skeleton className="h-4 w-28" />
          </div>
          <Skeleton className="size-9" />
        </div>
      ))}
    </div>
  )
}

export function RecipeImportDraftPageSkeleton() {
  return (
    <div className="space-y-5" role="status" aria-label="Загружаем черновик рецепта">
      <section className="space-y-4 rounded-md border p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div className="space-y-2">
            <Skeleton className="h-6 w-44" />
            <Skeleton className="h-4 w-80 max-w-full" />
          </div>
          <Skeleton className="h-10 w-44" />
        </div>
        <div className="grid gap-3 md:grid-cols-2">
          <Skeleton className="aspect-[4/3] w-full rounded-lg" />
          <Skeleton className="aspect-[4/3] w-full rounded-lg" />
        </div>
      </section>
      <div className="flex justify-end">
        <Skeleton className="h-4 w-48" />
      </div>
      <RecipeFormSkeleton />
    </div>
  )
}

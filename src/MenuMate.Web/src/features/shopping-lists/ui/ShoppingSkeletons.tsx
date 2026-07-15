import { Skeleton } from "@/shared/ui/skeleton"

export function ShoppingListSkeleton() {
  return (
    <div
      className="mx-auto max-w-3xl space-y-5"
      role="status"
      aria-label="Загружаем список покупок"
    >
      <section className="bg-primary/5 border-primary/15 rounded-xl border p-4">
        <Skeleton className="h-5 w-40" />
      </section>
      <div className="space-y-4">
        {Array.from({ length: 2 }, (_, index) => (
          <ShoppingCategorySkeleton key={index} itemCount={index === 0 ? 3 : 2} />
        ))}
      </div>
    </div>
  )
}

export function ShoppingPreviewSkeleton() {
  return (
    <div
      className="mx-auto max-w-3xl space-y-5 pb-20"
      role="status"
      aria-label="Загружаем предпросмотр списка покупок"
    >
      <section className="bg-primary/5 border-primary/15 space-y-2 rounded-xl border p-4">
        <Skeleton className="h-6 w-56" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-4/5" />
      </section>

      <div className="space-y-4">
        {Array.from({ length: 2 }, (_, recipeIndex) => (
          <section key={recipeIndex} className="bg-card rounded-xl border p-4 shadow-xs">
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0 flex-1 space-y-2">
                <Skeleton className="h-4 w-2/5" />
                <Skeleton className="h-4 w-28" />
              </div>
              <Skeleton className="h-10 w-28 rounded-full" />
            </div>
            <div className="mt-3 divide-y">
              {Array.from({ length: 3 }, (_, itemIndex) => (
                <div key={itemIndex} className="flex items-center gap-3 py-3">
                  <Skeleton className="size-4 rounded-sm" />
                  <div className="min-w-0 flex-1 space-y-2">
                    <Skeleton className="h-5 w-2/5" />
                    <Skeleton className="h-4 w-24" />
                  </div>
                </div>
              ))}
            </div>
          </section>
        ))}
      </div>

      <div className="bg-background/95 fixed inset-x-0 bottom-16 z-30 border-t p-3 backdrop-blur md:bottom-0 md:left-64">
        <Skeleton className="mx-auto h-10 w-full max-w-3xl" />
      </div>
    </div>
  )
}

function ShoppingCategorySkeleton({ itemCount }: { itemCount: number }) {
  return (
    <section className="bg-card rounded-xl border p-2 shadow-xs">
      <div className="px-2 py-2">
        <Skeleton className="h-4 w-28" />
      </div>
      <div className="divide-y">
        {Array.from({ length: itemCount }, (_, index) => (
          <div key={index} className="flex items-center gap-3 px-2 py-3">
            <Skeleton className="size-4 rounded-sm" />
            <div className="min-w-0 flex-1 space-y-2">
              <Skeleton className="h-5 w-2/5" />
              <Skeleton className="h-4 w-24" />
            </div>
            <Skeleton className="size-9" />
            <Skeleton className="size-9" />
          </div>
        ))}
      </div>
    </section>
  )
}

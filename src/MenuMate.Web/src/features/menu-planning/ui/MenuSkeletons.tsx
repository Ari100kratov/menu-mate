import { Skeleton } from "@/shared/ui/skeleton"

interface MenuCalendarSkeletonProps {
  dayCount?: number
  mealSlotCount?: number
}

export function MenuCalendarSkeleton({
  dayCount = 4,
  mealSlotCount = 3,
}: MenuCalendarSkeletonProps) {
  return (
    <section role="status" aria-label="Загружаем календарь меню">
      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        {Array.from({ length: dayCount }, (_, dayIndex) => (
          <article
            key={dayIndex}
            className="bg-secondary/35 border-primary/10 min-w-0 space-y-3 rounded-2xl border p-3"
          >
            <Skeleton className="mx-1 h-5 w-36" />
            <div className="space-y-2">
              {Array.from({ length: mealSlotCount }, (_, slotIndex) => (
                <section
                  key={slotIndex}
                  className="bg-card/90 border-primary/10 space-y-3 rounded-xl border p-3 shadow-sm"
                >
                  <div className="flex items-center justify-between gap-2">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="size-8" />
                  </div>
                  <Skeleton className="h-4 w-20" />
                </section>
              ))}
            </div>
          </article>
        ))}
      </div>
    </section>
  )
}

export function RecipePickerListSkeleton() {
  return (
    <div className="space-y-2" role="status" aria-label="Загружаем список рецептов">
      {Array.from({ length: 4 }, (_, index) => (
        <div
          key={index}
          className="bg-card grid w-full grid-cols-[4rem_minmax(0,1fr)] items-center gap-3 rounded-xl border p-2 shadow-sm"
        >
          <Skeleton className="size-16 rounded-lg" />
          <div className="space-y-2">
            <Skeleton className="h-4 w-3/5" />
            <Skeleton className="h-4 w-2/5" />
          </div>
        </div>
      ))}
    </div>
  )
}

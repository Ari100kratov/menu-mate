import * as React from "react"
import { DayPicker, getDefaultClassNames } from "react-day-picker"

import { buttonVariants } from "@/shared/ui/button"
import { cn } from "@/shared/lib/utils"

function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  ...props
}: React.ComponentProps<typeof DayPicker>) {
  const defaultClassNames = getDefaultClassNames()

  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      className={cn("p-3", className)}
      classNames={{
        root: cn("w-fit", defaultClassNames.root),
        months: cn("flex flex-col gap-4 sm:flex-row", defaultClassNames.months),
        month: cn("space-y-4", defaultClassNames.month),
        month_caption: cn(
          "relative flex h-9 items-center justify-center",
          defaultClassNames.month_caption,
        ),
        caption_label: cn("text-sm font-medium", defaultClassNames.caption_label),
        nav: cn(
          "absolute inset-x-3 top-3 flex items-center justify-between",
          defaultClassNames.nav,
        ),
        button_previous: cn(
          buttonVariants({ variant: "ghost", size: "icon-sm" }),
          defaultClassNames.button_previous,
        ),
        button_next: cn(
          buttonVariants({ variant: "ghost", size: "icon-sm" }),
          defaultClassNames.button_next,
        ),
        month_grid: cn("w-full border-collapse", defaultClassNames.month_grid),
        weekdays: cn("flex", defaultClassNames.weekdays),
        weekday: cn(
          "text-muted-foreground w-9 text-center text-xs font-normal",
          defaultClassNames.weekday,
        ),
        week: cn("mt-2 flex w-full", defaultClassNames.week),
        day: cn("relative size-9 p-0 text-center text-sm", defaultClassNames.day),
        day_button: cn(
          "hover:bg-accent hover:text-accent-foreground focus-visible:ring-ring flex size-9 items-center justify-center rounded-md outline-none focus-visible:ring-[3px]",
          defaultClassNames.day_button,
        ),
        today: cn("text-primary font-semibold", defaultClassNames.today),
        selected: cn("bg-primary text-primary-foreground", defaultClassNames.selected),
        range_start: cn(
          "bg-primary text-primary-foreground rounded-l-md",
          defaultClassNames.range_start,
        ),
        range_middle: cn(
          "bg-secondary text-secondary-foreground rounded-none",
          defaultClassNames.range_middle,
        ),
        range_end: cn(
          "bg-primary text-primary-foreground rounded-r-md",
          defaultClassNames.range_end,
        ),
        outside: cn("text-muted-foreground opacity-50", defaultClassNames.outside),
        disabled: cn("text-muted-foreground opacity-40", defaultClassNames.disabled),
        hidden: cn("invisible", defaultClassNames.hidden),
        ...classNames,
      }}
      {...props}
    />
  )
}

export { Calendar }

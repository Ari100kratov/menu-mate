import type { LucideIcon } from "lucide-react"
import type { ComponentProps, ReactNode } from "react"

import { cn } from "@/shared/lib/utils"

interface PageHeaderProps {
  title: string
  description: string
  action?: ReactNode
}

export function PageHeader({ title, description, action }: PageHeaderProps) {
  return (
    <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
      <div className="space-y-1">
        <h1 className="text-foreground text-2xl font-semibold tracking-normal md:text-3xl">
          {title}
        </h1>
        <p className="text-muted-foreground max-w-2xl text-sm md:text-base">{description}</p>
      </div>
      {action ? (
        <div className="flex w-full shrink-0 flex-wrap gap-2 md:w-auto">{action}</div>
      ) : null}
    </div>
  )
}

interface EmptyStateProps {
  icon: LucideIcon
  title: string
  description: string
  action?: ReactNode
}

export function EmptyState({ icon: Icon, title, description, action }: EmptyStateProps) {
  return (
    <section className="bg-card text-card-foreground mt-6 rounded-md border p-5 md:p-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
        <div className="bg-primary/10 text-primary flex size-11 shrink-0 items-center justify-center rounded-md">
          <Icon className="size-5" />
        </div>
        <div className="space-y-2">
          <h2 className="text-lg font-semibold tracking-normal">{title}</h2>
          <p className="text-muted-foreground max-w-2xl text-sm">{description}</p>
          {action ? <div className="pt-2">{action}</div> : null}
        </div>
      </div>
    </section>
  )
}

interface PageSectionProps extends ComponentProps<"section"> {
  title?: string
  description?: string
  action?: ReactNode
}

export function PageSection({
  title,
  description,
  action,
  className,
  children,
  ...props
}: PageSectionProps) {
  return (
    <section className={cn("space-y-4 rounded-md border p-4", className)} {...props}>
      {title || description || action ? (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div className="space-y-1">
            {title ? <h2 className="text-lg font-semibold tracking-normal">{title}</h2> : null}
            {description ? <p className="text-muted-foreground text-sm">{description}</p> : null}
          </div>
          {action ? <div className="flex shrink-0 flex-wrap gap-2">{action}</div> : null}
        </div>
      ) : null}
      {children}
    </section>
  )
}

export function MobileStickyActions({ className, children, ...props }: ComponentProps<"div">) {
  return (
    <div
      className={cn(
        "bg-background/95 sticky bottom-20 z-20 -mx-4 border-y px-4 py-3 shadow-sm backdrop-blur md:hidden",
        className,
      )}
      {...props}
    >
      <div className="flex gap-2 overflow-x-auto">{children}</div>
    </div>
  )
}

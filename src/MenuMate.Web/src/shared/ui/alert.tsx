import { cva, type VariantProps } from "class-variance-authority"
import type * as React from "react"

import { cn } from "@/shared/lib/utils"

const alertVariants = cva("relative w-full rounded-md border px-4 py-3 text-sm", {
  variants: {
    variant: {
      default: "bg-card text-card-foreground",
      destructive: "border-destructive/40 text-destructive dark:border-destructive/60",
    },
  },
  defaultVariants: {
    variant: "default",
  },
})

function Alert({
  className,
  variant,
  ...props
}: React.ComponentProps<"div"> & VariantProps<typeof alertVariants>) {
  return <div role="alert" className={cn(alertVariants({ variant }), className)} {...props} />
}

function AlertTitle({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div className={cn("mb-1 leading-none font-medium tracking-normal", className)} {...props} />
  )
}

function AlertDescription({ className, ...props }: React.ComponentProps<"div">) {
  return <div className={cn("text-sm [&_p]:leading-relaxed", className)} {...props} />
}

export { Alert, AlertDescription, AlertTitle }

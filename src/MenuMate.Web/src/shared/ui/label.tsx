import * as LabelPrimitive from "@radix-ui/react-label"
import type * as React from "react"

import { cn } from "@/shared/lib/utils"

function Label({ className, ...props }: React.ComponentProps<typeof LabelPrimitive.Root>) {
  return (
    <LabelPrimitive.Root
      className={cn("text-sm leading-none font-medium select-none", className)}
      {...props}
    />
  )
}

export { Label }

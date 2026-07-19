import type { ComponentProps } from "react"

import appMarkUrl from "@/assets/plan-est-mark-flat.png"
import { cn } from "@/shared/lib/utils"

type AppMarkProps = Omit<ComponentProps<"img">, "src">

export function AppMark({ alt = "", className, ...props }: AppMarkProps) {
  return (
    <img
      src={appMarkUrl}
      alt={alt}
      className={cn("block shrink-0 rounded-[22%] object-cover select-none", className)}
      draggable={false}
      {...props}
    />
  )
}

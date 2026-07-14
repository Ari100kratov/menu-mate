import type { ReactNode } from "react"

import { Dialog, DialogContent, DialogTitle, DialogTrigger } from "@/shared/ui/dialog"

interface RecipeImageLightboxProps {
  imageUrl: string
  imageAlt: string
  children: ReactNode
}

export function RecipeImageLightbox({ imageUrl, imageAlt, children }: RecipeImageLightboxProps) {
  return (
    <Dialog>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent
        className="h-fit max-h-[calc(100svh-1rem)] w-fit max-w-[calc(100vw-1rem)] rounded-none border-0 bg-transparent p-0 text-white shadow-none sm:w-fit sm:max-w-[calc(100vw-1rem)] sm:rounded-none"
        style={{
          top: "50%",
          right: "auto",
          bottom: "auto",
          left: "50%",
          transform: "translate(-50%, -50%)",
        }}
      >
        <DialogTitle className="sr-only">{imageAlt}</DialogTitle>
        <img
          className="h-auto max-h-[calc(100svh-1rem)] w-auto max-w-[calc(100vw-1rem)] object-contain"
          src={imageUrl}
          alt={imageAlt}
        />
      </DialogContent>
    </Dialog>
  )
}

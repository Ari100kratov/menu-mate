import type * as React from "react"

import { cn } from "@/shared/lib/utils"

function FieldGroup({ className, ...props }: React.ComponentProps<"div">) {
  return <div className={cn("space-y-4", className)} {...props} />
}

function Field({ className, ...props }: React.ComponentProps<"div">) {
  return <div className={cn("space-y-2", className)} {...props} />
}

function FieldLabel({ className, ...props }: React.ComponentProps<"label">) {
  return (
    <label
      className={cn(
        "text-foreground text-sm leading-none font-medium peer-disabled:cursor-not-allowed peer-disabled:opacity-70",
        className,
      )}
      {...props}
    />
  )
}

function FieldDescription({ className, ...props }: React.ComponentProps<"p">) {
  return <p className={cn("text-muted-foreground text-sm", className)} {...props} />
}

interface FieldErrorProps extends React.ComponentProps<"p"> {
  errors?: readonly unknown[]
}

function FieldError({ className, errors, children, ...props }: FieldErrorProps) {
  const message = children ?? errors?.map(getErrorMessage).find(Boolean)

  if (!message) {
    return null
  }

  return (
    <p className={cn("text-destructive text-sm font-medium", className)} {...props}>
      {message}
    </p>
  )
}

function getErrorMessage(error: unknown): string | null {
  if (typeof error === "string") {
    return error
  }

  if (
    typeof error === "object" &&
    error !== null &&
    "message" in error &&
    typeof error.message === "string"
  ) {
    return error.message
  }

  return null
}

export { Field, FieldDescription, FieldError, FieldGroup, FieldLabel }

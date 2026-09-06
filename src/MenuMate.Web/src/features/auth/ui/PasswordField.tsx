import { useState } from "react"
import { Eye, EyeOff } from "lucide-react"

import { Button } from "@/shared/ui/button"
import { Input } from "@/shared/ui/input"
import { cn } from "@/shared/lib/utils"

type PasswordFieldProps = Omit<React.ComponentProps<typeof Input>, "type">

export function PasswordField({ className, ...props }: PasswordFieldProps) {
  const [isVisible, setIsVisible] = useState(false)
  const label = isVisible ? "Скрыть пароль" : "Показать пароль"

  return (
    <div className="relative">
      <Input type={isVisible ? "text" : "password"} className={cn("pr-11", className)} {...props} />
      <Button
        type="button"
        variant="ghost"
        size="icon-sm"
        className="absolute top-1/2 right-1.5 -translate-y-1/2"
        aria-label={label}
        aria-pressed={isVisible}
        onClick={() => {
          setIsVisible((value) => !value)
        }}
      >
        {isVisible ? <EyeOff /> : <Eye />}
      </Button>
    </div>
  )
}

import { Monitor, Moon, Sun } from "lucide-react"
import { useTheme } from "next-themes"

import { Button } from "@/shared/ui/button"

export function ThemeToggle() {
  const { theme, setTheme } = useTheme()

  const nextTheme = theme === "dark" ? "light" : theme === "light" ? "system" : "dark"

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon"
      aria-label="Переключить тему"
      onClick={() => {
        setTheme(nextTheme)
      }}
    >
      {theme === "dark" ? <Moon /> : theme === "light" ? <Sun /> : <Monitor />}
    </Button>
  )
}

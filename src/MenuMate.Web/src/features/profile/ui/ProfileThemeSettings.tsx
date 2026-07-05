import { Monitor, Moon, Palette, Sun } from "lucide-react"
import { useTheme } from "next-themes"

import { Button } from "@/shared/ui/button"
import { PageSection } from "@/shared/ui/page"
import { SettingsRow } from "./SettingsRow"

const themeOptions = [
  { value: "system", label: "Система", icon: Monitor },
  { value: "light", label: "Светлая", icon: Sun },
  { value: "dark", label: "Темная", icon: Moon },
] as const

export function ProfileThemeSettings() {
  const { theme, setTheme } = useTheme()
  const currentTheme = theme ?? "system"

  return (
    <PageSection title="Интерфейс">
      <div className="grid grid-cols-3 gap-2">
        {themeOptions.map(({ value, label, icon: Icon }) => (
          <Button
            key={value}
            type="button"
            variant={currentTheme === value ? "default" : "outline"}
            aria-pressed={currentTheme === value}
            onClick={() => {
              setTheme(value)
            }}
          >
            <Icon />
            {label}
          </Button>
        ))}
      </div>

      <SettingsRow
        icon={Palette}
        title="Тема"
        description="Выбор доступен здесь и через компактный переключатель в верхней панели приложения."
      />
    </PageSection>
  )
}

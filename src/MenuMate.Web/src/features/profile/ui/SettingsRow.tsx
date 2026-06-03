import type { LucideIcon } from "lucide-react"

interface SettingsRowProps {
  icon: LucideIcon
  title: string
  description: string
}

export function SettingsRow({ icon: Icon, title, description }: SettingsRowProps) {
  return (
    <div className="flex gap-3 rounded-md border p-3">
      <div className="bg-primary/10 text-primary flex size-9 shrink-0 items-center justify-center rounded-md">
        <Icon className="size-4" />
      </div>
      <div className="space-y-1">
        <div className="font-medium">{title}</div>
        <p className="text-muted-foreground text-sm">{description}</p>
      </div>
    </div>
  )
}

import type { LucideIcon } from "lucide-react"

interface ProfileFieldProps {
  icon: LucideIcon
  label: string
  value: string
}

export function ProfileField({ icon: Icon, label, value }: ProfileFieldProps) {
  return (
    <div className="bg-muted/40 rounded-md p-3">
      <dt className="text-muted-foreground flex items-center gap-2 text-xs font-medium uppercase">
        <Icon className="size-3.5" />
        {label}
      </dt>
      <dd className="mt-1 text-sm font-medium break-words">{value}</dd>
    </div>
  )
}

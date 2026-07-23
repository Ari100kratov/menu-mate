import { ShieldCheck, UsersRound } from "lucide-react"
import { Link } from "react-router-dom"

import { Button } from "@/shared/ui/button"
import { PageSection } from "@/shared/ui/page"

export function AdminEntryCard() {
  return (
    <PageSection
      title="Администрирование"
      description="Просматривайте зарегистрированных пользователей и их активность."
      action={
        <Button asChild type="button">
          <Link to="/admin">
            <UsersRound />
            Пользователи
          </Link>
        </Button>
      }
    >
      <div className="bg-primary/5 text-muted-foreground flex items-start gap-3 rounded-md border p-3 text-sm">
        <ShieldCheck className="text-primary mt-0.5 size-4 shrink-0" />
        <p>Раздел доступен только администраторам.</p>
      </div>
    </PageSection>
  )
}

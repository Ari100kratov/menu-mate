import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { formatDate } from "@/features/menu-planning/model/menu-planning"

interface MenuPlanSummaryButtonProps {
  menuPlan: MenuPlan
  isActive: boolean
  onClick: () => void
}

export function MenuPlanSummaryButton({ menuPlan, isActive, onClick }: MenuPlanSummaryButtonProps) {
  return (
    <button
      type="button"
      className="bg-card text-card-foreground hover:bg-accent focus-visible:ring-ring w-full rounded-md border p-3 text-left outline-none focus-visible:ring-2"
      data-active={isActive}
      onClick={onClick}
    >
      <div className="font-medium">{menuPlan.name}</div>
      <div className="text-muted-foreground text-sm">
        {formatDate(menuPlan.startDate)} - {formatDate(menuPlan.endDate)}
      </div>
      <div className="text-muted-foreground text-xs">{menuPlan.items.length} поз.</div>
    </button>
  )
}

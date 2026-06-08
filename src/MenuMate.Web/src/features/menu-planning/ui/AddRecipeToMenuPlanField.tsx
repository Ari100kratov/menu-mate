import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { formatDate } from "@/features/menu-planning/model/menu-planning"
import { Field, FieldLabel } from "@/shared/ui/field"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

interface AddRecipeToMenuPlanFieldProps {
  menuPlans: readonly MenuPlan[]
  selectedMenuPlan: MenuPlan
  onSelectedMenuPlanChange: (menuPlanId: string) => void
}

export function AddRecipeToMenuPlanField({
  menuPlans,
  selectedMenuPlan,
  onSelectedMenuPlanChange,
}: AddRecipeToMenuPlanFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor="menuPlanId">План</FieldLabel>
      <Select value={selectedMenuPlan.id} onValueChange={onSelectedMenuPlanChange}>
        <SelectTrigger id="menuPlanId" className="w-full">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {menuPlans.map((menuPlan) => (
            <SelectItem key={menuPlan.id} value={menuPlan.id}>
              {menuPlan.name} · {formatDate(menuPlan.startDate)} - {formatDate(menuPlan.endDate)}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </Field>
  )
}

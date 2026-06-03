import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { formatDate } from "@/features/menu-planning/model/menu-planning"
import { Field, FieldLabel } from "@/shared/ui/field"
import { Select } from "@/shared/ui/select"

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
      <Select
        id="menuPlanId"
        value={selectedMenuPlan.id}
        onChange={(event) => {
          onSelectedMenuPlanChange(event.target.value)
        }}
      >
        {menuPlans.map((menuPlan) => (
          <option key={menuPlan.id} value={menuPlan.id}>
            {menuPlan.name} · {formatDate(menuPlan.startDate)} - {formatDate(menuPlan.endDate)}
          </option>
        ))}
      </Select>
    </Field>
  )
}

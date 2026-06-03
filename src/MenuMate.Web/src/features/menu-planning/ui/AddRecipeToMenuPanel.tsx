import { CalendarPlus } from "lucide-react"
import { useState } from "react"
import { Link } from "react-router-dom"

import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { useMenuPlansQuery } from "@/features/menu-planning/api/menu-plans.queries"
import type { AddRecipeToMenuRecipe } from "@/features/menu-planning/model/add-recipe-to-menu"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"
import { AddRecipeToMenuForm } from "./AddRecipeToMenuForm"

interface AddRecipeToMenuPanelProps {
  recipe: AddRecipeToMenuRecipe
}

export function AddRecipeToMenuPanel({ recipe }: AddRecipeToMenuPanelProps) {
  const menuPlansQuery = useMenuPlansQuery()
  const menuPlans = menuPlansQuery.data ?? []
  const firstMenuPlan = menuPlans.at(0)

  if (menuPlansQuery.isPending) {
    return <PageSkeleton />
  }

  if (menuPlansQuery.error) {
    return <ErrorAlert error={menuPlansQuery.error} />
  }

  if (!firstMenuPlan) {
    return (
      <EmptyState
        icon={CalendarPlus}
        title="Планов меню пока нет"
        description="Создайте недельный план, чтобы добавлять рецепты в расписание."
        action={
          <Button asChild>
            <Link to="/menu">
              <CalendarPlus />
              Создать план
            </Link>
          </Button>
        }
      />
    )
  }

  return <AddRecipeToMenuLoaded menuPlans={menuPlans} recipe={recipe} />
}

function AddRecipeToMenuLoaded({
  menuPlans,
  recipe,
}: {
  menuPlans: readonly MenuPlan[]
  recipe: AddRecipeToMenuRecipe
}) {
  const firstMenuPlan = menuPlans[0]
  const [selectedMenuPlanId, setSelectedMenuPlanId] = useState(firstMenuPlan.id)

  const selectedMenuPlan =
    menuPlans.find((menuPlan) => menuPlan.id === selectedMenuPlanId) ?? firstMenuPlan

  return (
    <AddRecipeToMenuForm
      key={`${recipe.id}-${selectedMenuPlan.id}`}
      menuPlans={menuPlans}
      selectedMenuPlan={selectedMenuPlan}
      recipe={recipe}
      onSelectedMenuPlanChange={setSelectedMenuPlanId}
    />
  )
}

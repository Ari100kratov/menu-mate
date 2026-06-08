import { CalendarPlus, ChevronUp } from "lucide-react"
import { useState } from "react"
import { Link } from "react-router-dom"

import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import { useMenuPlansQuery } from "@/features/menu-planning/api/menu-plans.queries"
import type { AddRecipeToMenuRecipe } from "@/features/menu-planning/model/add-recipe-to-menu"
import { Button } from "@/shared/ui/button"
import { AddRecipeToMenuForm } from "./AddRecipeToMenuForm"

interface AddRecipeToMenuPanelProps {
  recipe: AddRecipeToMenuRecipe
}

export function AddRecipeToMenuPanel({ recipe }: AddRecipeToMenuPanelProps) {
  const menuPlansQuery = useMenuPlansQuery()
  const menuPlans = menuPlansQuery.data ?? []
  const firstMenuPlan = menuPlans.at(0)

  if (menuPlansQuery.isPending) {
    return (
      <Button type="button" disabled>
        <CalendarPlus className="size-4" />
        Загружаем планы...
      </Button>
    )
  }

  if (menuPlansQuery.error) {
    return (
      <Button type="button" variant="outline" disabled title="Не удалось загрузить планы меню">
        <CalendarPlus className="size-4" />
        Добавить в меню
      </Button>
    )
  }

  if (!firstMenuPlan) {
    return (
      <Button asChild>
        <Link to="/menu">
          <CalendarPlus className="size-4" />
          Создать план меню
        </Link>
      </Button>
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
  const [isOpen, setIsOpen] = useState(false)

  const selectedMenuPlan =
    menuPlans.find((menuPlan) => menuPlan.id === selectedMenuPlanId) ?? firstMenuPlan

  return (
    <div className="contents">
      <Button
        type="button"
        onClick={() => {
          setIsOpen((value) => !value)
        }}
      >
        {isOpen ? <ChevronUp className="size-4" /> : <CalendarPlus className="size-4" />}
        {isOpen ? "Скрыть добавление" : "Добавить в меню"}
      </Button>
      {isOpen ? (
        <div className="basis-full pt-2">
          <AddRecipeToMenuForm
            key={`${recipe.id}-${selectedMenuPlan.id}`}
            menuPlans={menuPlans}
            selectedMenuPlan={selectedMenuPlan}
            recipe={recipe}
            onSelectedMenuPlanChange={setSelectedMenuPlanId}
          />
        </div>
      ) : null}
    </div>
  )
}

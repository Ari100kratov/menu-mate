import { CalendarPlus, ShoppingCart } from "lucide-react"
import { useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"

import {
  useCreateMenuPlanMutation,
  useDeleteMenuPlanMutation,
  useMenuPlansQuery,
} from "@/features/menu-planning/api/menu-plans.queries"
import { CreateMenuPlanForm } from "@/features/menu-planning/ui/CreateMenuPlanForm"
import { MenuPlanSummaryButton } from "@/features/menu-planning/ui/MenuPlanSummaryButton"
import { MenuPlanWorkspace } from "@/features/menu-planning/ui/MenuPlanWorkspace"
import { useRecipesQuery } from "@/features/recipes/api/recipes.queries"
import { useGenerateShoppingListMutation } from "@/features/shopping-lists/api/shopping-lists.queries"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"

export default function MenuPage() {
  const navigate = useNavigate()
  const menuPlansQuery = useMenuPlansQuery()
  const recipesQuery = useRecipesQuery({})
  const createMenuPlanMutation = useCreateMenuPlanMutation()
  const deleteMenuPlanMutation = useDeleteMenuPlanMutation()
  const generateShoppingListMutation = useGenerateShoppingListMutation()
  const [activeMenuPlanId, setActiveMenuPlanId] = useState<string | null>(null)

  const menuPlans = useMemo(() => menuPlansQuery.data ?? [], [menuPlansQuery.data])
  const firstMenuPlan = menuPlans.at(0)
  const activeMenuPlan =
    (activeMenuPlanId
      ? menuPlans.find((menuPlan) => menuPlan.id === activeMenuPlanId)
      : undefined) ?? firstMenuPlan

  function handleGenerateShoppingList(menuPlanId: string) {
    generateShoppingListMutation.mutate(
      {
        menuPlanId,
        manualItems: [],
      },
      {
        onSuccess: (shoppingList) => {
          void navigate(`/shopping?listId=${shoppingList.id}`)
        },
      },
    )
  }

  return (
    <div className="space-y-5">
      {activeMenuPlan ? (
        <div className="flex justify-end">
          <Button
            type="button"
            className="w-full sm:w-auto"
            disabled={generateShoppingListMutation.isPending}
            onClick={() => {
              handleGenerateShoppingList(activeMenuPlan.id)
            }}
          >
            <ShoppingCart />
            {generateShoppingListMutation.isPending ? "Собираем..." : "Список покупок"}
          </Button>
        </div>
      ) : null}

      {menuPlansQuery.error ? <ErrorAlert error={menuPlansQuery.error} /> : null}
      {recipesQuery.error ? <ErrorAlert error={recipesQuery.error} /> : null}
      {createMenuPlanMutation.error ? <ErrorAlert error={createMenuPlanMutation.error} /> : null}
      {deleteMenuPlanMutation.error ? <ErrorAlert error={deleteMenuPlanMutation.error} /> : null}
      {generateShoppingListMutation.error ? (
        <ErrorAlert error={generateShoppingListMutation.error} />
      ) : null}

      <CreateMenuPlanForm
        isSubmitting={createMenuPlanMutation.isPending}
        onSubmit={(values) => {
          createMenuPlanMutation.mutate(values, {
            onSuccess: (menuPlan) => {
              setActiveMenuPlanId(menuPlan.id)
            },
          })
        }}
      />

      {menuPlansQuery.isPending ? (
        <PageSkeleton />
      ) : activeMenuPlan ? (
        <section className="grid gap-4 lg:grid-cols-[18rem_1fr]">
          <div className="space-y-2">
            {menuPlans.map((menuPlan) => (
              <MenuPlanSummaryButton
                key={menuPlan.id}
                menuPlan={menuPlan}
                isActive={menuPlan.id === activeMenuPlan.id}
                onClick={() => {
                  setActiveMenuPlanId(menuPlan.id)
                }}
              />
            ))}
          </div>

          <MenuPlanWorkspace
            menuPlan={activeMenuPlan}
            recipes={recipesQuery.data ?? []}
            isDeletePending={deleteMenuPlanMutation.isPending}
            onDelete={() => {
              if (!window.confirm(`Удалить план «${activeMenuPlan.name}»?`)) {
                return
              }

              deleteMenuPlanMutation.mutate(activeMenuPlan.id, {
                onSuccess: () => {
                  setActiveMenuPlanId(null)
                },
              })
            }}
          />
        </section>
      ) : (
        <EmptyState
          icon={CalendarPlus}
          title="Планов пока нет"
          description="Создайте неделю и добавьте блюда."
        />
      )}
    </div>
  )
}

import { Plus, Trash2 } from "lucide-react"
import { useState } from "react"

import type { MenuPlan } from "@/features/menu-planning/api/menu-plans.api"
import {
  useAddMenuPlanItemMutation,
  useRemoveMenuPlanItemMutation,
  useUpdateMenuPlanItemMutation,
} from "@/features/menu-planning/api/menu-plans.queries"
import { formatDate } from "@/features/menu-planning/model/menu-planning"
import {
  getDefaultMenuPlanItemValues,
  getMenuPlanItemPreset,
  toMenuPlanItemFormValues,
  toMenuPlanItemRequest,
  type MenuPlanItemFormValues,
} from "@/features/menu-planning/model/menu-plan-form"
import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"
import { MenuPlanItemForm } from "./MenuPlanItemForm"
import { MenuPlanWeekGrid } from "./MenuPlanWeekGrid"

interface MenuPlanWorkspaceProps {
  menuPlan: MenuPlan
  recipes: readonly RecipeListItem[]
  isDeletePending: boolean
  onDelete: () => void
}

export function MenuPlanWorkspace({
  menuPlan,
  recipes,
  isDeletePending,
  onDelete,
}: MenuPlanWorkspaceProps) {
  const addItemMutation = useAddMenuPlanItemMutation(menuPlan.id)
  const updateItemMutation = useUpdateMenuPlanItemMutation(menuPlan.id)
  const removeItemMutation = useRemoveMenuPlanItemMutation(menuPlan.id)
  const [editingItemId, setEditingItemId] = useState<string | null>(null)
  const [draftForm, setDraftForm] = useState(() => ({
    menuPlanId: menuPlan.id,
    version: 0,
    values: getDefaultMenuPlanItemValues(menuPlan),
  }))
  const draftFormVersion = draftForm.menuPlanId === menuPlan.id ? draftForm.version : 0
  const draftInitialValues =
    draftForm.menuPlanId === menuPlan.id ? draftForm.values : getDefaultMenuPlanItemValues(menuPlan)

  function presetNewItem(date: string, mealType: string) {
    setDraftForm((current) => ({
      menuPlanId: menuPlan.id,
      version: current.menuPlanId === menuPlan.id ? current.version + 1 : 1,
      values: getMenuPlanItemPreset(date, mealType),
    }))
    window.requestAnimationFrame(() => {
      document.getElementById("menu-plan-item-form")?.scrollIntoView({
        behavior: "smooth",
        block: "start",
      })
    })
  }

  function updateItem(itemId: string, values: MenuPlanItemFormValues) {
    updateItemMutation.mutate(
      {
        itemId,
        request: toMenuPlanItemRequest(values, recipes),
      },
      {
        onSuccess: () => {
          setEditingItemId(null)
        },
      },
    )
  }

  return (
    <div className="space-y-5">
      {addItemMutation.error ? <ErrorAlert error={addItemMutation.error} /> : null}
      {updateItemMutation.error ? <ErrorAlert error={updateItemMutation.error} /> : null}
      {removeItemMutation.error ? <ErrorAlert error={removeItemMutation.error} /> : null}

      <div className="flex flex-col gap-3 rounded-md border p-4 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-xl font-semibold tracking-normal">{menuPlan.name}</h2>
          <p className="text-muted-foreground text-sm">
            {formatDate(menuPlan.startDate)} - {formatDate(menuPlan.endDate)}
          </p>
        </div>
        <Button type="button" variant="ghost" disabled={isDeletePending} onClick={onDelete}>
          <Trash2 />
          Удалить
        </Button>
      </div>

      <MenuPlanItemForm
        key={`${menuPlan.id}-${String(draftFormVersion)}`}
        formId="menu-plan-item-form"
        title="Добавить прием пищи"
        submitLabel="Добавить"
        submitIcon="add"
        menuPlan={menuPlan}
        recipes={recipes}
        isSubmitting={addItemMutation.isPending}
        initialValues={draftInitialValues}
        resetAfterSubmit
        onSubmit={(values) => {
          addItemMutation.mutate(toMenuPlanItemRequest(values, recipes))
        }}
      />

      {menuPlan.items.length === 0 ? (
        <EmptyState
          icon={Plus}
          title="Пункты не добавлены"
          description="Добавьте блюдо из рецептов или свободную строку."
        />
      ) : null}

      <MenuPlanWeekGrid
        menuPlan={menuPlan}
        recipes={recipes}
        editingItemId={editingItemId}
        isMutationPending={removeItemMutation.isPending || updateItemMutation.isPending}
        onEdit={setEditingItemId}
        onCancelEdit={() => {
          setEditingItemId(null)
        }}
        onRemove={(itemId) => {
          removeItemMutation.mutate(itemId)
        }}
        onQuickAdd={presetNewItem}
        onCopy={(item) => {
          addItemMutation.mutate(toMenuPlanItemRequest(toMenuPlanItemFormValues(item), recipes))
        }}
        onMove={(item, date) => {
          updateItem(item.id, {
            ...toMenuPlanItemFormValues(item),
            date,
          })
        }}
        onUpdate={updateItem}
      />
    </div>
  )
}

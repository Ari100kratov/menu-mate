import { BookOpen, Copy, Pencil, Trash2 } from "lucide-react"
import { Link } from "react-router-dom"

import type { MenuPlan, MenuPlanItem } from "@/features/menu-planning/api/menu-plans.api"
import {
  formatDate,
  formatWeekday,
  getMealTypeLabel,
} from "@/features/menu-planning/model/menu-planning"
import {
  type MenuPlanItemFormValues,
  toMenuPlanItemFormValues,
} from "@/features/menu-planning/model/menu-plan-form"
import type { RecipeListItem } from "@/features/recipes/api/recipes.api"
import { Button } from "@/shared/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"
import { MenuPlanItemForm } from "./MenuPlanItemForm"

interface MenuPlanItemCardProps {
  item: MenuPlanItem
  menuPlan: MenuPlan
  recipes: readonly RecipeListItem[]
  isEditing: boolean
  isPending: boolean
  availableDates: readonly string[]
  onEdit: () => void
  onCancelEdit: () => void
  onRemove: () => void
  onCopy: () => void
  onMove: (date: string) => void
  onUpdate: (values: MenuPlanItemFormValues) => void
}

export function MenuPlanItemCard({
  item,
  menuPlan,
  recipes,
  isEditing,
  isPending,
  availableDates,
  onEdit,
  onCancelEdit,
  onRemove,
  onCopy,
  onMove,
  onUpdate,
}: MenuPlanItemCardProps) {
  if (isEditing) {
    return (
      <MenuPlanItemForm
        title="Редактировать"
        submitLabel="Сохранить"
        submitIcon="save"
        menuPlan={menuPlan}
        recipes={recipes}
        isSubmitting={isPending}
        initialValues={toMenuPlanItemFormValues(item)}
        onSubmit={onUpdate}
        onCancel={onCancelEdit}
      />
    )
  }

  return (
    <article className="bg-card text-card-foreground rounded-md border p-3">
      <div className="space-y-2">
        <div className="flex items-start justify-between gap-2">
          <div className="min-w-0">
            {item.recipeId ? (
              <Link
                to={`/recipes/${item.recipeId}`}
                className="hover:text-primary flex items-center gap-1 font-medium tracking-normal hover:underline"
              >
                <BookOpen className="size-4 shrink-0" />
                <span className="truncate">{item.recipeTitle ?? "Рецепт"}</span>
              </Link>
            ) : (
              <h3 className="font-medium tracking-normal">{item.text}</h3>
            )}
            <p className="text-muted-foreground text-xs">
              {getMealTypeLabel(item.mealType)}, {String(item.servings)} порц.
            </p>
          </div>
          <div className="flex shrink-0">
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label="Скопировать пункт меню"
              disabled={isPending}
              onClick={onCopy}
            >
              <Copy />
            </Button>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label="Редактировать пункт меню"
              disabled={isPending}
              onClick={onEdit}
            >
              <Pencil />
            </Button>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              aria-label="Удалить пункт меню"
              disabled={isPending}
              onClick={onRemove}
            >
              <Trash2 />
            </Button>
          </div>
        </div>
        {item.comment ? <p className="text-muted-foreground text-sm">{item.comment}</p> : null}
        <label className="grid gap-1 text-xs">
          <span className="text-muted-foreground">Перенести на день</span>
          <Select
            value={item.date}
            disabled={isPending}
            onValueChange={(value) => {
              if (value !== item.date) {
                onMove(value)
              }
            }}
          >
            <SelectTrigger className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {availableDates.map((date) => (
                <SelectItem key={date} value={date}>
                  {formatWeekday(date)}, {formatDate(date)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </label>
      </div>
    </article>
  )
}

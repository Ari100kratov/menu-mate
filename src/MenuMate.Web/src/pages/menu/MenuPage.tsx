import { Check, ShoppingCart } from "lucide-react"
import { useState } from "react"
import { useNavigate, useSearchParams } from "react-router-dom"
import { toast } from "sonner"

import {
  useAddMenuCalendarItemMutation,
  useClearMenuCalendarMutation,
  useCreateMealSlotMutation,
  useDeleteMealSlotMutation,
  useMenuCalendarQuery,
  useMealSlotsQuery,
  useRemoveMenuCalendarItemMutation,
  useReorderMealSlotsMutation,
  useUpdateMealSlotMutation,
  useUpdateMenuCalendarItemMutation,
} from "@/features/menu-planning/api/menu-calendar.queries"
import type { MenuCalendarItemRequest } from "@/features/menu-planning/api/menu-calendar.api"
import {
  getRange,
  shiftRange,
  type MenuDateRange,
  type MenuRangeMode,
  type PlacementRecipe,
} from "@/features/menu-planning/model/menu-calendar"
import { MealSlotSettings } from "@/features/menu-planning/ui/MealSlotSettings"
import { MenuCalendarView } from "@/features/menu-planning/ui/MenuCalendarView"
import { MenuRangeToolbar } from "@/features/menu-planning/ui/MenuRangeToolbar"
import { RecipePickerPanel } from "@/features/menu-planning/ui/RecipePickerPanel"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/ui/alert-dialog"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"

interface PickerTarget {
  date: string
  mealSlotId: string
}

export default function MenuPage() {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const [rangeMode, setRangeMode] = useState<MenuRangeMode>("week")
  const [range, setRange] = useState<MenuDateRange>(() => getRange("week"))
  const [pickerTarget, setPickerTarget] = useState<PickerTarget | null>(null)
  const [showMealSlotSettings, setShowMealSlotSettings] = useState(false)
  const [showClearConfirm, setShowClearConfirm] = useState(false)
  const calendarQuery = useMenuCalendarQuery(range.startDate, range.endDate)
  const mealSlotsQuery = useMealSlotsQuery()
  const addItemMutation = useAddMenuCalendarItemMutation()
  const updateItemMutation = useUpdateMenuCalendarItemMutation()
  const removeItemMutation = useRemoveMenuCalendarItemMutation()
  const clearCalendarMutation = useClearMenuCalendarMutation()
  const createMealSlotMutation = useCreateMealSlotMutation()
  const updateMealSlotMutation = useUpdateMealSlotMutation()
  const deleteMealSlotMutation = useDeleteMealSlotMutation()
  const reorderMealSlotsMutation = useReorderMealSlotsMutation()
  const placementRecipe = getPlacementRecipe(searchParams)
  const isCalendarMutationPending =
    addItemMutation.isPending || updateItemMutation.isPending || removeItemMutation.isPending
  const isMealSlotMutationPending =
    createMealSlotMutation.isPending ||
    updateMealSlotMutation.isPending ||
    deleteMealSlotMutation.isPending ||
    reorderMealSlotsMutation.isPending

  function changeMode(mode: MenuRangeMode) {
    setRangeMode(mode)
    if (mode !== "custom") {
      setRange(getRange(mode))
    }
  }

  function goToToday() {
    const mode = rangeMode === "custom" ? "week" : rangeMode
    setRangeMode(mode)
    setRange(getRange(mode))
  }

  function addRecipe(target: PickerTarget, recipe: PlacementRecipe, keepPlacementMode: boolean) {
    addItemMutation.mutate(toRecipeRequest(target, recipe), {
      onSuccess: () => {
        toast.success(`«${recipe.title}» добавлен в меню`)
        if (!keepPlacementMode) {
          setPickerTarget(null)
        }
      },
    })
  }

  function finishPlacement() {
    setSearchParams({})
  }

  if (pickerTarget) {
    return (
      <RecipePickerPanel
        onBack={() => {
          setPickerTarget(null)
        }}
        onSelect={(recipe) => {
          addRecipe(pickerTarget, recipe, false)
        }}
        onAddText={(text) => {
          addItemMutation.mutate(toTextRequest(pickerTarget, text), {
            onSuccess: () => {
              toast.success("Блюдо добавлено в меню")
              setPickerTarget(null)
            },
          })
        }}
      />
    )
  }

  const calendar = calendarQuery.data
  const mealSlots = mealSlotsQuery.data ?? calendar?.mealSlots ?? []
  const hasItemsInRange = Boolean(
    calendar?.items.some((item) => item.date >= range.startDate && item.date <= range.endDate),
  )

  return (
    <div className="mx-auto max-w-4xl space-y-4">
      <MenuRangeToolbar
        mode={rangeMode}
        range={range}
        onModeChange={changeMode}
        onRangeChange={(nextRange) => {
          setRangeMode("custom")
          setRange(nextRange)
        }}
        onPrevious={() => {
          setRange((current) => shiftRange(current, rangeMode, -1))
        }}
        onNext={() => {
          setRange((current) => shiftRange(current, rangeMode, 1))
        }}
        onToday={goToToday}
        canClearRange={hasItemsInRange}
        onEditMealSlots={() => {
          setShowMealSlotSettings(true)
        }}
        onClearRange={() => {
          setShowClearConfirm(true)
        }}
      />

      {placementRecipe ? (
        <section className="bg-primary text-primary-foreground sticky top-16 z-20 flex items-center gap-3 rounded-xl p-3 shadow-lg">
          <div className="min-w-0 flex-1">
            <p className="type-supporting opacity-80">Выбрано блюдо</p>
            <p className="type-label truncate">{placementRecipe.title}</p>
            <p className="type-supporting opacity-80">
              Нажимайте «Добавить сюда» в нужных приемах пищи.
            </p>
          </div>
          <Button type="button" variant="secondary" size="sm" onClick={finishPlacement}>
            <Check />
            Готово
          </Button>
        </section>
      ) : null}

      <MealSlotSettings
        key={mealSlots.map((slot) => `${slot.id}:${slot.name}:${String(slot.sortOrder)}`).join("|")}
        open={showMealSlotSettings}
        mealSlots={mealSlots}
        isPending={isMealSlotMutationPending}
        onOpenChange={setShowMealSlotSettings}
        onCreate={(name) => {
          createMealSlotMutation.mutate(name)
        }}
        onRename={(mealSlotId, name) => {
          updateMealSlotMutation.mutate({ mealSlotId, name })
        }}
        onDelete={(mealSlotId) => {
          deleteMealSlotMutation.mutate(mealSlotId)
        }}
        onReorder={(mealSlotIds) => {
          reorderMealSlotsMutation.mutate(mealSlotIds)
        }}
      />

      <AlertDialog open={showClearConfirm} onOpenChange={setShowClearConfirm}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Очистить выбранный диапазон?</AlertDialogTitle>
            <AlertDialogDescription>
              Все блюда в текущем выбранном диапазоне будут убраны из меню.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Отмена</AlertDialogCancel>
            <AlertDialogAction
              disabled={clearCalendarMutation.isPending}
              onClick={() => {
                clearCalendarMutation.mutate(range)
              }}
            >
              Очистить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {calendarQuery.error ? <ErrorAlert error={calendarQuery.error} /> : null}
      {mealSlotsQuery.error ? <ErrorAlert error={mealSlotsQuery.error} /> : null}
      {addItemMutation.error ? <ErrorAlert error={addItemMutation.error} /> : null}
      {updateItemMutation.error ? <ErrorAlert error={updateItemMutation.error} /> : null}
      {removeItemMutation.error ? <ErrorAlert error={removeItemMutation.error} /> : null}
      {clearCalendarMutation.error ? <ErrorAlert error={clearCalendarMutation.error} /> : null}
      {createMealSlotMutation.error ? <ErrorAlert error={createMealSlotMutation.error} /> : null}
      {updateMealSlotMutation.error ? <ErrorAlert error={updateMealSlotMutation.error} /> : null}
      {deleteMealSlotMutation.error ? <ErrorAlert error={deleteMealSlotMutation.error} /> : null}
      {reorderMealSlotsMutation.error ? (
        <ErrorAlert error={reorderMealSlotsMutation.error} />
      ) : null}

      <MenuCalendarView
        range={range}
        mealSlots={mealSlots}
        items={calendar?.items ?? []}
        isPlacementMode={Boolean(placementRecipe)}
        isPending={isCalendarMutationPending}
        isItemsLoading={calendarQuery.isFetching}
        onAdd={(date, mealSlotId) => {
          const target = { date, mealSlotId }
          if (placementRecipe) {
            addRecipe(target, placementRecipe, true)
          } else {
            setPickerTarget(target)
          }
        }}
        onUpdate={(itemId, request) => {
          updateItemMutation.mutate({ itemId, request })
        }}
        onRemove={(itemId) => {
          removeItemMutation.mutate(itemId)
        }}
      />

      <Button
        type="button"
        size="icon-lg"
        className="fixed right-4 bottom-20 z-30 size-12 rounded-full shadow-lg md:right-6 md:bottom-6"
        aria-label="Создать список покупок по выбранному диапазону"
        title="Создать список покупок"
        onClick={() => {
          const query = new URLSearchParams({ startDate: range.startDate, endDate: range.endDate })
          void navigate(`/shopping/preview?${query.toString()}`)
        }}
      >
        <ShoppingCart className="size-5" />
      </Button>
    </div>
  )
}

function getPlacementRecipe(searchParams: URLSearchParams): PlacementRecipe | null {
  const id = searchParams.get("recipeId")
  const currentRevisionId = searchParams.get("revisionId")
  const title = searchParams.get("title")
  const servings = Number(searchParams.get("servings"))
  if (!id || !currentRevisionId || !title || !Number.isFinite(servings) || servings < 1) return null
  return { id, currentRevisionId, title, servings }
}

function toRecipeRequest(target: PickerTarget, recipe: PlacementRecipe): MenuCalendarItemRequest {
  return {
    ...target,
    recipeId: recipe.id,
    recipeRevisionId: recipe.currentRevisionId,
    recipeTitle: recipe.title,
    text: null,
    servings: recipe.servings,
    comment: null,
  }
}

function toTextRequest(target: PickerTarget, text: string): MenuCalendarItemRequest {
  return {
    ...target,
    recipeId: null,
    recipeRevisionId: null,
    recipeTitle: null,
    text,
    servings: 1,
    comment: null,
  }
}

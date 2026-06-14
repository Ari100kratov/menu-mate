import { Minus, Plus, ShoppingCart } from "lucide-react"
import { useMemo, useState } from "react"
import { useNavigate, useSearchParams } from "react-router-dom"
import { toast } from "sonner"

import {
  useMenuShoppingPreviewQuery,
  useReplaceShoppingListFromMenuMutation,
  useShoppingListQuery,
} from "@/features/shopping-lists/api/shopping-lists.queries"
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
import { Checkbox } from "@/shared/ui/checkbox"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"

export default function ShoppingPreviewPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const startDate = searchParams.get("startDate") ?? ""
  const endDate = searchParams.get("endDate") ?? ""
  const previewQuery = useMenuShoppingPreviewQuery(startDate, endDate)
  const currentListQuery = useShoppingListQuery()
  const replaceMutation = useReplaceShoppingListFromMenuMutation()
  const [excluded, setExcluded] = useState<Set<string>>(() => new Set())
  const [servings, setServings] = useState<Record<string, number>>({})
  const [showReplaceConfirm, setShowReplaceConfirm] = useState(false)

  const currentItemCount = useMemo(
    () =>
      currentListQuery.data?.categories.reduce(
        (total, category) => total + category.items.length,
        0,
      ) ?? 0,
    [currentListQuery.data],
  )
  const totalIngredientCount =
    previewQuery.data?.recipes.reduce((total, recipe) => total + recipe.ingredients.length, 0) ?? 0
  const selectedCount = totalIngredientCount - excluded.size

  function replaceList() {
    const preview = previewQuery.data
    if (!preview) return

    replaceMutation.mutate(
      {
        startDate: preview.startDate,
        endDate: preview.endDate,
        recipes: preview.recipes.map((recipe) => ({
          menuItemId: recipe.menuItemId,
          servings: servings[recipe.menuItemId] ?? recipe.servings,
          ingredientIds: recipe.ingredients
            .filter(
              (ingredient) =>
                !excluded.has(selectionKey(recipe.menuItemId, ingredient.ingredientId)),
            )
            .map((ingredient) => ingredient.ingredientId),
        })),
      },
      {
        onSuccess: () => {
          toast.success("Список покупок заменён")
          void navigate("/shopping")
        },
      },
    )
  }

  if (previewQuery.isPending) return <PageSkeleton />

  const preview = previewQuery.data
  if (!preview) return <ErrorAlert error={previewQuery.error} />

  return (
    <div className="mx-auto max-w-3xl space-y-5 pb-20">
      <section className="bg-primary/5 border-primary/15 rounded-xl border p-4">
        <h2 className="type-section-title">Проверьте будущий список</h2>
        <p className="type-supporting text-muted-foreground mt-1">
          Снимите отметки с ненужных ингредиентов. Совместимые единицы будут приведены к одной
          системе и объединены.
        </p>
      </section>

      {replaceMutation.error ? <ErrorAlert error={replaceMutation.error} /> : null}
      {currentListQuery.error ? <ErrorAlert error={currentListQuery.error} /> : null}

      {preview.recipes.length === 0 ? (
        <EmptyState
          icon={ShoppingCart}
          title="В диапазоне нет рецептов"
          description="Добавьте блюда с ингредиентами в меню и попробуйте снова."
        />
      ) : (
        <div className="space-y-4">
          {preview.recipes.map((recipe) => {
            const recipeServings = servings[recipe.menuItemId] ?? recipe.servings
            return (
              <section key={recipe.menuItemId} className="bg-card rounded-xl border p-4 shadow-xs">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <h2 className="type-label truncate">{recipe.title}</h2>
                    <p className="type-supporting text-muted-foreground">Количество порций</p>
                  </div>
                  <div className="bg-muted flex items-center gap-1 rounded-full p-1">
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Уменьшить количество порций"
                      disabled={recipeServings <= 1}
                      onClick={() => {
                        setServings((current) => ({
                          ...current,
                          [recipe.menuItemId]: recipeServings - 1,
                        }))
                      }}
                    >
                      <Minus />
                    </Button>
                    <span className="min-w-7 text-center text-sm font-semibold">
                      {recipeServings}
                    </span>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Увеличить количество порций"
                      onClick={() => {
                        setServings((current) => ({
                          ...current,
                          [recipe.menuItemId]: recipeServings + 1,
                        }))
                      }}
                    >
                      <Plus />
                    </Button>
                  </div>
                </div>
                <div className="mt-3 divide-y">
                  {recipe.ingredients.map((ingredient) => {
                    const key = selectionKey(recipe.menuItemId, ingredient.ingredientId)
                    return (
                      <label key={key} className="flex cursor-pointer items-center gap-3 py-3">
                        <Checkbox
                          checked={!excluded.has(key)}
                          onCheckedChange={(checked) => {
                            setExcluded((current) => {
                              const next = new Set(current)
                              if (checked === true) next.delete(key)
                              else next.add(key)
                              return next
                            })
                          }}
                        />
                        <span className="min-w-0 flex-1">
                          <span className="type-body block">{ingredient.name}</span>
                          <span className="type-supporting text-muted-foreground block">
                            {formatPreviewAmount(
                              ingredient.amount,
                              ingredient.unit,
                              recipe.servings,
                              recipeServings,
                            )}
                            {ingredient.isOptional ? " · необязательно" : ""}
                            {ingredient.comment ? ` · ${ingredient.comment}` : ""}
                          </span>
                        </span>
                      </label>
                    )
                  })}
                </div>
              </section>
            )
          })}
        </div>
      )}

      <div className="bg-background/95 fixed inset-x-0 bottom-16 z-30 border-t p-3 backdrop-blur md:bottom-0 md:left-64">
        <Button
          className="mx-auto flex w-full max-w-3xl"
          disabled={
            selectedCount === 0 ||
            replaceMutation.isPending ||
            currentListQuery.isPending ||
            Boolean(currentListQuery.error)
          }
          onClick={() => {
            if (currentItemCount > 0) setShowReplaceConfirm(true)
            else replaceList()
          }}
        >
          <ShoppingCart />
          Заменить список · {String(selectedCount)}
        </Button>
      </div>

      <AlertDialog open={showReplaceConfirm} onOpenChange={setShowReplaceConfirm}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Заменить текущий список покупок?</AlertDialogTitle>
            <AlertDialogDescription>
              Все текущие позиции и отметки будут удалены. В список попадут только выбранные
              ингредиенты из меню.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={replaceList}>Заменить список</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

function selectionKey(menuItemId: string, ingredientId: string) {
  return `${menuItemId}:${ingredientId}`
}

function formatPreviewAmount(
  amount: number | string | null,
  unit: string,
  originalServings: number,
  servings: number,
) {
  if (amount === null || unit === "ToTaste") return "по вкусу"
  const numericAmount = Number(amount)
  const scaled = Math.round((numericAmount * servings * 100) / originalServings) / 100
  const labels: Record<string, string> = {
    Gram: "г",
    Kilogram: "кг",
    Milliliter: "мл",
    Liter: "л",
    Piece: "шт.",
    Tablespoon: "ст. л.",
    Teaspoon: "ч. л.",
  }
  return `${String(scaled)} ${labels[unit] ?? unit}`
}

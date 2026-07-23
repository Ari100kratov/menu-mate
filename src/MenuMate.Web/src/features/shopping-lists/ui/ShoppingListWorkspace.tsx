import { CheckCircle2, ChevronDown, Plus, ShoppingBasket } from "lucide-react"
import { useMemo, useState } from "react"
import { toast } from "sonner"

import type {
  ShoppingList,
  ShoppingListItem,
} from "@/features/shopping-lists/api/shopping-lists.api"
import {
  useAddShoppingListItemMutation,
  useRemoveShoppingListItemMutation,
  useSetShoppingListItemStateMutation,
  useUpdateShoppingListItemMutation,
} from "@/features/shopping-lists/api/shopping-lists.queries"
import {
  getDefaultShoppingItemFormValues,
  toShoppingListItemFormValues,
  toShoppingListItemRequest,
  type ShoppingItemFormValues,
} from "@/features/shopping-lists/model/shopping-list-ui"
import { Button } from "@/shared/ui/button"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/shared/ui/collapsible"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog"
import { ErrorAlert } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"
import { PageFloatingActions } from "@/shared/ui/page-floating-actions"
import { ScrollToTopButton } from "@/shared/ui/scroll-to-top-button"
import { ShoppingListItemForm } from "./ShoppingListItemForm"
import { ShoppingListItemRow } from "./ShoppingListItemRow"

interface ShoppingListWorkspaceProps {
  shoppingList: ShoppingList
}

type ShoppingDialogItem = ShoppingListItem | "new"

export function ShoppingListWorkspace({ shoppingList }: ShoppingListWorkspaceProps) {
  const addItemMutation = useAddShoppingListItemMutation()
  const updateItemMutation = useUpdateShoppingListItemMutation()
  const stateMutation = useSetShoppingListItemStateMutation()
  const removeItemMutation = useRemoveShoppingListItemMutation()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogItem, setDialogItem] = useState<ShoppingDialogItem>("new")
  const [collapsedCategoryNames, setCollapsedCategoryNames] = useState<Set<string>>(() => new Set())
  const itemCount = useMemo(
    () => shoppingList.categories.reduce((total, category) => total + category.items.length, 0),
    [shoppingList.categories],
  )
  const purchasedCount = useMemo(
    () =>
      shoppingList.categories.reduce(
        (total, category) => total + category.items.filter((item) => item.isPurchased).length,
        0,
      ),
    [shoppingList.categories],
  )
  const isFormSubmitting = addItemMutation.isPending || updateItemMutation.isPending
  const sourcePeriod = formatSourcePeriod(shoppingList.sourceStartDate, shoppingList.sourceEndDate)
  const isAllPurchased = itemCount > 0 && purchasedCount === itemCount

  const initialValues =
    dialogItem === "new"
      ? getDefaultShoppingItemFormValues()
      : toShoppingListItemFormValues(dialogItem)

  function openCreateDialog() {
    setDialogItem("new")
    setDialogOpen(true)
  }

  function openEditDialog(item: ShoppingListItem) {
    setDialogItem(item)
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
  }

  function setCategoryExpanded(categoryName: string, isExpanded: boolean) {
    setCollapsedCategoryNames((current) => {
      const next = new Set(current)

      if (isExpanded) {
        next.delete(categoryName)
      } else {
        next.add(categoryName)
      }

      return next
    })
  }

  function submitItem(values: ShoppingItemFormValues) {
    if (dialogItem !== "new") {
      updateItemMutation.mutate(
        { itemId: dialogItem.id, request: toShoppingListItemRequest(values) },
        {
          onSuccess: () => {
            closeDialog()
            toast.success("Покупка обновлена")
          },
        },
      )
      return
    }

    addItemMutation.mutate(toShoppingListItemRequest(values), {
      onSuccess: () => {
        closeDialog()
        toast.success("Покупка добавлена")
      },
    })
  }

  return (
    <div className="space-y-5">
      {itemCount > 0 ? (
        <section className="bg-primary/5 border-primary/15 rounded-xl border p-4">
          <p className="type-label flex items-center gap-2">
            {isAllPurchased ? (
              <CheckCircle2
                className="text-primary size-5 shrink-0 self-center"
                aria-hidden="true"
              />
            ) : null}
            <span>
              Отмечено {String(purchasedCount)} из {String(itemCount)}
            </span>
          </p>
          {sourcePeriod ? (
            <p className="type-supporting text-muted-foreground mt-1">
              Список по меню: {sourcePeriod}
            </p>
          ) : null}
        </section>
      ) : null}

      {addItemMutation.error ? <ErrorAlert error={addItemMutation.error} /> : null}
      {updateItemMutation.error ? <ErrorAlert error={updateItemMutation.error} /> : null}
      {stateMutation.error ? <ErrorAlert error={stateMutation.error} /> : null}
      {removeItemMutation.error ? <ErrorAlert error={removeItemMutation.error} /> : null}

      {itemCount === 0 ? (
        <EmptyState
          icon={ShoppingBasket}
          title="Список пока пуст"
          description="Добавьте первую покупку или сформируйте список из меню."
        />
      ) : (
        <div className="space-y-4">
          {shoppingList.categories.map((category) => {
            const purchasedInCategory = category.items.filter((item) => item.isPurchased).length
            const isExpanded = !collapsedCategoryNames.has(category.name)
            const isCategoryPurchased = purchasedInCategory === category.items.length

            return (
              <section key={category.name} className="bg-card rounded-xl border p-2 shadow-xs">
                <Collapsible
                  open={isExpanded}
                  onOpenChange={(open) => {
                    setCategoryExpanded(category.name, open)
                  }}
                >
                  <h2>
                    <CollapsibleTrigger asChild>
                      <button
                        type="button"
                        className="hover:bg-muted/60 focus-visible:ring-ring/50 flex w-full items-center gap-3 rounded-lg px-2 py-2 text-left transition-colors focus-visible:ring-[3px] focus-visible:outline-none"
                        aria-label={`${isExpanded ? "Свернуть" : "Развернуть"} категорию ${category.name}`}
                      >
                        <span className="type-label min-w-0 flex-1 truncate">{category.name}</span>
                        <span className="type-supporting text-muted-foreground flex shrink-0 items-center gap-1">
                          {isCategoryPurchased ? (
                            <CheckCircle2
                              className="text-primary size-4 shrink-0 self-center"
                              aria-hidden="true"
                            />
                          ) : null}
                          <span className="sr-only">
                            Отмечено {String(purchasedInCategory)} из{" "}
                            {String(category.items.length)}
                          </span>
                          <span className="sm:hidden" aria-hidden="true">
                            {String(purchasedInCategory)}/{String(category.items.length)}
                          </span>
                          <span className="hidden sm:inline" aria-hidden="true">
                            Отмечено {String(purchasedInCategory)} из{" "}
                            {String(category.items.length)}
                          </span>
                        </span>
                        <ChevronDown
                          className={`text-muted-foreground size-4 shrink-0 transition-transform ${
                            isExpanded ? "rotate-180" : ""
                          }`}
                        />
                      </button>
                    </CollapsibleTrigger>
                  </h2>
                  <CollapsibleContent>
                    <div className="divide-y border-t">
                      {category.items.map((item) => (
                        <ShoppingListItemRow
                          key={item.id}
                          item={item}
                          isPending={
                            (stateMutation.isPending &&
                              stateMutation.variables.itemId === item.id) ||
                            (removeItemMutation.isPending &&
                              removeItemMutation.variables === item.id)
                          }
                          onCheckedChange={(isPurchased) => {
                            stateMutation.mutate({ itemId: item.id, request: { isPurchased } })
                          }}
                          onEdit={() => {
                            openEditDialog(item)
                          }}
                          onRemove={() => {
                            removeItemMutation.mutate(item.id, {
                              onSuccess: () => {
                                toast.success("Покупка удалена")
                              },
                            })
                          }}
                        />
                      ))}
                    </div>
                  </CollapsibleContent>
                </Collapsible>
              </section>
            )
          })}
        </div>
      )}

      <PageFloatingActions>
        <Button
          type="button"
          size="icon-lg"
          className="size-12 rounded-full shadow-lg"
          aria-label="Добавить покупку"
          onClick={openCreateDialog}
        >
          <Plus className="size-5" />
        </Button>
        <ScrollToTopButton className="size-12 rounded-full shadow-lg" />
      </PageFloatingActions>

      <Dialog
        open={dialogOpen}
        onOpenChange={(open) => {
          setDialogOpen(open)
        }}
      >
        <DialogContent className="flex max-h-[92svh] flex-col sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {dialogItem === "new" ? "Добавить покупку" : "Редактировать покупку"}
            </DialogTitle>
            <DialogDescription>Выберите продукт из каталога или добавьте новый.</DialogDescription>
          </DialogHeader>
          <div className="min-h-0 flex-1 overflow-y-auto px-5">
            <ShoppingListItemForm
              key={dialogItem === "new" ? "new" : dialogItem.id}
              submitLabel={dialogItem === "new" ? "Добавить" : "Сохранить"}
              submitIcon={dialogItem === "new" ? "add" : "save"}
              isSubmitting={isFormSubmitting}
              initialValues={initialValues}
              onSubmit={submitItem}
            />
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}

function formatSourcePeriod(startDate: string | null, endDate: string | null) {
  if (!startDate || !endDate) {
    return null
  }

  const start = toLocalDate(startDate)
  const end = toLocalDate(endDate)
  const dayFormatter = new Intl.DateTimeFormat("ru-RU", { day: "numeric" })
  const monthFormatter = new Intl.DateTimeFormat("ru-RU", { month: "long" })
  const dateFormatter = new Intl.DateTimeFormat("ru-RU", { day: "numeric", month: "long" })

  if (start.getFullYear() === end.getFullYear() && start.getMonth() === end.getMonth()) {
    return start.getDate() === end.getDate()
      ? dateFormatter.format(start)
      : `${dayFormatter.format(start)}–${dayFormatter.format(end)} ${monthFormatter.format(end)}`
  }

  return `${dateFormatter.format(start)} – ${dateFormatter.format(end)}`
}

function toLocalDate(value: string) {
  const [year, month, day] = value.split("-").map(Number)
  return new Date(year, month - 1, day)
}

import { Plus, ShoppingBasket } from "lucide-react"
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
          <p className="type-supporting text-muted-foreground">
            {String(purchasedCount)} из {String(itemCount)} отмечено
          </p>
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
          {shoppingList.categories.map((category) => (
            <section key={category.name} className="bg-card rounded-xl border p-2 shadow-xs">
              <h2 className="type-label px-2 py-2">{category.name}</h2>
              <div className="divide-y">
                {category.items.map((item) => (
                  <ShoppingListItemRow
                    key={item.id}
                    item={item}
                    isPending={
                      (stateMutation.isPending && stateMutation.variables.itemId === item.id) ||
                      (removeItemMutation.isPending && removeItemMutation.variables === item.id)
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
            </section>
          ))}
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

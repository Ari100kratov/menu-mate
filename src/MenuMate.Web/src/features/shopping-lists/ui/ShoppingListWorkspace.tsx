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
import { useUserPreferencesStore } from "@/shared/config/user-preferences.store"
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
import { ShoppingListItemForm } from "./ShoppingListItemForm"
import { ShoppingListItemRow } from "./ShoppingListItemRow"

interface ShoppingListWorkspaceProps {
  shoppingList: ShoppingList
}

export function ShoppingListWorkspace({ shoppingList }: ShoppingListWorkspaceProps) {
  const addItemMutation = useAddShoppingListItemMutation()
  const updateItemMutation = useUpdateShoppingListItemMutation()
  const stateMutation = useSetShoppingListItemStateMutation()
  const removeItemMutation = useRemoveShoppingListItemMutation()
  const defaultUnit = useUserPreferencesStore((state) => state.defaultShoppingUnit)
  const defaultCategory = useUserPreferencesStore((state) => state.defaultShoppingCategory)
  const [dialogItem, setDialogItem] = useState<ShoppingListItem | "new" | null>(null)
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
  const isPending =
    addItemMutation.isPending ||
    updateItemMutation.isPending ||
    stateMutation.isPending ||
    removeItemMutation.isPending

  const initialValues =
    dialogItem === "new"
      ? getDefaultShoppingItemFormValues({ unit: defaultUnit, category: defaultCategory })
      : dialogItem
        ? toShoppingListItemFormValues(dialogItem)
        : getDefaultShoppingItemFormValues()

  function submitItem(values: ShoppingItemFormValues) {
    if (dialogItem !== "new" && dialogItem) {
      updateItemMutation.mutate(
        { itemId: dialogItem.id, request: toShoppingListItemRequest(values) },
        {
          onSuccess: () => {
            setDialogItem(null)
            toast.success("Покупка обновлена")
          },
        },
      )
      return
    }

    addItemMutation.mutate(toShoppingListItemRequest(values), {
      onSuccess: () => {
        setDialogItem(null)
        toast.success("Покупка добавлена")
      },
    })
  }

  return (
    <div className="space-y-5">
      <section className="bg-primary/5 border-primary/15 flex items-center justify-between gap-4 rounded-xl border p-4">
        <div>
          <p className="type-section-title">Список покупок</p>
          <p className="type-supporting text-muted-foreground">
            {itemCount === 0
              ? "Добавьте продукты вручную или создайте список из меню."
              : `${String(purchasedCount)} из ${String(itemCount)} отмечено`}
          </p>
        </div>
        <Button
          type="button"
          variant="secondary"
          onClick={() => {
            setDialogItem("new")
          }}
        >
          <Plus />
          Добавить
        </Button>
      </section>

      {addItemMutation.error ? <ErrorAlert error={addItemMutation.error} /> : null}
      {updateItemMutation.error ? <ErrorAlert error={updateItemMutation.error} /> : null}
      {stateMutation.error ? <ErrorAlert error={stateMutation.error} /> : null}
      {removeItemMutation.error ? <ErrorAlert error={removeItemMutation.error} /> : null}

      {itemCount === 0 ? (
        <EmptyState
          icon={ShoppingBasket}
          title="Список пока пуст"
          description="Добавьте первую покупку или сформируйте список из меню."
          action={
            <Button
              onClick={() => {
                setDialogItem("new")
              }}
            >
              <Plus />
              Добавить покупку
            </Button>
          }
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
                    isPending={isPending}
                    onCheckedChange={(isPurchased) => {
                      stateMutation.mutate({ itemId: item.id, request: { isPurchased } })
                    }}
                    onEdit={() => {
                      setDialogItem(item)
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

      <Button
        type="button"
        size="icon-lg"
        className="fixed right-4 bottom-20 z-30 size-12 rounded-full shadow-lg md:right-6 md:bottom-6"
        aria-label="Добавить покупку"
        onClick={() => {
          setDialogItem("new")
        }}
      >
        <Plus className="size-5" />
      </Button>

      <Dialog
        open={dialogItem !== null}
        onOpenChange={(open) => {
          if (!open) setDialogItem(null)
        }}
      >
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {dialogItem === "new" ? "Добавить покупку" : "Редактировать покупку"}
            </DialogTitle>
            <DialogDescription>
              Выберите продукт из каталога или добавьте новый с нужной категорией.
            </DialogDescription>
          </DialogHeader>
          <div className="overflow-y-auto px-5 pb-5">
            <ShoppingListItemForm
              key={dialogItem === "new" ? "new" : dialogItem?.id}
              submitLabel={dialogItem === "new" ? "Добавить" : "Сохранить"}
              submitIcon={dialogItem === "new" ? "add" : "save"}
              isSubmitting={isPending}
              initialValues={initialValues}
              onSubmit={submitItem}
              onCancel={() => {
                setDialogItem(null)
              }}
            />
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}

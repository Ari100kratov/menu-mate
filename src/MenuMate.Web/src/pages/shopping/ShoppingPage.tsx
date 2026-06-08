import { ListPlus, ShoppingBasket } from "lucide-react"
import { useEffect, useMemo } from "react"
import { Link, useSearchParams } from "react-router-dom"

import {
  useDeleteShoppingListMutation,
  useShoppingListQuery,
  useShoppingListsQuery,
} from "@/features/shopping-lists/api/shopping-lists.queries"
import { ShoppingListSummaryButton } from "@/features/shopping-lists/ui/ShoppingListSummaryButton"
import { ShoppingListWorkspace } from "@/features/shopping-lists/ui/ShoppingListWorkspace"
import { Button } from "@/shared/ui/button"
import { ErrorAlert, PageSkeleton } from "@/shared/ui/feedback"
import { EmptyState } from "@/shared/ui/page"

export default function ShoppingPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const selectedListId = searchParams.get("listId") ?? undefined
  const shoppingListsQuery = useShoppingListsQuery()
  const shoppingListQuery = useShoppingListQuery(selectedListId)
  const deleteShoppingListMutation = useDeleteShoppingListMutation()

  const summaries = useMemo(() => shoppingListsQuery.data ?? [], [shoppingListsQuery.data])

  useEffect(() => {
    if (!selectedListId && summaries[0]) {
      setSearchParams({ listId: summaries[0].id }, { replace: true })
    }
  }, [selectedListId, setSearchParams, summaries])

  function selectList(shoppingListId: string) {
    setSearchParams({ listId: shoppingListId })
  }

  return (
    <div className="space-y-5">
      <div className="flex justify-end">
        <Button asChild variant="secondary" className="w-full sm:w-auto">
          <Link to="/menu">
            <ListPlus />
            Создать из меню
          </Link>
        </Button>
      </div>

      {shoppingListsQuery.error ? <ErrorAlert error={shoppingListsQuery.error} /> : null}
      {shoppingListQuery.error ? <ErrorAlert error={shoppingListQuery.error} /> : null}
      {deleteShoppingListMutation.error ? (
        <ErrorAlert error={deleteShoppingListMutation.error} />
      ) : null}

      {shoppingListsQuery.isPending ? (
        <PageSkeleton />
      ) : summaries.length > 0 ? (
        <section className="grid gap-4 lg:grid-cols-[18rem_1fr]">
          <div className="space-y-2">
            {summaries.map((summary) => (
              <ShoppingListSummaryButton
                key={summary.id}
                summary={summary}
                isActive={summary.id === selectedListId}
                onClick={() => {
                  selectList(summary.id)
                }}
              />
            ))}
          </div>

          {shoppingListQuery.isPending ? <PageSkeleton /> : null}
          {shoppingListQuery.data ? (
            <ShoppingListWorkspace
              shoppingList={shoppingListQuery.data}
              isDeletePending={deleteShoppingListMutation.isPending}
              onDelete={() => {
                if (!window.confirm("Удалить список покупок?")) {
                  return
                }

                deleteShoppingListMutation.mutate(shoppingListQuery.data.id, {
                  onSuccess: () => {
                    setSearchParams({})
                  },
                })
              }}
            />
          ) : null}
        </section>
      ) : (
        <EmptyState
          icon={ShoppingBasket}
          title="Списков пока нет"
          description="Создайте список из плана меню."
          action={
            <Button asChild>
              <Link to="/menu">
                <ListPlus />
                Перейти к меню
              </Link>
            </Button>
          }
        />
      )}
    </div>
  )
}

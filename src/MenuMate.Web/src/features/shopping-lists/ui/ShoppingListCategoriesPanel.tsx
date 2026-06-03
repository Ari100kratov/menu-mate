import { ListChecks } from "lucide-react"

import type { ShoppingList } from "@/features/shopping-lists/api/shopping-lists.api"
import type { ShoppingItemFormValues } from "@/features/shopping-lists/model/shopping-list-ui"
import type {
  ShoppingListCategory,
  ShoppingListItemState,
} from "@/features/shopping-lists/model/shopping-list-state"
import { EmptyState } from "@/shared/ui/page"
import { ShoppingListCategorySection } from "./ShoppingListCategorySection"

interface ShoppingListCategoriesPanelProps {
  shoppingList: ShoppingList
  visibleCategories: readonly ShoppingListCategory[]
  collapsedCategoryNames: ReadonlySet<string>
  isPending: boolean
  isStoreMode: boolean
  editingItemId: string | null
  onToggleCategory: (categoryName: string) => void
  onCategoryStateChange: (category: ShoppingListCategory, nextState: ShoppingListItemState) => void
  onEdit: (itemId: string) => void
  onCancelEdit: () => void
  onUpdate: (itemId: string, values: ShoppingItemFormValues) => void
  onStateChange: (itemId: string, nextState: ShoppingListItemState) => void
  onRemove: (itemId: string) => void
}

export function ShoppingListCategoriesPanel({
  shoppingList,
  visibleCategories,
  collapsedCategoryNames,
  isPending,
  isStoreMode,
  editingItemId,
  onToggleCategory,
  onCategoryStateChange,
  onEdit,
  onCancelEdit,
  onUpdate,
  onStateChange,
  onRemove,
}: ShoppingListCategoriesPanelProps) {
  if (shoppingList.categories.length === 0) {
    return (
      <EmptyState
        icon={ListChecks}
        title="Позиции не добавлены"
        description="Добавьте продукт вручную или создайте список из меню."
      />
    )
  }

  if (visibleCategories.length === 0) {
    return (
      <EmptyState
        icon={ListChecks}
        title="Нет позиций по фильтру"
        description="Смените фильтр, чтобы увидеть остальные продукты."
      />
    )
  }

  return (
    <div className="space-y-4">
      {visibleCategories.map((category) => (
        <ShoppingListCategorySection
          key={category.name}
          category={category}
          isCollapsed={collapsedCategoryNames.has(category.name)}
          isPending={isPending}
          isStoreMode={isStoreMode}
          editingItemId={editingItemId}
          onToggle={() => {
            onToggleCategory(category.name)
          }}
          onMarkPurchased={() => {
            onCategoryStateChange(category, { isPurchased: true, isInStock: false })
          }}
          onMarkInStock={() => {
            onCategoryStateChange(category, { isPurchased: false, isInStock: true })
          }}
          onReset={() => {
            onCategoryStateChange(category, { isPurchased: false, isInStock: false })
          }}
          onEdit={onEdit}
          onCancelEdit={onCancelEdit}
          onUpdate={onUpdate}
          onStateChange={onStateChange}
          onRemove={onRemove}
        />
      ))}
    </div>
  )
}

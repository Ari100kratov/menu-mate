import { useState } from "react"

import type {
  RecipeListTagFilter,
  RecipeOwnershipFilter,
} from "@/features/recipes/model/recipe-list-filter-state"
import { RecipeTagFilter } from "@/features/recipes/ui/RecipeTagFilter"
import { Button } from "@/shared/ui/button"
import { Checkbox } from "@/shared/ui/checkbox"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog"
import { Label } from "@/shared/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

interface RecipeAdvancedFiltersDialogProps {
  open: boolean
  favoritesOnly: boolean
  ownership: RecipeOwnershipFilter
  selectedTags: RecipeListTagFilter[]
  showTagsInMain: boolean
  onOpenChange: (open: boolean) => void
  onApply: (values: RecipeAdvancedFilterValues) => void
}

export interface RecipeAdvancedFilterValues {
  favoritesOnly: boolean
  ownership: RecipeOwnershipFilter
  tags: RecipeListTagFilter[]
  showTagsInMain: boolean
}

export function RecipeAdvancedFiltersDialog({
  open,
  favoritesOnly,
  ownership,
  selectedTags,
  showTagsInMain,
  onOpenChange,
  onApply,
}: RecipeAdvancedFiltersDialogProps) {
  const [draft, setDraft] = useState<RecipeAdvancedFilterValues>(() =>
    createDraft(favoritesOnly, ownership, selectedTags, showTagsInMain),
  )

  const resetValues: RecipeAdvancedFilterValues = {
    favoritesOnly: false,
    ownership: "all",
    tags: [],
    showTagsInMain,
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>Расширенные фильтры</DialogTitle>
          <DialogDescription>Настройте условия и примените их к списку рецептов.</DialogDescription>
        </DialogHeader>

        <div className="space-y-6 overflow-y-auto px-5 pb-1">
          <div className="space-y-3">
            <Label className="text-sm font-medium">Принадлежность</Label>
            <Select
              value={draft.ownership}
              onValueChange={(value) => {
                setDraft((current) => ({
                  ...current,
                  ownership: value as RecipeOwnershipFilter,
                }))
              }}
            >
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все рецепты</SelectItem>
                <SelectItem value="mine">Только мои</SelectItem>
                <SelectItem value="others">Только чужие</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <FilterCheckbox
            id="favorites-only"
            checked={draft.favoritesOnly}
            label="Только избранные"
            onCheckedChange={(checked) => {
              setDraft((current) => ({ ...current, favoritesOnly: checked }))
            }}
          />

          <div className="space-y-3">
            <Label className="text-sm font-medium">Теги</Label>
            <RecipeTagFilter
              selectedTags={draft.tags}
              onChange={(tags) => {
                setDraft((current) => ({ ...current, tags }))
              }}
            />
          </div>

          <FilterCheckbox
            id="show-tags-in-main"
            checked={draft.showTagsInMain}
            label="Показывать фильтр по тегам над списком"
            onCheckedChange={(checked) => {
              setDraft((current) => ({ ...current, showTagsInMain: checked }))
            }}
          />
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => {
              setDraft(resetValues)
              onApply(resetValues)
              onOpenChange(false)
            }}
          >
            Сбросить фильтры
          </Button>
          <Button
            type="button"
            onClick={() => {
              onApply(draft)
              onOpenChange(false)
            }}
          >
            Применить
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function FilterCheckbox({
  id,
  checked,
  label,
  onCheckedChange,
}: {
  id: string
  checked: boolean
  label: string
  onCheckedChange: (checked: boolean) => void
}) {
  return (
    <div className="flex items-center gap-3">
      <Checkbox
        id={id}
        checked={checked}
        onCheckedChange={(nextChecked) => {
          onCheckedChange(nextChecked === true)
        }}
      />
      <Label htmlFor={id} className="cursor-pointer text-sm leading-5 font-normal">
        {label}
      </Label>
    </div>
  )
}

function createDraft(
  favoritesOnly: boolean,
  ownership: RecipeOwnershipFilter,
  tags: RecipeListTagFilter[],
  showTagsInMain: boolean,
): RecipeAdvancedFilterValues {
  return {
    favoritesOnly,
    ownership,
    tags,
    showTagsInMain,
  }
}

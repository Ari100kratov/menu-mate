import { BookOpen, Pencil, Trash2, Users, X } from "lucide-react"
import { useState } from "react"
import { Link } from "react-router-dom"

import type {
  MealSlot,
  MenuCalendarItem,
  MenuCalendarItemRequest,
} from "@/features/menu-planning/api/menu-calendar.api"
import { Button } from "@/shared/ui/button"
import { Input } from "@/shared/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

interface MenuItemRowProps {
  item: MenuCalendarItem
  mealSlots: readonly MealSlot[]
  isPending: boolean
  onUpdate: (request: MenuCalendarItemRequest) => void
  onRemove: () => void
}

export function MenuItemRow({ item, mealSlots, isPending, onUpdate, onRemove }: MenuItemRowProps) {
  const [isEditing, setIsEditing] = useState(false)
  const [mealSlotId, setMealSlotId] = useState(item.mealSlotId)
  const [servings, setServings] = useState(String(item.servings))
  const [comment, setComment] = useState(item.comment ?? "")
  const [text, setText] = useState(item.text ?? "")
  const title = item.recipeTitle ?? item.text ?? "Блюдо"

  if (isEditing) {
    return (
      <form
        className="bg-background space-y-3 rounded-lg border p-3"
        onSubmit={(event) => {
          event.preventDefault()
          onUpdate({
            date: item.date,
            mealSlotId,
            recipeId: item.recipeId,
            recipeRevisionId: item.recipeRevisionId,
            recipeTitle: item.recipeTitle,
            text: item.recipeId ? null : text.trim(),
            servings: Number(servings),
            comment: comment.trim() || null,
          })
          setIsEditing(false)
        }}
      >
        {!item.recipeId ? (
          <Input
            value={text}
            aria-label="Название блюда"
            onChange={(event) => {
              setText(event.target.value)
            }}
          />
        ) : (
          <p className="type-label">{title}</p>
        )}
        <div className="grid grid-cols-[minmax(0,1fr)_7rem] gap-2">
          <Select value={mealSlotId} onValueChange={setMealSlotId}>
            <SelectTrigger className="w-full" aria-label="Прием пищи">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {mealSlots.map((slot) => (
                <SelectItem key={slot.id} value={slot.id}>
                  {slot.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Input
            type="number"
            min={1}
            max={100}
            value={servings}
            aria-label="Количество порций"
            onChange={(event) => {
              setServings(event.target.value)
            }}
          />
        </div>
        <Input
          value={comment}
          placeholder="Комментарий"
          onChange={(event) => {
            setComment(event.target.value)
          }}
        />
        <div className="flex gap-2">
          <Button
            type="submit"
            size="sm"
            disabled={isPending || !Number(servings) || (!item.recipeId && !text.trim())}
          >
            Сохранить
          </Button>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            onClick={() => {
              setIsEditing(false)
            }}
          >
            <X />
            Отмена
          </Button>
        </div>
      </form>
    )
  }

  return (
    <article className="bg-background/85 group flex items-center gap-3 rounded-lg border p-2.5">
      {item.imageUrl ? (
        <img src={item.imageUrl} alt="" className="size-11 shrink-0 rounded-lg object-cover" />
      ) : (
        <div className="bg-primary/10 text-primary flex size-11 shrink-0 items-center justify-center rounded-lg">
          <BookOpen className="size-4" />
        </div>
      )}
      <div className="min-w-0 flex-1">
        {item.recipeId ? (
          <Link
            to={`/recipes/${item.recipeId}`}
            className="type-label hover:text-primary block truncate hover:underline"
          >
            {title}
          </Link>
        ) : (
          <p className="type-label truncate">{title}</p>
        )}
        <div className="type-supporting text-muted-foreground flex items-center gap-1">
          <Users className="size-4" />
          {String(item.servings)}
          {item.comment ? <span className="truncate">· {item.comment}</span> : null}
        </div>
      </div>
      <div className="flex shrink-0">
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          aria-label="Редактировать блюдо"
          disabled={isPending}
          onClick={() => {
            setIsEditing(true)
          }}
        >
          <Pencil />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          className="text-destructive hover:text-destructive"
          aria-label="Убрать блюдо"
          disabled={isPending}
          onClick={onRemove}
        >
          <Trash2 />
        </Button>
      </div>
    </article>
  )
}

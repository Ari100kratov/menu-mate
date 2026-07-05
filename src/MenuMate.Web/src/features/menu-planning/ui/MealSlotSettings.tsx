import { GripVertical, Plus, Save, Trash2 } from "lucide-react"
import { useState } from "react"
import {
  closestCenter,
  DndContext,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from "@dnd-kit/core"
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable"
import { CSS } from "@dnd-kit/utilities"

import type { MealSlot } from "@/features/menu-planning/api/menu-calendar.api"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/ui/alert-dialog"
import { Button } from "@/shared/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/shared/ui/dialog"
import { Input } from "@/shared/ui/input"

interface MealSlotSettingsProps {
  open: boolean
  mealSlots: readonly MealSlot[]
  isPending: boolean
  onOpenChange: (open: boolean) => void
  onCreate: (name: string) => void
  onRename: (mealSlotId: string, name: string) => void
  onDelete: (mealSlotId: string) => void
  onReorder: (mealSlotIds: readonly string[]) => void
}

export function MealSlotSettings({
  open,
  mealSlots,
  isPending,
  onOpenChange,
  onCreate,
  onRename,
  onDelete,
  onReorder,
}: MealSlotSettingsProps) {
  const [orderedSlots, setOrderedSlots] = useState(() => [...mealSlots])
  const [names, setNames] = useState<Record<string, string>>(() =>
    Object.fromEntries(mealSlots.map((slot) => [slot.id, slot.name])),
  )
  const [newName, setNewName] = useState("")
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 6 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  )

  function handleDragEnd(event: DragEndEvent) {
    if (!event.over || event.active.id === event.over.id) return

    const fromIndex = orderedSlots.findIndex((slot) => slot.id === event.active.id)
    const toIndex = orderedSlots.findIndex((slot) => slot.id === event.over?.id)
    const next = arrayMove(orderedSlots, fromIndex, toIndex)
    setOrderedSlots(next)
    onReorder(next.map((slot) => slot.id))
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="flex flex-col sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Приемы пищи</DialogTitle>
          <DialogDescription>Изменения применяются ко всему календарю.</DialogDescription>
        </DialogHeader>

        <div className="min-h-0 flex-1 space-y-3 overflow-y-auto px-5 py-4">
          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragEnd={handleDragEnd}
          >
            <SortableContext
              items={orderedSlots.map((slot) => slot.id)}
              strategy={verticalListSortingStrategy}
            >
              <div className="space-y-2">
                {orderedSlots.map((slot) => (
                  <SortableMealSlot
                    key={slot.id}
                    slot={slot}
                    name={names[slot.id] ?? slot.name}
                    canDelete={mealSlots.length > 1}
                    isPending={isPending}
                    onNameChange={(name) => {
                      setNames((current) => ({ ...current, [slot.id]: name }))
                    }}
                    onRename={() => {
                      onRename(slot.id, (names[slot.id] ?? slot.name).trim())
                    }}
                    onDelete={() => {
                      onDelete(slot.id)
                    }}
                  />
                ))}
              </div>
            </SortableContext>
          </DndContext>

          <form
            className="flex items-center gap-2 border-t pt-4"
            onSubmit={(event) => {
              event.preventDefault()
              const normalized = newName.trim()
              if (!normalized) return
              onCreate(normalized)
              setNewName("")
            }}
          >
            <Input
              value={newName}
              placeholder="Новый прием пищи"
              onChange={(event) => {
                setNewName(event.target.value)
              }}
            />
            <Button type="submit" className="h-10" disabled={isPending || !newName.trim()}>
              <Plus />
              Добавить
            </Button>
          </form>
        </div>
      </DialogContent>
    </Dialog>
  )
}

interface SortableMealSlotProps {
  slot: MealSlot
  name: string
  canDelete: boolean
  isPending: boolean
  onNameChange: (name: string) => void
  onRename: () => void
  onDelete: () => void
}

function SortableMealSlot({
  slot,
  name,
  canDelete,
  isPending,
  onNameChange,
  onRename,
  onDelete,
}: SortableMealSlotProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: slot.id,
    disabled: isPending,
  })
  const canSave = Boolean(name.trim()) && name.trim() !== slot.name

  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition }}
      className={
        isDragging
          ? "bg-secondary border-primary/30 relative z-10 grid grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-2 rounded-xl border p-2 shadow-lg"
          : "bg-card grid grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-2 rounded-xl border p-2"
      }
    >
      <Button
        type="button"
        variant="ghost"
        size="icon"
        className="cursor-grab touch-none active:cursor-grabbing"
        aria-label={`Изменить порядок: ${slot.name}`}
        disabled={isPending}
        {...attributes}
        {...listeners}
      >
        <GripVertical />
      </Button>
      <Input
        value={name}
        aria-label={`Название приема пищи ${slot.name}`}
        onChange={(event) => {
          onNameChange(event.target.value)
        }}
      />
      <div className="flex">
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          aria-label={`Сохранить ${slot.name}`}
          disabled={isPending || !canSave}
          onClick={onRename}
        >
          <Save />
        </Button>
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button
              type="button"
              variant="ghost"
              size="icon-sm"
              className="text-destructive hover:text-destructive"
              aria-label={`Удалить ${slot.name}`}
              disabled={isPending || !canDelete}
            >
              <Trash2 />
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Убрать прием пищи?</AlertDialogTitle>
              <AlertDialogDescription>
                «{slot.name}» и все запланированные в нем блюда будут удалены из календаря.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Отмена</AlertDialogCancel>
              <AlertDialogAction onClick={onDelete}>Удалить</AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  )
}

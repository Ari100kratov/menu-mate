import {
  CalendarDays,
  ChevronLeft,
  ChevronRight,
  RotateCcw,
  Settings2,
  Trash2,
  Utensils,
} from "lucide-react"

import type { MenuDateRange, MenuRangeMode } from "@/features/menu-planning/model/menu-calendar"
import { MenuDateRangePicker } from "@/features/menu-planning/ui/MenuDateRangePicker"
import { Button } from "@/shared/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/ui/dropdown-menu"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/ui/select"

interface MenuRangeToolbarProps {
  mode: MenuRangeMode
  range: MenuDateRange
  onModeChange: (mode: MenuRangeMode) => void
  onRangeChange: (range: MenuDateRange) => void
  onPrevious: () => void
  onNext: () => void
  onToday: () => void
  canClearRange: boolean
  onEditMealSlots: () => void
  onClearRange: () => void
}

export function MenuRangeToolbar({
  mode,
  range,
  onModeChange,
  onRangeChange,
  onPrevious,
  onNext,
  onToday,
  canClearRange,
  onEditMealSlots,
  onClearRange,
}: MenuRangeToolbarProps) {
  return (
    <section className="bg-secondary/60 border-primary/15 space-y-3 rounded-2xl border p-3 shadow-sm sm:p-4">
      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="outline"
          size="icon"
          aria-label="Предыдущий диапазон"
          onClick={onPrevious}
        >
          <ChevronLeft />
        </Button>
        <div className="flex min-w-0 flex-1 justify-center">
          <MenuDateRangePicker
            key={`${range.startDate}:${range.endDate}`}
            range={range}
            onChange={onRangeChange}
          />
        </div>
        <Button
          type="button"
          variant="outline"
          size="icon"
          aria-label="Следующий диапазон"
          onClick={onNext}
        >
          <ChevronRight />
        </Button>
      </div>

      <div className="grid grid-cols-[minmax(0,1fr)_auto_auto] gap-2">
        <Select
          value={mode}
          onValueChange={(value) => {
            onModeChange(value as MenuRangeMode)
          }}
        >
          <SelectTrigger className="w-full rounded-lg">
            <CalendarDays className="size-4" />
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="week">Неделя</SelectItem>
            <SelectItem value="month">Месяц</SelectItem>
            <SelectItem value="custom">Свободный диапазон</SelectItem>
          </SelectContent>
        </Select>
        <Button
          type="button"
          variant="secondary"
          aria-label="Перейти к текущему диапазону"
          title="Сегодня"
          onClick={onToday}
        >
          <RotateCcw />
          <span className="hidden sm:inline">Сегодня</span>
        </Button>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button type="button" variant="outline" size="icon" aria-label="Настройки меню">
              <Settings2 />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onSelect={onEditMealSlots}>
              <Utensils />
              Изменить приемы пищи
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              variant="destructive"
              disabled={!canClearRange}
              onSelect={onClearRange}
            >
              <Trash2 />
              Очистить выбранный диапазон
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </section>
  )
}

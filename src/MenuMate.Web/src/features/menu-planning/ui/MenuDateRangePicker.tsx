import { CalendarRange } from "lucide-react"
import { ru } from "date-fns/locale"
import type { DateRange } from "react-day-picker"
import { useState } from "react"

import {
  formatRangeLabel,
  formatInputDate,
  parseInputDate,
  type MenuDateRange,
} from "@/features/menu-planning/model/menu-calendar"
import { Button } from "@/shared/ui/button"
import { Calendar } from "@/shared/ui/calendar"
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/ui/popover"

interface MenuDateRangePickerProps {
  range: MenuDateRange
  onChange: (range: MenuDateRange) => void
}

export function MenuDateRangePicker({ range, onChange }: MenuDateRangePickerProps) {
  const selected = {
    from: parseInputDate(range.startDate),
    to: parseInputDate(range.endDate),
  }
  const [open, setOpen] = useState(false)
  const [draft, setDraft] = useState<DateRange | undefined>(selected)

  function handleSelect(nextRange: DateRange | undefined) {
    setDraft(nextRange)
    if (!nextRange?.from || !nextRange.to) return

    onChange({
      startDate: formatInputDate(nextRange.from),
      endDate: formatInputDate(nextRange.to),
    })
    setOpen(false)
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button type="button" variant="ghost" className="max-w-full min-w-0 px-2">
          <CalendarRange />
          <span className="truncate">{formatRangeLabel(range)}</span>
        </Button>
      </PopoverTrigger>
      <PopoverContent align="center">
        <Calendar
          mode="range"
          selected={draft}
          defaultMonth={selected.from}
          locale={ru}
          onSelect={handleSelect}
        />
      </PopoverContent>
    </Popover>
  )
}

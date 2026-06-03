import { BookOpen, CalendarDays, ClipboardList, UserRound, type LucideIcon } from "lucide-react"

export interface NavigationItem {
  title: string
  path: string
  icon: LucideIcon
}

export const primaryNavigation: NavigationItem[] = [
  {
    title: "Рецепты",
    path: "/recipes",
    icon: BookOpen,
  },
  {
    title: "Меню",
    path: "/menu",
    icon: CalendarDays,
  },
  {
    title: "Покупки",
    path: "/shopping",
    icon: ClipboardList,
  },
  {
    title: "Профиль",
    path: "/profile",
    icon: UserRound,
  },
]

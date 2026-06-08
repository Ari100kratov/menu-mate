import { BookOpen, CalendarDays, ClipboardList, UserRound, type LucideIcon } from "lucide-react"

export interface NavigationItem {
  title: string
  path: string
  icon: LucideIcon
}

export const workspaceNavigation: NavigationItem[] = [
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
]

export const accountNavigationItem: NavigationItem = {
  title: "Профиль",
  path: "/profile",
  icon: UserRound,
}

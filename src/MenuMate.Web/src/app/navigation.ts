import { BookOpen, CalendarDays, ClipboardList, UserRound, type LucideIcon } from "lucide-react"

import { readPersistentString, writePersistentString } from "@/shared/lib/persistent-state"

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

const lastWorkspaceSectionStorageKey = "menumate:navigation:last-section:v1"

export function rememberWorkspaceSection(pathname: string) {
  const section = workspaceNavigation.find(
    (item) => pathname === item.path || pathname.startsWith(`${item.path}/`),
  )
  if (section) {
    writePersistentString(lastWorkspaceSectionStorageKey, section.path)
  }
}

export function getLastWorkspaceSection() {
  const storedPath = readPersistentString(lastWorkspaceSectionStorageKey)
  if (storedPath && workspaceNavigation.some((item) => item.path === storedPath)) {
    return storedPath
  }

  return "/recipes"
}

export function getScrollRestorationKey(pathname: string, locationKey: string) {
  const shouldPreserveSectionScroll = workspaceNavigation.some((item) => item.path === pathname)
  return shouldPreserveSectionScroll ? pathname : locationKey
}

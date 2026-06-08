import { ArrowLeft, LogOut } from "lucide-react"
import { Link, NavLink, Outlet, matchPath, useLocation } from "react-router-dom"

import { accountNavigationItem, workspaceNavigation } from "@/app/navigation"
import { useCurrentUserQuery, useLogoutMutation } from "@/features/auth/api/auth.queries"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/ui/button"
import { ThemeToggle } from "@/shared/ui/theme-toggle"

interface PageChrome {
  title: string
  description?: string
  backTo?: string
}

export function AppShell() {
  const location = useLocation()
  const currentUserQuery = useCurrentUserQuery()
  const logoutMutation = useLogoutMutation()
  const chrome = getPageChrome(location.pathname)

  return (
    <div className="bg-background min-h-svh">
      <DesktopSidebar
        displayName={currentUserQuery.data?.displayName ?? "MenuMate"}
        onLogout={() => {
          logoutMutation.mutate()
        }}
      />
      <div className="md:pl-64">
        <AppTopBar chrome={chrome} pathname={location.pathname} />
        <main className="mx-auto w-full max-w-6xl px-4 py-4 pb-24 md:px-6 md:py-6 md:pb-8">
          <Outlet />
        </main>
      </div>
      <BottomNavigation />
    </div>
  )
}

function getPageChrome(pathname: string): PageChrome {
  const recipeEditMatch = matchPath({ path: "/recipes/:recipeId/edit", end: true }, pathname)
  if (recipeEditMatch?.params.recipeId) {
    return {
      title: "Редактирование рецепта",
      description: "Состав, ингредиенты и шаги",
      backTo: `/recipes/${recipeEditMatch.params.recipeId}`,
    }
  }

  if (matchPath({ path: "/recipes/new", end: true }, pathname)) {
    return {
      title: "Новый рецепт",
      description: "Мастер создания рецепта",
      backTo: "/recipes",
    }
  }

  if (matchPath({ path: "/recipes/:recipeId", end: true }, pathname)) {
    return {
      title: "Рецепт",
      description: "Ингредиенты, готовка и меню",
      backTo: "/recipes",
    }
  }

  if (matchPath({ path: "/menu", end: false }, pathname)) {
    return {
      title: "План меню",
      description: "Неделя, приемы пищи и список покупок",
    }
  }

  if (matchPath({ path: "/shopping", end: false }, pathname)) {
    return {
      title: "Покупки",
      description: "Списки, категории и поход в магазин",
    }
  }

  if (matchPath({ path: "/profile", end: false }, pathname)) {
    return {
      title: "Профиль",
      description: "Аккаунт, тема и локальные настройки",
    }
  }

  return {
    title: "Рецепты",
    description: "Рабочий список и быстрый поиск",
  }
}

function AppTopBar({ chrome, pathname }: { chrome: PageChrome; pathname: string }) {
  const isProfileActive = pathname.startsWith(accountNavigationItem.path)

  return (
    <header className="bg-background/95 sticky top-0 z-30 flex min-h-14 items-center justify-between gap-3 border-b px-3 py-2 backdrop-blur md:min-h-16 md:px-6">
      <div className="flex min-w-0 items-center gap-2">
        {chrome.backTo ? (
          <Button asChild type="button" variant="ghost" size="icon" aria-label="Назад">
            <Link to={chrome.backTo}>
              <ArrowLeft />
            </Link>
          </Button>
        ) : (
          <div className="bg-primary/10 text-primary flex size-9 shrink-0 items-center justify-center rounded-md text-sm font-semibold md:hidden">
            M
          </div>
        )}

        <div className="min-w-0">
          <h1 className="type-page-title truncate">{chrome.title}</h1>
          {chrome.description ? (
            <p className="type-supporting text-muted-foreground hidden truncate sm:block">
              {chrome.description}
            </p>
          ) : null}
        </div>
      </div>

      <div className="flex shrink-0 items-center gap-1">
        <ThemeToggle />
        <Button
          asChild
          type="button"
          variant={isProfileActive ? "secondary" : "ghost"}
          size="icon"
          aria-label={accountNavigationItem.title}
        >
          <Link to={accountNavigationItem.path}>
            <accountNavigationItem.icon />
          </Link>
        </Button>
      </div>
    </header>
  )
}

interface DesktopSidebarProps {
  displayName: string
  onLogout: () => void
}

function DesktopSidebar({ displayName, onLogout }: DesktopSidebarProps) {
  return (
    <aside className="bg-sidebar text-sidebar-foreground fixed inset-y-0 left-0 z-30 hidden w-64 flex-col border-r md:flex">
      <NavLink
        to={accountNavigationItem.path}
        className={({ isActive }) =>
          cn(
            "block border-b px-5 py-5 transition-colors",
            isActive
              ? "bg-sidebar-accent text-sidebar-accent-foreground"
              : "hover:bg-sidebar-accent",
          )
        }
      >
        <div className="text-lg font-semibold tracking-normal">MenuMate</div>
        <div className="text-muted-foreground truncate text-sm">{displayName}</div>
      </NavLink>

      <nav className="flex-1 space-y-1 px-3 py-4">
        {workspaceNavigation.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-sidebar-accent text-sidebar-accent-foreground"
                  : "text-muted-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
              )
            }
          >
            <item.icon className="size-4" />
            {item.title}
          </NavLink>
        ))}
      </nav>

      <div className="border-t p-3">
        <Button type="button" variant="ghost" className="w-full justify-start" onClick={onLogout}>
          <LogOut />
          Выйти
        </Button>
      </div>
    </aside>
  )
}

function BottomNavigation() {
  const navigationColumnCount = String(workspaceNavigation.length)

  return (
    <nav className="bg-background/95 fixed inset-x-0 bottom-0 z-40 border-t px-2 pb-[env(safe-area-inset-bottom)] backdrop-blur md:hidden">
      <div
        className="mx-auto grid h-16 max-w-sm"
        style={{ gridTemplateColumns: `repeat(${navigationColumnCount}, minmax(0, 1fr))` }}
      >
        {workspaceNavigation.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              cn(
                "flex min-w-0 flex-col items-center justify-center gap-1 rounded-md px-1 text-xs font-medium transition-colors",
                isActive ? "text-primary" : "text-muted-foreground",
              )
            }
          >
            <item.icon className="size-5" />
            <span className="max-w-full truncate">{item.title}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}

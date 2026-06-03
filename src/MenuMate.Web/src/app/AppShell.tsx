import { LogOut } from "lucide-react"
import { NavLink, Outlet } from "react-router-dom"

import { primaryNavigation } from "@/app/navigation"
import { useCurrentUserQuery, useLogoutMutation } from "@/features/auth/api/auth.queries"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/ui/button"
import { ThemeToggle } from "@/shared/ui/theme-toggle"

export function AppShell() {
  const currentUserQuery = useCurrentUserQuery()
  const logoutMutation = useLogoutMutation()

  return (
    <div className="bg-background min-h-svh">
      <DesktopSidebar
        displayName={currentUserQuery.data?.displayName ?? "MenuMate"}
        onLogout={() => {
          logoutMutation.mutate()
        }}
      />
      <div className="md:pl-64">
        <header className="bg-background/95 sticky top-0 z-20 flex h-14 items-center justify-between border-b px-4 backdrop-blur md:h-16 md:px-6">
          <div>
            <div className="text-sm font-semibold md:text-base">MenuMate</div>
            <div className="text-muted-foreground hidden text-xs md:block">
              Рецепты, меню и покупки в одном рабочем потоке
            </div>
          </div>
          <ThemeToggle />
        </header>
        <main className="mx-auto w-full max-w-6xl px-4 py-5 pb-24 md:px-6 md:pb-8">
          <Outlet />
        </main>
      </div>
      <BottomNavigation />
    </div>
  )
}

interface DesktopSidebarProps {
  displayName: string
  onLogout: () => void
}

function DesktopSidebar({ displayName, onLogout }: DesktopSidebarProps) {
  return (
    <aside className="bg-sidebar text-sidebar-foreground fixed inset-y-0 left-0 z-30 hidden w-64 flex-col border-r md:flex">
      <div className="border-b px-5 py-5">
        <div className="text-lg font-semibold tracking-normal">MenuMate</div>
        <div className="text-muted-foreground truncate text-sm">{displayName}</div>
      </div>

      <nav className="flex-1 space-y-1 px-3 py-4">
        {primaryNavigation.map((item) => (
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
  const navigationColumnCount = String(primaryNavigation.length)

  return (
    <nav className="bg-background/95 fixed inset-x-0 bottom-0 z-40 border-t px-2 pb-[env(safe-area-inset-bottom)] backdrop-blur md:hidden">
      <div
        className="mx-auto grid h-16 max-w-md"
        style={{ gridTemplateColumns: `repeat(${navigationColumnCount}, minmax(0, 1fr))` }}
      >
        {primaryNavigation.map((item) => (
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

import { ChevronLeft, ChevronRight, LoaderCircle, Search, UsersRound } from "lucide-react"
import { useState } from "react"

import { useAdminUsersQuery } from "@/features/admin/api/admin.queries"
import type { AdminUserListItem } from "@/features/admin/api/admin.api"
import { formatRole } from "@/features/auth/model/roles"
import { useDebouncedValue } from "@/shared/lib/use-debounced-value"
import { Button } from "@/shared/ui/button"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Input } from "@/shared/ui/input"
import { EmptyState, PageSection } from "@/shared/ui/page"
import { Skeleton } from "@/shared/ui/skeleton"

const pageSize = 20

export default function AdminUsersPage() {
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const debouncedSearch = useDebouncedValue(search, 350)
  const usersQuery = useAdminUsersQuery({
    search: debouncedSearch,
    page,
    pageSize,
  })

  const users = usersQuery.data?.items ?? []
  const totalPages = Math.max(1, Math.ceil((usersQuery.data?.totalCount ?? 0) / pageSize))
  const isSearchPending = search.trim() !== debouncedSearch.trim()

  return (
    <div className="space-y-5">
      <PageSection
        title="Пользователи"
        description="Поиск по имени или email. Показаны только данные, нужные для администрирования."
      >
        <div className="relative max-w-xl">
          {isSearchPending ? (
            <LoaderCircle className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 animate-spin" />
          ) : (
            <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
          )}
          <Input
            type="search"
            className="h-11 pl-9"
            value={search}
            placeholder="Имя или email"
            aria-label="Поиск пользователей по имени или email"
            aria-busy={isSearchPending}
            autoComplete="off"
            onChange={(event) => {
              setSearch(event.target.value)
              setPage(1)
            }}
          />
        </div>
      </PageSection>

      {usersQuery.error ? <ErrorAlert error={usersQuery.error} /> : null}

      {usersQuery.isPending ? (
        <AdminUsersSkeleton />
      ) : users.length > 0 ? (
        <>
          <section
            className="overflow-hidden rounded-md border"
            aria-label="Зарегистрированные пользователи"
          >
            <div className="bg-muted/50 text-muted-foreground hidden grid-cols-[minmax(0,1.5fr)_minmax(10rem,0.8fr)_minmax(9rem,0.75fr)_minmax(6rem,0.4fr)] gap-4 px-4 py-3 text-xs font-medium md:grid">
              <span>Пользователь</span>
              <span>Роли</span>
              <span>Регистрация</span>
              <span className="text-right">Рецепты</span>
            </div>
            <div className="divide-y">
              {users.map((user) => (
                <AdminUserRow key={user.id} user={user} />
              ))}
            </div>
          </section>
          <Pagination
            page={usersQuery.data?.page ?? page}
            totalPages={totalPages}
            totalCount={usersQuery.data?.totalCount ?? 0}
            onPageChange={setPage}
          />
        </>
      ) : (
        <EmptyState
          icon={UsersRound}
          title="Пользователи не найдены"
          description={
            search.trim()
              ? "Попробуйте изменить запрос."
              : "В системе пока нет зарегистрированных пользователей."
          }
        />
      )}
    </div>
  )
}

function AdminUserRow({ user }: { user: AdminUserListItem }) {
  return (
    <article className="grid gap-3 p-4 md:grid-cols-[minmax(0,1.5fr)_minmax(10rem,0.8fr)_minmax(9rem,0.75fr)_minmax(6rem,0.4fr)] md:items-center md:gap-4">
      <div className="min-w-0">
        <p className="truncate font-medium">{user.displayName}</p>
        <p className="text-muted-foreground text-sm break-all">{user.email}</p>
      </div>
      <div>
        <p className="text-muted-foreground mb-1 text-xs md:hidden">Роли</p>
        <div className="flex flex-wrap gap-1.5">
          {user.roles.map((role) => (
            <span
              key={role}
              className="bg-secondary text-secondary-foreground rounded-md px-2 py-1 text-xs font-medium"
            >
              {formatRole(role)}
            </span>
          ))}
        </div>
      </div>
      <div>
        <p className="text-muted-foreground mb-1 text-xs md:hidden">Регистрация</p>
        <p className="text-sm">{formatRegisteredAt(user.registeredAt)}</p>
      </div>
      <div className="md:text-right">
        <p className="text-muted-foreground mb-1 text-xs md:hidden">Рецепты</p>
        <p className="text-sm font-medium">{formatRecipeCount(user.recipesCount)}</p>
      </div>
    </article>
  )
}

function Pagination({
  page,
  totalPages,
  totalCount,
  onPageChange,
}: {
  page: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
}) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-3">
      <p className="text-muted-foreground text-sm" aria-live="polite">
        Всего пользователей: {totalCount}
      </p>
      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={page <= 1}
          onClick={() => {
            onPageChange(page - 1)
          }}
        >
          <ChevronLeft />
          Назад
        </Button>
        <span className="text-muted-foreground min-w-24 text-center text-sm">
          {page} из {totalPages}
        </span>
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => {
            onPageChange(page + 1)
          }}
        >
          Далее
          <ChevronRight />
        </Button>
      </div>
    </div>
  )
}

function AdminUsersSkeleton() {
  return (
    <section
      className="space-y-3 rounded-md border p-4"
      role="status"
      aria-label="Загружаем пользователей"
    >
      {Array.from({ length: 5 }, (_, index) => (
        <div
          key={index}
          className="grid gap-3 border-b pb-3 last:border-b-0 last:pb-0 md:grid-cols-4"
        >
          <div className="space-y-2">
            <Skeleton className="h-4 w-40" />
            <Skeleton className="h-3 w-56" />
          </div>
          <Skeleton className="h-7 w-24" />
          <Skeleton className="h-4 w-32" />
          <Skeleton className="h-4 w-20" />
        </div>
      ))}
    </section>
  )
}

function formatRegisteredAt(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value))
}

function formatRecipeCount(count: number) {
  const remainder100 = count % 100
  const remainder10 = count % 10
  const noun =
    remainder100 >= 11 && remainder100 <= 14
      ? "рецептов"
      : remainder10 === 1
        ? "рецепт"
        : remainder10 >= 2 && remainder10 <= 4
          ? "рецепта"
          : "рецептов"

  return `${String(count)} ${noun}`
}

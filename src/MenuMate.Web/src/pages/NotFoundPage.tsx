import { Link } from "react-router-dom"

import { Button } from "@/shared/ui/button"

export default function NotFoundPage() {
  return (
    <main className="bg-background flex min-h-svh items-center justify-center px-4 py-10">
      <section className="w-full max-w-sm space-y-4 text-center">
        <div className="text-muted-foreground text-sm font-medium">404</div>
        <h1 className="text-foreground text-2xl font-semibold tracking-normal">
          Страница не найдена
        </h1>
        <p className="text-muted-foreground text-sm">
          Такого раздела в приложении «План есть» нет или он еще не подключен.
        </p>
        <Button asChild>
          <Link to="/recipes">К рецептам</Link>
        </Button>
      </section>
    </main>
  )
}
